using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SqlKata.Compilers;
using PetaPoco.Providers;

namespace PetaPoco.SqlKata.Tests
{
    public class DefaultCompilersTests
    {
        [Theory]
        [InlineData(CompilerType.MySql, typeof(MySqlCompiler))]
        [InlineData(CompilerType.Firebird, typeof(FirebirdCompiler))]
        public void Get_Should_Work_For_Known_Types(CompilerType compilerType, Type compiler)
        {
            var output = DefaultCompilers.Get(compilerType);
            output.Should().BeOfType(compiler);
        }

        [Fact]
        public void Get_For_Custom_Should_Throw()
        {
            Action act = () => DefaultCompilers.Get(CompilerType.Custom);
            act.Should().Throw<ArgumentException>();
        }

        public class MyDatabaseProvider : OracleDatabaseProvider
        { }

        [Fact]
        public void Adding_And_Removing_Custom_Works()
        {
            var provider = new MyDatabaseProvider();
            DefaultCompilers.TryGetCustom(provider.GetType(), out var compiler).Should().BeFalse();

            DefaultCompilers.RegisterFor<MyDatabaseProvider>(new PercentCompiler());

            DefaultCompilers.TryGetCustom(provider, out compiler).Should().BeTrue();
            compiler.Should().BeOfType<PercentCompiler>();

            DefaultCompilers.RegisterFor<MyDatabaseProvider>(null);
            DefaultCompilers.TryGetCustom(provider.GetType(), out compiler).Should().BeFalse();
        }
    }
}
