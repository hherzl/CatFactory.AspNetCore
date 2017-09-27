using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class ListResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public ListResponseInterfaceDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "IListResponse<TModel>";

            Implements.Add("IResponse");

            Namespaces.Add("System.Collections.Generic");

            Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));
        }
    }
}
