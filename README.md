# Moq.Dapper
Moq extensions for Dapper methods.

[![NuGet Version and Downloads count](https://buildstats.info/nuget/Moq.Dapper)](https://www.nuget.org/packages/Moq.Dapper)
[![](https://dev.azure.com/unosd/Moq.Dapper/_apis/build/status/Publish%20to%20NuGet)]()

# Example usage

Mocking a call to `Query` with a simple type:

```csharp
[Test]
public void QueryGeneric()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[] { 7, 77, 777 };

    connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
              .Returns(expected);

    var actual = connection.Object.Query<int>("", null, null, true, null, null).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
}
```

Mocking a call to `Query` with a complex type:

```csharp
[Test]
public void QueryGenericComplexType()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[]
    {
        new ComplexType { StringProperty = "String1", IntegerProperty = 7 },
        new ComplexType { StringProperty = "String2", IntegerProperty = 77 },
        new ComplexType { StringProperty = "String3", IntegerProperty = 777 }
    };

    connection.SetupDapper(c => c.Query<ComplexType>(It.IsAny<string>(), null, null, true, null, null))
              .Returns(expected);

    var actual = connection.Object.Query<ComplexType>("", null, null, true, null, null).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));

    foreach (var complexObject in expected)
    {
        var match = actual.Where(co => co.StringProperty == complexObject.StringProperty && co.IntegerProperty == complexObject.IntegerProperty);

        Assert.That(match.Count, Is.EqualTo(1));
    }
}
```

Mocking a call to `ExecuteScalar`:

```csharp
[Test]
public void ExecuteScalar()
{
    var connection = new Mock<IDbConnection>();

    const int expected = 77;

    connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
              .Returns(expected);

    var actual = connection.Object.ExecuteScalar<int>("", null, null, null);

    Assert.That(actual, Is.EqualTo(expected));
}
```

Mocking a call to `QueryAsync`

```csharp
[Test]
public async Task QueryAsyncGeneric()
{
    var connection = new Mock<DbConnection>();

    var expected = new[] { 7, 77, 777 };

    connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
              .ReturnsAsync(expected);

    var actual = (await connection.Object.QueryAsync<int>("", null, null, true, null, null)).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
}
```

Mocking a call to `ExecuteScalarAsync`:

```csharp
[Test]
public void ExecuteScalarAsync()
{
    var connection = new Mock<DbConnection>();
    
    const string expected = "Hello";

    connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
              .ReturnsAsync(expected);

    var actual = connection.Object
                           .ExecuteScalarAsync<object>("")
                           .GetAwaiter()
                           .GetResult();

    Assert.That(actual, Is.EqualTo(expected));
}
```

If you are interested in capturing the query string, you can mock the `Query` API with a simple type and callback to catpure sql request:

```csharp
[Test]
public void QueryGeneric()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[] { 7, 77, 777 };
    string expectedQuery = "Select * From Test;";
    string SqlCommand = null;

    connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
              .Returns(expected)
              .Callback<string>(sql => SqlCommand = sql);

    var actual = connection.Object.Query<int>("Select * From Test;", null, null, true, null, null).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
    Assert.AreEqual(expectedQuery, SqlCommand);
}
```

You can caputure the query string and its arguments as well by mocking the `Query` API and specifying an `IEnumerable<object>` in the callback. 
You'll, however, have to cast them to they `type` you expect them to be, in the following way:

```csharp
public void QueryGenericWithCallbackSqlQueryAndOneArg()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[] { 7, 77, 777 };
    const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
    const string expectedArg = "mockId";
    string sqlCommand = null;
    string capturedArg = null;

    connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
        .Returns(expected)
        .Callback<string, IEnumerable<object>>((sql, args) =>
        {
            sqlCommand = sql;
            capturedArg = args.First() as string;
        });

    var actual = connection.Object.Query<int>("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
    Assert.AreEqual(expectedQuery, sqlCommand);
    Assert.AreEqual(expectedArg, capturedArg);
}
```

It is also possible to caputure the query's arguments names. That requires an `IEnumerable<KeyValuePair<string, string>>` specified in the callback in the following way:

```csharp
[Test]
public void QueryGenericWithCallbackSqlQueryAndTwoArgsNamesAndValues()
{
    var connection = new Mock<IDbConnection>();

    var expected = new[] { 7, 77, 777 };
    const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
    var expectedArgs = new[]
    {
        new KeyValuePair<string, string>("Id", "mockId"),
        new KeyValuePair<string, string>("Name", "mockName")
    }.ToList();
    string sqlCommand = null;
    IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

    connection.SetupDapper(c => c.Query<int>(It.IsAny<string>(), null, null, true, null, null))
        .Returns(expected)
        .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
        {
            sqlCommand = sql;
            capturedArgs =
                args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
        });

    var actual = connection.Object.Query<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
        new { Id = "mockId", Name = "mockName" }).ToList();

    Assert.That(actual.Count, Is.EqualTo(expected.Length));
    Assert.That(actual, Is.EquivalentTo(expected));
    Assert.AreEqual(expectedQuery, sqlCommand);
    Assert.AreEqual(expectedArgs, capturedArgs.ToList());
}
```


