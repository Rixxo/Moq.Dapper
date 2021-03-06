﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.Dapper
{
    public static class DbConnectionMockExtensions
    {
        public static ISetup<DbConnection, Task<TResult>> SetupDapperAsync<TResult>(
            this Mock<DbConnection> mock, Expression<Func<DbConnection, Task<TResult>>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.QueryAsync):
                case nameof(SqlMapper.QueryFirstAsync):
                case nameof(SqlMapper.QueryFirstOrDefaultAsync):
                case nameof(SqlMapper.QuerySingleAsync):
                case nameof(SqlMapper.QuerySingleOrDefaultAsync):
                    return SetupQueryAsync<TResult>(mock);
                case nameof(SqlMapper.ExecuteAsync) when typeof(TResult) == typeof(int):
                    return (ISetup<DbConnection, Task<TResult>>) SetupExecuteAsync(mock);
                case nameof(SqlMapper.ExecuteScalarAsync):
                    return (ISetup<DbConnection, Task<TResult>>) SetupExecuteScalarAsync(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        static ISetup<DbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<DbConnection> mock) =>
            DbCommandSetup.SetupCommandAsync<TResult, DbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                    .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(() => result().ToDataTable(typeof(TResult))
                        .ToDataTableReader());
            });


        static ISetup<DbConnection, Task<int>> SetupExecuteAsync(Mock<DbConnection> mock) =>
            SetupNonQueryCommandAsync(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });

        static ISetup<DbConnection, Task<int>> SetupNonQueryCommandAsync(Mock<DbConnection> mock, Action<Mock<DbCommand>, Func<int>> mockResult)
        {
            var setupMock = new Mock<ISetup<DbConnection, Task<int>>>();
            var returnsMock = new Mock<IReturnsResult<DbConnection>>();

            var result = default(int);

            Action callback = null;
            Action<string> sqlCallback = null;
            Action<string, IEnumerable<object>> sqlCallbackWithArgsValues = null;
            Action<string, IEnumerable<KeyValuePair<string, object>>> sqlCallbackWithArgsNamesAndValues = null;
            string sqlQuery = null;
            var argsValues = new List<object>();
            var argsNames = new List<string>();

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<int>>>()))
                        .Returns(returnsMock.Object)
                     .Callback<Func<Task<int>>>(r => result = r().Result);

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

            mock.As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            return setupMock.Object;
        }

        static ISetup<DbConnection, Task<object>> SetupExecuteScalarCommandAsync(Mock<DbConnection> mock, Action<Mock<DbCommand>, Func<object>> mockResult)
        {
            var setupMock = new Mock<ISetup<DbConnection, Task<object>>>();
            var returnsMock = new Mock<IReturnsResult<DbConnection>>();

            var result = default(object);

            Action callback = null;
            Action<string> sqlCallback = null;
            Action<string, IEnumerable<object>> sqlCallbackWithArgsValues = null;
            Action<string, IEnumerable<KeyValuePair<string, object>>> sqlCallbackWithArgsNamesAndValues = null;
            string sqlQuery = null;
            var argsValues = new List<object>();
            var argsNames = new List<string>();

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<Task<object>>>()))
                    .Returns(returnsMock.Object)
                     .Callback<Func<Task<object>>>(r => result = r().Result);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action>()))
                .Callback<Action>(a => callback = a);

            returnsMock.Setup(rm => rm.Callback(It.IsAny<Action<string>>()))
                .Callback<Action<string>>(a => sqlCallback = a);

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

            mock.As<IDbConnection>()
                .Setup(m => m.CreateCommand())
                .Returns(commandMock.Object);

            return setupMock.Object;
        }

        static ISetup<DbConnection, Task<object>> SetupExecuteScalarAsync(Mock<DbConnection> mock) =>
            SetupExecuteScalarCommandAsync(mock, (commandMock, result) =>
            {
                commandMock.Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(result);
            });
    }
}
