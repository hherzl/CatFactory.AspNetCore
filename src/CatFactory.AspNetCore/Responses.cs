using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.AspNetCore
{
    //using System.Collections.Generic;
    //using CatFactory.DotNetCore;

    //namespace CatFactory.EfCore
    //{
    //    public static class BusinessLayerExtensions
    //    {
    //public static EfCoreProject GenerateViewModels(this EfCoreProject project)
    //{
    //    foreach (var table in project.Database.Tables)
    //    {
    //        var codeBuilder = new CSharpClassBuilder()
    //        {
    //            ObjectDefinition = new ViewModelClassDefinition(project, table)
    //            {
    //                Namespace = project.GetDataLayerDataContractsNamespace(),
    //            },
    //            OutputDirectory = project.OutputDirectory
    //        };

    //        codeBuilder.CreateFile(project.GetDataLayerDataContractsDirectory());
    //    }

    //    return project;
    //}

    //private static void GenerateBusinessLayerContracts(EfCoreProject project, CSharpInterfaceDefinition interfaceDefinition)
    //{
    //    var codeBuilder = new CSharpInterfaceBuilder
    //    {
    //        ObjectDefinition = interfaceDefinition,
    //        OutputDirectory = project.OutputDirectory
    //    };

    //    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetEntityLayerNamespace());

    //    codeBuilder.CreateFile(project.GetBusinessLayerContractsDirectory());
    //}

    //public static EfCoreProject GenerateBusinessObject(this EfCoreProject project)
    //{
    //    var codeBuilder = new CSharpClassBuilder
    //    {
    //        ObjectDefinition = new CSharpClassDefinition()
    //        {
    //            Name = "BusinessObject",
    //            Namespace = project.GetBusinessLayerNamespace()
    //        },
    //        OutputDirectory = project.OutputDirectory
    //    };

    //    codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerNamespace());

    //    codeBuilder.CreateFile(project.GetBusinessLayerDirectory());

    //    return project;
    //}

    //public static EfCoreProject GenerateBusinessObjects(this EfCoreProject project)
    //{
    //    project.GenerateBusinessObject();

    //    foreach (var projectFeature in project.Features)
    //    {
    //        var codeBuilder = new CSharpClassBuilder
    //        {
    //            ObjectDefinition = new BusinessObjectClassDefinition(projectFeature)
    //            {
    //                Namespace = project.GetBusinessLayerNamespace()
    //            },
    //            OutputDirectory = project.OutputDirectory
    //        };

    //        codeBuilder.ObjectDefinition.Namespaces.Add(project.GetDataLayerNamespace());
    //        codeBuilder.ObjectDefinition.Namespaces.Add(project.GetBusinessLayerContractsNamespace());

    //        var interfaceDef = (codeBuilder.ObjectDefinition as CSharpClassDefinition).RefactInterface();

    //        interfaceDef.Namespace = project.GetBusinessLayerContractsNamespace();

    //        GenerateBusinessLayerContracts(project, interfaceDef);

    //        codeBuilder.CreateFile(project.GetBusinessLayerDirectory());
    //    }

    //    return project;
    //}

    //public static EfCoreProject GenerateBusinessInterfacesResponses(this EfCoreProject project)
    //{
    //    var interfacesDefinitions = new List<CSharpInterfaceDefinition>()
    //    {
    //        new ResponseInterfaceDefinition(),
    //        new SingleModelResponseInterfaceDefinition(),
    //        new ListModelResponseInterfaceDefinition()
    //    };

    //    foreach (var definition in interfacesDefinitions)
    //    {
    //        definition.Namespace = project.GetBusinessLayerResponsesNamespace();

    //        var codeBuilder = new CSharpInterfaceBuilder
    //        {
    //            ObjectDefinition = definition,
    //            OutputDirectory = project.OutputDirectory
    //        };

    //        codeBuilder.CreateFile(project.GetBusinessLayerResponsesDirectory());
    //    }

    //    return project;
    //}

    //public static EfCoreProject GenerateBusinessClassesResponses(this EfCoreProject project)
    //{
    //    var classesDefinitions = new List<CSharpClassDefinition>()
    //    {
    //        new SingleModelResponseClassDefinition(),
    //        new ListModelResponseClassDefinition()
    //    };

    //    foreach (var definition in classesDefinitions)
    //    {
    //        definition.Namespace = project.GetBusinessLayerResponsesNamespace();

    //        var codeBuilder = new CSharpClassBuilder
    //        {
    //            ObjectDefinition = definition,
    //            OutputDirectory = project.OutputDirectory
    //        };

    //        codeBuilder.CreateFile(project.GetBusinessLayerResponsesDirectory());
    //    }

    //    return project;
    //}
    //    }
    //}


    //using System;
    //using CatFactory.DotNetCore;
    //using CatFactory.Mapping;
    //using CatFactory.OOP;

    //namespace CatFactory.EfCore
    //{
    //    public class BusinessObjectClassDefinition : CSharpClassDefinition
    //    {
    //        public BusinessObjectClassDefinition(ProjectFeature projectFeature)
    //        {
    //            Name = projectFeature.GetBusinessClassName();

    //            BaseClass = "BusinessObject";

    //            Implements.Add(projectFeature.GetBusinessInterfaceName());

    //            foreach (var dbObject in projectFeature.DbObjects)
    //            {
    //                Methods.Add(new MethodDefinition(String.Format("IEnumerable<{0}>", dbObject.GetSingularName()), String.Format("Get{0}", dbObject.GetPluralName())));

    //                Methods.Add(new MethodDefinition(dbObject.GetSingularName(), String.Format("Get{0}", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity")));

    //                if (!projectFeature.IsView(dbObject))
    //                {
    //                    Methods.Add(new MethodDefinition("void", String.Format("Add{0}", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity")));

    //                    Methods.Add(new MethodDefinition("void", String.Format("Update{0}", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "changes")));

    //                    Methods.Add(new MethodDefinition("void", String.Format("Delete{0}", dbObject.GetSingularName()), new ParameterDefinition(dbObject.GetSingularName(), "entity")));
    //                }
    //            }
    //        }
    //    }
    //}


    //using System.Collections.Generic;
    //using CatFactory.DotNetCore;
    //using CatFactory.Mapping;
    //using CatFactory.OOP;

    //namespace CatFactory.EfCore
    //{
    //    public class ViewModelClassDefinition : CSharpClassDefinition
    //    {
    //        public ViewModelClassDefinition(EfCoreProject project, IDbObject dbObject)
    //        {
    //            Namespaces.Add("System");
    //            Namespaces.Add("System.ComponentModel");

    //            Implements.Add("INotifyPropertyChanged");

    //            Events.Add(new EventDefinition("PropertyChangedEventHandler", "PropertyChanged"));

    //            Name = dbObject.GetViewModelName();

    //            Constructors.Add(new ClassConstructorDefinition());

    //            var resolver = new ClrTypeResolver() as ITypeResolver;

    //            var columns = default(IEnumerable<Column>);

    //            var tableCast = dbObject as ITable;

    //            if (tableCast != null)
    //            {
    //                columns = tableCast.Columns;
    //            }

    //            var viewCast = dbObject as IView;

    //            if (viewCast != null)
    //            {
    //                columns = viewCast.Columns;
    //            }

    //            if (tableCast != null || viewCast != null)
    //            {
    //                foreach (var column in columns)
    //                {
    //                    this.AddViewModelProperty(resolver.Resolve(column.Type), column.GetPropertyName());
    //                }
    //            }

    //            if (project.Settings.SimplifyDataTypes)
    //            {
    //                this.SimplifyDataTypes();
    //            }
    //        }
    //    }
    //}



    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using CatFactory.CodeFactory;
    //using CatFactory.DotNetCore;
    //using CatFactory.OOP;

    //namespace CatFactory.EfCore
    //{
    //    public static class XunitTddExtensions
    //    {  
    //        public static CSharpClassDefinition GetTestClass(this CSharpClassDefinition classDefinition)
    //        {
    //            var testClass = new CSharpClassDefinition();

    //            testClass.Namespaces.Add("System");
    //            testClass.Namespaces.Add("System.Threading.Tasks");  
    //            testClass.Namespaces.Add("Xunit");

    //            testClass.Namespace = "Tests";
    //            testClass.Name = String.Format("{0}Test", classDefinition.Name);

    //            foreach (var method in classDefinition.Methods)
    //            {
    //                var lines = new List<ILine>();

    //                lines.Add(new CommentLine(" Arrange"));
    //                lines.Add(new CodeLine("var instance = new {0}();", classDefinition.Name));

    //                foreach (var parameter in method.Parameters)
    //                {
    //                    if (String.IsNullOrEmpty(parameter.DefaultValue))
    //                    {
    //                        lines.Add(new CodeLine("var {0} = default({1});", parameter.Name, parameter.Type));
    //                    }
    //                    else
    //                    {
    //                        lines.Add(new CodeLine("var {0} = {1};", parameter.Name, parameter.DefaultValue));
    //                    }
    //                }

    //                lines.Add(new CodeLine());

    //                lines.Add(new CommentLine(" Act"));

    //                if (method.IsAsync)
    //                {
    //                    lines.Add(new CodeLine("var result = await instance.{0}({1});", method.Name, method.Parameters.Count == 0 ? String.Empty : String.Join(", ", method.Parameters.Select(item => item.Name))));
    //                }
    //                else
    //                {
    //                    lines.Add(new CodeLine("var result = instance.{0}({1});", method.Name, method.Parameters.Count == 0 ? String.Empty : String.Join(", ", method.Parameters.Select(item => item.Name))));
    //                }

    //                lines.Add(new CodeLine());

    //                lines.Add(new CommentLine(" Assert"));

    //                testClass.Methods.Add(new MethodDefinition(method.IsAsync ? "Task" : "void", String.Format("{0}Test", method.Name))
    //                {
    //                    IsAsync = method.IsAsync,
    //                    Attributes = new List<MetadataAttribute>()
    //                    {
    //                        new MetadataAttribute("Fact")
    //                    },
    //                    Lines = lines
    //                });
    //            }

    //            return testClass;
    //        }
    //    }
    //}

    // todo: add logic for this setting
    // public Boolean GenerateTestsForRepositories { get; set; }

}
