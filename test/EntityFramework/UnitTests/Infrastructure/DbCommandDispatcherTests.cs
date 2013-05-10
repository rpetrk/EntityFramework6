﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbCommandDispatcherTests
    {
        public class NonQuery : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, 11, interceptionContext)).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.NonQueryExecuted(mockCommand.Object, 11, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.NonQuery(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteNonQuery());
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, 0, It.Is<DbCommandInterceptionContext>(c => c.Exception == exception)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteNonQuery()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().NonQuery(mockCommand.Object, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteNonQuery());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().NonQuery(new Mock<DbCommand>().Object, null)).ParamName);
            }
        }

        public class Scalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.ScalarExecuted(mockCommand.Object, 11, interceptionContext)).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Equal(13, dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(m => m.ScalarExecuted(mockCommand.Object, 11, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.Scalar(mockCommand.Object, interceptionContext));

                mockCommand.Verify(m => m.ExecuteScalar());
                mockInterceptor.Verify(m => m.ScalarExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(c => c.Exception == exception)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Setup(m => m.ExecuteScalar()).Returns(11);

                Assert.Equal(11, new DbCommandDispatcher().Scalar(mockCommand.Object, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteScalar());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Scalar(new Mock<DbCommand>().Object, null)).ParamName);
            }
        }

        public class Reader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var reader = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Returns(reader);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, reader, interceptionContext))
                               .Returns(interceptReader);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                Assert.Same(interceptReader, dispatcher.Reader(mockCommand.Object, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, reader, interceptionContext));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Throws(new Exception("Bang!"));

                var mockInterceptor = new Mock<IDbCommandInterceptor>();

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var exception = Assert.Throws<Exception>(() => dispatcher.Reader(mockCommand.Object, interceptionContext));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
                mockInterceptor.Verify(m => m.ReaderExecuting(mockCommand.Object, interceptionContext));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(c => c.Exception == exception)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var reader = new Mock<DbDataReader>().Object;
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.SequentialAccess)
                           .Returns(reader);

                Assert.Same(
                    reader,
                    new DbCommandDispatcher().Reader(
                        mockCommand.Object,
                        new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess)));

                mockCommand.Protected().Verify("ExecuteDbDataReader", Times.Once(), CommandBehavior.SequentialAccess);
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(null, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().Reader(new Mock<DbCommand>().Object, null)).ParamName);
            }
        }

#if !NET40
        public class AsyncNonQuery : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11);

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, 11, It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncNonQuery(mockCommand.Object, cancellationToken, new DbCommandInterceptionContext());
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, 11, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, 0, It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncNonQuery(mockCommand.Object, CancellationToken.None, new DbCommandInterceptionContext());
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
                mockInterceptor.Verify(m => m.NonQueryExecuting(mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, 0, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Faulted) && c.Exception.Message == "Bang!")));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_is_canceled()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<int>(() => 11, cancellationToken);

                mockCommand.Setup(m => m.ExecuteNonQueryAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(m => m.NonQueryExecuted(mockCommand.Object, 0, It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncNonQuery(mockCommand.Object, cancellationToken, new DbCommandInterceptionContext());
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.NonQueryExecuted(
                        mockCommand.Object, 0, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Canceled) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = Task.FromResult(11);
                mockCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncNonQuery(mockCommand.Object, CancellationToken.None, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncNonQuery(null, CancellationToken.None, new DbCommandInterceptionContext()))
                          .ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncNonQuery(new Mock<DbCommand>().Object, CancellationToken.None, null)).ParamName);
            }
        }

        public class AsyncScalar : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext();
                var cancellationToken = new CancellationToken();

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11);

                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(
                        mockCommand.Object, 11, It.IsAny<DbCommandInterceptionContext>())).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncScalar(mockCommand.Object, cancellationToken, interceptionContext);
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Equal(13, interceptResult.Result);
                Assert.Equal(13, awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, 11, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => { throw new Exception("Bang!"); });

                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<object>(), It.IsAny<DbCommandInterceptionContext>())).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncScalar(mockCommand.Object, CancellationToken.None, new DbCommandInterceptionContext());
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Faulted) && c.Exception.Message == "Bang!")));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_canceled()
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var mockCommand = new Mock<DbCommand>();
                var result = new Task<object>(() => 11, cancellationToken);

                mockCommand.Setup(m => m.ExecuteScalarAsync(cancellationToken)).Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                mockInterceptor.Setup(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<object>(), It.IsAny<DbCommandInterceptionContext>())).Returns(13);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncScalar(mockCommand.Object, cancellationToken, new DbCommandInterceptionContext());
                Assert.NotSame(result, interceptResult);

                mockCommand.Verify(m => m.ExecuteScalarAsync(cancellationToken));
                mockInterceptor.Verify(
                    m => m.ScalarExecuting(
                        mockCommand.Object, It.Is<DbCommandInterceptionContext>(c => c.IsAsync)));
                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, It.IsAny<int>(), It.IsAny<DbCommandInterceptionContext>()), Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ScalarExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync && c.TaskStatus.HasFlag(TaskStatus.Canceled) && c.Exception == null)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var mockCommand = new Mock<DbCommand>();
                var result = Task.FromResult<object>(11);
                mockCommand.Setup(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>())).Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncScalar(mockCommand.Object, CancellationToken.None, new DbCommandInterceptionContext()));

                mockCommand.Verify(m => m.ExecuteScalarAsync(It.IsAny<CancellationToken>()));
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncScalar(null, CancellationToken.None, new DbCommandInterceptionContext()))
                          .ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => new DbCommandDispatcher().AsyncScalar(new Mock<DbCommand>().Object, CancellationToken.None, null)).ParamName);
            }
        }

        public class AsyncReader : TestBase
        {
            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_which_can_change_result()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);
                var cancellationToken = new CancellationToken();

                var originalReader = new Mock<DbDataReader>().Object;
                var result = new Task<DbDataReader>(() => originalReader);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, originalReader, It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(interceptReader);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult =
                    dispatcher.AsyncReader(mockCommand.Object, cancellationToken, interceptionContext);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbDataReader>(), It.IsAny<DbCommandInterceptionContext>()),
                    Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);
                awaited.Wait();

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Same(interceptReader, interceptResult.Result);
                Assert.Same(interceptReader, awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, originalReader, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.RanToCompletion)
                                 && c.Exception == null
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_throws()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);

                var result = new Task<DbDataReader>(() => { throw new Exception("Bang!"); });

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>())
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbDataReader>(), It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(interceptReader);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncReader(mockCommand.Object, CancellationToken.None, interceptionContext);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, It.IsAny<CancellationToken>());

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbDataReader>(), It.IsAny<DbCommandInterceptionContext>()),
                    Times.Never());

                result.Start();
                var awaited = AwaitMe(interceptResult);

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.False(interceptResult.IsCanceled);
                Assert.False(awaited.IsCanceled);
                Assert.True(interceptResult.IsFaulted);
                Assert.True(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.Faulted)
                                 && c.Exception.Message == "Bang!"
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess)));
            }

            [Fact]
            public void Dispatch_method_executes_command_and_dispatches_to_interceptors_even_if_operation_is_canceled()
            {
                var interceptionContext = new DbCommandInterceptionContext().WithCommandBehavior(CommandBehavior.SequentialAccess);
                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;

                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object, cancellationToken);

                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", CommandBehavior.SequentialAccess, cancellationToken)
                           .Returns(result);

                var mockInterceptor = new Mock<IDbCommandInterceptor>();
                var interceptReader = new Mock<DbDataReader>().Object;
                mockInterceptor.Setup(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbDataReader>(), It.IsAny<DbCommandInterceptionContext>()))
                               .Returns(interceptReader);

                var dispatcher = new DbCommandDispatcher();
                var internalDispatcher = dispatcher.InternalDispatcher;
                internalDispatcher.Add(mockInterceptor.Object);

                var interceptResult = dispatcher.AsyncReader(mockCommand.Object, cancellationToken, interceptionContext);
                Assert.NotSame(result, interceptResult);

                mockCommand.Protected()
                           .Verify("ExecuteDbDataReaderAsync", Times.Once(), CommandBehavior.SequentialAccess, cancellationToken);

                mockInterceptor.Verify(
                    m => m.ReaderExecuting(
                        mockCommand.Object,
                        It.Is<DbCommandInterceptionContext>(c => c.IsAsync && c.CommandBehavior == CommandBehavior.SequentialAccess)));
                mockInterceptor.Verify(
                    m => m.ReaderExecuted(mockCommand.Object, It.IsAny<DbDataReader>(), It.IsAny<DbCommandInterceptionContext>()),
                    Times.Never());

                var awaited = AwaitMe(interceptResult);

                cancellationTokenSource.Cancel();

                Assert.Throws<AggregateException>(() => awaited.Wait());

                Assert.True(interceptResult.IsCanceled);
                Assert.True(awaited.IsCanceled);
                Assert.False(interceptResult.IsFaulted);
                Assert.False(awaited.IsFaulted);

                Assert.Throws<AggregateException>(() => interceptResult.Result);
                Assert.Throws<AggregateException>(() => awaited.Result);

                mockInterceptor.Verify(
                    m => m.ReaderExecuted(
                        mockCommand.Object, null, It.Is<DbCommandInterceptionContext>(
                            c => c.IsAsync
                                 && c.TaskStatus.HasFlag(TaskStatus.Canceled)
                                 && c.Exception == null
                                 && c.CommandBehavior == CommandBehavior.SequentialAccess)));
            }

            [Fact]
            public void Dispatch_method_executes_command_even_if_no_interceptors_are_registered()
            {
                var result = new Task<DbDataReader>(() => new Mock<DbDataReader>().Object);
                var mockCommand = new Mock<DbCommand>();
                mockCommand.Protected()
                           .Setup<Task<DbDataReader>>(
                               "ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                           .Returns(result);

                Assert.Same(
                    result,
                    new DbCommandDispatcher().AsyncReader(
                        mockCommand.Object, CancellationToken.None, new DbCommandInterceptionContext()));

                mockCommand.Protected()
                           .Verify(
                               "ExecuteDbDataReaderAsync", Times.Once(), ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>());
            }

            [Fact]
            public void Dispatch_method_checks_for_nulls()
            {
                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().AsyncReader(
                            null, CancellationToken.None, new DbCommandInterceptionContext())).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        new DbCommandDispatcher().AsyncReader(
                            new Mock<DbCommand>().Object, CancellationToken.None, null)).ParamName);
            }
        }

        private static async Task<T> AwaitMe<T>(Task<T> task)
        {
            return await task;
        }
#endif
    }
}