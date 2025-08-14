using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TennisScoresAPI;

public class CustomOperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];

        // Exemple : "Tournaments_GetAll" ou "Tournaments_GetById"
        operation.OperationId = $"{controllerName}_{actionName}";
    }
}
