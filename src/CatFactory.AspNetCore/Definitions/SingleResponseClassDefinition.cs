using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class SingleResponseClassDefinition : CSharpClassDefinition
    {
        public SingleResponseClassDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "SingleResponse<TModel>";

            Implements.Add("ISingleResponse<TModel>");

            Namespaces.Add("System");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
            Properties.Add(new PropertyDefinition("TModel", "Model"));
        }
    }
}
