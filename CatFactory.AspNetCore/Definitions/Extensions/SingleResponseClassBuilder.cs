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
                    new PropertyDefinition("string", "Message"),
                    new PropertyDefinition("bool", "DidError"),
                    new PropertyDefinition("string", "ErrorMessage"),
                    new PropertyDefinition("TModel", "Model")
                }
            };
    }
}
