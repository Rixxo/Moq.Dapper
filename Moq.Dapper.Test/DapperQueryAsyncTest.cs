using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperQueryAsyncTest
    {
        [Test]
        public void QueryAsyncGeneric()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryAsync<int>("").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void QueryAsyncGenericWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(
                    c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryAsync<int>("Select * From Test;").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryAsyncGenericWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void QueryAsyncGenericWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryAsyncGenericWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null,null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryAsyncGenericWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryAsync<int>("").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryAsync<int>("Select * From Test;").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterfaceWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            var expected = new[] { 7, 77, 777 };
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryAsyncGenericUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
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

            connection.SetupDapperAsync(c => c.QueryAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult().ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));
            Assert.That(actual, Is.EquivalentTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleAsync()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QuerySingleAsyncWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QuerySingleAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QuerySingleAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QuerySingleAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QuerySingleAsyncUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QuerySingleAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QuerySingleAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QuerySingleAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QuerySingleAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleOrDefaultAsync()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleOrDefaultAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleOrDefaultAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QuerySingleOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QuerySingleOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QuerySingleOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstAsync()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryFirstAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QueryFirstAsyncWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryFirstAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryFirstAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryFirstAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryFirstAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QueryFirstAsyncUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryFirstAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryFirstAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryFirstAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryFirstAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstOrDefaultAsync()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstOrDefaultAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstOrDefaultAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            var expected = 7;
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("Select * From Test;").GetAwaiter().GetResult();

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void QueryFirstOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryFirstOrDefaultAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 7;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<int>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.QueryFirstOrDefaultAsync<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();


            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void QueryAsyncGenericComplexType()
        {
            var connection = new Mock<DbConnection>();

            var expected = new[]
            {
                new ComplexType
                {
                    StringProperty = "String1",
                    IntegerProperty = 7,
                    LongProperty = 70,
                    BigIntegerProperty = 700,
                    GuidProperty = Guid.Parse("CF01F32D-A55B-4C4A-9B33-AAC1C20A85BB"),
                    DateTimeProperty = new DateTime(2000, 1, 1),
                    NullableDateTimeProperty = new DateTime(2000, 1, 1),
                    NullableIntegerProperty = 9,
                    ByteArrayPropery = new byte[] { 7 }
                },
                new ComplexType
                {
                    StringProperty = "String2",
                    IntegerProperty = 77,
                    LongProperty = 770,
                    BigIntegerProperty = 7700,
                    GuidProperty = Guid.Parse("FBECE122-6E2E-4791-B781-C30843DFE343"),
                    DateTimeProperty = new DateTime(2000, 1, 2),
                    NullableDateTimeProperty = new DateTime(2000, 1, 2),
                    NullableIntegerProperty = 99,
                    ByteArrayPropery = new byte[] { 7, 7 }
                },
                new ComplexType
                {
                    StringProperty = "String3",
                    IntegerProperty = 777,
                    LongProperty = 7770,
                    BigIntegerProperty = 77700,
                    GuidProperty = Guid.Parse("712B6DA1-71D8-4D60-8FEF-3F4800A6B04F"),
                    DateTimeProperty = new DateTime(2000, 1, 3),
                    NullableDateTimeProperty = null,
                    NullableIntegerProperty = null,
                    ByteArrayPropery = new byte[] { 7, 7, 7 }
                }
            };

            connection.SetupDapperAsync(c => c.QueryAsync<ComplexType>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .QueryAsync<ComplexType>("")
                                   .GetAwaiter()
                                   .GetResult()
                                   .ToList();

            Assert.That(actual.Count, Is.EqualTo(expected.Length));

            foreach (var complexObject in expected)
            {
                var match = actual.Where(co => co.StringProperty == complexObject.StringProperty &&
                                               co.IntegerProperty == complexObject.IntegerProperty &&
                                               co.LongProperty == complexObject.LongProperty &&
                                               co.BigIntegerProperty == complexObject.BigIntegerProperty &&
                                               co.GuidProperty == complexObject.GuidProperty &&
                                               co.DateTimeProperty == complexObject.DateTimeProperty &&
                                               co.NullableIntegerProperty == complexObject.NullableIntegerProperty &&
                                               co.NullableDateTimeProperty == complexObject.NullableDateTimeProperty &&
                                               co.ByteArrayPropery == complexObject.ByteArrayPropery);

                Assert.That(match.Count, Is.EqualTo(1));
            }
        }

       

        public class ComplexType
        {
            public BigInteger BigIntegerProperty { get; set; }
            public long LongProperty { get; set; }
            public int IntegerProperty { get; set; }
            public string StringProperty { get; set; }
            public Guid GuidProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public DateTime? NullableDateTimeProperty { get; set; }
            public int? NullableIntegerProperty { get; set; }
            public byte[] ByteArrayPropery { get; set; }
        }
    }
}