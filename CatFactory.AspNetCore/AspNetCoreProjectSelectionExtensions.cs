using System;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectSelectionExtensions
    {
        public static AspNetCoreProject GlobalSelection(this AspNetCoreProject project, Action<AspNetCoreProjectSettings> action = null)
        {
            var settings = new AspNetCoreProjectSettings();

            var selection = project.Selections.FirstOrDefault(item => item.IsGlobal);

            if (selection == null)
            {
                selection = new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = ProjectSelection<AspNetCoreProjectSettings>.GlobalPattern,
                    Settings = settings
                };

                project.Selections.Add(selection);
            }
            else
            {
                settings = selection.Settings;
            }

            action?.Invoke(settings);

            return project;
        }

        public static ProjectSelection<AspNetCoreProjectSettings> GlobalSelection(this AspNetCoreProject project)
            => project.Selections.FirstOrDefault(item => item.IsGlobal);

        public static AspNetCoreProject Selection(this AspNetCoreProject project, string pattern, Action<AspNetCoreProjectSettings> action = null)
        {
            var selection = project.Selections.FirstOrDefault(item => item.Pattern == pattern);

            if (selection == null)
            {
                var globalSettings = project.GlobalSelection().Settings;

                selection = new ProjectSelection<AspNetCoreProjectSettings>
                {
                    Pattern = pattern,
                    Settings = new AspNetCoreProjectSettings
                    {
                        ForceOverwrite = globalSettings.ForceOverwrite,
                        UseLogger = globalSettings.UseLogger
                    }
                };

                project.Selections.Add(selection);
            }

            action?.Invoke(selection.Settings);

            return project;
        }

        [Obsolete("Use Selection method")]
        public static AspNetCoreProject Select(this AspNetCoreProject project, string pattern, Action<AspNetCoreProjectSettings> action = null)
            => project.Selection(pattern, action);

        public static ProjectSelection<AspNetCoreProjectSettings> GetSelection(this AspNetCoreProject project, IDbObject dbObject)
        {
            // Sales.OrderHeader
            var selectionForFullName = project.Selections.FirstOrDefault(item => item.Pattern == dbObject.FullName);

            if (selectionForFullName != null)
                return selectionForFullName;

            // Sales.*
            var selectionForSchema = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("{0}.*", dbObject.Schema));

            if (selectionForSchema != null)
                return selectionForSchema;

            // *.OrderHeader
            var selectionForName = project.Selections.FirstOrDefault(item => item.Pattern == string.Format("*.{0}", dbObject.Name));

            if (selectionForName != null)
                return selectionForName;

            return project.GlobalSelection();
        }
    }
}
