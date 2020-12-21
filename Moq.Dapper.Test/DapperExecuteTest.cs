using System.Data;
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
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.ExecuteScalar<int>("Select * From Test;");

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
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
            string expectedQuery = "Select * From Test Where id = @id;";
            string SqlCommand = null;

            connection.SetupDapper(c => c.ExecuteScalar<int>(It.IsAny<string>(), null, null, null, null))
                .Returns(expected)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var actual = connection.Object.ExecuteScalar<int>("Select * From Test Where id = @id;", new { id = 1 });

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }

        [Test]
        public void Execute()
        {
            var connection = new Mock<IDbConnection>();

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                      .Returns(1);

            var result = connection.Object
                                   .Execute("");

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteWithCallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapper(c => c.Execute(It.IsAny<string>(), null, null, null, null))
                .Returns(1)
                .Callback<string>(sql =>
                {
                    SqlCommand = sql;
                });

            var result = connection.Object
                .Execute("Select * From Test;");

            Assert.That(result, Is.EqualTo(1));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }
    }
}