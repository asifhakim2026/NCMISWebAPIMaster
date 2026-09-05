using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NCMISAPI.Swagger;

/// <summary>
/// Maps file upload actions to multipart/form-data request bodies for OpenAPI.
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileProperties = new Dictionary<string, OpenApiSchema>();
        var required = new HashSet<string>();

        foreach (var parameter in context.ApiDescription.ParameterDescriptions)
        {
            if (parameter.Source != BindingSource.Form && parameter.Source != BindingSource.Body)
                continue;

            if (parameter.Type == typeof(IFormFile))
            {
                var name = parameter.Name ?? "file";
                fileProperties[name] = CreateFileSchema();
                required.Add(name);
                continue;
            }

            foreach (var property in parameter.Type.GetProperties())
            {
                if (property.PropertyType != typeof(IFormFile))
                    continue;

                fileProperties[property.Name] = CreateFileSchema();
                required.Add(property.Name);
            }
        }

        if (fileProperties.Count == 0)
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileProperties,
                        Required = required
                    }
                }
            }
        };

        var fileParamNames = fileProperties.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        operation.Parameters = operation.Parameters
            .Where(p => p.Name is null || !fileParamNames.Contains(p.Name))
            .ToList();
    }

    private static OpenApiSchema CreateFileSchema() =>
        new()
        {
            Type = "string",
            Format = "binary"
        };
}
