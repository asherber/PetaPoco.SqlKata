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

using PetaPoco.Core;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetaPoco.SqlKata
{
    public static class DefaultCompilers
    {
        private static readonly Dictionary<CompilerType, Lazy<Compiler>> _defaults = new Dictionary<CompilerType, Lazy<Compiler>>()
        {
            { CompilerType.SqlServer, new Lazy<Compiler>(() => new SqlServerCompiler()) },
            { CompilerType.MySql, new Lazy<Compiler>(() => new MySqlCompiler()) },
            { CompilerType.Postgres, new Lazy<Compiler>(() => new PostgresCompiler()) },
            { CompilerType.Firebird, new Lazy<Compiler>(() => new FirebirdCompiler()) },
            { CompilerType.SQLite, new Lazy<Compiler>(() => new SqliteCompiler()) },
            { CompilerType.Oracle, new Lazy<Compiler>(() => new OracleCompiler()) },
        };

        private static readonly Dictionary<Type, Compiler> _custom = new Dictionary<Type, Compiler>();

        internal static Compiler Get(CompilerType type)
        {
            if (_defaults.TryGetValue(type, out var result))
                return result.Value;
            else
                throw new ArgumentException($"No compiler found for type '{type}'.");
        }

        internal static bool TryGetCustom(IProvider provider, out Compiler compiler)
        {
            provider = provider ?? throw new ArgumentNullException(nameof(provider));
            return TryGetCustom(provider.GetType(), out compiler);
        }

        internal static bool TryGetCustom(Type providerType, out Compiler compiler)
        {
            providerType = providerType ?? throw new ArgumentNullException(nameof(providerType));
            return _custom.TryGetValue(providerType, out compiler);
        }

        /// <summary>
        /// Register a custom SqlKata <seealso cref="Compiler"/> for a given PetaPoco <seealso cref="IProvider"/>.
        /// This compiler will be used in place of any default for the provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compiler"></param>
        public static void RegisterFor<T>(Compiler compiler) where T : IProvider
        {
            RegisterFor(typeof(T), compiler);
        }

        /// <summary>
        /// Register a custom SqlKata <seealso cref="Compiler"/> for a given PetaPoco <seealso cref="IProvider"/>.
        /// This compiler will be used in place of any default for the provider.
        /// </summary>
        /// <param name="providerType"></param>
        /// <param name="compiler"></param>
        public static void RegisterFor(Type providerType, Compiler compiler)
        {
            if (!typeof(IProvider).IsAssignableFrom(providerType))
                throw new ArgumentException($"Type '{providerType}' does not implement IProvider.", nameof(providerType));
            
            if (compiler != null)
                _custom[providerType] = compiler;
            else if (_custom.ContainsKey(providerType))
                _custom.Remove(providerType);
        }
    }
}
