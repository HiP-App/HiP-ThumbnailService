using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.ThumbnailService.Utility;
using PaderbornUniversity.SILab.Hip.Webservice;
using Swashbuckle.AspNetCore.Swagger;

namespace PaderbornUniversity.SILab.Hip.ThumbnailService
{
    public class Startup
    {
        private const string Name = "HiP Thumbnail Service";
        private const string Version = "v1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                // Define a Swagger document
                c.SwaggerDoc("v1", new Info { Title = Name, Version = Version });
                c.OperationFilter<SwaggerOperationFilter>();
                c.DescribeAllEnumsAsStrings();
            });

            services.Configure<EndpointConfig>(Configuration.GetSection("EndpointConfig"))
                .Configure<DirConfig>(Configuration.GetSection("DirConfig"))
                .Configure<SizeConfig>(Configuration.GetSection("SizeConfig"))
                .Configure<AuthConfig>(Configuration.GetSection("Auth"));

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
                //options.AddPolicy("write:datastore",
                //    policy => policy.Requirements.Add(new HasScopeRequirement("write:datastore", domain)));
                //options.AddPolicy("write:cms",
                //    policy => policy.Requirements.Add(new HasScopeRequirement("write:cms", domain)));
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<EndpointConfig> endpointConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();

            // Swagger / Swashbuckle configuration:
            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
            });

            // Configure SwaggerUI endpoint
            app.UseSwaggerUI(c =>
            {
                var swaggerJsonUrl = string.IsNullOrEmpty(endpointConfig.Value.SwaggerEndpoint)
                    ? $"/swagger/{Version}/swagger.json"
                    : endpointConfig.Value.SwaggerEndpoint;

                c.SwaggerEndpoint(swaggerJsonUrl, $"{Name} {Version}");
            });

        }
    }
}
