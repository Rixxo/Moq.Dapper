using System.Data.Common;
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
            string expectedQuery = "Select * From Test;";
            string SqlCommand = null;

            connection.SetupDapperAsync(c => c.ExecuteAsync("", null, null, null, null))
                .ReturnsAsync(1)
                .Callback<string>(sql => SqlCommand = sql);

            var result = connection.Object
                .ExecuteAsync("Select * From Test;")
                .GetAwaiter()
                .GetResult();

            Assert.That(result, Is.EqualTo(1));
            Assert.AreEqual(expectedQuery, SqlCommand);
        }
    }
}