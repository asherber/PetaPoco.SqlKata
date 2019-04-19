using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using PetaPoco.Providers;
using SqlKata;
using PetaPoco.Extensions;
using PetaPoco.Core;

namespace PetaPoco.SqlKata.Tests
{
    public class DatabaseExtensionTests
    {
        private Mock<IDatabase> _mockDb;
        private Sql _lastSql;

        public class UnderscoreMapper : ConventionMapper
        {
            public UnderscoreMapper()
            {
                InflectColumnName = (i, cn) => i.Underscore(cn);
                InflectTableName = (i, tn) => i.Underscore(tn);
            }
        }

        public class SomeClass
        {
            public int SomeID { get; set; }
            public string SomeName { get; set; }
        }

        public DatabaseExtensionTests()
        {
            Mappers.RevokeAll();   // to clear cache
            _mockDb = new Mock<IDatabase>();
            _mockDb.Setup(m => m.Query<SomeClass>(It.IsAny<Sql>()))
                .Returns(new List<SomeClass>())
                .Callback<Sql>(s => _lastSql = s);
            _mockDb.Setup(m => m.Execute(It.IsAny<Sql>()))
                .Returns(0)
                .Callback<Sql>(s => _lastSql = s);
        }

        [Theory]
        [MemberData(nameof(QuerySetups))]
        public void Query_Uses_MapperAndProvider(IProvider provider, IMapper mapper, string selectText)
        {            
            _mockDb.Setup(m => m.Provider).Returns(provider);
            _mockDb.Setup(m => m.DefaultMapper).Returns(mapper);

            var q = new Query().Where("Foo", "Bar");
            var output = _mockDb.Object.Query<SomeClass>(q);
            var expected = new Sql(selectText, "Bar");

            _mockDb.VerifyGet(db => db.Provider, Times.Once());
            _mockDb.VerifyGet(db => db.DefaultMapper, Times.Once());
            _lastSql.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> QuerySetups => new[]
        {
            new object[] { new MySqlDatabaseProvider(), new UnderscoreMapper(),
                "SELECT `some_id`, `some_name` FROM `some_class` WHERE `Foo` = @0" },
            new object[] { new SqlServerDatabaseProvider(), new ConventionMapper(),
                "SELECT [SomeID], [SomeName] FROM [SomeClass] WHERE [Foo] = @0"},
        };

        [Fact]
        public void First_Should_Throw()
        {
            Action act = () => _mockDb.Object.First<SomeClass>(new Query());
            act.Should().Throw<InvalidOperationException>().WithMessage("Sequence contains no elements");
        }

        [Theory]
        [MemberData(nameof(ExecuteSetups))]
        public void Execute_Uses_Provider(IProvider provider, string expectedSql)
        {
            _mockDb.Setup(m => m.Provider).Returns(provider);
            
            var q = new Query("Foo").AsUpdate(new { Bar = "Fizzbin" }).Where("Bar", "Baz");
            var output = _mockDb.Object.Execute(q);
            var expected = new Sql(expectedSql, "Fizzbin", "Baz");

            _mockDb.VerifyGet(db => db.Provider, Times.Once());
            _lastSql.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> ExecuteSetups => new[]
        {
            new object[] { new MySqlDatabaseProvider(), "UPDATE `Foo` SET `Bar` = @0 WHERE `Bar` = @1" },
            new object[] { new SqlServerDatabaseProvider(), "UPDATE [Foo] SET [Bar] = @0 WHERE [Bar] = @1"},
        };
    }
}
