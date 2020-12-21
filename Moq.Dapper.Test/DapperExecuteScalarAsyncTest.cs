using System.Data.Common;
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
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(
                    c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql => SqlCommand = sql);

            var actual = connection.Object
                .ExecuteScalarAsync<object>("Select * From Test;")
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
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
            string expectedQuery = "Select * From Test Where id = @id;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.ExecuteScalarAsync<object>(It.IsAny<string>(), null, null, null, null))
                .ReturnsAsync(expected)
                .Callback<string>(sql => SqlCommand = sql);

            var actual = connection.Object
                .ExecuteScalarAsync<object>("Select * From Test Where id = @id;", new { id = 1 })
                .GetAwaiter()
                .GetResult();

            Assert.That(actual, Is.EqualTo(expected));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }


    }
}