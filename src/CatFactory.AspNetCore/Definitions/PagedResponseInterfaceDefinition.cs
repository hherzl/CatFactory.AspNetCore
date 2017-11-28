using System.Collections.Generic;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class PagedResponseInterfaceDefinition
    {
        public static CSharpInterfaceDefinition GetPagedResponseInterfaceDefinition(this EntityFrameworkCoreProject project)
        {
            var definition = new CSharpInterfaceDefinition();

            definition.Namespaces.Add("System");
            definition.Namespace = project.GetResponsesNamespace();
            definition.Name = "PagedResponse";

            definition.GenericTypes = new List<GenericTypeDefinition>
            {
                new GenericTypeDefinition { Name = "TModel", Constraint = "TModel : class" }
            };

            definition.Implements.Add("IListResponse");
            definition.Properties.Add(new PropertyDefinition("Int32", "ItemsCount"));
            definition.Properties.Add(new PropertyDefinition("Int32", "PageCount"));

            return definition;
        }
    }
}
