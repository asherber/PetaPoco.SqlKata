![Icon](https://github.com/asherber/PetaPoco.SqlKata/raw/master/media/database-64.png)

# PetaPoco.SqlKata [![NuGet](https://img.shields.io/nuget/v/PetaPoco.SqlKata.svg)](https://nuget.org/packages/PetaPoco.SqlKata)

[PetaPoco](https://github.com/CollaboratingPlatypus/PetaPoco) is a handy micro-ORM, but the SQL builder that comes with it is extremely limited. This library lets you use [SqlKata](https://sqlkata.com) as a replacement query builder.

## Usage

### Basic

```csharp
using (var db = new PetaPoco.Database(...))
{
    // Build any SqlKata query
    var query = new Query("MyTable")
        .Where("Foo", "bar");
    
    // Use the query in place of PetaPoco.Sql
    var records = db.Fetch<MyClass>(query.ToSql());
}

```

Note that while PetaPoco has an `EnableAutoSelect` feature that lets you omit the `SELECT` part of a query if your classes are set up correctly, SqlKata requires a table name in order to generate a query. If you try to use a `Query` without a table name, SqlKata will throw an `InvalidOperationException` when you call `ToSql()`.

### Generate from POCO

Since part of the benefit of PetaPoco is that it understands information embedded in a POCO, this library also has two extension methods to help do the same thing, letting you avoid retyping table and column names.

```csharp
public class MyClass
{
    property int ID { get; set; }
    [Column("NAME_FIELD")]
    property string Name { get; set; }
}

// These are all equivalent to new Query("MyClass")
// If the class has a TableName property, that will be used instead.
var query = new Query().ForType<MyClass>();
var query = new Query().ForType(typeof(MyClass));
var query = new Query().ForObject(new MyClass());

// SELECT [ID], [NAME_FIELD] FROM [MyClass]
var query = new Query().GenerateSelect<MyClass>();  
var query = new Query().GenerateSelect(typeof(MyClass));
var query = new Query().GenerateSelect(new MyClass());

```

These methods all use a default `ConventionMapper`. They also have overloads that let you pass in your own `IMapper` instance, or you can assign one to `SqlKataExtensions.DefaultMapper`.

### Compilers

Transforming a SqlKata `Query` into a SQL string requires a compiler. SqlKata comes with compilers for SQL Server, Postgres, MySql, Firebird, and SQLite. For many simple queries, the generated SQL looks the same regardless of which compiler you use, but for certain queries the compiler will produce SQL tailored for that specific database. The compilers also know which characters to use to escape identifiers.

By default, this library uses the SQL Server compiler. If you want to use a different compiler, there are a couple of different ways you can do so.

```csharp
// Specify the compiler for one SQL statement
var sql = query.ToSql(CompilerType.MySql);

// Change the default compiler for all SQL statements
SqlKataExtensions.DefaultCompiler = CompilerType.Postgres;
```

