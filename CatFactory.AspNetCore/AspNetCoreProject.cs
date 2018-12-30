using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore.CodeFactory;
using CatFactory.ObjectRelationalMapping;
using Microsoft.Extensions.Logging;

namespace CatFactory.AspNetCore
{
    public class AspNetCoreProject : Project<AspNetCoreProjectSettings>
    {
        public AspNetCoreProject()
            : base()
        {
            CodeNamingConvention = new DotNetNamingConvention();
        }

        public AspNetCoreProject(ILogger<AspNetCoreProject> logger)
            : base(logger)
        {
            CodeNamingConvention = new DotNetNamingConvention();
        }

        public string Version { get; set; }

        public EntityFrameworkCoreProject EntityFrameworkCoreProject { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private AspNetCoreProjectNamespaces m_aspNetCoreProjectNamespaces;

        public AspNetCoreProjectNamespaces AspNetCoreProjectNamespaces
        {
            get
            {
                return m_aspNetCoreProjectNamespaces ?? (m_aspNetCoreProjectNamespaces = new AspNetCoreProjectNamespaces());
            }
            set
            {
                m_aspNetCoreProjectNamespaces = value;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EntityFrameworkCoreProjectNamespaces m_projectNamespaces;

        public EntityFrameworkCoreProjectNamespaces EntityFrameworkCoreProjectNamespaces
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
                .Where(item => item.Schema == schema)
                .Select(item => new DbObject { Schema = item.Schema, Name = item.Name, Type = "Table" }));

            result.AddRange(Database
                .Views
                .Where(item => item.Schema == schema)
                .Select(item => new DbObject { Schema = item.Schema, Name = item.Name, Type = "View" }));

            return result;
        }
    }
}
