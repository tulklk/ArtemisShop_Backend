using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace AtermisShop_API.Swagger;

public class CamelCaseSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
            return;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var newProperties = new Dictionary<string, OpenApiSchema>();

        foreach (var property in schema.Properties)
        {
            var camelCaseName = ConvertToCamelCase(property.Key);
            newProperties[camelCaseName] = property.Value;
        }

        // Update properties
        schema.Properties.Clear();
        foreach (var property in newProperties)
        {
            schema.Properties[property.Key] = property.Value;
        }

        // Update required properties
        if (schema.Required != null && schema.Required.Count > 0)
        {
            var newRequired = schema.Required
                .Select(ConvertToCamelCase)
                .Where(name => schema.Properties.ContainsKey(name))
                .ToList();
            
            schema.Required.Clear();
            foreach (var req in newRequired)
            {
                schema.Required.Add(req);
            }
        }
    }

    private string ConvertToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}

