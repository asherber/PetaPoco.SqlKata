/**
 * Copyright 2018-21 Aaron Sherber
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
using SqlKata.Compilers;

namespace PetaPoco.Extensions
{
    public static class DatabaseExtensions
    {
        private static Compiler ToCompiler(this IProvider provider)
        {
            CompilerType compilerType;

            if (provider is MySqlDatabaseProvider)
                compilerType = CompilerType.MySql;
            else if (provider is PostgreSQLDatabaseProvider)
                compilerType = CompilerType.Postgres;
            else if (provider is FirebirdDbDatabaseProvider)
                compilerType = CompilerType.Firebird;
            else if (provider is SQLiteDatabaseProvider)
                compilerType = CompilerType.SQLite;
            else if (provider is OracleDatabaseProvider)
                compilerType = CompilerType.Oracle;
            else
                compilerType = CompilerType.SqlServer;

            return DefaultCompilers.Get(compilerType);
        }

        private static IMapper GetMapper<T>(this IDatabase db) => Mappers.GetMapper(typeof(T), db.DefaultMapper);
        private static Compiler GetCompiler(this IDatabase db) => db.Provider.ToCompiler();

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
            return db.Query<T>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Runs an SQL query, returning the results as an IEnumerable collection
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>An enumerable collection of result records</returns>
        /// <remarks>
        ///     For some DB providers, care should be taken to not start a new Query before finishing with
        ///     and disposing the previous one. In cases where this is an issue, consider using Fetch which
        ///     returns the results as a List rather than an IEnumerable.
        /// </remarks>
        public static IEnumerable<T> Query<T>(this IDatabase db, Query query, Compiler compiler)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.Query<T>(query.ToSql(compiler));
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
            return db.Execute(query, db.GetCompiler());
        }

        /// <summary>
        ///     Executes a non-query command
        /// </summary>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>The number of rows affected</returns>
        public static int Execute(this IDatabase db, Query query, Compiler compiler)
        {
            return db.Execute(query.ToSql(compiler));
        }

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <returns>The scalar value cast to T</returns>
        public static T ExecuteScalar<T>(this IDatabase db, Query query)
        {
            return db.ExecuteScalar<T>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Executes a query and return the first column of the first row in the result set.
        /// </summary>
        /// <typeparam name="T">The type that the result value should be cast to</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>The scalar value cast to T</returns>
        public static T ExecuteScalar<T>(this IDatabase db, Query query, Compiler compiler)
        {
            return db.ExecuteScalar<T>(query.ToSql(compiler));
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, Query query)
        {
            return db.Page<T>(page, itemsPerPage, query, db.GetCompiler());
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static Page<T> Page<T>(this IDatabase db, long page, long itemsPerPage, Query query, Compiler compiler)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.Page<T>(page, itemsPerPage, query.ToSql(compiler));
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
            return db.SkipTake<T>(skip, take, query, db.GetCompiler());
        }

        /// <summary>
        ///     Retrieves a range of records from result set
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="skip">The number of rows at the start of the result set to skip over</param>
        /// <param name="take">The number of rows to retrieve</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A List of results</returns>
        /// <remarks>
        ///     PetaPoco will automatically modify the supplied SELECT statement to only retrieve the
        ///     records for the specified range.
        /// </remarks>
        public static List<T> SkipTake<T>(this IDatabase db, long skip, long take, Query query, Compiler compiler)
        {
            query = query.GenerateSelect<T>(db.GetMapper<T>());
            return db.SkipTake<T>(skip, take, query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-results set query
        /// </summary>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A GridReader to be queried</returns>
        public static IGridReader QueryMultiple(this IDatabase db, Query query)
        {
            return db.QueryMultiple(query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-results set query
        /// </summary>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"
        /// <returns>A GridReader to be queried</returns>
        public static IGridReader QueryMultiple(this IDatabase db, Query query, Compiler compiler)
        {
            return db.QueryMultiple(query.ToSql(compiler));
        }

        #region Multi Poco

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<TRet> Fetch<T1, T2, TRet>(this IDatabase db, Func<T1, T2, TRet> cb, Query query)
        {
            return db.Query(cb, query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<TRet> Fetch<T1, T2, T3, TRet>(this IDatabase db, Func<T1, T2, T3, TRet> cb, Query query)
        {
            return db.Query(cb, query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<TRet> Fetch<T1, T2, T3, T4, TRet>(this IDatabase db, Func<T1, T2, T3, T4, TRet> cb, Query query)
        {
            return db.Query(cb, query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The returned list POCO type</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(this IDatabase db, Func<T1, T2, T3, T4, T5, TRet> cb, Query query)
        {
            return db.Query(cb, query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, TRet>(this IDatabase db, Func<T1, T2, TRet> cb, Query query)
        {
            return db.Query(cb, query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, TRet>(this IDatabase db, Func<T1, T2, TRet> cb, Query query, Compiler compiler)
        {
            return db.Query(cb, query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, TRet>(this IDatabase db, Func<T1, T2, T3, TRet> cb, Query query)
        {
            return db.Query(cb, query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, TRet>(this IDatabase db, Func<T1, T2, T3, TRet> cb, Query query, Compiler compiler)
        {
            return db.Query(cb, query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(this IDatabase db, Func<T1, T2, T3, T4, TRet> cb, Query query)
        {
            return db.Query(cb, query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(this IDatabase db, Func<T1, T2, T3, T4, TRet> cb, Query query, Compiler compiler)
        {
            return db.Query(cb, query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(this IDatabase db, Func<T1, T2, T3, T4, T5, TRet> cb, Query query)
        {
            return db.Query(cb, query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <typeparam name="TRet">The type of objects in the returned IEnumerable</typeparam>
        /// <param name="cb">A callback function to connect the POCO instances, or null to automatically guess the relationships</param>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(this IDatabase db, Func<T1, T2, T3, T4, T5, TRet> cb, Query query, Compiler compiler)
        {
            return db.Query(cb, query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<T1> Fetch<T1, T2>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2>(query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<T1> Fetch<T1, T2, T3>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3>(query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<T1> Fetch<T1, T2, T3, T4>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3, T4>(query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco fetch
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fourth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as a List</returns>
        public static List<T1> Fetch<T1, T2, T3, T4, T5>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3, T4, T5>(query).ToList();
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2>(this IDatabase db, Query query, Compiler compiler)
        {
            return db.Query<T1, T2>(query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3>(this IDatabase db, Query query, Compiler compiler)
        {
            return db.Query<T1, T2, T3>(query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3, T4>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3, T4>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <param name="compiler"></param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3, T4>(this IDatabase db, Query query, Compiler compiler)
        {
            return db.Query<T1, T2, T3, T4>(query.ToSql(compiler));
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3, T4, T5>(this IDatabase db, Query query)
        {
            return db.Query<T1, T2, T3, T4, T5>(query, db.GetCompiler());
        }

        /// <summary>
        ///     Perform a multi-poco query
        /// </summary>
        /// <typeparam name="T1">The first POCO type</typeparam>
        /// <typeparam name="T2">The second POCO type</typeparam>
        /// <typeparam name="T3">The third POCO type</typeparam>
        /// <typeparam name="T4">The fourth POCO type</typeparam>
        /// <typeparam name="T5">The fifth POCO type</typeparam>
        /// <param name="query">A SqlKata <seealso cref="Query"/> representing the base SQL query and its arguments</param>
        /// <returns>A collection of POCOs as an IEnumerable</returns>
        public static IEnumerable<T1> Query<T1, T2, T3, T4, T5>(this IDatabase db, Query query, Compiler compiler)
        {
            return db.Query<T1, T2, T3, T4, T5>(query.ToSql(compiler));
        }
        #endregion
    }
}
