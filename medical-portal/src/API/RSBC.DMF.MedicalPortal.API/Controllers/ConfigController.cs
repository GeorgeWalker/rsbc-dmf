﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace RSBC.DMF.MedicalPortal.API.Controllers
{
    /// <summary>
    /// Configuration endpoint
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ConfigController : ControllerBase
    {
        private readonly ILogger<ConfigController> _logger;
        private readonly IHostEnvironment env;
        private readonly MedicalPortalConfiguration configuration;

        public ConfigController(ILogger<ConfigController> logger, IHostEnvironment env, MedicalPortalConfiguration configuration)
        {
            _logger = logger;
            this.env = env;
            this.configuration = configuration;
        }

        /// <summary>
        /// Get the client configuration for this environment
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<PublicConfiguration> Get()
        {
            var config = new PublicConfiguration();
            config.Environment = env.EnvironmentName;
            config.Keycloak = configuration.Keycloak;
            config.ChefsFormId = configuration.ChefsFormId;
            return Ok(config);
        }

        public class EFormsOptions
        {
            public string FormServerUrl { get; set; }

            public string EmrVendorId { get; set; }

            public string FhirServerUrl { get; set; }
            public string FormsMap { get; set; }

            public EFormDetails[] Forms =>
                string.IsNullOrEmpty(FormsMap)
                ? Array.Empty<EFormDetails>()
                : JsonSerializer.Deserialize<EFormDetails[]>(FormsMap, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public class EFormDetails
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class OidcOptions
        {
            public string Issuer { get; set; }
            public string Scope { get; set; }
            public string ClientId { get; set; }
        }
    }
}