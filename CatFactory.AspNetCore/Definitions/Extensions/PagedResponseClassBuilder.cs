﻿using CatFactory.CodeFactory;
using CatFactory.ObjectOrientedProgramming;

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
                AccessModifier = AccessModifier.Public,
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
                    new PropertyDefinition(AccessModifier.Public, "string", "Message") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "bool", "DidError") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "string", "ErrorMessage") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "IEnumerable<TModel>", "Model") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "int", "PageSize") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "int", "PageNumber") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "int", "ItemsCount") { IsAutomatic = true },
                    new PropertyDefinition(AccessModifier.Public, "double", "PageCount")
                    {
                        IsReadOnly = true,
                        GetBody =
                        {
                            new CodeLine("ItemsCount < PageSize ? 1 : (int)(((double)ItemsCount / PageSize) + 1);")
                        }
                    }
                }
            };
    }
}
