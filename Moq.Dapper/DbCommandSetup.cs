using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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

            Action callback = null;
            Action<string> sqlCallback = null;
            string sqlQuery = null;
            Action<string, IEnumerable<object>> sqlCallbackWithArgsValues = null;
            Action<string, IEnumerable<KeyValuePair<string, object>>> sqlCallbackWithArgsNamesAndValues = null;
            var argsValues = new List<object>();
            var argsNames = new List<string>();

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<TResult>>>()))
                .Returns(returnsMock.Object)
                .Callback<Func<Task<TResult>>>(r => result = r().Result);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action>()))
                .Callback<Action>(a => callback = a);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action<string>>()))
                .Callback<Action<string>>(a => sqlCallback = a);

            returnsMock.Setup(rm =>
                    rm.Callback(It.IsAny<Action<string, IEnumerable<object>>>()))
                .Callback<Action<string, IEnumerable<object>>>(a =>
                    sqlCallbackWithArgsValues = a);

            returnsMock.Setup(rm =>
                    rm.Callback(It.IsAny<Action<string, IEnumerable<KeyValuePair<string, object>>>>()))
                .Callback<Action<string, IEnumerable<KeyValuePair<string, object>>>>(a =>
                    sqlCallbackWithArgsNamesAndValues = a);

            var commandMock = new Mock<DbCommand>();

            commandMock.SetupAllProperties();

            commandMock.SetupSet(p => p.CommandText = It.IsAny<string>()).Callback<string>(s => sqlQuery = s);

            commandMock.Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(new Mock<DbParameterCollection>().Object);

            var mockDbParameter = new Mock<DbParameter>();
            mockDbParameter.SetupSet(p => p.ParameterName = It.IsAny<string>()).Callback<string>(name =>
            {
                argsNames.Add(name);
            });
            mockDbParameter.SetupSet(p => p.Value = It.IsAny<object>()).Callback<object>(val =>
            {
                argsValues.Add(val);
            });

            commandMock.Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(mockDbParameter.Object);

            mockResult(commandMock, () => 
            {
                callback?.Invoke();
                sqlCallback?.Invoke(sqlQuery);
                sqlCallbackWithArgsValues?.Invoke(sqlQuery, argsValues);
                sqlCallbackWithArgsNamesAndValues?.Invoke(sqlQuery,
                    argsNames.Zip(argsValues,
                        (name, value) => new KeyValuePair<string, object>(name, value)));
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