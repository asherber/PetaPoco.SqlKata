using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Compilers;
using PetaPoco;

namespace PetaPoco.SqlKata
{
    public enum CompilerType { SqlServer, MySql, Postgres, Firebird };

    public static class SqlKataExtensions
    {
        private static Dictionary<CompilerType, Lazy<Compiler>> _compilers = new Dictionary<CompilerType, Lazy<Compiler>>()
        {
            {  CompilerType.SqlServer, new Lazy<Compiler>(() => new SqlServerCompiler()) },
            {  CompilerType.MySql, new Lazy<Compiler>(() => new MySqlCompiler()) },
            {  CompilerType.Postgres, new Lazy<Compiler>(() => new PostgresCompiler()) },
            {  CompilerType.Firebird, new Lazy<Compiler>(() => new FirebirdCompiler()) },
        };

        /// <summary>
        /// Indicates the compiler that gets used when one is not specified.
        /// Defaults to SqlServer.
        /// </summary>
        public static CompilerType DefaultCompiler { get; set; } = CompilerType.SqlServer;
        
        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object, 
        /// using a <seealso cref="SqlServerCompiler"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Sql ToSql(this Query query) => query.ToSql(DefaultCompiler);

        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="compilerType"></param>
        /// <returns></returns>
        public static Sql ToSql(this Query query, CompilerType compilerType)
        {
            var compiler = _compilers[compilerType].Value;
            var compiled = compiler.Compile(query);
            var ppSql = Helper.ReplaceAll(compiled.RawSql, "?", x => "@" + x);

            return new Sql(ppSql, compiled.Bindings);
        }
    }
}
