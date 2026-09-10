using ApiGateway.DTOs;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiGateway.Swagger;

public sealed class CoachTypeSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CoachType))
        {
            schema.Description = "0 = General, 1 = Sleeper, 2 = AC3Tier, 3 = AC2Tier, 4 = AC1Tier.";
        }
    }
}
