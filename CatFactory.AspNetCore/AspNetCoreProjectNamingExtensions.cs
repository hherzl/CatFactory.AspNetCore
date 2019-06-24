using CatFactory.EntityFrameworkCore;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectNamingExtensions
    {
        public static string GetControllerGetAllAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Get", project.EntityFrameworkCoreProject.GetPluralName(table), "Async");

        public static string GetControllerGetAllAsyncMethodName(this AspNetCoreProject project, IView table)
            => string.Format("{0}{1}{2}", "Get", project.EntityFrameworkCoreProject.GetPluralName(table), "Async");

        public static string GetResponsesNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, project.AspNetCoreProjectNamespaces.Responses);

        public static string GetRequestsNamespace(this AspNetCoreProject project)
            => string.Format("{0}.{1}", project.Name, project.AspNetCoreProjectNamespaces.Requests);

        public static string GetControllerGetAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Get", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerPostAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Post", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerPutAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Put", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetControllerDeleteAsyncMethodName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}{1}{2}", "Delete", project.EntityFrameworkCoreProject.GetEntityName(table), "Async");

        public static string GetPostRequestName(this AspNetCoreProject project, ITable table)
            => string.Format("Post{0}Request", project.EntityFrameworkCoreProject.GetEntityName(table));

        public static string GetPutRequestName(this AspNetCoreProject project, ITable table)
            => string.Format("Put{0}Request", project.EntityFrameworkCoreProject.GetEntityName(table));

        public static string GetRequestExtensionName(this AspNetCoreProject project, ITable table)
            => string.Format("{0}Extensions", project.GetPostRequestName(table));

        public static string GetEntityLayerNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.ProjectNamespaces.EntityLayer));

        public static string GetEntityLayerNamespace(this AspNetCoreProject project, string ns)
            => string.IsNullOrEmpty(ns) ? GetEntityLayerNamespace(project) : string.Join(".", project.EntityFrameworkCoreProject.Name, project.EntityFrameworkCoreProject.ProjectNamespaces.EntityLayer, ns);

        public static string GetDataLayerContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProject.ProjectNamespaces.DataLayer, project.EntityFrameworkCoreProject.ProjectNamespaces.Contracts);

        public static string GetDataLayerDataContractsNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProject.ProjectNamespaces.DataLayer, project.EntityFrameworkCoreProject.ProjectNamespaces.DataContracts);

        public static string GetDataLayerRepositoriesNamespace(this AspNetCoreProject project)
            => string.Join(".", project.CodeNamingConvention.GetNamespace(project.EntityFrameworkCoreProject.Name), project.EntityFrameworkCoreProject.ProjectNamespaces.DataLayer, project.EntityFrameworkCoreProject.ProjectNamespaces.Repositories);
    }
}
