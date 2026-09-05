using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NCMISAPI.Swagger;

/// <summary>
/// Prevents Swashbuckle from failing when it encounters IFormFile during parameter generation.
/// </summary>
public class FormFileParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo?.ParameterType == typeof(IFormFile))
        {
            parameter.Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };
        }
    }
}
