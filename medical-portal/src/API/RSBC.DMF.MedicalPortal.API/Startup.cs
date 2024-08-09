using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using RSBC.DMF.MedicalPortal.API.Services;
using Serilog;
using Serilog.Events;
using System.Net;
using System.Security.Claims;
using System.Reflection;
using System.Data;
using static RSBC.DMF.MedicalPortal.API.Auth.AuthConstant;
using RSBC.DMF.MedicalPortal.API.Auth.Extension;
using Keycloak.AuthServices.Authentication;
using Newtonsoft.Json;
using RSBC.DMF.MedicalPortal.API.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Rsbc.Dmf.IcbcAdapter.Client;

namespace RSBC.DMF.MedicalPortal.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        private IConfiguration configuration { get; }
        private const string HealthCheckReadyTag = "ready";
        private readonly IHostEnvironment environment;

        public void ConfigureServices(IServiceCollection services)
        {
            // TODO change this later, this is not standard configuration, used driver-portal as a reference
            var config = this.InitializeConfiguration(services);

            services.AddKeycloakWebApiAuthentication(
                keycloakOptions =>
                {
                    keycloakOptions.Realm = config.Keycloak.Config.Realm;
                    keycloakOptions.Audience = config.Keycloak.Config.Audience;
                    keycloakOptions.AuthServerUrl = config.Keycloak.Config.Url;
                    keycloakOptions.VerifyTokenAudience = false;
                },
                jwtBearerOptions =>
                {
                    jwtBearerOptions.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context => await OnTokenValidatedAsync(context),
                        OnAuthenticationFailed = context =>
                        {
                            Log.Error(context.Exception, "Error validating bearer token");
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policies.Oidc,
                    policy => policy
                        // confirm this is working by using a bad secret, currently the secret is not being validated
                        .RequireAuthenticatedUser().AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    // TODO verify if we need to add scope medical-portal-ui and medical-portal-api or if just medical-portal will do, in other projects there are api and ui
                    // the below does not work, since the scope claim looks something like "email profile openid". This problem has already been solved, research the proper way to handle scope
                    // need to add the scope to keycloak admin UI before we can add the scope to FE, which would pass the scope claim to the BE
                    //.RequireClaim(Claims.Scope, "medical-portal")
                );

                options.AddPolicy(
                    Policies.MedicalPractitioner,
                    policy => policy
                        .RequireAuthenticatedUser()
                        .RequireRole(Claims.IdentityProvider, Roles.Practitoner, Roles.Moa));

                options.AddPolicy(Policies.Enrolled, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(Claims.IdentityProvider, Roles.Dmft));
            });

            services.AddControllers(options => { options.Filters.Add(new HttpResponseExceptionFilter()); })
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.Formatting = Formatting.Indented;
                    opts.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    opts.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

                    // ReferenceLoopHandling is set to Ignore to prevent JSON parser issues with the user / roles model.
                    opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RSBC.DMF.MedicalPortal.API", Version = "v1" });
            });

            var dpBuilder = services.AddDataProtection();
            var keyRingPath = configuration.GetValue("DATAPROTECTION__PATH", string.Empty);
            if (!string.IsNullOrWhiteSpace(keyRingPath))
            {
                //configure data protection folder for key sharing
                dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
            }

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                //set known network of forward headers
                options.ForwardLimit = 2;
                var configvalue = configuration.GetValue("app:knownNetwork", string.Empty)?.Split('/');
                if (configvalue.Length == 2)
                {
                    var knownNetwork = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse(configvalue[0]),
                        int.Parse(configvalue[1]));
                    options.KnownNetworks.Add(knownNetwork);
                }
            });
            services.AddCors(setupAction => setupAction.AddPolicy(Constants.CorsPolicy,
                corsPolicyBuilder => corsPolicyBuilder.WithOrigins(config.Settings.Cors.AllowedOrigins)));
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddResponseCompression();
            services.AddHealthChecks().AddCheck("Medical Portal API", () => HealthCheckResult.Healthy("OK"),
                new[] { HealthCheckReadyTag });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddHttpContextAccessor();

            // Add Case Management Service

            // NOTE use Rsbc.Dmf.CaseManagement.Client ServiceCollectionExtensions AddCaseManagementAdapterClient instead
            // Add Case Management System (CMS) Adapter 
            services.AddCaseManagementAdapterClient(configuration);

            // Add Document Storage Adapter
            services.AddDocumentStorageClient(configuration);

            // Add ICBC Adapter
            services.AddIcbcAdapterClient(configuration);
            services.AddSingleton<ICachedIcbcAdapterClient, CachedIcbcAdapterClient>();

            services.AddPidpAdapterClient(configuration);
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<DocumentFactory>();
            services.AddAutoMapperSingleton();
        }

        private async Task OnTokenValidatedAsync(TokenValidatedContext context)
        {
            if (context.Principal?.Identity is ClaimsIdentity identity
                && identity.IsAuthenticated)
            {
                // Flatten the Resource Access claim
                identity.AddClaims(
                    identity.GetResourceAccessRoles(Clients.License)
                        .Select(role => new Claim(identity.RoleClaimType, role))
                );

                identity.AddClaims(
                    identity.GetResourceAccessRoles(Clients.DmftStatus)
                        .Select(role => new Claim(identity.RoleClaimType, role))
                );

                if (environment.IsDevelopment() && configuration.GetValue<bool>("FEATURES_SIMPLE_AUTH"))
                {
                    identity.AddClaim(new Claim(identity.RoleClaimType, Roles.Practitoner));
                    identity.AddClaim(new Claim(identity.RoleClaimType, Roles.Dmft));
                }

                // TODO I think this is wrong, we should only need to call this once but this is validating on every request
                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                context.Principal = await userService.Login(context.Principal);
            }
        }

        private MedicalPortalConfiguration InitializeConfiguration(IServiceCollection services)
        {
            var config = new MedicalPortalConfiguration();
            this.configuration.Bind(config);

            services.AddSingleton(config);

            Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
            Log.Logger.Information("### PIdP Configuration:{0} ###", JsonSerializer.Serialize(config));

            return config;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging(opts =>
            {
                opts.GetLevel = ExcludeHealthChecks;
                opts.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
                {
                    diagCtx.Set("User", httpCtx.User.FindFirstValue(ClaimTypes.Upn));
                    diagCtx.Set("Host", httpCtx.Request.Host);
                    diagCtx.Set("UserAgent", httpCtx.Request.Headers["User-Agent"].ToString());
                    diagCtx.Set("RemoteIP", httpCtx.Connection.RemoteIpAddress.ToString());
                    diagCtx.Set("ConnectionId", httpCtx.Connection.Id);
                    diagCtx.Set("Forwarded", httpCtx.Request.Headers["Forwarded"].ToString());
                    diagCtx.Set("ContentLength", httpCtx.Response.ContentLength);
                };
            });

            app.UseHealthChecks("/hc/ready", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/hc/live", new HealthCheckOptions
            {
                // Exclude all checks and return a 200-Ok.
                Predicate = _ => false
            });

            app.UseAuthentication();

            app.UseCors(Constants.CorsPolicy);

            app.UseRouting();
            app.UseResponseCompression();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization(Policies.Oidc);
            });
        }

        private static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode >= (int)HttpStatusCode.InternalServerError
                    ? LogEventLevel.Error
                    : ctx.Request.Path.StartsWithSegments("/hc", StringComparison.InvariantCultureIgnoreCase)
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;
    }
}