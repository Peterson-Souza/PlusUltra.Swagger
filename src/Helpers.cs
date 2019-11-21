using System.IO;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlusUltra.Swagger
{
    public static class Helpers
    {
        public static OpenApiInfo CreateInfoForApiVersion(OpenApiInfo info, ApiVersionDescription description)
        {
            info.Version = description.ApiVersion.ToString();

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        public static SwaggerGenOptions IncludeXmls(this SwaggerGenOptions options)
        {
            var app = PlatformServices.Default.Application;
            var path = app.ApplicationBasePath;

            var files = Directory.GetFiles(path, "*.xml");
            foreach (var item in files)
                options.IncludeXmlComments(item);

            return options;
        }
    }
}