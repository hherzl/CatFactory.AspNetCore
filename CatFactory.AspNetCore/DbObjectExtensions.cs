using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.EntityFrameworkCore;
using CatFactory.Mapping;
using CatFactory.NetCore;

namespace CatFactory.AspNetCore
{
    public static class DbObjectExtensions
    {
        private static readonly ICodeNamingConvention namingConvention;

        static DbObjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetControllerGetAllAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Get", table.GetPluralName(), "Async");

        public static string GetControllerGetAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Get", table.GetEntityName(), "Async");

        public static string GetControllerPostAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Post", table.GetEntityName(), "Async");

        public static string GetControllerPutAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Put", table.GetEntityName(), "Async");

        public static string GetControllerDeleteAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Delete", table.GetEntityName(), "Async");

        public static string GetRequestModelName(this IDbObject dbObject)
            => string.Format("{0}RequestModel", dbObject.GetEntityName());

        public static string GetRequestModelExtensionName(this ITable table)
            => string.Format("{0}Extensions", NamingConvention.GetPascalCase(table.Name));

        public static IEnumerable<Column> GetUpdateColumns(this ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var settings = projectFeature.GetAspNetCoreProject().GetSelection(table).Settings;

            foreach (var column in table.Columns)
            {
                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                    continue;

                if (settings.AuditEntity != null && settings.AuditEntity.Names.Contains(column.Name))
                    continue;

                if (!string.IsNullOrEmpty(settings.ConcurrencyToken) && string.Compare(settings.ConcurrencyToken, column.Name) == 0)
                    continue;

                yield return column;
            }
        }
    }
}
