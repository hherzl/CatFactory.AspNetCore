﻿using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.CodeFactory.Scaffolding;
using CatFactory.Collections;
using CatFactory.EntityFrameworkCore;
using CatFactory.NetCore;
using CatFactory.NetCore.ObjectOrientedProgramming;
using CatFactory.ObjectOrientedProgramming;
using CatFactory.ObjectRelationalMapping;
using CatFactory.ObjectRelationalMapping.Actions;

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
                AccessModifier = AccessModifier.Public,
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
            var tables = projectFeature.Project.Database.Tables.Where(item => dbos.Contains(item.FullName)).ToList();
            var views = projectFeature.Project.Database.Views.Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var table in tables)
            {
                if (table.Columns.Count == table.PrimaryKey?.Key.Count)
                    continue;

                var selection = aspNetCoreProject.GetSelection(table);

                if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                    definition.Methods.Add(GetGetAllMethod(projectFeature, definition, table));

                if (selection.Settings.Actions.Any(item => item is ReadByKeyAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethod(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is AddEntityAction))
                    definition.Methods.Add(GetPostMethod(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is UpdateEntityAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetPutMethod(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is RemoveEntityAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetDeleteMethod(projectFeature, table));
            }

            foreach (var view in views)
            {
                var selection = aspNetCoreProject.GetSelection(view);

                if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                    definition.Methods.Add(GetGetAllMethod(projectFeature, definition, view));
            }

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

            return new ClassConstructorDefinition(AccessModifier.Public, parameters.ToArray())
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

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var aspNetCoreSelection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var efCoreSelection = efCoreProject.GetSelection(table);

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            if (efCoreSelection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetDataLayerDataContractsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetDataContractName(table)));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
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
                    var column = (Column)table.GetColumnsFromConstraint(foreignKey).First();

                    parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), aspNetCoreProject.CodeNamingConvention.GetParameterName(column.Name), "null"));

                    foreignKeys.Add(aspNetCoreProject.CodeNamingConvention.GetParameterName(column.Name));
                }
                else
                {
                    // todo: add logic for multiple columns in key
                }
            }

            lines.Add(new CommentLine(1, " Get query from repository"));

            if (foreignKeys.Count == 0)
                lines.Add(new CodeLine(1, "var query = Repository.{0}();", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(table)));
            else
                lines.Add(new CodeLine(1, "var query = Repository.{0}({1});", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(table), string.Join(", ", foreignKeys)));

            lines.Add(new CodeLine());

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CommentLine(1, " Set paging's information"));
                lines.Add(new CodeLine(1, "response.PageSize = (int)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (int)pageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve items by page size and page number, set model for response"));
                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, CSharpClassDefinition definition, IView table)
        {
            if (projectFeature.Project.Database.HasDefaultSchema(table))
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace());
            else
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace(table.Schema));

            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var aspNetCoreSelection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var efCoreSelection = efCoreProject.GetSelection(table);

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            if (efCoreSelection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetDataLayerDataContractsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetDataContractName(table)));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Get query from repository"));

            lines.Add(new CodeLine(1, "var query = Repository.{0}();", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(table)));

            lines.Add(new CodeLine());

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CommentLine(1, " Set paging's information"));
                lines.Add(new CodeLine(1, "response.PageSize = (int)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (int)pageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve items by page size and page number, set model for response"));
                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(table),
                Parameters =
                {
                    new ParameterDefinition("int?", "pageSize", "10"),
                    new ParameterDefinition("int?", "pageNumber", "1")
                },
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
                    var column = (Column)table.GetColumnsFromConstraint(table.PrimaryKey).First();

                    parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), "id"));
                }
                else if (table.PrimaryKey.Key.Count > 1)
                {
                    parameters.Add(new ParameterDefinition("string", "id"));
                }
            }

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);

            var lines = new List<ILine>();

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            var efCoreProject = projectFeature.GetAspNetCoreProject().EntityFrameworkCoreProject;

            if (table.PrimaryKey?.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table)));
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "if (entity == null)"));
                lines.Add(new CodeLine(2, "return NotFound();"));
                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "response.Model = entity;"));
            }
            else if (table.PrimaryKey?.Key.Count > 1)
            {
                lines.Add(new CodeLine(1, "var key = id.Split('|');"));
                lines.Add(new CodeLine());

                var key = table.GetColumnsFromConstraint(table.PrimaryKey).ToList();

                for (var i = 0; i < key.Count; i++)
                {
                    var column = key[i];

                    var parameterName = efCoreProject.CodeNamingConvention.GetParameterName(column.Name);

                    if (projectFeature.Project.Database.ColumnIsInt16(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt16(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt64(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt64(key[{1}]);", parameterName, (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", parameterName, (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", efCoreProject.CodeNamingConvention.GetParameterName(item.Name))));

                lines.Add(new EmptyLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table), exp));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "if (entity == null)"));
                lines.Add(new CodeLine(2, "return NotFound();"));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "response.Model = entity;"));
            }

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The entity was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));

            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}/{1}\"", efCoreProject.GetEntityName(table), "{id}")),
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetPostMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)"));
            lines.Add(new CodeLine(1, "return BadRequest(request);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = request.ToEntity();", aspNetCoreProject.EntityFrameworkCoreProject.GetAddRepositoryMethodName(table)));
            lines.Add(new CodeLine());

            foreach (var unique in table.Uniques)
            {
                lines.Add(new CommentLine(1, " Check if entity exists"));
                lines.Add(new CodeLine(1, "if ((await Repository.{0}(entity)) != null)", aspNetCoreProject.EntityFrameworkCoreProject.GetGetByUniqueRepositoryMethodName(table, unique)));
                lines.Add(new CodeLine(2, "return BadRequest();"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Add entity to database"));
            lines.Add(new CodeLine(1, "Repository.Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "await Repository.CommitChangesAsync();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = entity;"));

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
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpPost", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerPostAsyncMethodName(table),
                Parameters =
                {
                    new ParameterDefinition(aspNetCoreProject.GetPostRequestName(table), "request", new MetadataAttribute("FromBody"))
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetPutMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)"));
            lines.Add(new CodeLine(1, "return BadRequest(request);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new Response();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            if (table.PrimaryKey?.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
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

                    var parameterName = aspNetCoreProject.CodeNamingConvention.GetParameterName(column.Name);

                    if (projectFeature.Project.Database.ColumnIsInt16(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt16(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt64(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt64(key[{1}]);", parameterName, (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", parameterName, (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", aspNetCoreProject.EntityFrameworkCoreProject.CodeNamingConvention.GetParameterName(item.Name))));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity == null)"));
            lines.Add(new CodeLine(2, "return NotFound();"));

            lines.Add(new EmptyLine());

            lines.Add(new TodoLine(1, " Check properties to update"));

            lines.Add(new EmptyLine());

            lines.Add(new CommentLine(1, " Apply changes on entity"));

            foreach (var column in projectFeature.GetUpdateColumns(table))
            {
                lines.Add(new CodeLine(1, "entity.{0} = request.{0};", aspNetCoreProject.GetPropertyName(table, column)));
            }

            lines.Add(new EmptyLine());

            lines.Add(new CommentLine(1, " Save changes for entity in database"));
            lines.Add(new CodeLine(1, "Repository.Update(entity);"));
            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await Repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The entity was updated successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
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
                var column = (Column)table.GetColumnsFromConstraint(table.PrimaryKey).First();

                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), "id"));
            }
            else if (table.PrimaryKey?.Key.Count > 1)
            {
                parameters.Add(new ParameterDefinition("string", "id"));
            }

            parameters.Add(new ParameterDefinition(aspNetCoreProject.GetPutRequestName(table), "request", new MetadataAttribute("FromBody")));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpPut", string.Format("\"{0}/{{id}}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerPutAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetDeleteMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new Response();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            if (table.PrimaryKey.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
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

                    var parameterName = aspNetCoreProject.CodeNamingConvention.GetParameterName(column.Name);

                    if (projectFeature.Project.Database.ColumnIsInt16(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt16(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt32(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt32(key[{1}]);", parameterName, (i + 1).ToString()));
                    else if (projectFeature.Project.Database.ColumnIsInt64(column))
                        lines.Add(new CodeLine(1, "var {0} = Convert.ToInt64(key[{1}]);", parameterName, (i + 1).ToString()));
                    else
                        lines.Add(new CodeLine(1, "var {0} = key[{1}];", parameterName, (i + 1).ToString()));
                }

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", aspNetCoreProject.CodeNamingConvention.GetParameterName(item.Name))));

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}({2}));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity == null)"));
            lines.Add(new CodeLine(2, "return NotFound();"));

            lines.Add(new EmptyLine());

            lines.Add(new CommentLine(1, " Remove entity from database"));
            lines.Add(new CodeLine(1, "Repository.Remove(entity);"));

            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await Repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(Logger, nameof({0}), ex);", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey.Key.Count == 1)
            {
                var column = (Column)table.GetColumnsFromConstraint(table.PrimaryKey).First();

                parameters.Add(new ParameterDefinition(projectFeature.Project.Database.ResolveDatabaseType(column), "id"));
            }
            else if (table.PrimaryKey.Key.Count > 1)
            {
                parameters.Add(new ParameterDefinition("string", "id"));
            }

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpDelete", string.Format("\"{0}/{{id}}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerDeleteAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }
    }
}
