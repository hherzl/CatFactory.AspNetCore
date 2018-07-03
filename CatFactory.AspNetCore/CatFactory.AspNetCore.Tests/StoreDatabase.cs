using System.Collections.Generic;
using CatFactory.Mapping;

namespace CatFactory.AspNetCore.Tests
{
    public static class StoreDatabase
    {
        public static Database Mock
            => new Database
            {
                Name = "Store",
                DefaultSchema = "dbo",
                Tables = new List<Table>
                {
                    new Table
                    {
                        Schema = "dbo",
                        Name = "EventLog",
                        Columns =
                        {
                            new Column { Name = "EventLogID", Type = "uniqueidentifier" },
                            new Column { Name = "EventType", Type = "int" },
                            new Column { Name = "Key", Type = "varchar", Length = 255 },
                            new Column { Name = "Message", Type = "varchar" },
                            new Column { Name = "EntryDate", Type = "datetime" }
                        }
                    },

                    new Table
                    {
                        Schema = "dbo",
                        Name = "ChangeLog",
                        Columns =
                        {
                            new Column { Name = "ChangeLogID", Type = "int" },
                            new Column { Name = "ClassName", Type = "varchar", Length = 128 },
                            new Column { Name = "PropertyName", Type = "varchar", Length = 128 },
                            new Column { Name = "Key", Type = "varchar", Length = 255 },
                            new Column { Name = "OriginalValue", Type = "varchar", Nullable = true },
                            new Column { Name = "CurrentValue", Type = "varchar", Nullable = true },
                            new Column { Name = "UserName", Type = "varchar", Length = 25 },
                            new Column { Name = "ChangeDate", Type = "varchar", Length = 128 }
                        },
                        Identity = new Identity { Name = "ChangeLogID", Seed = 1, Increment = 1 }
                    },

                    new Table
                    {
                        Schema = "dbo",
                        Name = "ChangeLogExclusion",
                        Columns =
                        {
                            new Column { Name = "ChangeLogExclusionID", Type = "varchar", Length = 25 },
                            new Column { Name = "TableName", Type = "varchar", Length = 128 },
                            new Column { Name = "ColumnName", Type = "varchar", Length = 128 }
                        }
                    },

                    new Table
                    {
                        Schema = "HumanResources",
                        Name = "Employee",
                        Columns =
                        {
                            new Column { Name = "EmployeeID", Type = "int" },
                            new Column { Name = "FirstName", Type = "varchar", Length = 25 },
                            new Column { Name = "MiddleName", Type = "varchar", Length = 25, Nullable = true },
                            new Column { Name = "LastName", Type = "varchar", Length = 25 },
                            new Column { Name = "BirthDate", Type = "datetime" }
                        },
                        Identity = new Identity { Name = "EmployeeID", Seed = 1, Increment = 1 }
                    },

                    new Table
                    {
                        Schema = "Production",
                        Name = "ProductCategory",
                        Columns =
                        {
                            new Column { Name = "ProductCategoryID", Type = "int" },
                            new Column { Name = "ProductCategoryName", Type = "varchar", Length = 100 },
                        },
                        Identity = new Identity { Name = "ProductCategoryID", Seed = 1, Increment = 1 },
                        Uniques =
                        {
                            new Unique("ProductCategoryName") { ConstraintName = "U_ProductCategoryName" }
                        }
                    },

                    new Table
                    {
                        Schema = "Production",
                        Name = "Warehouse",
                        Columns =
                        {
                            new Column { Name = "WarehouseID", Type = "varchar", Length = 5 },
                            new Column { Name = "WarehouseName", Type = "varchar", Length = 100 }
                        }
                    },

                    new Table
                    {
                        Schema = "Production",
                        Name = "Product",
                        Columns =
                        {
                            new Column { Name = "ProductID", Type = "int" },
                            new Column { Name = "ProductName", Type = "varchar", Length = 100 },
                            new Column { Name = "ProductCategoryID", Type = "int" },
                            new Column { Name = "UnitPrice", Type = "decimal", Prec = 8, Scale = 4 },
                            new Column { Name = "Description", Type = "varchar", Length = 255, Nullable = true },
                            new Column { Name = "Discontinued", Type = "bit" }
                        },
                        Identity = new Identity { Name = "ProductID", Seed = 1, Increment = 1 },
                        Uniques =
                        {
                            new Unique("ProductName") { ConstraintName = "U_ProductName" }
                        }
                    },

                    new Table
                    {
                        Schema = "Production",
                        Name = "ProductInventory",
                        Columns =
                        {
                            new Column { Name = "ProductInventoryID", Type = "int" },
                            new Column { Name = "ProductID", Type = "int" },
                            new Column { Name = "WarehouseID", Type = "varchar", Length = 5 },
                            new Column { Name = "EntryDate", Type = "datetime" },
                            new Column { Name = "Quantity", Type = "int" },
                            new Column { Name = "Stocks", Type = "int" }
                        },
                        Identity = new Identity { Name = "ProductInventoryID", Seed = 1, Increment = 1 }
                    },

                    new Table
                    {
                        Schema = "Sales",
                        Name = "Customer",
                        Columns =
                        {
                            new Column { Name = "CustomerID", Type = "int" },
                            new Column { Name = "CompanyName", Type = "varchar", Length = 100, Nullable = true },
                            new Column { Name = "ContactName", Type = "varchar", Length = 100, Nullable = true }
                        },
                        Identity = new Identity { Name = "CustomerID", Seed = 1, Increment = 1 },
                        Uniques =
                        {
                            new Unique("CompanyName") { ConstraintName = "U_CompanyName" }
                        }
                    },

                    new Table
                    {
                        Schema = "Sales",
                        Name = "Shipper",
                        Columns =
                        {
                            new Column { Name = "ShipperID", Type = "int" },
                            new Column { Name = "CompanyName", Type = "varchar", Length = 100, Nullable = true },
                            new Column { Name = "ContactName", Type = "varchar", Length = 100, Nullable = true }
                        },
                        Identity = new Identity { Name = "ShipperID", Seed = 1, Increment = 1 },
                        Uniques =
                        {
                            new Unique("CompanyName") { ConstraintName = "U_CompanyName" }
                        }
                    },

                    new Table
                    {
                        Schema = "Sales",
                        Name = "OrderStatus",
                        Columns =
                        {
                            new Column { Name = "OrderStatusID", Type = "smallint" },
                            new Column { Name = "Description", Type = "varchar", Length = 100, Nullable = true }
                        }
                    },

                    new Table
                    {
                        Schema = "Sales",
                        Name = "Order",
                        Columns =
                        {
                            new Column { Name = "OrderID", Type = "int" },
                            new Column { Name = "OrderStatusID", Type = "smallint" },
                            new Column { Name = "OrderDate", Type = "datetime" },
                            new Column { Name = "CustomerID", Type = "int" },
                            new Column { Name = "EmployeeID", Type = "int", Nullable = true },
                            new Column { Name = "ShipperID", Type = "int" },
                            new Column { Name = "Total", Type = "decimal", Prec = 12, Scale = 4 },
                            new Column { Name = "Comments", Type = "varchar", Length = 255, Nullable = true }
                        },
                        Identity = new Identity { Name = "OrderID", Seed = 1, Increment = 1 }
                    },

                    new Table
                    {
                        Schema = "Sales",
                        Name = "OrderDetail",
                        Columns =
                        {
                            new Column { Name = "OrderDetailID", Type = "int" },
                            new Column { Name = "OrderID", Type = "int" },
                            new Column { Name = "ProductID", Type = "int" },
                            new Column { Name = "ProductName", Type = "varchar", Length = 255 },
                            new Column { Name = "UnitPrice", Type = "decimal", Prec = 8, Scale = 4 },
                            new Column { Name = "Quantity", Type = "int" },
                            new Column { Name = "Total", Type = "decimal", Prec = 8, Scale = 4 }
                        },
                        Identity = new Identity { Name = "OrderDetailID", Seed = 1, Increment = 1 }
                    }
                },
                Views =
                {
                    new View
                    {
                        Schema = "Sales",
                        Name = "OrderSummary",
                        Columns =
                        {
                            new Column { Name = "OrderID", Type = "int" },
                            new Column { Name = "OrderDate", Type = "datetime" },
                            new Column { Name = "CustomerName", Type = "varchar", Length = 100 },
                            new Column { Name = "EmployeeName", Type = "varchar", Length = 100 },
                            new Column { Name = "ShipperName", Type = "varchar", Length = 100 },
                            new Column { Name = "OrderLines", Type = "int" }
                        }
                    }
                }
            }
            .AddColumnsForTables(new Column[]
            {
                new Column { Name = "CreationUser", Type = "varchar", Length = 25 },
                new Column { Name = "CreationDateTime", Type = "datetime" },
                new Column { Name = "LastUpdateUser", Type = "varchar", Length = 25, Nullable = true },
                new Column { Name = "LastUpdateDateTime", Type = "datetime", Nullable = true },
                new Column { Name = "Timestamp", Type = "rowversion", Nullable = true }
            }, "dbo.EventLog", "dbo.ChangeLog", "dbo.ChangeLogExclusion")
            .AddDbObjectsFromTables()
            .AddDbObjectsFromViews()
            .SetPrimaryKeyToTables()
            .LinkTables();
    }
}
