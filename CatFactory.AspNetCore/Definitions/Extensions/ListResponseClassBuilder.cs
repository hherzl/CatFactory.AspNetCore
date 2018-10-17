using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ListResponseClassBuilder
    {
        public static ListResponseClassDefinition GetListResponseClassDefinition(this AspNetCoreProject project)
            => new ListResponseClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "ListResponse",
                GenericTypes =
                {
                    new GenericTypeDefinition
                    {
                        Name = "TModel",
                        Constraint = "TModel : class"
                    }
                },
                Implements =
                {
                    "IListResponse<TModel>"
                },
                Properties =
                {
                    new PropertyDefinition("String", "Message"),
                    new PropertyDefinition("Boolean", "DidError"),
                    new PropertyDefinition("String", "ErrorMessage"),
                    new PropertyDefinition("IEnumerable<TModel>", "Model")
                }
            };
    }
}
