using CatFactory.CodeFactory;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class PagedResponseClassBuilder
    {
        public static PagedResponseClassDefinition GetPagedResponseClassDefinition(this AspNetCoreProject project)
            => new PagedResponseClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Collections.Generic"
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
                    "IListResponse<TModel>"
                },
                Properties =
                {
                    new PropertyDefinition("String", "Message"),
                    new PropertyDefinition("Boolean", "DidError"),
                    new PropertyDefinition("String", "ErrorMessage"),
                    new PropertyDefinition("IEnumerable<TModel>", "Model"),
                    new PropertyDefinition("Int32", "PageSize"),
                    new PropertyDefinition("Int32", "PageNumber"),
                    new PropertyDefinition("Int32", "ItemsCount"),
                    new PropertyDefinition("Int32", "PageCount")
                    {
                        IsReadOnly = true,
                        GetBody =
                        {
                            new CodeLine("PageSize == 0 ? 0 : ItemsCount / PageSize;")
                        }
                    }
                }
            };
    }
}
