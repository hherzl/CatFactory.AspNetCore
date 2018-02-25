using CatFactory.CodeFactory;
using CatFactory.DotNetCore;

namespace CatFactory.AspNetCore
{
    public static class ProjectFeaturetExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static ProjectFeaturetExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetControllerName(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => namingConvention.GetClassName(string.Format("{0}{1}", projectFeature.Name, "Controller"));

        public static AspNetCoreProject GetAspNetCoreProject(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
            => projectFeature.Project as AspNetCoreProject;
    }
}
