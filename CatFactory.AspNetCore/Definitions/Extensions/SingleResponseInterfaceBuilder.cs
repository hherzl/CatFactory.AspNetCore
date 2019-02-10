using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class SingleResponseInterfaceBuilder
    {
        public static SingleResponseInterfaceDefinition GetSingleResponseInterfaceDefinition(this AspNetCoreProject project)
            => new SingleResponseInterfaceDefinition
            {
                Namespace = project.GetResponsesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "ISingleResponse",
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
                    "IResponse"
                },
                Properties =
                {
                    new PropertyDefinition("TModel", "Model")
                }
            };
    }
}
