using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.NetCore.CodeFactory;

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
