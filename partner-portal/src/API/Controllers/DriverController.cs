﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rsbc.Dmf.IcbcAdapter.Client;
using Rsbc.Dmf.IcbcAdapter;
using Rsbc.Dmf.PartnerPortal.Api.ViewModels;
using System.Net;
using AutoMapper;

[Route("api/[controller]")]
[ApiController]
public class DriverController : Controller
{
    private readonly ICachedIcbcAdapterClient _icbcAdapterClient;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public DriverController(ICachedIcbcAdapterClient icbcAdapterClient, IMapper mapper, ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _icbcAdapterClient = icbcAdapterClient;
        _mapper = mapper;
        _logger = loggerFactory.CreateLogger<DriverController>();
        _configuration = configuration;
    }

    [HttpGet("info/{driverLicenceNumber}")]
    [ProducesResponseType(typeof(Driver), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ActionName(nameof(GetHistory))]
    public async Task<ActionResult<DriverInfoReply>> GetHistory([FromRoute]string driverLicenceNumber)
    {
        try
        {
            var request = new DriverInfoRequest();
            request.DriverLicence = driverLicenceNumber;
            var reply = await _icbcAdapterClient.GetDriverInfoAsync(request);

            var result = _mapper.Map<Driver>(reply);
            result.LicenseNumber = driverLicenceNumber;
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetHistory)} failed.");
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
