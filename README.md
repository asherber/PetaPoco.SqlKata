![Icon](https://github.com/asherber/PetaPoco.SqlKata/raw/master/media/database-64.png)

# PetaPoco.SqlKata [![NuGet](https://img.shields.io/nuget/v/PetaPoco.SqlKata.svg)](https://nuget.org/packages/PetaPoco.SqlKata)

[PetaPoco](https://github.com/CollaboratingPlatypus/PetaPoco) is a handy micro-ORM, but the SQL builder that comes with it is extremely limited. This library lets you use [SqlKata](https://sqlkata.com) as a replacement query builder.

## Usage

```csharp
using (var db = new PetaPoco.Database(...))
{
    // Build a SqlKata query
    var query = new Query("MyTable")
        .Where("Foo", "bar");
    
    // Use the query in place of PetaPoco.Sql
    var records = db.Fetch<MyClass>(query.ToSql());
}

```

Note that while PetaPoco has an `AutoGenerateSelect` feature that lets you omit the `SELECT` part of a query if your classes are set up correctly, SqlKata requires a table name in order to generate a query. If you try to use a `Query` without a table name, SqlKata will throw an `InvalidOperationException` when you call `ToSql()`.

### Compilers

Transforming a SqlKata `Query` into a SQL string requires a compiler. SqlKata comes with compilers for SQL Server, Postgres, MySql, and Firebird. For many simple queries, the generated SQL looks the same regardless of which compiler you use, but for certain queries the compiler will produce SQL tailored for that specific database.

By default, this library uses the SQL Server compiler. If you want to use a different compiler, there are a couple of different ways you can do so.

```csharp
// Specify the compiler for one SQL statement
var sql = query.ToSql(CompilerType.MySql);

// Change the default compiler for all SQL statements
SqlKataExtensions.DefaultCompiler = CompilerType.Postgres;
```

