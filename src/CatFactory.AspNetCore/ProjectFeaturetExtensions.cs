using System;
using CatFactory.EfCore;

namespace CatFactory.AspNetCore
{
    public static class ProjectFeaturetExtensions
    {
        public static String GetControllerName(this ProjectFeature projectFeature)
            => String.Format("{0}Controller", NamingConvention.GetPascalCase(projectFeature.Name));
    }
}
