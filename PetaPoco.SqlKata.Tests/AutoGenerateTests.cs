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
        public void ForType_T_Simple_Class()
        {
            var q = new Query().ForType<MyClass>();
            var expected = new Query("MyClass");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForType_T_With_TableName()
        {
            var q = new Query().ForType<MyClassWithName>();
            var expected = new Query("TableName");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForType_Simple_Class()
        {
            var q = new Query().ForType(typeof(MyClass));
            var expected = new Query("MyClass");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForType_With_TableName()
        {
            var q = new Query().ForType(typeof(MyClassWithName));
            var expected = new Query("TableName");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForObject_Simple_Class()
        {
            var obj = new MyClass();
            var q = new Query().ForObject(obj);
            var expected = new Query("MyClass");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ForObject_With_TableName()
        {
            var obj = new MyClassWithName();
            var q = new Query().ForObject(obj);
            var expected = new Query("TableName");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_T_SimpleClass()
        {
            var q = new Query().GenerateSelect<MyClass>();
            var expected = new Query("MyClass").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_T_NoFields()
        {
            var q = new Query().GenerateSelect<NoFields>();
            var expected = new Query("NoFields").SelectRaw("NULL");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_T_With_TableName()
        {
            var q = new Query().GenerateSelect<MyClassWithName>();
            var expected = new Query("TableName").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_T_With_ColumnNames()
        {
            var q = new Query().GenerateSelect<MyClassWithColumnNames>();
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_SimpleClass()
        {
            var q = new Query().GenerateSelect(typeof(MyClass));
            var expected = new Query("MyClass").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_NoFields()
        {
            var q = new Query().GenerateSelect(typeof(NoFields));
            var expected = new Query("NoFields").SelectRaw("NULL");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_TableName()
        {
            var q = new Query().GenerateSelect(typeof(MyClassWithName));
            var expected = new Query("TableName").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_ColumnNames()
        {
            var q = new Query().GenerateSelect(typeof(MyClassWithColumnNames));
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_SimpleObject()
        {
            var obj = new MyClass();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("MyClass").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_NoFields_Object()
        {
            var obj = new NoFields();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("NoFields").SelectRaw("NULL");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_TableName_Object()
        {
            var obj = new MyClassWithName();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("TableName").Select("ID", "Name");
            q.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Generate_With_ColumnNames_Object()
        {
            var obj = new MyClassWithColumnNames();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            q.Should().BeEquivalentTo(expected);
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
