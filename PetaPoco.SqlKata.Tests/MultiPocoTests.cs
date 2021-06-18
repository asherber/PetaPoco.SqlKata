using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using SqlKata;
using PetaPoco.Extensions;
using PetaPoco.Providers;
using PetaPoco.Core;

namespace PetaPoco.SqlKata.Tests
{
    public class MultiPocoTests
    {
        private readonly Mock<IDatabase> _mockDb;
        private readonly Sql _expected = It.Is<Sql>(s => s.SQL == "SELECT [Foo] FROM [Bar]");
        private readonly Query _query = new Query("Bar").Select("Foo");

        public class A { }

        public MultiPocoTests()
        {
            _mockDb = new Mock<IDatabase>();
            _mockDb.Setup(m => m.Provider).Returns(new SqlServerDatabaseProvider());
        }

        [Fact]
        public void Fetch_12Ret_Works()
        {
            Func<A, A, A> cb = null;
            _mockDb.Setup(m => m.Query<A, A, A>(null, _expected))
                .Returns(new List<A>());
            
            _mockDb.Object.Fetch(cb, _query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_123Ret_Works()
        {
            Func<A, A, A, A> cb = null;
            _mockDb.Setup(m => m.Query<A, A, A, A>(null, _expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch(cb, _query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_1234Ret_Works()
        {
            Func<A, A, A, A, A> cb = null;
            _mockDb.Setup(m => m.Query<A, A, A, A, A>(null, _expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch(cb, _query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_12345Ret_Works()
        {
            Func<A, A, A, A, A, A> cb = null;
            _mockDb.Setup(m => m.Query<A, A, A, A, A, A>(null, _expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch(cb, _query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_12_Works()
        {
            _mockDb.Setup(m => m.Query<A, A>(_expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch<A, A>(_query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_123_Works()
        {
            _mockDb.Setup(m => m.Query<A, A, A>(_expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch<A, A, A>(_query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_1234_Works()
        {
            _mockDb.Setup(m => m.Query<A, A, A, A>(_expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch<A, A, A, A>(_query);
            _mockDb.Verify();
        }

        [Fact]
        public void Fetch_12345_Works()
        {
            _mockDb.Setup(m => m.Query<A, A, A, A, A>(_expected))
                .Returns(new List<A>());

            _mockDb.Object.Fetch<A, A, A, A, A>(_query);
            _mockDb.Verify();
        }
    }
}
