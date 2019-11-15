using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using PlusUltra.Swagger.Filters;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.ReDoc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PlusUltra.Swagger.Extensions
{
    public static class SwaggerExtensions
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
                        options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(info, description));
                    }

                    // integrate xml comments
                    IncludeXMLS(options);

                    options.OperationFilter<AddResponseHeadersFilter>();
                    options.OperationFilter<AuthResponsesOperationFilter>();
                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    configuration?.Invoke(options);
                });

            return services;
        }

        public static IServiceCollection AddDocumentation(this IServiceCollection services, OpenApiInfo info, string groupName = "v1", Action<SwaggerGenOptions> configuration = null)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.DescribeAllParametersInCamelCase();

                    options.SwaggerDoc(groupName, info);

                    // integrate xml comments
                    IncludeXMLS(options);

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

        static OpenApiInfo CreateInfoForApiVersion(OpenApiInfo info, ApiVersionDescription description)
        {
            info.Version = description.ApiVersion.ToString();

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private static void IncludeXMLS(SwaggerGenOptions options)
        {
            var app = PlatformServices.Default.Application;
            var path = app.ApplicationBasePath;

            var files = Directory.GetFiles(path, "*.xml");
            foreach (var item in files)
                options.IncludeXmlComments(item);

        }
    }
}