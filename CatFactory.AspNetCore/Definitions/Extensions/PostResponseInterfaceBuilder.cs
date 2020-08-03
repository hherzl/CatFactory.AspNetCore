using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class PostResponseInterfaceBuilder
    {
        public static PostResponseInterfaceDefinition GetPostResponseInterfaceDefinition(this AspNetCoreProject project)
            => new PostResponseInterfaceDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "IPostResponse",
                Implements =
                {
                    "IResponse"
                },
                Properties =
                {
                    new PropertyDefinition("object", "Id")
                }
            };
    }
}
