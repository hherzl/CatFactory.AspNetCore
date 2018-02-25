using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.EfCore;
using CatFactory.Mapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProject : Project<AspNetCoreProjectSettings>
    {
        public AspNetCoreProject()
        {
        }

        public AspNetCoreProject(ILogger<AspNetCoreProject> logger)
            : base(logger)
        {
        }

        public override void BuildFeatures()
        {
            if (Database == null)
                return;

            Features = Database
                .DbObjects
                .Select(item => item.Schema)
                .Distinct()
                .Select(item => new ProjectFeature<AspNetCoreProjectSettings>(item, GetDbObjects(Database, item)) { Project = this })
                .ToList();
        }

        private IEnumerable<DbObject> GetDbObjects(Database database, string schema)
        {
            var result = new List<DbObject>();

            result.AddRange(Database
                .Tables
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "Table" }));

            result.AddRange(Database
                .Views
                .Where(x => x.Schema == schema)
                .Select(y => new DbObject { Schema = y.Schema, Name = y.Name, Type = "View" }));

            return result;
        }

        public ProjectNamespaces Namespaces { get; set; } = new ProjectNamespaces();
    }
}
