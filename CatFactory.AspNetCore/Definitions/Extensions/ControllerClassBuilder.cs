using System.Collections.Generic;
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
        public static ControllerClassDefinition GetControllerClassDefinitionForDomainDrivenDesign(this ProjectFeature<AspNetCoreProjectSettings> projectFeature)
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
                    aspNetCoreProject.EntityFrameworkCoreProject.Name,
                    aspNetCoreProject.GetResponsesNamespace(),
                    aspNetCoreProject.GetRequestsNamespace(),
                    aspNetCoreProject.EntityFrameworkCoreProject.GetDomainModelsNamespace(),
                },
                Namespace = string.Format("{0}.{1}", aspNetCoreProject.Name, "Controllers"),
                AccessModifier = AccessModifier.Public,
                Name = projectFeature.GetControllerName(),
                Attributes = new List<MetadataAttribute>
                {
                    new MetadataAttribute("Route", string.IsNullOrEmpty(aspNetCoreProject.ApiVersion) ? "\"api/[controller]\"" : string.Format("\"api/{0}/[controller]\"", aspNetCoreProject.Version)),
                    new MetadataAttribute("ApiController")
                },
                BaseClass = "ControllerBase"
            };

            if (aspNetCoreProject.EntityFrameworkCoreProject.GlobalSelection().Settings.EntitiesWithDataContracts)
                definition.Namespaces.Add(aspNetCoreProject.EntityFrameworkCoreProject.GetDomainQueryModelsNamespace());

            var settings = aspNetCoreProject.GlobalSelection().Settings;

            var dbContextClassName = aspNetCoreProject.EntityFrameworkCoreProject.GetDbContextName(projectFeature.Project.Database);

            definition.Fields.Add(new FieldDefinition(AccessModifier.Private, dbContextClassName, "_dbContext")
            {
                IsReadOnly = true
            });

            if (settings.UseLogger)
            {
                definition.Fields.Add(new FieldDefinition(AccessModifier.Private, "ILogger", "_logger")
                {
                    IsReadOnly = true
                });
            }

            definition.Constructors.Add(GetConstructorForDomainDrivenDesign(projectFeature));

            var dbos = projectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = projectFeature.Project.Database.Tables.Where(item => dbos.Contains(item.FullName)).ToList();
            var views = projectFeature.Project.Database.Views.Where(item => dbos.Contains(item.FullName)).ToList();

            foreach (var table in tables)
            {
                if (table.Columns.Count == table.PrimaryKey?.Key.Count)
                    continue;

                var selection = aspNetCoreProject.GetSelection(table);

                if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                    definition.Methods.Add(GetGetAllMethodForDomainDrivenDesign(projectFeature, definition, table));

                if (selection.Settings.Actions.Any(item => item is ReadByKeyAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetGetMethodForDomainDrivenDesign(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is AddEntityAction))
                    definition.Methods.Add(GetPostMethodForDomainDrivenDesign(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is UpdateEntityAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetPutMethodForDomainDrivenDesign(projectFeature, table));

                if (selection.Settings.Actions.Any(item => item is RemoveEntityAction) && table.PrimaryKey != null)
                    definition.Methods.Add(GetDeleteMethodForDomainDrivenDesign(projectFeature, table));
            }

            foreach (var view in views)
            {
                var selection = aspNetCoreProject.GetSelection(view);

                // todo: Fix scaffolding extension method for views

                //if (selection.Settings.Actions.Any(item => item is ReadAllAction))
                //    definition.Methods.Add(GetGetAllMethodForDomainDrivenDesign(projectFeature, definition, view));
            }

            definition.SimplifyDataTypes();

            return definition;
        }

        private static ClassConstructorDefinition GetConstructorForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature)
        {
            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var dbContextClassName = aspNetCoreProject.EntityFrameworkCoreProject.GetDbContextName(projectFeature.Project.Database);

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition(dbContextClassName, "dbContext")
            };

            var lines = new List<ILine>
            {
                new CodeLine("_dbContext = dbContext;")
            };

            var settings = aspNetCoreProject.GlobalSelection().Settings;

            if (settings.UseLogger)
            {
                parameters.Add(new ParameterDefinition(string.Format("ILogger<{0}>", projectFeature.GetControllerName()), "logger"));

                lines.Add(new CodeLine("_logger = logger;"));
            }

            return new ClassConstructorDefinition(AccessModifier.Public, parameters.ToArray())
            {
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, CSharpClassDefinition definition, ITable table)
        {
            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var aspNetCoreSelection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var efCoreSelection = efCoreProject.GetSelection(table);

            if (projectFeature.Project.Database.HasDefaultSchema(table))
                definition.Namespaces.AddUnique(efCoreProject.GetDomainModelsNamespace());
            else
                definition.Namespaces.AddUnique(efCoreProject.GetDomainModelsNamespace(table.Schema));

            var lines = new List<ILine>();

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            if (efCoreSelection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(efCoreProject.GetDomainQueryModelsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", efCoreProject.GetQueryModelName(table)));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", efCoreProject.GetEntityName(table)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            var parameters = new List<ParameterDefinition>
            {
                new ParameterDefinition(aspNetCoreProject.GetGetRequestName(table), "request")
                {
                    Attributes =
                    {
                        new MetadataAttribute("FromQuery")
                    }
                }
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

                    foreignKeys.Add(string.Format("request.{0}", aspNetCoreProject.CodeNamingConvention.GetPropertyName(column.Name)));
                }
                else
                {
                    // todo: add logic for multiple columns in key
                }
            }

            lines.Add(new CommentLine(1, " Get query from repository"));

            if (foreignKeys.Count == 0)
                lines.Add(new CodeLine(1, "var query = _dbContext.{0}();", efCoreProject.GetGetAllExtensionMethodName(table)));
            else
                lines.Add(new CodeLine(1, "var query = _dbContext.{0}({1});", efCoreProject.GetGetAllExtensionMethodName(table), string.Join(", ", foreignKeys)));

            lines.Add(new CodeLine());

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CommentLine(1, " Set paging's information"));
                lines.Add(new CodeLine(1, "response.PageSize = (int)request.PageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (int)request.PageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve items by page size and page number, set model for response"));
                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", efCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "200", string.Format("Type = typeof(IPagedResponse<{0}>)", efCoreSelection.Settings.EntitiesWithDataContracts ? efCoreProject.GetQueryModelName(table) : efCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, CSharpClassDefinition definition, IView view)
        {
            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var aspNetCoreSelection = aspNetCoreProject.GetSelection(view);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var efCoreSelection = efCoreProject.GetSelection(view);

            if (projectFeature.Project.Database.HasDefaultSchema(view))
                definition.Namespaces.AddUnique(efCoreProject.GetDomainModelsNamespace());
            else
                definition.Namespaces.AddUnique(efCoreProject.GetDomainModelsNamespace(view.Schema));

            var lines = new List<ILine>();

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(view)));
                lines.Add(new CodeLine());
            }

            if (efCoreSelection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(efCoreProject.GetDomainQueryModelsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", efCoreProject.GetQueryModelName(view)));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", efCoreProject.GetEntityName(view)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Get query from repository"));

            lines.Add(new CodeLine(1, "var query = _dbContext.{0}();", efCoreProject.GetGetAllExtensionMethodName(view)));

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

                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(view)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", efCoreProject.GetEntityName(view))),
                    new MetadataAttribute("ProducesResponseType", "200", string.Format("Type = typeof(IPagedResponse<{0}>)", efCoreSelection.Settings.EntitiesWithDataContracts ? efCoreProject.GetDataContractName(view) : efCoreProject.GetEntityName(view))),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(view),
                Parameters =
                {
                    new ParameterDefinition("int?", "pageSize", "10"),
                    new ParameterDefinition("int?", "pageNumber", "1")
                },
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
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
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var selection = aspNetCoreProject.GetSelection(table);

            var lines = new List<ILine>();

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", efCoreProject.GetEntityName(table)));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            if (table.PrimaryKey?.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}(id));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}({2}));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table), exp));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "if (entity == null)"));
                lines.Add(new CodeLine(2, "return NotFound();"));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "response.Model = entity;"));
            }

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));

            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}/{1}\"", efCoreProject.GetEntityName(table), "{id}")),
                    new MetadataAttribute("ProducesResponseType", "200", $"Type = typeof(ISingleResponse<{efCoreProject.GetEntityName(table)}>)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetPostMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)"));
            lines.Add(new CodeLine(1, "return BadRequest(request);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new PostResponse();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = request.ToEntity();", efCoreProject.GetAddRepositoryMethodName(table)));
            lines.Add(new CodeLine());

            foreach (var unique in table.Uniques)
            {
                lines.Add(new CommentLine(1, " Check if entity exists"));
                lines.Add(new CodeLine(1, "if ((await _dbContext.{0}(entity)) != null)", efCoreProject.GetGetByUniqueRepositoryMethodName(table, unique)));
                lines.Add(new CodeLine(2, "return BadRequest();"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Add entity to database"));
            lines.Add(new CodeLine(1, "_dbContext.Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "await _dbContext.SaveChangesAsync();"));
            lines.Add(new CodeLine());

            if (table.Identity != null)
            {
                lines.Add(new CodeLine(1, "response.{0} = entity.{1};", "Id", projectFeature.Project.CodeNamingConvention.GetPropertyName(table.Identity.Name)));
            }

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was created successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpPost", string.Format("\"{0}\"", efCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "200", "Type = typeof(IPostResponse)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "500")
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

        private static MethodDefinition GetPutMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}(id));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table)));
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

                var exp = string.Join(", ", key.Select(item => string.Format("{0}", efCoreProject.CodeNamingConvention.GetParameterName(item.Name))));

                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve entity"));
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}({2}));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table), exp));
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
            lines.Add(new CodeLine(1, "_dbContext.Update(entity);"));
            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await _dbContext.SaveChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was updated successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

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
                    new MetadataAttribute("HttpPut", string.Format("\"{0}/{{id}}\"", efCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "200", "Type = typeof(IResponse)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerPutAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetDeleteMethodForDomainDrivenDesign(ProjectFeature<AspNetCoreProjectSettings> projectFeature, ITable table)
        {
            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var selection = aspNetCoreProject.GetSelection(table);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new Response();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            if (table.PrimaryKey.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}(id));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _dbContext.{0}(new {1}({2}));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity == null)"));
            lines.Add(new CodeLine(2, "return NotFound();"));

            lines.Add(new EmptyLine());

            lines.Add(new CommentLine(1, " Remove entity from database"));
            lines.Add(new CodeLine(1, "_dbContext.Remove(entity);"));

            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await _dbContext.SaveChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

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
                    new MetadataAttribute("HttpDelete", string.Format("\"{0}/{{id}}\"", efCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "typeof(IResponse)", "200"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                AccessModifier = AccessModifier.Public,
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerDeleteAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

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
                    new MetadataAttribute("Route", string.IsNullOrEmpty(aspNetCoreProject.ApiVersion) ? "\"api/[controller]\"" : string.Format("\"api/{0}/[controller]\"", aspNetCoreProject.Version)),
                    new MetadataAttribute("ApiController")
                },
                BaseClass = "ControllerBase"
            };

            var settings = aspNetCoreProject.GlobalSelection().Settings;

            definition.Fields.Add(new FieldDefinition(AccessModifier.Private, projectFeature.GetInterfaceRepositoryName(), "_repository")
            {
                IsReadOnly = true
            });

            if (settings.UseLogger)
                definition.Fields.Add(new FieldDefinition(AccessModifier.Private, "ILogger", "_logger")
                {
                    IsReadOnly = true
                });

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
                new CodeLine("_repository = repository;")
            };

            var settings = projectFeature.GetAspNetCoreProject().GlobalSelection().Settings;

            if (settings.UseLogger)
            {
                parameters.Add(new ParameterDefinition(string.Format("ILogger<{0}>", projectFeature.GetControllerName()), "logger"));

                lines.Add(new CodeLine("_logger = logger;"));
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
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
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
                new ParameterDefinition(aspNetCoreProject.GetGetRequestName(table), "request")
                {
                    Attributes =
                    {
                        new MetadataAttribute("FromQuery")
                    }
                }
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

                    foreignKeys.Add(string.Format("request.{0}", aspNetCoreProject.CodeNamingConvention.GetPropertyName(column.Name)));
                }
                else
                {
                    // todo: add logic for multiple columns in key
                }
            }

            lines.Add(new CommentLine(1, " Get query from repository"));

            if (foreignKeys.Count == 0)
                lines.Add(new CodeLine(1, "var query = _repository.{0}();", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(table)));
            else
                lines.Add(new CodeLine(1, "var query = _repository.{0}({1});", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(table), string.Join(", ", foreignKeys)));

            lines.Add(new CodeLine());

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CommentLine(1, " Set paging's information"));
                lines.Add(new CodeLine(1, "response.PageSize = (int)request.PageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (int)request.PageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine(1, " Retrieve items by page size and page number, set model for response"));
                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", string.Format("typeof(IPagedResponse<{0}>)", efCoreSelection.Settings.EntitiesWithDataContracts ? aspNetCoreProject.EntityFrameworkCoreProject.GetDataContractName(table) : aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)), "200"),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(table),
                Parameters = parameters,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature<AspNetCoreProjectSettings> projectFeature, CSharpClassDefinition definition, IView view)
        {
            if (projectFeature.Project.Database.HasDefaultSchema(view))
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace());
            else
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetEntityLayerNamespace(view.Schema));

            var lines = new List<ILine>();

            var aspNetCoreProject = projectFeature.GetAspNetCoreProject();
            var aspNetCoreSelection = aspNetCoreProject.GetSelection(view);
            var efCoreProject = aspNetCoreProject.EntityFrameworkCoreProject;
            var efCoreSelection = efCoreProject.GetSelection(view);

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAllAsyncMethodName(view)));
                lines.Add(new CodeLine());
            }

            if (efCoreSelection.Settings.EntitiesWithDataContracts)
            {
                definition.Namespaces.AddUnique(projectFeature.GetAspNetCoreProject().GetDataLayerDataContractsNamespace());

                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetDataContractName(view)));
            }
            else
            {
                lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(view)));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CommentLine(1, " Get query from repository"));

            lines.Add(new CodeLine(1, "var query = _repository.{0}();", aspNetCoreProject.EntityFrameworkCoreProject.GetGetAllRepositoryMethodName(view)));

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

                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}.\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (aspNetCoreSelection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAllAsyncMethodName(view)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                AccessModifier = AccessModifier.Public,
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(view))),
                    new MetadataAttribute("ProducesResponseType", string.Format("typeof(IPagedResponse<{0}>)", efCoreSelection.Settings.EntitiesWithDataContracts ? aspNetCoreProject.EntityFrameworkCoreProject.GetDataContractName(view) : aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(view)), "200"),
                    new MetadataAttribute("ProducesResponseType", "500")
                },
                IsAsync = true,
                Type = "Task<IActionResult>",
                Name = aspNetCoreProject.GetControllerGetAllAsyncMethodName(view),
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
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}(id));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}({2}));", efCoreProject.GetGetRepositoryMethodName(table), efCoreProject.GetEntityName(table), exp));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "if (entity == null)"));
                lines.Add(new CodeLine(2, "return NotFound();"));

                lines.Add(new EmptyLine());

                lines.Add(new CodeLine(1, "response.Model = entity;"));
            }

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerGetAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));

            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpGet", string.Format("\"{0}/{1}\"", efCoreProject.GetEntityName(table), "{id}")),
                    new MetadataAttribute("ProducesResponseType", $"typeof(ISingleResponse<{aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)}>)", "200"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
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
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(" Validate request model"));
            lines.Add(new CodeLine("if (!ModelState.IsValid)"));
            lines.Add(new CodeLine(1, "return BadRequest(request);"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("var response = new PostResponse();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = request.ToEntity();", aspNetCoreProject.EntityFrameworkCoreProject.GetAddRepositoryMethodName(table)));
            lines.Add(new CodeLine());

            foreach (var unique in table.Uniques)
            {
                lines.Add(new CommentLine(1, " Check if entity exists"));
                lines.Add(new CodeLine(1, "if ((await _repository.{0}(entity)) != null)", aspNetCoreProject.EntityFrameworkCoreProject.GetGetByUniqueRepositoryMethodName(table, unique)));
                lines.Add(new CodeLine(2, "return BadRequest();"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CommentLine(1, " Add entity to database"));
            lines.Add(new CodeLine(1, "_repository.Add(entity);"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "await _repository.CommitChangesAsync();"));
            lines.Add(new CodeLine());

            if (table.Identity != null)
            {
                lines.Add(new CodeLine(1, "response.{0} = entity.{1};", "Id", projectFeature.Project.CodeNamingConvention.GetPropertyName(table.Identity.Name)));
            }

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was created successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPostAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

            return new MethodDefinition
            {
                Attributes =
                {
                    new MetadataAttribute("HttpPost", string.Format("\"{0}\"", aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table))),
                    new MetadataAttribute("ProducesResponseType", "200", "Type = typeof(IPostResponse)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "500")
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
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}(id));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}({2}));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table), exp));
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
            lines.Add(new CodeLine(1, "_repository.Update(entity);"));
            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await _repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was updated successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerPutAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

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
                    new MetadataAttribute("ProducesResponseType", "200", "Type = typeof(IResponse)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
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
                lines.Add(new CodeLine("_logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new Response();"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            if (table.PrimaryKey.Key.Count == 1)
            {
                lines.Add(new CommentLine(1, " Retrieve entity by id"));
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}(id));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table)));
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
                lines.Add(new CodeLine(1, "var entity = await _repository.{0}(new {1}({2}));", aspNetCoreProject.EntityFrameworkCoreProject.GetGetRepositoryMethodName(table), aspNetCoreProject.EntityFrameworkCoreProject.GetEntityName(table), exp));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "if (entity == null)"));
            lines.Add(new CodeLine(2, "return NotFound();"));

            lines.Add(new EmptyLine());

            lines.Add(new CommentLine(1, " Remove entity from database"));
            lines.Add(new CodeLine(1, "_repository.Remove(entity);"));

            lines.Add(new EmptyLine());
            lines.Add(new CodeLine(1, "await _repository.CommitChangesAsync();"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new EmptyLine());
                lines.Add(new CodeLine(1, "_logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (selection.Settings.UseLogger)
            {
                lines.Add(new CodeLine(1, "response.SetError(_logger, nameof({0}), ex);", aspNetCoreProject.GetControllerDeleteAsyncMethodName(table)));
            }
            else
            {
                lines.Add(new CodeLine(1, "response.DidError = true;"));
                lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new EmptyLine());

            lines.Add(new CodeLine("return response.ToHttpResult();"));

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
                    new MetadataAttribute("ProducesResponseType", "200", "Type = typeof(IResponse)"),
                    new MetadataAttribute("ProducesResponseType", "400"),
                    new MetadataAttribute("ProducesResponseType", "404"),
                    new MetadataAttribute("ProducesResponseType", "500")
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
