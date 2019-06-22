using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Utility;
using PaderbornUniversity.SILab.Hip.Webservice;
using PaderbornUniversity.SILab.Hip.Webservice.Logging;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<ThumbnailConfig>(Configuration.GetSection("Thumbnails"))
                .Configure<AuthConfig>(Configuration.GetSection("Auth"))
                .Configure<CorsConfig>(Configuration)
                .Configure<LoggingConfig>(Configuration.GetSection("HiPLoggerConfig"));

            var serviceProvider = services.BuildServiceProvider(); // allows us to actually get the configured services
            var authConfig = serviceProvider.GetService<IOptions<AuthConfig>>();

            // Configure authentication
            services
                .AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = authConfig.Value.Audience;
                    options.Authority = authConfig.Value.Authority;
                });

            // Configure authorization
            var domain = authConfig.Value.Authority;
            services.AddAuthorization(options =>
            {
                options.AddPolicy("read:datastore",
                    policy => policy.Requirements.Add(new HasScopeRequirement("read:datastore", domain)));
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<CorsConfig> corsConfig, IOptions<LoggingConfig> loggingConfig)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"))
                .AddDebug()
                .AddHipLogger(loggingConfig.Value);

            var logger = loggerFactory.CreateLogger("ApplicationStartup");
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRequestSchemeFixer();

                // Use CORS (important: must be before app.UseMvc())
                app.UseCors(builder =>
                   {
                       var corsEnvConf = corsConfig.Value.Cors[env.EnvironmentName];
                       builder
                        .WithOrigins(corsEnvConf.Origins)
                        .WithMethods(corsEnvConf.Methods)
                        .WithHeaders(corsEnvConf.Headers)
                        .WithExposedHeaders(corsEnvConf.ExposedHeaders);
                   });

                app.UseAuthentication();
                app.UseMvc();
                app.UseSwaggerUiHip();

                logger.LogInformation("ThumbnailService started successfully");
            }
            catch (Exception e)
            {
                logger.LogCritical($"ThumbnailService Startup Failed:{e.Message}");
                throw;
            }

        }
    }
}
