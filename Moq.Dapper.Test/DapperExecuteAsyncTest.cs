using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteAsyncTest
    {
        [Test]
        public void ExecuteAsync()
        {
            var connection = new Mock<DbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                      .ReturnsAsync(1);

            var result = connection.Object
                                   .ExecuteAsync("")
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAsyncWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "Select * From Test;";
            string sqlCommand = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql => sqlCommand = sql);

            var result = connection.Object
                .ExecuteAsync("Select * From Test;")
                .GetAwaiter()
                .GetResult();

            Assert.That(result, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }

        [Test]
        public void ExecuteAsyncWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void ExecuteAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void ExecuteAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void ExecuteAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());

        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterface()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                .ReturnsAsync(1);

            var result = connection.Object
                .ExecuteAsync("")
                .GetAwaiter()
                .GetResult();

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterfaceWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();
            const string expectedQuery = "Select * From Test;";
            string sqlCommand = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                .ReturnsAsync(1)
                .Callback<string>(sql => sqlCommand = sql);

            var result = connection.Object
                .ExecuteAsync("Select * From Test;")
                .GetAwaiter()
                .GetResult();

            Assert.That(result, Is.EqualTo(1));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }
        
        [Test]
        public void ExecuteAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void ExecuteAsyncUsingDbConnectionInterfaceWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.ExecuteAsync("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }
    }
}