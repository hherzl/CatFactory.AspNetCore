using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProjectSettings : ProjectSettings
    {
        public bool ForceOverwrite { get; set; }

        public bool UseLogger { get; set; } = true;

        public string ConcurrencyToken { get; set; }

        public AuditEntity AuditEntity { get; set; }

        public bool EntitiesWithDataContracts { get; set; }
    }
}
