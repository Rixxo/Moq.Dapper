using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperExecuteTest
    {
        [Test]
        public void ExecuteScalar()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("");

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            const string expectedQuery = "Select * From Test;";
            string sqlCommand = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string>(sql =>
                {
                    sqlCommand = sql;
                });

            var actual = connection.Object.ExecuteScalar<int>("Select * From Test;");

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }
        
        [Test]
        public void ExecuteScalarWithParameters()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var actual = connection.Object.ExecuteScalar<int>("", new { id = 1 });

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteScalarWithParametersWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            const string expectedQuery = "Select * From Test Where id = @id;";
            string sqlCommand = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string>(sql =>
                {
                    sqlCommand = sql;
                });

            var actual = connection.Object.ExecuteScalar<int>("Select * From Test Where id = @id;", new { id = 1 });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }

        [Test]
        public void ExecuteScalarWithParametersWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.ExecuteScalar<int>("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void ExecuteScalarWithParametersWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            var expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.ExecuteScalar<int>("SELECT * FROM Test WHERE id = @Id;");

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void ExecuteScalarWithParametersWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.ExecuteScalar<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void ExecuteScalarWithParametersWithCallbackSqlQueryAndTwoArgsNamesAndValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 77;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[]
            {
                new KeyValuePair<string, string>("Id", "mockId"),
                new KeyValuePair<string, string>("Name", "mockName")
            }.ToList();
            string sqlCommand = null;
            IEnumerable<KeyValuePair<string, string>> capturedArgs = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.ExecuteScalar<int>("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void Execute()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                      .Returns(expected);

            var result = connection.Object
                                   .Execute("");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ExecuteWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "Select * From Test;";
            string sqlCommand = null;

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string>(sql =>
                {
                    sqlCommand = sql;
                });

            var result = connection.Object
                .Execute("Select * From Test;");

            Assert.That(result, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
        }

        [Test]
        public void ExecuteWithCallbackSqlQueryAndOneArg()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            const string expectedArg = "mockId";
            string sqlCommand = null;
            string capturedArg = null;

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args.First() as string;
                });

            var actual = connection.Object.Execute("SELECT * FROM Test WHERE id = @Id;", new { Id = "mockId" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArg, capturedArg);
        }

        [Test]
        public void ExecuteWithCallbackSqlQueryWithoutArgs()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id;";
            string sqlCommand = null;
            IEnumerable<object> capturedArg = null;

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArg = args;
                });

            var actual = connection.Object.Execute("SELECT * FROM Test WHERE id = @Id;");

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(Enumerable.Empty<object>(), capturedArg);
        }

        [Test]
        public void ExecuteWithCallbackSqlQueryAndTwoArgsOnlyValues()
        {
            var connection = new Mock<IDbConnection>();

            const int expected = 1;
            const string expectedQuery = "SELECT * FROM Test WHERE id = @Id AND name = @Name;";
            var expectedArgs = new[] { "mockId", "mockName" }.ToList();
            string sqlCommand = null;
            IEnumerable<string> capturedArgs = null;

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<object>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs = args.Cast<string>();
                });

            var actual = connection.Object.Execute("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }

        [Test]
        public void ExecuteCallbackSqlQueryAndTwoArgsNamesAndValues()
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

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string, IEnumerable<KeyValuePair<string, object>>>((sql, args) =>
                {
                    sqlCommand = sql;
                    capturedArgs =
                        args.Select(v => new KeyValuePair<string, string>(v.Key, v.Value as string));
                });

            var actual = connection.Object.Execute("SELECT * FROM Test WHERE id = @Id AND name = @Name;",
                new { Id = "mockId", Name = "mockName" });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, sqlCommand);
            Assert.AreEqual(expectedArgs, capturedArgs.ToList());
        }
    }
}