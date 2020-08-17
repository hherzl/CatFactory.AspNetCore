using CatFactory.ObjectOrientedProgramming;

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
                AccessModifier = AccessModifier.Public,
                Name = "IPagedResponse",
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
                    new PropertyDefinition("int", "PageSize"),
                    new PropertyDefinition("int", "PageNumber"),
                    new PropertyDefinition("int", "ItemsCount"),
                    new PropertyDefinition("double", "PageCount")
                    {
                        IsReadOnly = true
                    }
                }
            };
    }
}
