using CatFactory.ObjectOrientedProgramming;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ResponseClassBuilder
    {
        public static ResponseClassDefinition GetResponseClassDefinition(this AspNetCoreProject project)
            => new ResponseClassDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "Response",
                Implements =
                {
                    "IResponse"
                },
                Properties =
                {
                    new PropertyDefinition("string", "Message"),
                    new PropertyDefinition("bool", "DidError"),
                    new PropertyDefinition("string", "ErrorMessage")
                }
            };
    }
}
