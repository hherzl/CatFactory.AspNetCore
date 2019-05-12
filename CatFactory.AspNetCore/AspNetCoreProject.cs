using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProject : CSharpProject<AspNetCoreProjectSettings>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private AspNetCoreProjectNamespaces m_aspNetCoreProjectNamespaces;

        public AspNetCoreProject()
            : base()
        {
        }

        public AspNetCoreProject(ILogger<AspNetCoreProject> logger)
            : base(logger)
        {
        }

        public string Version { get; set; }

        public EntityFrameworkCoreProject EntityFrameworkCoreProject { get; set; }

        public AspNetCoreProjectNamespaces AspNetCoreProjectNamespaces
        {
            get => m_aspNetCoreProjectNamespaces ?? (m_aspNetCoreProjectNamespaces = new AspNetCoreProjectNamespaces());
            set => m_aspNetCoreProjectNamespaces = value;
        }

        public override void BuildFeatures()
        {
            if (Database == null)
                return;

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature<AspNetCoreProjectSettings>(item, GetDbObjectsBySchema(item), this))
                .ToList();
        }

        public override void Scaffold(IObjectDefinition objectDefinition, string outputDirectory, string subdirectory = "")
        {
            var codeBuilder = default(ICodeBuilder);

            var selection = objectDefinition.DbObject == null ? this.GlobalSelection() : this.GetSelection(objectDefinition.DbObject);

            if (objectDefinition is CSharpClassDefinition)
            {
                codeBuilder = new CSharpClassBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }
            else if (objectDefinition is CSharpInterfaceDefinition)
            {
                codeBuilder = new CSharpInterfaceBuilder
                {
                    OutputDirectory = outputDirectory,
                    ForceOverwrite = selection.Settings.ForceOverwrite,
                    ObjectDefinition = objectDefinition
                };
            }

            OnScaffoldingDefinition(new ScaffoldingDefinitionEventArgs(Logger, codeBuilder));

            codeBuilder.CreateFile(subdirectory: subdirectory);

            OnScaffoldedDefinition(new ScaffoldedDefinitionEventArgs(Logger, codeBuilder));
        }
    }
}
