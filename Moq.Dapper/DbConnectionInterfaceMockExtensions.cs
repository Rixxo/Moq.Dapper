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
    public static class DbConnectionInterfaceMockExtensions
    {
        public static ISetup<IDbConnection, TResult> SetupDapper<TResult>(this Mock<IDbConnection> mock,
            Expression<Func<IDbConnection, TResult>> expression)
        {
            var call = expression.Body as MethodCallExpression;

            if (call?.Method.DeclaringType != typeof(SqlMapper))
                throw new ArgumentException("Not a Dapper method.");

            switch (call.Method.Name)
            {
                case nameof(SqlMapper.Execute):
                    return (ISetup<IDbConnection, TResult>) SetupExecute(mock);

                case nameof(SqlMapper.ExecuteScalar):
                    return SetupExecuteScalar<TResult>(mock);

                case nameof(SqlMapper.Query):
                case nameof(SqlMapper.QueryFirst):
                case nameof(SqlMapper.QueryFirstOrDefault):
                case nameof(SqlMapper.QuerySingle):
                case nameof(SqlMapper.QuerySingleOrDefault):
                    return SetupQuery<TResult>(mock);

                default:
                    throw new NotSupportedException();
            }
        }

        public static ISetup<IDbConnection, Task<TResult>> SetupDapperAsync<TResult>(
            this Mock<IDbConnection> mock, Expression<Func<IDbConnection, Task<TResult>>> expression)
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
                    return (ISetup<IDbConnection, Task<TResult>>) SetupExecuteAsync(mock);
                // case nameof(SqlMapper.ExecuteScalarAsync):
                //     return (ISetup<IDbConnection, Task<TResult>>)SetupExecuteScalarAsync(mock);
                default:
                    throw new NotSupportedException();
            }
        }

        static ISetup<IDbConnection, Task<TResult>> SetupQueryAsync<TResult>(Mock<IDbConnection> mock) =>
            DbCommandSetup.SetupCommandAsync<TResult, IDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .ReturnsAsync(() => result().ToDataTable(typeof(TResult))
                                                                  .ToDataTableReader());
            });

        static ISetup<IDbConnection, TResult> SetupQuery<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, getResult) =>
            {
                commandMock.Setup(command => command.ExecuteReader(It.IsAny<CommandBehavior>()))
                           .Returns(() => getResult().ToDataTable(typeof(TResult))
                                                                .ToDataTableReader());
            });

        static ISetup<IDbConnection, Task<int>> SetupExecuteAsync(Mock<IDbConnection> mock) =>
            DbCommandSetup.SetupCommandAsync<int, IDbConnection>(mock, (commandMock, result) =>
            {
                commandMock.Setup(command => command.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(result);
            });

        static ISetup<IDbConnection, TResult> SetupCommand<TResult>(Mock<IDbConnection> mock, Action<Mock<IDbCommand>, Func<TResult>> mockResult)
        {
            var setupMock = new Mock<ISetup<IDbConnection, TResult>>();
            var returnsMock = new Mock<IReturnsResult<IDbConnection>>();

            Func<TResult> getResult = null;
            Action callback = null;
            Action<string> sqlCallback = null;
            Action<string, IEnumerable<object>> sqlCallbackWithArgsValues = null;
            Action<string, IEnumerable<KeyValuePair<string, object>>> sqlCallbackWithArgsNamesAndValues = null;
            string sqlQuery = null;
            var argsValues = new List<object>();
            var argsNames = new List<string>();

            setupMock.Setup(setup => setup.Returns(It.IsAny<Func<TResult>>()))
                     .Returns(returnsMock.Object)
                     .Callback<Func<TResult>>(r => getResult = r);

            setupMock.Setup(setup => setup.Returns(It.IsAny<TResult>()))
                     .Returns(returnsMock.Object)
                     .Callback<TResult>(r => getResult = () => r);

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

            var commandMock = new Mock<IDbCommand>();

            commandMock.SetupSet(p => p.CommandText = It.IsAny<string>()).Callback<string>(s => sqlQuery = s);

            commandMock.SetupGet(a => a.Parameters)
                   .Returns(new Mock<IDataParameterCollection>().Object);

            var mockDataParameter = new Mock<IDataParameter>();
            mockDataParameter.SetupSet(p => p.ParameterName = It.IsAny<string>()).Callback<string>(name =>
            {
                argsNames.Add(name);
            });
            mockDataParameter.SetupSet(p => p.Value = It.IsAny<object>()).Callback<object>(val =>
            {
                argsValues.Add(val);
            });

            commandMock.Setup(a => a.CreateParameter())
                .Returns(mockDataParameter.As<IDbDataParameter>().Object);

            mockResult(commandMock, () =>
            {
                var result = getResult();
                callback?.Invoke();
                sqlCallback?.Invoke(sqlQuery);
                sqlCallbackWithArgsValues?.Invoke(sqlQuery, argsValues);
                sqlCallbackWithArgsNamesAndValues?.Invoke(sqlQuery,
                    argsNames.Zip(argsValues,
                        (name, value) => new KeyValuePair<string, object>(name, value)));
                return result;
            });

            mock.Setup(connection => connection.CreateCommand())
                .Returns(commandMock.Object);

            return setupMock.Object;
        }

        static ISetup<IDbConnection, TResult> SetupExecuteScalar<TResult>(Mock<IDbConnection> mock) =>
            SetupCommand<TResult>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteScalar())
                           .Returns(() => result()));

        static ISetup<IDbConnection, int> SetupExecute(Mock<IDbConnection> mock) =>
            SetupCommand<int>(mock, (commandMock, result) =>
                commandMock.Setup(command => command.ExecuteNonQuery())
                           .Returns(result));
    }
}
