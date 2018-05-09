using CatFactory.CodeFactory;
using CatFactory.NetCore;
using CatFactory.Mapping;

namespace CatFactory.AspNetCore
{
    public static class NamingExtensions
    {
        public static ICodeNamingConvention namingConvention;
        public static INamingService namingService;

        static NamingExtensions()
        {
            namingConvention = new DotNetNamingConvention();
            namingService = new NamingService();
        }

        public static string GetEntityName(this IDbObject dbObject)
            => namingConvention.GetClassName(dbObject.Name);
    }
}
