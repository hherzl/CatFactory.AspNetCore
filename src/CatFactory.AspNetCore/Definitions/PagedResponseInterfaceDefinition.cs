using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class PagedResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public PagedResponseInterfaceDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "PagedResponse<TModel>";

            Implements.Add("IListResponse");

            Namespaces.Add("System");

            Properties.Add(new PropertyDefinition("Int32", "ItemsCount"));
            Properties.Add(new PropertyDefinition("Int32", "PageCount"));
        }
    }
}
