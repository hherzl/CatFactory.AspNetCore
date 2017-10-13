using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.EfCore;
using CatFactory.Mapping;

namespace CatFactory.AspNetCore
{
    public static class DbObjectsExtensions
    {
        public static String GetControllerGetAllAsyncMethodName(this ITable table)
            => String.Format("{0}{1}{2}", "Get", table.GetPluralName(), "Async");

        public static String GetControllerGetAsyncMethodName(this ITable table)
            => String.Format("{0}{1}{2}", "Get", table.GetSingularName(), "Async");

        public static String GetControllerPostAsyncMethodName(this ITable table)
            => String.Format("{0}{1}{2}", "Post", table.GetSingularName(), "Async");

        public static String GetControllerPutAsyncMethodName(this ITable table)
            => String.Format("{0}{1}{2}", "Put", table.GetSingularName(), "Async");

        public static String GetControllerDeleteAsyncMethodName(this ITable table)
            => String.Format("{0}{1}{2}", "Delete", table.GetSingularName(), "Async");

        public static String GetViewModelExtensionName(this ITable table)
            => String.Format("{0}Extensions", NamingConvention.GetPascalCase(table.Name));

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
