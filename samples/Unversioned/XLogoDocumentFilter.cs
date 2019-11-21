using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace sample
{
    public class XLogoDocumentFilter : IDocumentFilter
    {

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // need to check if extension already exists, otherwise swagger 
            // tries to re-add it and results in error  
            if (!swaggerDoc.Info.Extensions.ContainsKey("x-logo"))
            {
                swaggerDoc.Info.Extensions.Add("x-logo", new OpenApiObject
                {
                    {"url", new OpenApiString("https://redocly.github.io/redoc/petstore-logo.png")},
                    {"altText", new OpenApiString("PetStore Logo")}
                });
            }
        }
    }
}