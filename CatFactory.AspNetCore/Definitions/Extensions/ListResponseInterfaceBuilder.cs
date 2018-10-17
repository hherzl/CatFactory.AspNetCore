using System.Collections.Generic;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ListResponseInterfaceBuilder
    {
        public static ListResponseInterfaceDefinition GetListResponseInterfaceDefinition(this AspNetCoreProject project)
            => new ListResponseInterfaceDefinition
            {
                Namespaces =
                {
                    "System.Collections.Generic"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "IListResponse",
                GenericTypes = new List<GenericTypeDefinition>
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Implements =
                {
                    "IResponse"
                },
                Properties =
                {
                    new PropertyDefinition("IEnumerable<TModel>", "Model")
                }
            };
    }
}
