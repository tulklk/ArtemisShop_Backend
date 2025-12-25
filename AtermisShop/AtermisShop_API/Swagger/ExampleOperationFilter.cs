using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AtermisShop_API.Swagger;

public class ExampleOperationFilter : IOperationFilter
{
    private const int MaxDepth = 10; // Prevent infinite recursion

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var visitedSchemas = new HashSet<string>();
        
        // Add examples to all responses
        foreach (var response in operation.Responses)
        {
            if (response.Value.Content != null)
            {
                foreach (var content in response.Value.Content)
                {
                    if (content.Value.Schema != null)
                    {
                        // Generate example based on schema
                        var example = GenerateExampleFromSchema(content.Value.Schema, context, visitedSchemas, 0);
                        if (example != null)
                        {
                            content.Value.Example = example;
                        }
                    }
                }
            }
        }

        // Add examples to request body
        if (operation.RequestBody?.Content != null)
        {
            foreach (var content in operation.RequestBody.Content)
            {
                if (content.Value.Schema != null)
                {
                    var example = GenerateExampleFromSchema(content.Value.Schema, context, visitedSchemas, 0);
                    if (example != null)
                    {
                        content.Value.Example = example;
                    }
                }
            }
        }

        // Add examples to parameters
        if (operation.Parameters != null)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (parameter.Schema != null && parameter.Example == null)
                {
                    var example = GenerateExampleFromSchema(parameter.Schema, context, visitedSchemas, 0);
                    if (example != null)
                    {
                        parameter.Example = example;
                    }
                }
            }
        }
    }

    private IOpenApiAny? GenerateExampleFromSchema(OpenApiSchema schema, OperationFilterContext context, HashSet<string> visitedSchemas, int depth)
    {
        if (schema == null || depth >= MaxDepth) return null;

        // Handle reference types first
        if (schema.Reference != null)
        {
            var referenceId = schema.Reference.Id;
            if (!string.IsNullOrEmpty(referenceId))
            {
                // Check for circular reference
                if (visitedSchemas.Contains(referenceId))
                {
                    // Return a simple placeholder to avoid infinite recursion
                    return new OpenApiObject();
                }

                if (context.SchemaRepository.Schemas.TryGetValue(referenceId, out var referencedSchema))
                {
                    visitedSchemas.Add(referenceId);
                    var result = GenerateExampleFromSchema(referencedSchema, context, visitedSchemas, depth + 1);
                    visitedSchemas.Remove(referenceId); // Remove after processing to allow reuse in different contexts
                    return result;
                }
            }
        }

        // Handle AllOf (composition)
        if (schema.AllOf != null && schema.AllOf.Count > 0)
        {
            var obj = new OpenApiObject();
            foreach (var allOfSchema in schema.AllOf)
            {
                var allOfExample = GenerateExampleFromSchema(allOfSchema, context, visitedSchemas, depth + 1);
                if (allOfExample is OpenApiObject allOfObj)
                {
                    foreach (var prop in allOfObj)
                    {
                        obj[prop.Key] = prop.Value;
                    }
                }
            }
            if (obj.Count > 0) return obj;
        }

        // Handle array types
        if (schema.Type == "array")
        {
            if (schema.Items != null)
            {
                var itemExample = GenerateExampleFromSchema(schema.Items, context, visitedSchemas, depth + 1);
                if (itemExample != null)
                {
                    return new OpenApiArray { itemExample };
                }
            }
            // If no items schema, return empty array
            return new OpenApiArray();
        }

        // Handle object types
        if (schema.Type == "object" || (schema.Properties != null && schema.Properties.Count > 0))
        {
            var obj = new OpenApiObject();
            foreach (var property in schema.Properties)
            {
                var propertyExample = GenerateExampleFromProperty(property.Key, property.Value, context, visitedSchemas, depth + 1);
                if (propertyExample != null)
                {
                    obj[property.Key] = propertyExample;
                }
            }
            return obj.Count > 0 ? obj : null;
        }

        // Handle primitive types
        if (schema.Type != null)
        {
            return GeneratePrimitiveExample(schema);
        }

        return null;
    }

    private IOpenApiAny? GenerateExampleFromProperty(string propertyName, OpenApiSchema propertySchema, OperationFilterContext context, HashSet<string> visitedSchemas, int depth)
    {
        if (propertySchema == null || depth >= MaxDepth) return null;

        // Handle reference types
        if (propertySchema.Reference != null)
        {
            var referenceId = propertySchema.Reference.Id;
            if (!string.IsNullOrEmpty(referenceId))
            {
                // Check for circular reference
                if (visitedSchemas.Contains(referenceId))
                {
                    // Return a simple placeholder to avoid infinite recursion
                    return new OpenApiObject();
                }

                if (context.SchemaRepository.Schemas.TryGetValue(referenceId, out var referencedSchema))
                {
                    visitedSchemas.Add(referenceId);
                    var result = GenerateExampleFromSchema(referencedSchema, context, visitedSchemas, depth + 1);
                    visitedSchemas.Remove(referenceId); // Remove after processing
                    return result;
                }
            }
        }

        // Handle AllOf
        if (propertySchema.AllOf != null && propertySchema.AllOf.Count > 0)
        {
            var obj = new OpenApiObject();
            foreach (var allOfSchema in propertySchema.AllOf)
            {
                var allOfExample = GenerateExampleFromSchema(allOfSchema, context, visitedSchemas, depth + 1);
                if (allOfExample is OpenApiObject allOfObj)
                {
                    foreach (var prop in allOfObj)
                    {
                        obj[prop.Key] = prop.Value;
                    }
                }
            }
            if (obj.Count > 0) return obj;
        }

        // Handle array types
        if (propertySchema.Type == "array")
        {
            if (propertySchema.Items != null)
            {
                var itemExample = GenerateExampleFromSchema(propertySchema.Items, context, visitedSchemas, depth + 1);
                if (itemExample != null)
                {
                    return new OpenApiArray { itemExample };
                }
            }
            // If no items schema, return empty array
            return new OpenApiArray();
        }

        // Handle object types
        if (propertySchema.Type == "object" || (propertySchema.Properties != null && propertySchema.Properties.Count > 0))
        {
            return GenerateExampleFromSchema(propertySchema, context, visitedSchemas, depth + 1);
        }

        // Handle primitive types with context-aware examples
        return GeneratePrimitiveExample(propertySchema, propertyName);
    }

    private IOpenApiAny? GeneratePrimitiveExample(OpenApiSchema schema, string? propertyName = null)
    {
        // Handle UUID format
        if (schema.Format == "uuid" || (schema.Type == "string" && schema.Format == "uuid"))
        {
            return new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        }

        // Handle based on property name for better examples
        if (!string.IsNullOrEmpty(propertyName))
        {
            var lowerName = propertyName.ToLowerInvariant();
            
            if (lowerName.Contains("email"))
            {
                return new OpenApiString("user@example.com");
            }
            if (lowerName.Contains("phone"))
            {
                return new OpenApiString("+1234567890");
            }
            if (lowerName.Contains("url") || lowerName.Contains("image") || lowerName.Contains("avatar"))
            {
                return new OpenApiString("https://example.com/image.jpg");
            }
            if (lowerName.Contains("price") || lowerName.Contains("amount") || lowerName.Contains("total"))
            {
                return new OpenApiDouble(99.99);
            }
            if (lowerName.Contains("quantity") || lowerName.Contains("count") || lowerName.Contains("stock"))
            {
                return new OpenApiInteger(10);
            }
            if (lowerName.Contains("name") || lowerName.Contains("title"))
            {
                return new OpenApiString("Sample Name");
            }
            if (lowerName.Contains("description"))
            {
                return new OpenApiString("Sample description");
            }
            if (lowerName.Contains("active") || lowerName.Contains("isactive") || lowerName.Contains("isenabled"))
            {
                return new OpenApiBoolean(true);
            }
        }

        // Handle based on schema type and format
        switch (schema.Type)
        {
            case "string":
                if (schema.Format == "date-time")
                {
                    return new OpenApiString(DateTime.UtcNow.ToString("o"));
                }
                if (schema.Format == "date")
                {
                    return new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-dd"));
                }
                if (schema.Format == "email")
                {
                    return new OpenApiString("user@example.com");
                }
                return new OpenApiString("string");
            case "integer":
            case "int32":
            case "int64":
                return new OpenApiInteger(0);
            case "number":
            case "double":
            case "float":
                return new OpenApiDouble(0.0);
            case "boolean":
                return new OpenApiBoolean(false);
            default:
                return null;
        }
    }
}

