﻿/**
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Compilers;
using PetaPoco;
using PetaPoco.Core;

#if DEBUG
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("PetaPoco.SqlKata.Tests")]
#endif

namespace PetaPoco.SqlKata
{
    public enum CompilerType { SqlServer, MySql, Postgres, Firebird, SQLite, Oracle, Custom };

    public static class SqlKataExtensions
    {
        private static CompilerType _defaultCompilerType = CompilerType.SqlServer;
        private static Compiler _customCompiler;
        private readonly static object _compilerLock = new object();

        /// <summary>
        /// Indicates the <seealso cref="Compiler"/> that gets used when one is not specified.
        /// Defaults to SqlServer.
        /// </summary>
        public static CompilerType DefaultCompilerType
        {
            get => _defaultCompilerType;
            set
            {
                lock (_compilerLock)
                {
                    if (value != _defaultCompilerType && value != CompilerType.Custom)
                    {
                        _customCompiler = null;
                    }

                    _defaultCompilerType = value;
                }
            }
        }

        [Obsolete("Use DefaultCompilerType instead.")]
        public static CompilerType DefaultCompiler { get => DefaultCompilerType; set => DefaultCompilerType = value; }

        /// <summary>
        /// A custom <seealso cref="Compiler"/> instance to use when one is not specified.
        /// </summary>
        public static Compiler CustomCompiler 
        { 
            get => _customCompiler; 
            set 
            {
                lock (_compilerLock)
                {
                    if (value != null)
                    {
                        _defaultCompilerType = CompilerType.Custom;
                    }

                    _customCompiler = value;
                }
            } 
        }

        /// <summary>
        /// The PetaPoco mapper used to map table and column names.
        /// Defaults to a <seealso cref="ConventionMapper"/>.
        /// </summary>
        public static IMapper DefaultMapper { get; set; } = new ConventionMapper();        

        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object, 
        /// using a <seealso cref="SqlServerCompiler"/>.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Sql ToSql(this Query query) => query.ToSql(DefaultCompilerType);

        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="compilerType"></param>
        /// <returns></returns>
        public static Sql ToSql(this Query query, CompilerType compilerType)
        {
            Compiler compiler;
            if (compilerType == CompilerType.Custom)
            {
                compiler = CustomCompiler
                    ?? throw new InvalidOperationException($"'{nameof(compilerType)}' is 'Custom' but no CustomCompiler was provided.");
            }
            else
            {
                compiler = DefaultCompilers.Get(compilerType);
            }

            return query.ToSql(compiler);
        }

        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
        /// </summary>
        /// <typeparam name="T">Type of <seealso cref="Compiler"/> to use.</typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Sql ToSql<T>(this Query query) where T : Compiler, new()
        {
            var compiler = new T();
            return query.ToSql(compiler);
        }

        /// <summary>
        /// Convert a <seealso cref="Query"/> object to a <seealso cref="Sql" /> object.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="compiler"></param>
        /// <returns></returns>
        public static Sql ToSql(this Query query, Compiler compiler)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));
            compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));

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
        public static Query ForType<T>(this Query query) => query.ForType<T>(DefaultMapper);

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query ForType<T>(this Query query, IMapper mapper) => query.ForType(typeof(T), mapper);

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type, using a default mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Query ForType(this Query query, Type type) => query.ForType(type, DefaultMapper);

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query ForType(this Query query, Type type, IMapper mapper)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));
            mapper = mapper ?? DefaultMapper ?? throw new ArgumentNullException(nameof(mapper));

            var tableInfo = mapper.GetTableInfo(type);
            return query.From(tableInfo.TableName);
        }

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given object, using a default mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="poco"></param>
        /// <returns></returns>
        public static Query ForObject(this Query query, object poco) => query.ForObject(poco, DefaultMapper);

        /// <summary>
        /// Sets the table name for the <seealso cref="Query"/> based on the <seealso cref="PocoData"/> for the given object and mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="poco"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query ForObject(this Query query, object poco, IMapper mapper) => query.ForType(poco.GetType(), mapper);


        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type, using a default mapper. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static Query GenerateSelect<T>(this Query query) => query.GenerateSelect<T>(DefaultMapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query GenerateSelect<T>(this Query query, IMapper mapper) => query.GenerateSelect(typeof(T), mapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given object, using a default mapper. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="poco"></param>
        /// <returns></returns>
        public static Query GenerateSelect(this Query query, object poco) => query.GenerateSelect(poco, DefaultMapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given object and mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="poco"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query GenerateSelect(this Query query, object poco, IMapper mapper) 
            => query.GenerateSelect(poco.GetType(), mapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type, using a default mapper. 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Query GenerateSelect(this Query query, Type type) => query.GenerateSelect(type, DefaultMapper);

        /// <summary>
        /// Generates a SELECT query based on the <seealso cref="PocoData"/> for the given type and mapper.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="type"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static Query GenerateSelect(this Query query, Type type, IMapper mapper)
        {
            query = query ?? throw new ArgumentNullException(nameof(query));

            if (!query.HasComponent("select"))
            {
                mapper = mapper ?? DefaultMapper ?? throw new ArgumentNullException(nameof(mapper));
                var pd = PocoData.ForType(type, mapper);
                query = pd.Columns.Any() ? query.Select(pd.QueryColumns) : query.SelectRaw("NULL");

                if (!query.HasComponent("from"))
                    query = query.From(pd.TableInfo.TableName);
            }

            return query;
        }
    }
}
