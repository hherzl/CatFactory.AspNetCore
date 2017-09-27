using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class ResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public ResponseInterfaceDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "IResponse";

            Namespaces.Add("System");

            Properties.Add(new PropertyDefinition("String", "Message"));
            Properties.Add(new PropertyDefinition("Boolean", "DidError"));
            Properties.Add(new PropertyDefinition("String", "ErrorMessage"));
        }
    }
}
