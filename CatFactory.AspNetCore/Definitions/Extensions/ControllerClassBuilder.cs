using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;

namespace CatFactory.AspNetCore.Definitions.Extensions
{
    public static class ControllerClassBuilder
    {
        public static ControllerClassDefinition GetControllerClassDefinition(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
        {
            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();

            var definition = new ControllerClassDefinition
            {
                Namespaces =
                {
                    "System",
                    "System.Linq",
                    "System.Threading.Tasks",
                    "Microsoft.AspNetCore.Mvc",
                    "Microsoft.EntityFrameworkCore",
                    "Microsoft.Extensions.Logging",
                    aspNetCoreProject.GetDataLayerContractsNamespace(),
                    aspNetCoreProject.GetDataLayerRepositoriesNamespace(),
                    aspNetCoreProject.GetResponsesNamespace(),
                    aspNetCoreProject.GetRequestsNamespace()
                },
                Namespace = string.Format("{0}.{1}", aspNetCoreProject.Name, "Controllers"),
                Name = projectFeature.GetControllerName(),
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("Route", string.IsNullOrEmpty(aspNetCoreProject.Version) ? "\"api/[controller]\"" : string.Format("\"api/{0}/[controller]\"", aspNetCoreProject.Version)),
                    new MetadataAttribute("ApiController")
                },
                BaseClass = "ControllerBase",
                Fields =
                {
                    new FieldDefinition(AccessModifier.Protected, projectFeature.GetInterfaceRepositoryName(), "Repository")
                    {
                        IsReadOnly = true
                    }
                }
            };

            var settings = aspNetCoreProject.GlobalSelection().Settings;

            if (settings.UseLogger)
                definition.Fields.Add(new FieldDefinition(AccessModifier.Protected, "ILogger", "Logger"));

            definition.Constructors.Add(GetConstructor(projectFeature));

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = projectFeature.Project.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                if (table.Columns.Count == table.PrimaryKey?.Key.Count)
                    continue;

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

            // todo: Add views in controller

            definition.SimplifyDataTypes();

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
            if (projectFeature.Project.Database.HasDefaultSchema(table))
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
                new ParameterDefinition("int?", "pageSize", "10"),
                new ParameterDefinition("int?", "pageNumber", "1")
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
                lines.Add(new CodeLine(1, "response.PageSize = (int)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (int)pageNumber;"));
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
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", table.GetControllerGetAllAsyncMethodName()));
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
                Attributes =
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

            if (table.PrimaryKey != null)
            {
                if (table.PrimaryKey.Key.Count == 1)
                {
                    var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key.First());

                    parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), "id"));
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    parameters.Add(new ParameterDefinition("string", "id"));
                }
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

            if (table.PrimaryKey?.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "if (entity != null)"));
                lines.Add(new CodeLine(1, "{"));
                lines.Add(new CodeLine(2, "response.Model = entity.ToRequest();"));
            }
            else if (table.PrimaryKey?.Key.Count > 1)
            {
                lines.Add(new CodeLine(1, "var key = id.Split('|');"));
                lines.Add(new CodeLine());

                var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", key[i].GetParameterName(), (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", key[i].GetParameterName(), (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", item.GetParameterName())));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", table.GetGetRepositoryMethodName(), table.GetEntityName(), exp));
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "if (entity != null)"));
                lines.Add(new CodeLine(1, "{"));
                lines.Add(new CodeLine(2, "response.Model = entity.ToRequest();"));
            }

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
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", table.GetControllerGetAsyncMethodName()));
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
                Attributes =
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
            lines.Add(new CodeLine(1, "Repository.Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "await Repository.CommitChangesAsync();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = entity.ToRequest();"));

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
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", table.GetControllerPostAsyncMethodName()));
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
                Attributes =
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

            if (table.PrimaryKey?.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
                lines.Add(new CodeLine());
            }
            else if (table.PrimaryKey?.Key.Count > 1)
            {
                lines.Add(new CodeLine(1, "var key = id.Split('|');"));
                lines.Add(new CodeLine());

                var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", key[i].GetParameterName(), (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", key[i].GetParameterName(), (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", item.GetParameterName())));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", table.GetGetRepositoryMethodName(), table.GetEntityName(), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));
            lines.Add(new CodeLine(2, "response.Model = entity.ToRequest();"));

            lines.Add(new CodeLine());

            lines.Add(new TodoLine(2, " Check properties to update"));

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(2, " Apply changes on entity"));

            foreach (var column in projectFeature.GetUpdateColumns(table))
            {
                lines.Add(new CodeLine(2, "entity.{0} = requestModel.{0};", column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CommentLine(2, " Save changes for entity in database"));
            lines.Add(new CodeLine(2, "Repository.Update(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(2, "await Repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was updated successfully\");"));

                lines.Add(new CodeLine());

                lines.Add(new CodeLine(2, "response.Model = entity.ToRequest();"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", table.GetControllerPutAsyncMethodName()));
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
            else if (table.PrimaryKey?.Key.Count > 1)
            {
                parameters.Add(new ParameterDefinition("string", "id"));
            }

            parameters.Add(new ParameterDefinition(table.GetRequestModelName(), "requestModel", new MetadataAttribute("FromBody")));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPutAsyncMethodName(), parameters.ToArray())
            {
                Attributes =
                {
                    new MetadataAttribute("HttpPut", string.Format("\"{0}/{{id}}\"", table.GetEntityName())),
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

            if (table.PrimaryKey.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
                lines.Add(new CodeLine());
            }
            else if (table.PrimaryKey.Key.Count > 1)
            {
                lines.Add(new CodeLine(1, "var key = id.Split('|');"));
                lines.Add(new CodeLine());

                var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", key[i].GetParameterName(), (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", key[i].GetParameterName(), (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", item.GetParameterName())));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", table.GetGetRepositoryMethodName(), table.GetEntityName(), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));

            lines.Add(new CommentLine(2, " Remove entity from database"));
            lines.Add(new CodeLine(2, "Repository.Remove(entity);"));

            lines.Add(new CodeLine());
            lines.Add(new CodeLine(2, "await Repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "response.Model = entity.ToRequest();"));

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", table.GetControllerDeleteAsyncMethodName()));
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

            if (table.PrimaryKey.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key.First());

                parameters.Add(new ParameterDefinition(EntityFrameworkCore.DatabaseExtensions.ResolveType(projectFeature.Project.Database, column), "id"));
            }
            else if (table.PrimaryKey.Key.Count > 1)
            {
                parameters.Add(new ParameterDefinition("string", "id"));
            }

            return new MethodDefinition("Task<IActionResult>", table.GetControllerDeleteAsyncMethodName(), parameters.ToArray())
            {
                Attributes =
                {
                    new MetadataAttribute("HttpDelete", string.Format("\"{0}/{{id}}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
