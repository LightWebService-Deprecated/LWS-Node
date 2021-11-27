using System.Diagnostics.CodeAnalysis;
using LWS_Node.Configuration;
using LWS_Node.Middleware;
using LWS_Node.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace LWS_Node
{
    [ExcludeFromCodeCoverage]
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
            // Add Configuration
            services.AddSingleton(Configuration.GetSection("NodeConfiguration").Get<NodeConfiguration>());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "LWS Node Server", Version = "v1"});
                c.OperationFilter<SwaggerHeaderOptions>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, NodeConfiguration configuration)
        {
            logger.LogInformation("Copy Node key within node manager!");
            logger.LogInformation($"Node Key: {configuration.NodeKey}");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "LWS-Node v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<NodeAuthMiddleware>();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}