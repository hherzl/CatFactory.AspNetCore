using CatFactory.ObjectOrientedProgramming;

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
                AccessModifier = AccessModifier.Public,
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
                    new PropertyDefinition(AccessModifier.Public, "string", "Message") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "bool", "DidError") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "string", "ErrorMessage") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "IEnumerable<TModel>", "Model") { IsAutomatic = true }
                }
            };
    }
}
