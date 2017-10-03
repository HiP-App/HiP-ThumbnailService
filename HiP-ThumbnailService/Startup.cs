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
            services.AddMvc();

            // Register the Swagger generator
            services.AddSwaggerGen(c =>
            {
                // Define a Swagger document
                c.SwaggerDoc("v1", new Info { Title = Name, Version = Version });
                c.OperationFilter<SwaggerOperationFilter>();
                c.OperationFilter<SwaggerFileUploadOperationFilter>();
                c.DescribeAllEnumsAsStrings();
            });

            services.Configure<EndpointConfig>(Configuration.GetSection("Endpoints"))
                .Configure<UploadFilesConfig>(Configuration.GetSection("UploadFiles"))
                .Configure<SizeConfig>(Configuration.GetSection("SizeConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<EndpointConfig> endpointConfig)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
