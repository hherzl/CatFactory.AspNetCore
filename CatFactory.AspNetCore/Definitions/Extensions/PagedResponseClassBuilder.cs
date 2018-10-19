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
                    new PropertyDefinition("string", "Message"),
                    new PropertyDefinition("bool", "DidError"),
                    new PropertyDefinition("string", "ErrorMessage"),
                    new PropertyDefinition("IEnumerable<TModel>", "Model"),
                    new PropertyDefinition("int", "PageSize"),
                    new PropertyDefinition("int", "PageNumber"),
                    new PropertyDefinition("int", "ItemsCount"),
                    new PropertyDefinition("int", "PageCount")
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
