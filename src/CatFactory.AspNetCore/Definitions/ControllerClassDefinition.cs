using System;
using System.Collections.Generic;
using System.Linq;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.EfCore;
using CatFactory.Mapping;
using CatFactory.OOP;

namespace CatFactory.AspNetCore.Definitions
{
    public class ControllerClassDefinition : CSharpClassDefinition
    {
        public ControllerClassDefinition(ProjectFeature projectFeature)
            : base()
        {
            ProjectFeature = projectFeature;

            Init();
        }

        public Boolean UseLogger { get; set; } = true;

        public ProjectFeature ProjectFeature { get; set; }
        
        public void Init()
        {
            Namespaces.Add("System");
            Namespaces.Add("System.Linq");
            Namespaces.Add("System.Threading.Tasks");
            Namespaces.Add("Microsoft.AspNetCore.Mvc");
            Namespaces.Add("Microsoft.EntityFrameworkCore");
            Namespaces.Add("Microsoft.Extensions.Logging");

            Namespaces.Add(ProjectFeature.GetEfCoreProject().GetDataLayerContractsNamespace());

            Namespace = "Controllers";

            Attributes = new List<MetadataAttribute>()
            {
                new MetadataAttribute("Route", "\"api/[controller]\"")
            };

            Name = ProjectFeature.GetControllerName();

            BaseClass = "Controller";

            Fields.Add(new FieldDefinition(AccessModifier.Protected, ProjectFeature.GetInterfaceRepositoryName(), "Repository")
            {
                IsReadOnly = true
            });

            if (UseLogger)
            {
                Fields.Add(new FieldDefinition(AccessModifier.Protected, "ILogger", "Logger"));
            }

            Constructors.Add(GetConstructor(ProjectFeature));

            Methods.Add(new MethodDefinition(AccessModifier.Protected, "void", "Dispose", new ParameterDefinition("Boolean", "disposing"))
            {
                IsOverride = true,
                Lines = new List<ILine>()
                {
                    new CodeLine("Repository?.Dispose();"),
                    new CodeLine(),
                    new CodeLine("base.Dispose(disposing);")
                }
            });

            var dbos = ProjectFeature.DbObjects.Select(dbo => dbo.FullName).ToList();
            var tables = ProjectFeature.Database.Tables.Where(t => dbos.Contains(t.FullName)).ToList();

            foreach (var table in tables)
            {
                Methods.Add(GetGetAllMethod(table));

                Methods.Add(GetGetMethod(table));

                Methods.Add(GetPostMethod(table));

                Methods.Add(GetPutMethod(table));

                Methods.Add(GetDeleteMethod(table));
            }
        }

        public ClassConstructorDefinition GetConstructor(ProjectFeature projectFeature)
        {
            var parameters = new List<ParameterDefinition>()
            {
                new ParameterDefinition(projectFeature.GetInterfaceRepositoryName(), "repository")
            };

            var lines = new List<ILine>()
            {
                new CodeLine("Repository = repository;")
            };

            if (UseLogger)
            {
                parameters.Add(new ParameterDefinition(String.Format("ILogger<{0}>", ProjectFeature.GetControllerName()), "logger"));

                lines.Add(new CodeLine("Logger = logger;"));
            }

            return new ClassConstructorDefinition(parameters.ToArray())
            {
                Lines = lines
            };
        }

        public MethodDefinition GetGetAllMethod(ITable table)
        {
            var lines = new List<ILine>();

            if (UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAllAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new PagedResponse<{0}>();", table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "var query = Repository.{0}();",  table.GetGetAllRepositoryMethodName()));
            lines.Add(new CodeLine());

            if (UseLogger)
            {
                lines.Add(new CodeLine(1, "response.PageSize = (Int32)pageSize;"));
                lines.Add(new CodeLine(1, "response.PageNumber = (Int32)pageNumber;"));
                lines.Add(new CodeLine(1, "response.ItemsCount = await query.CountAsync();"));
                lines.Add(new CodeLine());

                lines.Add(new CodeLine(1, "response.Model = await query.Paging(response.PageSize, response.PageNumber).ToListAsync();"));
                lines.Add(new CodeLine());
                
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "response.DidError = true;"));
            lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogError(\"Error on '{{0}}': {{1}}\", nameof({0}), ex.ToString());", table.GetControllerGetAllAsyncMethodName()));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine());
            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerGetAllAsyncMethodName(), new ParameterDefinition("Int32?", "pageSize", "10"), new ParameterDefinition("Int32?", "pageNumber", "1"))
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpGet", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetGetMethod(ITable table)
        {
            var parameters = new List<ParameterDefinition>();

            if (table.PrimaryKey?.Key.Count == 1)
            {
                var column = table.Columns.FirstOrDefault(item => item.Name == table.PrimaryKey.Key[0]);

                var resolver = new ClrTypeResolver { UseNullableTypes = false };

                parameters.Add(new ParameterDefinition(resolver.Resolve(column.Type), NamingConvention.GetParameterName("id")));
            }

            var lines = new List<ILine>();

            if (UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerGetAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{{"));
            
            lines.Add(new CodeLine(1, "response.Model = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "response.DidError = true;"));
            lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogError(\"Error on '{{0}}': {{1}}\", nameof({0}), ex.ToString());", table.GetControllerGetAllAsyncMethodName()));
            }

            lines.Add(new CodeLine("}}"));
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

        public MethodDefinition GetPostMethod(ITable table)
        {
            var lines = new List<ILine>();

            if (UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPostAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{{"));

            lines.Add(new CodeLine(1, "await Repository.{0}(value);", table.GetAddRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "response.Model = value;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "response.DidError = true;"));
            lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogError(\"Error on '{{0}}': {{1}}\", nameof({0}), ex.ToString());", table.GetControllerPostAsyncMethodName()));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPostAsyncMethodName(), new ParameterDefinition(table.GetViewModelName(), "value", new MetadataAttribute("FromBody")))
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpPost", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetPutMethod(ITable table)
        {
            var lines = new List<ILine>();

            if (UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerPutAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{{"));

            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(value);", table.GetGetRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{{"));

            foreach (var column in table.GetUpdateColumns(ProjectFeature.GetEfCoreProject().Settings))
            {
                lines.Add(new CodeLine(2, "entity.{0} = {0};", column.GetPropertyName()));
            }

            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "await Repository.{0}(value);", table.GetUpdateRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "response.Model = value;"));

            lines.Add(new CodeLine(1, "}}"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "response.DidError = true;"));
            lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogError(\"Error on '{{0}}': {{1}}\", nameof({0}), ex.ToString());", table.GetControllerPutAsyncMethodName()));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerPutAsyncMethodName(), new ParameterDefinition("int", "id"), new ParameterDefinition(table.GetViewModelName(), "value", new MetadataAttribute("FromBody")))
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpPut", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }

        public MethodDefinition GetDeleteMethod(ITable table)
        {
            var lines = new List<ILine>();

            if (UseLogger)
            {
                lines.Add(new CodeLine("Logger?.LogDebug(\"'{{0}}' has been invoked\", nameof({0}));", table.GetControllerDeleteAsyncMethodName()));
                lines.Add(new CodeLine());
            }

            lines.Add(new CodeLine("var response = new SingleResponse<{0}>();", table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("try"));
            lines.Add(new CodeLine("{{"));

            lines.Add(new CodeLine(1, "var entity = await Repository.{0}(new {1}(id));", table.GetGetRepositoryMethodName(), table.GetEntityName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(1, "if (entity != null)"));
            lines.Add(new CodeLine(1, "{{"));

            lines.Add(new CodeLine(2, "await Repository.{0}(value);", table.GetRemoveRepositoryMethodName()));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine(2, "response.Model = value;"));

            lines.Add(new CodeLine(1, "}}"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogInformation(\"The data was retrieved successfully\");"));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine("catch (Exception ex)"));
            lines.Add(new CodeLine("{{"));
            lines.Add(new CodeLine(1, "response.DidError = true;"));
            lines.Add(new CodeLine(1, "response.ErrorMessage = ex.Message;"));

            if (UseLogger)
            {
                lines.Add(new CodeLine());
                lines.Add(new CodeLine(1, "Logger?.LogError(\"Error on '{{0}}': {{1}}\", nameof({0}), ex.ToString());", table.GetControllerDeleteAsyncMethodName()));
            }

            lines.Add(new CodeLine("}}"));
            lines.Add(new CodeLine());

            lines.Add(new CodeLine("return response.ToHttpResponse();"));

            return new MethodDefinition("Task<IActionResult>", table.GetControllerDeleteAsyncMethodName(), new ParameterDefinition("int", "id"), new ParameterDefinition(table.GetViewModelName(), "value", new MetadataAttribute("FromBody")))
            {
                Attributes = new List<MetadataAttribute>()
                {
                    new MetadataAttribute("HttpPut", String.Format("\"{0}\"", table.GetEntityName())),
                },
                IsAsync = true,
                Lines = lines
            };
        }
    }
}
