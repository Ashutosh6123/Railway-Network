using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TrainService.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerRequiresAuthorization = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(inherit: true)
            .OfType<AuthorizeAttribute>()
            .Any() == true;

        var actionRequiresAuthorization = context.MethodInfo
            .GetCustomAttributes(inherit: true)
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!controllerRequiresAuthorization && !actionRequiresAuthorization)
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>()
        });
    }
}
