using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PlusUltra.Swagger.Filters;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlusUltra.Swagger.Extensions
{
    public static class UnvesionedExtensions
    {

        public static IServiceCollection AddDocumentation(this IServiceCollection services, OpenApiInfo info, string groupName = "v1", Action<SwaggerGenOptions> configuration = null)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.DescribeAllParametersInCamelCase();

                    options.SwaggerDoc(groupName, info);

                    // integrate xml comments
                    options.IncludeXmls();

                    options.OperationFilter<AddResponseHeadersFilter>();
                    options.OperationFilter<AuthResponsesOperationFilter>();
                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    configuration?.Invoke(options);
                });

            return services;
        }



        public static IApplicationBuilder UseDocumentation(this IApplicationBuilder app, string groupName = "v1", Action<ReDocOptions> configuration = null)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseReDoc(c =>
            {
                c.SpecUrl($"../swagger/{groupName}/swagger.json");
                c.RoutePrefix = "docs";
                c.HideDownloadButton();
                c.ExpandResponses("200,201");
                c.RequiredPropsFirst();
                c.PathInMiddlePanel();
                c.NativeScrollbars();
                c.SortPropsAlphabetically();

                configuration?.Invoke(c);
            });

            return app;
        }
    }
}