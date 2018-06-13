using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Swagger
{
    public class AddHeaderOperationFilter : IOperationFilter
    {
        private readonly string parameterName;
        private readonly string description;

        public AddHeaderOperationFilter(string parameterName, string description)
        {
            this.parameterName = parameterName;
            this.description = description;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IParameter>();
            }

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = parameterName,
                In = "header",
                Description = description,
                Required = false,
                Type = "string"
            });
        }
    }
}
