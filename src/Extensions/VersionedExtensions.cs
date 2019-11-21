using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PlusUltra.Swagger.Filters;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PlusUltra.Swagger.Extensions
{
    public static class VersionedExtensions
    {

        public static IServiceCollection AddVersionedDocumentation(this IServiceCollection services, OpenApiInfo info, Action<SwaggerGenOptions> configuration = null)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.DescribeAllParametersInCamelCase();

                    // resolve the IApiVersionDescriptionProvider service
                    // note: that we have to build a temporary service provider here because one has not been created yet
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    // add a swagger document for each discovered API version
                    // note: you might choose to skip or document deprecated API versions differently
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, Helpers.CreateInfoForApiVersion(info, description));
                    }

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

        public static IApplicationBuilder UseVersionedDocumentation(this IApplicationBuilder app, IApiVersionDescriptionProvider provider, Action<SwaggerUIOptions> configuration = null)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"./swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }

                configuration?.Invoke(c);
            });

            return app;
        }
    }
}