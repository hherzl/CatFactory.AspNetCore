using CatFactory.OOP;

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
                    new PropertyDefinition("String", "Message"),
                    new PropertyDefinition("Boolean", "DidError"),
                    new PropertyDefinition("String", "ErrorMessage"),
                    new PropertyDefinition("TModel", "Model")
                }
            };
    }
}
