using SqlKata;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;

namespace PetaPoco.SqlKata.Tests
{
    public class Tests
    {
        [Fact]
        public void Simple_Select()
        {
            var input = new Query("Foo");
            var expected = new Sql("SELECT * FROM [Foo]");
            var output = input.ToSql();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Specify_Columns()
        {
            var input = new Query("Foo").Select("Fruit", "Vegetable");
            var expected = new Sql("SELECT [Fruit], [Vegetable] FROM [Foo]");
            var output = input.ToSql();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Select_With_Where()
        {
            var input = new Query()
                .From("Foo")
                .Where("Fruit", "apple");
            var expected = new Sql("SELECT * FROM [Foo] WHERE [Fruit] = @0", "apple");
            var output = input.ToSql();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Multiple_Where()
        {
            var input = new Query("Foo")
                .Where("Fruit", "apple")
                .Where("Vegetable", ">", "carrot");
            var expected = new Sql("SELECT * FROM [Foo] WHERE [Fruit] = @0 AND [Vegetable] > @1", "apple", "carrot");
            var output = input.ToSql();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Where_Or()
        {
            var input = new Query("Foo")
                .Where("Fruit", "apple")
                .OrWhere("Fruit", "banana");
            var expected = new Sql("SELECT * FROM [Foo] WHERE [Fruit] = @0 OR [Fruit] = @1", "apple", "banana");
            var output = input.ToSql();
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Cant_Have_Just_Where()
        {
            var input = new Query().Where("Fruit", "banana");
            Action act = () => input.ToSql();
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
