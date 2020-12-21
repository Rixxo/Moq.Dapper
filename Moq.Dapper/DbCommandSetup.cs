using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbCommandSetup
    {
        internal static ISetup<TConnection, Task<TResult>> SetupCommandAsync<TResult, TConnection>(
            Mock<TConnection> mock, Action<Mock<DbCommand>, Func<TResult>> mockResult)
            where TConnection : class, IDbConnection
        {
            var setupMock = new Mock<ISetup<TConnection, Task<TResult>>>();
            var returnsMock = new Mock<IReturnsResult<TConnection>>();

            var result = default(TResult);

            Action<string> sqlCallback = null;
            string sqlQuery = null;

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<TResult>>>()))
                .Returns(returnsMock.Object)
                .Callback<Func<Task<TResult>>>(r => result = r().Result);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action<string>>()))
                .Callback<Action<string>>(a => sqlCallback = a);

            var commandMock = new Mock<DbCommand>();

            commandMock.SetupAllProperties();

            commandMock.SetupSet(p => p.CommandText = It.IsAny<string>()).Callback<string>(s => sqlQuery = s);

            commandMock.Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(new Mock<DbParameterCollection>().Object);

            commandMock.Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(new Mock<DbParameter>().Object);

            mockResult(commandMock, () => {
                sqlCallback?.Invoke(sqlQuery);
                return result;
            });

            var iDbConnectionMock = mock.As<IDbConnection>();

            iDbConnectionMock.Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            iDbConnectionMock.SetupGet(m => m.State)
                .Returns(ConnectionState.Open);

            if (typeof(TConnection) == typeof(DbConnection))
                mock.Protected()
                    .Setup<DbCommand>("CreateDbCommand")
                    .Returns(commandMock.Object);

            return setupMock.Object;
        }
    }
}