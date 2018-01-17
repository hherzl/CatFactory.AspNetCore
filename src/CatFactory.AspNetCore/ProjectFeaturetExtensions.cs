using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.EfCore;

namespace CatFactory.AspNetCore
{
    public static class ProjectFeaturetExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static ProjectFeaturetExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetControllerName(this ProjectFeature<EntityFrameworkCoreProjectSettings> projectFeature)
            => namingConvention.GetClassName(string.Format("{0}{1}", projectFeature.Name, "Controller"));
    }
}
