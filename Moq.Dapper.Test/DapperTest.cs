using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NUnit.Framework;

namespace Moq.Dapper.Test
{
    [TestFixture]
    public class DapperTest
    {
        [Test]
        public void NonDapperMethodException()
        {
            var connection = new Mock<IDbConnection>();

            object BeginTransaction() => connection.SetupDapper(c => c.BeginTransaction());

            Assert.That(BeginTransaction, Throws.ArgumentException);
        }

        [Test]
        public void Deferred()
        {
            var connection = new Mock<IDbConnection>();

            int[] expected = { 15 };

            connection.SetupDapper(x => x.Query<int>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(() => expected);

            var actual = connection.Object.Query<int>("");

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void Callback()
        {
            var connection = new Mock<IDbConnection>();

            int[] firstExpected = { 15 };
            int[] secondExpected = { 20 };

            IEnumerable<int> expected = firstExpected;

            connection.SetupDapper(x => x.Query<int>(It.IsAny<string>(), null, null, true, null, null))
                      .Returns(() => expected)
                      .Callback(() => expected = secondExpected);

            var firstActual = connection.Object.Query<int>("");
            Assert.That(firstActual, Is.EquivalentTo(firstExpected));

            var secondActual = connection.Object.Query<int>("");
            Assert.That(secondActual, Is.EquivalentTo(secondExpected));
        }


        [Test]
        public void CallbackSqlQuery()
        {
            var connection = new Mock<IDbConnection>();

            int[] firstExpected = { 15 };
            string expectedQuery = "Select * From Test;";

            IEnumerable<int> expected = firstExpected;
            string SqlCommand = null;

            connection.SetupDapper(x => x.Query<int>(It.IsAny<string>(), null, null, true, null, null))
                .Returns(() => expected)
                .Callback<string>(sql => SqlCommand = sql);

            var firstActual = connection.Object.Query<int>("Select * From Test;");
            Assert.That(firstActual, Is.EquivalentTo(firstExpected));

            Assert.AreEqual(expectedQuery, SqlCommand);
        }
    }
}