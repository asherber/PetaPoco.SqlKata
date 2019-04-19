/**
 * Copyright 2018-19 Aaron Sherber
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 *  limitations under the License.
 */
 
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco.SqlKata;
using PetaPoco.Core;
using PetaPoco.Providers;

namespace PetaPoco.Extensions
{
    public static class DatabaseExtensions
    {
        private static CompilerType ToCompiler(this IProvider provider)
        {
            if (provider is MySqlDatabaseProvider)
                return CompilerType.MySql;
            else if (provider is PostgreSQLDatabaseProvider)
                return CompilerType.Postgres;
            else if (provider is FirebirdDbDatabaseProvider)
                return CompilerType.Firebird;
            else if (provider is SQLiteDatabaseProvider)
                return CompilerType.SQLite;
            else if (provider is OracleDatabaseProvider)
                return CompilerType.Oracle;
            else
                return CompilerType.SqlServer;
        }

        private static IMapper GetMapper<T>(this IDatabase db) => Mappers.GetMapper(typeof(T), db.DefaultMapper);
        private static CompilerType GetCompiler(this IDatabase db) => db.Provider.ToCompiler();

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public static IEnumerable<T> Query<T>(this IDatabase db, Query query)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.Query<T>(query.ToSql(db.GetCompiler()));
        }

        /// <summary>
        ///     Runs a query and returns the result set as a typed list
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>A List holding the results of the query</returns>
        public static List<T> Fetch<T>(this IDatabase db, Query query)
        {
            return db.Query<T>(query).ToList();
        }

        /// <summary>
        ///     Runs a query that should always return at least one row
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>The first record in the result set</returns>
        public static T First<T>(this IDatabase db, Query query)
        {
            return db.Query<T>(query).First();
        }

        /// <summary>
        ///     Runs a query and returns the first record, or the default value if no matching records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>The first record in the result set, or default(T) if no matching records</returns>
        public static T FirstOrDefault<T>(this IDatabase db, Query query)
        {
            return db.Query<T>(query).FirstOrDefault();
        }


        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>The number of rows affected</returns>
        public static int Execute(this IDatabase db, Query query)
        {
            return db.Execute(query.ToSql(db.GetCompiler()));
        }

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>The scalar value cast to T</returns>
        public static T ExecuteScalar<T>(this IDatabase db, Query query)
        {
            return db.ExecuteScalar<T>(query.ToSql(db.GetCompiler()));
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, Query query)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.Page<T>(page, itemsPerPage, query.ToSql(db.GetCompiler()));
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static List<T> SkipTake<T>(this IDatabase db, long skip, long take, Query query)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.SkipTake<T>(skip, take, query.ToSql(db.GetCompiler()));
        }
    }
}
