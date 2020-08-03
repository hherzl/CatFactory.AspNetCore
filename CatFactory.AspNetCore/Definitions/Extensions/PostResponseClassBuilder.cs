using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class PostResponseClassBuilder
    {
        public static PostResponseClassDefinition GetPostResponseClassDefinition(this AspNetCoreProject project)
            => new PostResponseClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic"
                },
                Namespace = project.GetResponsesNamespace(),
                AccessModifier = AccessModifier.Public,
                Name = "PostResponse",
                BaseClass = "Response",
                Implements =
                {
                    "IPostResponse"
                },
                Properties =
                {
                    new PropertyDefinition(AccessModifier.Public, "object", "Id")
                    {
                        IsAutomatic = true
                    }
                }
            };
    }
}
