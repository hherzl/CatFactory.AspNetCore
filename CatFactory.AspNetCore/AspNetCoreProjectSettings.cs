using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProjectSettings : IProjectSettings
    {
        public ValidationResult Validate()
        {
            // todo: Add this implementation

            throw new System.NotImplementedException();
        }

        public bool ForceOverwrite { get; set; }

        public bool UseLogger { get; set; } = true;
    }
}
