using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.Mapping;
using CatFactory.NetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ControllerClassDefinition
    {
        public static CSharpClassDefinition GetControllerClassDefinition(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespaces.Add("System.Linq");
            definition.Namespaces.Add("System.Threading.Tasks");
            definition.Namespaces.Add("Microsoft.AspNetCore.Mvc");
            definition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            definition.Namespaces.Add("Microsoft.Extensions.Logging");

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();

            definition.Namespaces.Add(aspNetCoreProject.GetDataLayerContractsNamespace());
            definition.Namespaces.Add(aspNetCoreProject.GetDataLayerRepositoriesNamespace());

            var settings = aspNetCoreProject.GlobalSelection().Settings;

            definition.Namespaces.Add(aspNetCoreProject.GetResponsesNamespace());
            definition.Namespaces.Add(aspNetCoreProject.GetRequestModelsNamespace());

            definition.Namespace = string.Format("{0}.{1}", aspNetCoreProject.Name, "Controllers");

            definition.Attributes = new List<MetadataAttribute>
            {
                new MetadataAttribute("Route", string.IsNullOrEmpty(aspNetCoreProject.Version) ? "\"api/[controller]\"" : string.Format("\"api/{0}/[controller]\"", aspNetCoreProject.Version))
            };

            definition.Name = projectFeature.GetControllerName();

            definition.BaseClass = "Controller";

            definition.Fields.Add(new FieldDefinition(AccessModifier.Protected, projectFeature.GetInterfaceRepositoryName(), "Repository")
            {
                IsReadOnly = true
            });

            if (settings.UseLogger)
                definition.Fields.Add(new FieldDefinition(AccessModifier.Protected, "ILogger", "Logger"));

            definition.Constructors.Add(GetConstructor(projectFeature));

            definition.Methods.Add(new MethodDefinition(AccessModifier.Protected, "void", "Dispose", new ParameterDefinition("Boolean", "disposing"))
            {
                IsOverride = true,
                Lines = new List<ILine>
                {
                    new CodeLine("Repository?.Dispose();"),
                    new CodeLine(),
                    new CodeLine("base.Dispose(disposing);")
                }
            });

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = projectFeature.Project.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                definition.Methods.Add(GetGetAllMethod(projectFeature, definition, table));

                if (table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethod(projectFeature, table));

                definition.Methods.Add(GetPostMethod(projectFeature, table));

                if (table.PrimaryKey != null)
                {
                    definition.Methods.Add(GetPutMethod(projectFeature, table));

                    definition.Methods.Add(GetDeleteMethod(projectFeature, table));
                }
            }

            return definition;
        }

        private static ClassConstructorDefinition GetConstructor(ProjectFeature<AspNetCoreProjectSettings> projectFeature)
        {
            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition(projectFeature.GetInterfaceRepositoryName(), "repository")
            };

            var lines = new List<ILine>
            {
                new CodeLine("Repository = repository;")
            };

            var settings = projectFeature.GetAspNetCoreProject().GlobalSelection().Settings;

            if (settings.UseLogger)
            {
                parameters.Add(new ParameterDefinition(string.Format("ILogger<{0}>", projectFeature.GetControllerName()), "logger"));

                lines.Add(new CodeLine("Logger = logger;"));
            }

            return new ClassConstructorDefinition(parameters.ToArray())
            {
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, CSharpClassDefinition definition, ITable table)
        {
            if (table.HasDefaultSchema())
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace());
            else
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace(table.Schema));

            var lines = new List<ILine>();

            var selection = projectFeature.GetAspNetCoreProject().GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAllAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            if (selection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetDataLayerDataContractsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", table.GetDataContractName()));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", table.GetEntityName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition("Int32?", "pageSize", "10"),
                new ParameterDefinition("Int32?", "pageNumber", "1")
            };

            var foreignKeys = new List<string>();

            foreach (var foreignKey in table.ForeignKeys)
            {
                var parentTable = projectFeature.Project.Database.FindTable(foreignKey.References);

                if (parentTable == null)
                    continue;

                if (parentTable.PrimaryKey?.Key.Count == 1)
                {
                    // todo: add logic for multiple columns in key
                    var column = parentTable.GetColumnsFromConstraint(parentTable.PrimaryKey).First();

                    parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), column.GetParameterName(), "null"));

                    foreignKeys.Add(column.GetParameterName());
                }
            }

            lines.Add(new CommentLine(1, " Get query from repository"));

            if (foreignKeys.Count == 0)
                lines.Add(new CodeLine(1, "var query = Repository.{0}();", table.GetGetAllRepositoryMethodName()));
            else
                lines.Add(new CodeLine(1, "var query = Repository.{0}({1});", table.GetGetAllRepositoryMethodName(), string.Join(", ", foreignKeys)));

            lines.Add(new CodeLine());

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CommentLine(1, " Set paging's information"));
                lines.Add(new CodeLine(1, "response.PageSize = (Int32)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (Int32)pageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve items by page size and page number, set model for response"));
                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(ex, Logger);"));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerGetAllAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey?.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key.First());

                parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), "id"));
            }

            var selection = projectFeature.GetAspNetCoreProject().GetSelection(table);

            var lines = new List<ILine>();

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetRequestModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Retrieve entity by id"));
            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));
            lines.Add(new CodeLine(2, "response.Model = entity.ToRequestModel();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was retrieved successfully\");"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(ex, Logger);"));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerGetAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}/{1}\"", table.GetEntityName(), "{id}")),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetPostMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetAspNetCoreProject().GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPostAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)", table.GetRequestModelName()));
            lines.Add(new CodeLine(1, "return BadRequest(requestModel);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetRequestModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = requestModel.ToEntity();", table.GetAddRepositoryMethodName()));
            lines.Add(new CodeLine());

            foreach (var unique in table.Uniques)
            {
                lines.Add(new CommentLine(1, " Check if entity exists"));
                lines.Add(new CodeLine(1, "if ((await Repository.{0}(entity)) != null)", table.GetGetByUniqueRepositoryMethodName(unique)));
                lines.Add(new CodeLine(1, "{"));
                lines.Add(new CodeLine(2, "return BadRequest();"));
                lines.Add(new CodeLine(1, "}"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Add entity to database"));
            lines.Add(new CodeLine(1, "await Repository.{0}(entity);", table.GetAddRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = entity.ToRequestModel();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The entity was created successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(ex, Logger);"));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPostAsyncMethodName(), new ParameterDefinition(table.GetRequestModelName(), "requestModel", new MetadataAttribute("FromBody")))
            {
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("HttpPost", string.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetPutMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetAspNetCoreProject().GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPutAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)", table.GetRequestModelName()));
            lines.Add(new CodeLine(1, "return BadRequest(requestModel);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetRequestModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Retrieve entity by id"));
            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));

            lines.Add(new TodoLine(2, " Check properties to update"));
            lines.Add(new CommentLine(2, " Apply changes on entity"));

            foreach (var column in projectFeature.GetUpdateColumns(table))
            {
                lines.Add(new CodeLine(2, "entity.{0} = requestModel.{0};", column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(2, " Save changes for entity in database"));
            lines.Add(new CodeLine(2, "await Repository.{0}(entity);", table.GetUpdateRepositoryMethodName()));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was updated successfully\");"));

                lines.Add(new CodeLine());

                lines.Add(new CodeLine(2, "response.Model = entity.ToRequestModel();"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(ex, Logger);"));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey?.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key.First());

                parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), "id"));
            }

            parameters.Add(new ParameterDefinition(table.GetRequestModelName(), "requestModel", new MetadataAttribute("FromBody")));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPutAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("HttpPut", string.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetDeleteMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var selection = projectFeature.GetAspNetCoreProject().GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerDeleteAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetRequestModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Retrieve entity by id"));
            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));

            lines.Add(new CommentLine(2, " Remove entity from database"));
            lines.Add(new CodeLine(2, "await Repository.{0}(entity);", table.GetRemoveRepositoryMethodName()));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "response.Model = entity.ToRequestModel();"));

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(ex, Logger);"));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey?.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key.First());

                parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), "id"));
            }

            return new MethodDefinition("Task<IActionResult>", table.GetControllerDeleteAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("HttpDelete", string.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
