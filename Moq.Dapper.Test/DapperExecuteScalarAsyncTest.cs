using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteScalarAsyncTest
    {
        [Test]
        public void ExecuteScalarAsyncGeneric()
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

        [Test]
        public void ExecuteScalarAsyncGenericWithCallbackSqlQuery()
        {

            var connection = new Mock<DbConnection>();

            const string expected = "Hello";
            const string expectedQuery = "Select * From Test;";
            string sqlCommand = null;

            connection.SetupDapperAsync(
                    c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql => sqlCommand = sql);

            var actual = connection.Object
                .ExecuteScalarAsync<object>("Select * From Test;")
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }

        [Test]
        public void ExecuteScalarAsyncGenericWithParameters()
        {
            var connection = new Mock<DbConnection>();
            
            const int expected = 1;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                      .ReturnsAsync(expected);

            var actual = connection.Object
                                   .ExecuteScalarAsync<object>("", new { id = 1 })
                                   .GetAwaiter()
                                   .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
        }
        [Test]
        public void ExecuteScalarAsyncGenericWithParametersWithCallbackSqlQuery()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "Select * From Test Where id = @id;";
            string sqlCommand = null;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql => sqlCommand = sql);

            var actual = connection.Object
                .ExecuteScalarAsync<object>("Select * From Test Where id = @id;", new { id = 1 })
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }

        [Test]
        public void ExecuteScalarAsyncWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.ExecuteScalarAsync<object>("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void ExecuteScalarAsyncWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.ExecuteScalarAsync<object>("SELECT * FROM Test WHERE id = @Id;").GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void ExecuteScalarAsyncWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<DbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.ExecuteScalarAsync<object>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" }).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void ExecuteScalarAsyncWithCallbackSqlQueryAndTwoArgsNamesAndValues()
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

            connection.SetupDapperAsync(c =>
                    c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.ExecuteScalarAsync<object>(
                "SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new {Id = "mockId", Name = "mockName"}).GetAwaiter().GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }
    }
}