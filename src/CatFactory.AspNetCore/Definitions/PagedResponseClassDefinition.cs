using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class PagedResponseClassDefinition : CSharpClassDefinition
    {
        public PagedResponseClassDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "PagedResponse<TModel>";

            Implements.Add("IListResponse<TModel>");

            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));
            Properties.Add(new PropertyDefinition("Int32", "PageSize"));
            Properties.Add(new PropertyDefinition("Int32", "PageNumber"));
            Properties.Add(new PropertyDefinition("Int32", "ItemsCount"));
            Properties.Add(new PropertyDefinition("Int32", "PageCount")
            {
                IsReadOnly = true,
                GetBody = new List<ILine>()
                {
                    new CodeLine("PageSize == 0 ? 0 : ItemsCount / PageSize;")
                }
            });
        }
    }
}
