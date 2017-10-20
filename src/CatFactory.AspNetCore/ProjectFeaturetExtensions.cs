namespace CatFactory.AspNetCore
{
    public static class ProjectFeaturetExtensions
    {
        public static string GetControllerName(this ProjectFeature projectFeature)
            => string.Format("{0}Controller", NamingConvention.GetPascalCase(projectFeature.Name));
    }
}
