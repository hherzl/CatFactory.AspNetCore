using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.Mapping;

namespace CatFactory.AspNetCore
{
    public static class DbObjectExtensions
    {
        private static ICodeNamingConvention namingConvention;

        static DbObjectExtensions()
        {
            namingConvention = new DotNetNamingConvention();
        }

        public static string GetControllerGetAllAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Get", table.GetPluralName(), "Async");

        public static string GetControllerGetAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Get", table.GetSingularName(), "Async");

        public static string GetControllerPostAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Post", table.GetSingularName(), "Async");

        public static string GetControllerPutAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Put", table.GetSingularName(), "Async");

        public static string GetControllerDeleteAsyncMethodName(this ITable table)
            => string.Format("{0}{1}{2}", "Delete", table.GetSingularName(), "Async");

        public static string GetViewModelName(this IDbObject dbObject)
            => string.Format("{0}ViewModel", dbObject.GetEntityName());

        public static string GetViewModelExtensionName(this ITable table)
            => string.Format("{0}Extensions", NamingConvention.GetPascalCase(table.Name));

        public static IEnumerable<Column> GetUpdateColumns(this ITable table, EfCoreProjectSettings settings)
        {
            var list = new List<Column>();

            foreach (var column in table.Columns)
            {
                if (table.PrimaryKey != null && table.PrimaryKey.Key.Contains(column.Name))
                {
                    continue;
                }

                if (settings.AuditEntity != null && settings.AuditEntity.Names.Contains(column.Name))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(settings.ConcurrencyToken) && string.Compare(settings.ConcurrencyToken, column.Name) == 0)
                {
                    continue;
                }

                list.Add(column);
            }

            return list;
        }
    }
}
