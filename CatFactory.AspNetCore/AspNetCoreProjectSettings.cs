using System;
using System.Collections.Generic;
using System.Diagnostics;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Diagnostics;
using CatFactory.ObjectRelationalMapping.Actions;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProjectSettings : IProjectSettings
    {
        public ValidationResult Validate()
        {
            // todo: Add this implementation

            throw new NotImplementedException();
        }

        public bool ForceOverwrite { get; set; }

        public bool UseLogger { get; set; } = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<IEntityAction> m_actions;

        public List<IEntityAction> Actions
        {
            get
            {
                return m_actions ?? (m_actions = new List<IEntityAction>());
            }
            set
            {
                m_actions = value;
            }
        }
    }
}
