using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Compilers;
using PetaPoco;
using PetaPoco.Core;

namespace PetaPoco.SqlKata
{
    public enum CompilerType { SqlServer, MySql, Postgres, Firebird };

    public static class SqlKataExtensions
    {
        private static readonly Dictionary<CompilerType, Lazy<Compiler>> _compilers = new Dictionary<CompilerType, Lazy<Compiler>>()
        {
            {  CompilerType.SqlServer, new Lazy<Compiler>(() => new SqlServerCompiler()) },
            {  CompilerType.MySql, new Lazy<Compiler>(() => new MySqlCompiler()) },
            {  CompilerType.Postgres, new Lazy<Compiler>(() => new PostgresCompiler()) },
            {  CompilerType.Firebird, new Lazy<Compiler>(() => new FirebirdCompiler()) },
        };

        private static readonly IMapper _mapper = new ConventionMapper();

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

            return new Sql(ppSql, compiled.Bindings.ToArray());
        }

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Query ForType<T>(this Query query) => query.ForType<T>(_mapper);

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query ForType<T>(this Query query, IMapper mapper)
        {
            var tableInfo = mapper.GetTableInfo(typeof(T));
            return query.From(tableInfo.TableName);
        }

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type, using a default mapper. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Query GenerateSelect<T>(this Query query) => query.GenerateSelect<T>(_mapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query GenerateSelect<T>(this Query query, IMapper mapper)
        {
            var pd = PocoData.ForType(typeof(T), mapper);
            query = query.From(pd.TableInfo.TableName);

            query = pd.Columns.Any() ? query.Select(pd.QueryColumns) : query.SelectRaw("NULL");
            return query;
        }
    }
}
