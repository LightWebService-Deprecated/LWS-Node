using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LWS_Node.Swagger;

public class SwaggerHeaderOptions: IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-NODE-AUTH",
            Description = "Node Token for authorization",
            In = ParameterLocation.Header,
            Schema = new OpenApiSchema { Type = "string" },
            Required = true
        });
    }
}