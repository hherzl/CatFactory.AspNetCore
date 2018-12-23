using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProject : Project<AspNetCoreProjectSettings>
    {
        public AspNetCoreProject()
            : base()
        {
        }

        public AspNetCoreProject(ILogger<AspNetCoreProject> logger)
            : base(logger)
        {
        }

        public string Version { get; set; }

        public string ReferencedProjectName { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EntityFrameworkCoreProjectNamespaces m_projectNamespaces;

        public EntityFrameworkCoreProjectNamespaces ProjectNamespaces
        {
            get
            {
                return m_projectNamespaces ?? (m_projectNamespaces = new EntityFrameworkCoreProjectNamespaces());
            }
            set
            {
                m_projectNamespaces = value;
            }
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
    }
}
