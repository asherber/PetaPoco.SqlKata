using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SqlKata;
using FluentAssertions.Equivalency;

namespace PetaPoco.SqlKata.Tests
{
    public class AutoGenerateTests
    {
        private static void Compare(Query output, Query expected) => output.Should().BeEquivalentTo(expected, o => o.RespectingRuntimeTypes());

        [Fact]
        public void ForType_T_Simple_Class()
        {
            var q = new Query().ForType<MyClass>();
            var expected = new Query("MyClass");
            Compare(q, expected);
        }

        [Fact]
        public void ForType_T_With_TableName()
        {
            var q = new Query().ForType<MyClassWithName>();
            var expected = new Query("TableName");
            Compare(q, expected);
        }

        [Fact]
        public void ForType_Simple_Class()
        {
            var q = new Query().ForType(typeof(MyClass));
            var expected = new Query("MyClass");
            Compare(q, expected);
        }

        [Fact]
        public void ForType_With_TableName()
        {
            var q = new Query().ForType(typeof(MyClassWithName));
            var expected = new Query("TableName");
            Compare(q, expected);
        }

        [Fact]
        public void ForObject_Simple_Class()
        {
            var obj = new MyClass();
            var q = new Query().ForObject(obj);
            var expected = new Query("MyClass");
            Compare(q, expected);
        }

        [Fact]
        public void ForObject_With_TableName()
        {
            var obj = new MyClassWithName();
            var q = new Query().ForObject(obj);
            var expected = new Query("TableName");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_SimpleClass()
        {
            var q = new Query().GenerateSelect<MyClass>();
            var expected = new Query("MyClass").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_NoFields()
        {
            var q = new Query().GenerateSelect<NoFields>();
            var expected = new Query("NoFields").SelectRaw("NULL");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_With_TableName()
        {
            var q = new Query().GenerateSelect<MyClassWithName>();
            var expected = new Query("TableName").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_With_ColumnNames()
        {
            var q = new Query().GenerateSelect<MyClassWithColumnNames>();
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_SimpleClass()
        {
            var q = new Query().GenerateSelect(typeof(MyClass));
            var expected = new Query("MyClass").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_NoFields()
        {
            var q = new Query().GenerateSelect(typeof(NoFields));
            var expected = new Query("NoFields").SelectRaw("NULL");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_With_TableName()
        {
            var q = new Query().GenerateSelect(typeof(MyClassWithName));
            var expected = new Query("TableName").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_With_ColumnNames()
        {
            var q = new Query().GenerateSelect(typeof(MyClassWithColumnNames));
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_SimpleObject()
        {
            var obj = new MyClass();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("MyClass").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_NoFields_Object()
        {
            var obj = new NoFields();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("NoFields").SelectRaw("NULL");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_With_TableName_Object()
        {
            var obj = new MyClassWithName();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("TableName").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_With_ColumnNames_Object()
        {
            var obj = new MyClassWithColumnNames();
            var q = new Query().GenerateSelect(obj);
            var expected = new Query("MyClassWithColumnNames").Select("ID_FIELD", "NAME_FIELD");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_SimpleClass_HasFrom()
        {
            var q = new Query().From("OtherTable").GenerateSelect<MyClass>();
            var expected = new Query("OtherTable").Select("ID", "Name");
            Compare(q, expected);
        }

        [Fact]
        public void Generate_T_SimpleClass_HasSelect()
        {
            var q = new Query("OtherTable").Select("SomeField").GenerateSelect<MyClass>();
            var expected = new Query("OtherTable").Select("SomeField");
            Compare(q, expected);
        }

        [Fact]
        public void ForType_NullQuery_Throws()
        {
            Query query = null;
            Action act = () => query.ForType<MyClass>();
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GenerateSelect_NullQuery_Throws()
        {
            Query query = null;
            Action act = () => query.GenerateSelect<MyClass>();
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ForType_NullMapper_Throws()
        {
            try
            {
                SqlKataExtensions.DefaultMapper = null;
                Action act = () => new Query().ForType<MyClass>(null);
                act.Should().Throw<ArgumentNullException>();
            }
            finally
            {
                SqlKataExtensions.DefaultMapper = new ConventionMapper();
            }
        }

        [Fact]
        public void GenerateSelect_NullMapper_Throws()
        {
            try
            {
                SqlKataExtensions.DefaultMapper = null;
                Action act = () => new Query().GenerateSelect<MyClass>(null);
                act.Should().Throw<ArgumentNullException>();
            }
            finally
            {
                SqlKataExtensions.DefaultMapper = new ConventionMapper();
            }
        }

        [Theory]
        [MemberData(nameof(Mappers))]
        public void Different_Mappers(IMapper mapper, string tableName, params string[] fieldNames)
        {
            try
            {
                var q = new Query().GenerateSelect<MyOtherClass>(mapper);
                var expected = new Query(tableName).Select(fieldNames);
                Compare(q, expected);
            }
            finally
            {
                PetaPoco.Mappers.RevokeAll();  // To flush the PocoData cache
            }
        }

        [Theory]
        [MemberData(nameof(Mappers))]
        public void Different_Default_Mappers(IMapper mapper, string tableName, params string[] fieldNames)
        {
            if (mapper != null)
            {
                try
                {
                    SqlKataExtensions.DefaultMapper = mapper;
                    var q = new Query().GenerateSelect<MyOtherClass>();
                    var expected = new Query(tableName).Select(fieldNames);
                    Compare(q, expected);
                }
                finally
                {
                    SqlKataExtensions.DefaultMapper = new ConventionMapper();
                    PetaPoco.Mappers.RevokeAll();
                }
            }
        }

        public static IEnumerable<object[]> Mappers => new[]
        {
            new object[] { new UnderscoreMapper(), "my_other_class", "other_id", "other_name" },
            new object[] { new ConventionMapper(), "MyOtherClass", "OtherID", "OtherName" },
            new object[] { null, "MyOtherClass", "OtherID", "OtherName" },
        };

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

        public class UnderscoreMapper : ConventionMapper
        {
            public UnderscoreMapper()
            {
                InflectColumnName = (i, cn) => i.Underscore(cn);
                InflectTableName = (i, tn) => i.Underscore(tn);
            }
        }

        public class MyOtherClass
        {
            public int OtherID { get; set; }
            public string OtherName { get; set; }
        }
    }
}
