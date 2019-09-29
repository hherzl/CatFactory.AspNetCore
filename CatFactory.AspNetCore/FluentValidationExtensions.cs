using CatFactory.AspNetCore.Definitions.Extensions;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class FluentValidationExtensions
    {
        public static bool Used = false;

        public static string GetPostValidatorName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}Validator", project.GetPostRequestName(table));

        public static string GetPutValidatorName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}Validator", project.GetPutRequestName(table));

        public static string GetValidatorsNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}.{2}", project.Name, project.AspNetCoreProjectNamespaces.Requests, "Validators");

        public static AspNetCoreProject ScaffoldFluentValidation(this AspNetCoreProject project)
        {
            Used = true;

            foreach (var table in project.Database.Tables)
            {
                var postValidatorClassDefinition = project.GetPostValidatorClassDefinition(table);

                project.Scaffold(postValidatorClassDefinition, project.OutputDirectory, string.Format("{0}\\{1}", project.AspNetCoreProjectNamespaces.Requests, "Validators"));
            }

            foreach (var table in project.Database.Tables)
            {
                var postValidatorClassDefinition = project.GetPutValidatorClassDefinition(table);

                project.Scaffold(postValidatorClassDefinition, project.OutputDirectory, string.Format("{0}\\{1}", project.AspNetCoreProjectNamespaces.Requests, "Validators"));
            }

            return project;
        }
    }
}
