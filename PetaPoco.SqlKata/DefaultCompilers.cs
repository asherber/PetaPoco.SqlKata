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

using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetaPoco.SqlKata
{
    internal static class DefaultCompilers
    {
        private static readonly Dictionary<CompilerType, Lazy<Compiler>> _compilers = new Dictionary<CompilerType, Lazy<Compiler>>()
        {
            { CompilerType.SqlServer, new Lazy<Compiler>(() => new SqlServerCompiler()) },
            { CompilerType.MySql, new Lazy<Compiler>(() => new MySqlCompiler()) },
            { CompilerType.Postgres, new Lazy<Compiler>(() => new PostgresCompiler()) },
            { CompilerType.Firebird, new Lazy<Compiler>(() => new FirebirdCompiler()) },
            { CompilerType.SQLite, new Lazy<Compiler>(() => new SqliteCompiler()) },
            { CompilerType.Oracle, new Lazy<Compiler>(() => new OracleCompiler()) },
        };

        public static Compiler Get(CompilerType type)
        {
            if (_compilers.TryGetValue(type, out var result))
                return result.Value;
            else
                throw new ArgumentException($"No compiler found for type '{type}'.");
        }
    }
}
