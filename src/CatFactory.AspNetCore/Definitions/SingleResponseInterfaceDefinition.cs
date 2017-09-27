using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class SingleResponseInterfaceDefinition : CSharpInterfaceDefinition
    {
        public SingleResponseInterfaceDefinition()
            : base()
        {
            Init();
        }

        public void Init()
        {
            Name = "ISingleResponse<TModel>";

            Implements.Add("IResponse");

            Properties.Add(new PropertyDefinition("TModel", "Model"));
        }
    }
}
