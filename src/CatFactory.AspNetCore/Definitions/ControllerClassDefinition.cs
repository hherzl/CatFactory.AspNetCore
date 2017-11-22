using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.Collections;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public static class ControllerClassDefinition
    {
        public static CSharpClassDefinition GetControllerClassDefinition(this ProjectFeature projectFeature, bool useLogger = true)
        {
            var definition = new CSharpClassDefinition();

            definition.Namespaces.Add("System");
            definition.Namespaces.Add("System.Linq");
            definition.Namespaces.Add("System.Threading.Tasks");
            definition.Namespaces.Add("Microsoft.AspNetCore.Mvc");
            definition.Namespaces.Add("Microsoft.EntityFrameworkCore");
            definition.Namespaces.Add("Microsoft.Extensions.Logging");

            definition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().GetDataLayerContractsNamespace());
            definition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().GetDataLayerRepositoriesNamespace());
            definition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().GetResponsesNamespace());
            definition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().GetViewModelsNamespace());

            definition.Namespace = "Controllers";

            definition.Attributes = new List<MetadataAttribute>()
            {
                new MetadataAttribute("Route", "\"api/[controller]\"")
            };

            definition.Name = projectFeature.GetControllerName();

            definition.BaseClass = "Controller";

            definition.Fields.Add(new FieldDefinition(AccessModifier.Protected, projectFeature.GetInterfaceRepositoryName(), "Repository")
            {
                IsReadOnly = true
            });

            if (useLogger)
            {
                definition.Fields.Add(new FieldDefinition(AccessModifier.Protected, "ILogger", "Logger"));
            }

            definition.Constructors.Add(GetConstructor(projectFeature));

            definition.Methods.Add(new MethodDefinition(AccessModifier.Protected, "void", "Dispose", new ParameterDefinition("Boolean", "disposing"))
            {
                IsOverride = true,
                Lines = new List<ILine>()
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
                definition.Methods.Add(GetGetAllMethod(projectFeature, definition, table, useLogger));

                if (table.PrimaryKey != null)
                {
                    definition.Methods.Add(GetGetMethod(table, useLogger));
                }

                definition.Methods.Add(GetPostMethod(table, useLogger));

                if (table.PrimaryKey != null)
                {
                    definition.Methods.Add(GetPutMethod(projectFeature, table, useLogger));

                    definition.Methods.Add(GetDeleteMethod(table, useLogger));
                }
            }

            return definition;
        }

        private static ClassConstructorDefinition GetConstructor(ProjectFeature projectFeature, bool useLogger = true)
        {
            var parameters = new List<ParameterDefinition>()
            {
                new ParameterDefinition(projectFeature.GetInterfaceRepositoryName(), "repository")
            };

            var lines = new List<ILine>()
            {
                new CodeLine("Repository = repository;")
            };

            if (useLogger)
            {
                parameters.Add(new ParameterDefinition(String.Format("ILogger<{0}>", projectFeature.GetControllerName()), "logger"));

                lines.Add(new CodeLine("Logger = logger;"));
            }

            return new ClassConstructorDefinition(parameters.ToArray())
            {
                Lines = lines
            };
        }

        private static MethodDefinition GetGetAllMethod(ProjectFeature projectFeature, CSharpClassDefinition definition, ITable table, bool useLogger = true)
        {
            if (table.HasDefaultSchema())
            {
                definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace());
            }
            else
            {
                definition.Namespaces.AddUnique(projectFeature.GetEntityFrameworkCoreProject().GetEntityLayerNamespace(table.Schema));
            }

            var lines = new List<ILine>();

            if (useLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAllAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            if (projectFeature.GetEntityFrameworkCoreProject().Settings.EntitiesWithDataContracts.Contains(table.FullName))
            {
                definition.Namespaces.Add(projectFeature.GetEntityFrameworkCoreProject().GetDataLayerDataContractsNamespace());
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
                var parentTable = projectFeature.Project.Database.FindTableByFullName(foreignKey.References);

                if (parentTable == null)
                {
                    continue;
                }

                if (parentTable.PrimaryKey?.Key.Count == 1)
                {
                    var column = parentTable.PrimaryKey.GetColumns(parentTable).First();

                    var resolver = new ClrTypeResolver();

                    parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), column.GetParameterName(), "null"));

                    foreignKeys.Add(column.GetParameterName());
                }
            }

            if (foreignKeys.Count == 0)
            {
                lines.Add(new CodeLine(1, "var query = Repository.{0}();", table.GetGetAllRepositoryMethodName()));
            }
            else
            {
                lines.Add(new CodeLine(1, "var query = Repository.{0}({1});", table.GetGetAllRepositoryMethodName(), string.Join(", ", foreignKeys)));
            }

            lines.Add(new CodeLine());

            if (useLogger)
            {
                lines.Add(new CodeLine(1, "response.PageSize = (Int32)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (Int32)pageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"Page {0} of {1}, Total of rows: {2}\", response.PageNumber, response.PageCount, response.ItemsCount);"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (useLogger)
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
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpGet", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetGetMethod(ITable table, bool useLogger = true)
        {
            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey?.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key[0]);

                var resolver = new ClrTypeResolver { UseNullableTypes = false };

                parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), "id"));
            }

            var lines = new List<ILine>();

            if (useLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetViewModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));
            lines.Add(new CodeLine(2, "response.Model = entity.ToViewModel();"));

            if (useLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (useLogger)
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
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpGet", String.Format("\"{0}/{1}\"", table.GetEntityName(), "{id}")),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetPostMethod(ITable table, bool useLogger = true)
        {
            var lines = new List<ILine>();

            if (useLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPostAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetViewModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = request.ToEntity();", table.GetAddRepositoryMethodName()));
            lines.Add(new CodeLine());

            foreach (var unique in table.Uniques)
            {
                lines.Add(new CodeLine(1, "if ((await Repository.{0}(entity)) != null)", table.GetGetByUniqueRepositoryMethodName(unique)));
                lines.Add(new CodeLine(1, "{"));
                lines.Add(new CodeLine(2, "return BadRequest();"));
                lines.Add(new CodeLine(1, "}"));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine(1, "await Repository.{0}(entity);", table.GetAddRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = entity.ToViewModel();"));

            if (useLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (useLogger)
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

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPostAsyncMethodName(), new ParameterDefinition(table.GetViewModelName(), "request", new MetadataAttribute("FromBody")))
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpPost", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetPutMethod(ProjectFeature projectFeature, ITable table, bool useLogger = true)
        {
            var lines = new List<ILine>();

            if (useLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPutAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetViewModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));

            foreach (var column in table.GetUpdateColumns(projectFeature.GetEntityFrameworkCoreProject().Settings))
            {
                lines.Add(new CodeLine(2, "entity.{0} = request.{0};", column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "await Repository.{0}(entity);", table.GetUpdateRepositoryMethodName()));

            if (useLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The data was updated successfully\");"));

                lines.Add(new CodeLine());

                lines.Add(new CodeLine(2, "response.Model = entity.ToViewModel();"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (useLogger)
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
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key[0]);

                var resolver = new ClrTypeResolver { UseNullableTypes = false };

                parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), (new DotNetNamingConvention()).GetParameterName("id")));
            }

            parameters.Add(new ParameterDefinition(table.GetViewModelName(), "request", new MetadataAttribute("FromBody")));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPutAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpPut", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        private static MethodDefinition GetDeleteMethod(ITable table, bool useLogger = true)
        {
            var lines = new List<ILine>();

            if (useLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerDeleteAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetViewModelName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{"));

            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{"));

            lines.Add(new CodeLine(2, "await Repository.{0}(entity);", table.GetRemoveRepositoryMethodName()));

            if (useLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(2, "Logger?.LogInformation(\"The entity was deleted successfully\");"));
            }

            lines.Add(new CodeLine(1, "}"));

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = entity?.ToViewModel();"));

            lines.Add(new CodeLine("}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{"));

            if (useLogger)
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
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key[0]);

                var resolver = new ClrTypeResolver { UseNullableTypes = false };

                parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), (new DotNetNamingConvention()).GetParameterName("id")));
            }

            return new MethodDefinition("Task<IActionResult>", table.GetControllerDeleteAsyncMethodName(), parameters.ToArray())
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpDelete", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
