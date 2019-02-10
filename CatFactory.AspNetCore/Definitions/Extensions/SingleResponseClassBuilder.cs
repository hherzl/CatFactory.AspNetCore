using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class SingleResponseClassBuilder
    {
        public static SingleResponseClassDefinition GetSingleResponseClassDefinition(this AspNetCoreProject project)
            => new SingleResponseClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "SingleResponse",
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
                    "ISingleResponse<TModel>"
                },
                Properties =
                {
                    new PropertyDefinition(AccessModifier.Public, "string", "Message") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "bool", "DidError") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "string", "ErrorMessage") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "TModel", "Model") { IsAutomatic = true }
                }
            };
    }
}
