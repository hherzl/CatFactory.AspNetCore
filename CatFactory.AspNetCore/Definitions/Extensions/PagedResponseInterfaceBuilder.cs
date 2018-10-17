using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class PagedResponseInterfaceBuilder
    {
        public static PagedResponseInterfaceDefinition GetPagedResponseInterfaceDefinition(this AspNetCoreProject project)
            => new PagedResponseInterfaceDefinition
            {
                Namespaces =
                {
                    "System"
                },
                Namespace = project.GetResponsesNamespace(),
                Name = "PagedResponse",
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
                    "IListResponse"
                },
                Properties =
                {
                    new PropertyDefinition("Int32", "ItemsCount"),
                    new PropertyDefinition("Int32", "PageCount")
                }
            };
    }
}
