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
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private List<IEntityAction> m_actions;

        public AspNetCoreProjectSettings()
        {
            Actions.Add(new ReadAllAction());
            Actions.Add(new ReadByKeyAction());
            Actions.Add(new ReadByUniqueAction());
            Actions.Add(new AddEntityAction());
            Actions.Add(new UpdateEntityAction());
            Actions.Add(new RemoveEntityAction());
        }

        public bool ForceOverwrite { get; set; }

        public bool UseLogger { get; set; } = true;

        public List<IEntityAction> Actions
        {
            get => m_actions ?? (m_actions = new List<IEntityAction>());
            set => m_actions = value;
        }

        // todo: Add this implementation

        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
