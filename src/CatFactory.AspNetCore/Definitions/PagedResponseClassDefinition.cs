using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class PagedResponseClassDefinition
    {
        public static CSharpClassDefinition GetPagedResponseClassDefinition(this EntityFrameworkCoreProject project)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespaces.Add("System.Collections.Generic");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "PagedResponse";

            definition.GenericTypes = new List<GenericTypeDefinition>
            {
                new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
            };

            definition.Implements.Add("IListResponse<TModel>");
            definition.Properties.Add(new PropertyDefinition("String", "Message"));
            definition.Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            definition.Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            definition.Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));
            definition.Properties.Add(new PropertyDefinition("Int32", "PageSize"));
            definition.Properties.Add(new PropertyDefinition("Int32", "PageNumber"));
            definition.Properties.Add(new PropertyDefinition("Int32", "ItemsCount"));
            definition.Properties.Add(new PropertyDefinition("Int32", "PageCount")
            {
                IsReadOnly = true,
                GetBody = new List<ILine>
                {
                    new CodeLine("PageSize == 0 ? 0 : ItemsCount / PageSize;")
                }
            });

            return definition;
        }
    }
}
