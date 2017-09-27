using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class ListResponseClassDefinition : CSharpClassDefinition
    {
        public ListResponseClassDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "ListResponse<TModel>";

            Implements.Add("IListResponse<TModel>");

            Namespaces.Add("System");
            Namespaces.Add("System.Collections.Generic");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            Properties.Add(new PropertyDefinition("IEnumerable<TModel>", "Model"));
        }
    }
}
