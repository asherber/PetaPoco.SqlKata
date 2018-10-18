using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SqlKata;

namespace PetaPoco.SqlKata.Tests
{
    public class AutoGenerateTests
    {
        [Fact]
        public void ForType_Simple_Class()
        {
            var q = new Query().ForType<MyClass>();
            var expected = new FromClause() { Table = "MyClass", Alias = "MyClass", Component = "from" };
            q.Clauses.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForType_With_TableName()
        {
            var q = new Query().ForType<MyClassWithName>();
            var expected = new FromClause() { Table = "TableName", Alias = "TableName", Component = "from" };
            q.Clauses.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_SimpleClass()
        {
            var q = new Query().GenerateSelect<MyClass>();
            var expected = new AbstractClause[]
            {
                new FromClause() { Table = "MyClass", Alias = "MyClass", Component = "from" },
                new Column() { Component = "select", Name = "ID" },
                new Column() { Component = "select", Name = "Name" },
            };
            q.Clauses.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_NoFields()
        {
            var q = new Query().GenerateSelect<NoFields>();
            var expected = new AbstractClause[]
            {
                new FromClause() { Table = "NoFields", Alias = "NoFields", Component = "from" },
                new RawColumn() { Component = "select", Expression = "NULL", Bindings = new object[0] },  
            };
            q.Clauses.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_TableName()
        {
            var q = new Query().GenerateSelect<MyClassWithName>();
            var expected = new AbstractClause[]
            {
                new FromClause() { Table = "TableName", Alias = "TableName", Component = "from" },
                new Column() { Component = "select", Name = "ID" },
                new Column() { Component = "select", Name = "Name" },
            };
            q.Clauses.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_ColumnNames()
        {
            var q = new Query().GenerateSelect<MyClassWithColumnNames>();
            var expected = new AbstractClause[]
            {
                new FromClause() { Table = "MyClassWithColumnNames", Alias = "MyClassWithColumnNames", Component = "from" },
                new Column() { Component = "select", Name = "ID_FIELD" },
                new Column() { Component = "select", Name = "NAME_FIELD" },
            };
            q.Clauses.Should().BeEquivalentTo(expected);
        }
    }





    public class MyClass
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class NoFields
    {
    }

    [TableName("TableName")]
    public class MyClassWithName
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class MyClassWithColumnNames
    {
        [Column("ID_FIELD")]
        public int ID { get; set; }
        [Column("NAME_FIELD")]
        public string Name { get; set; }
    }
}
