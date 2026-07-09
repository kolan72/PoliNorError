using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    public class PipelineErrorProcessorsAddForExceptionTests
    {
        #region AddForException(Action<TException>) Tests

        [Test]
        public void Should_AddForException_Action_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;
            InvalidOperationException captured = null;

            sut.AddForException<InvalidOperationException>(ex =>
            {
                invoked = true;
                captured = ex;
            });

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.First().Process(error);

            Assert.That(invoked, Is.True);
            Assert.That(captured, Is.SameAs(error));
        }

        [Test]
        public void Should_AddForException_Action_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new ArgumentException("wrong type");
            bool invoked = false;

            sut.AddForException<InvalidOperationException>(_ => invoked = true);

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_DoesNotHandleDerivedType()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new FormatException("derived type");
            bool invoked = false;

            sut.AddForException<ArgumentException>(_ => invoked = true);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(_ => { });

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Action<TException>, CancellationType) Tests

        [Test]
        public void Should_AddForException_Action_WithCancellationType_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;

            sut.AddForException<InvalidOperationException>(_ => invoked = true, CancellationType.Precancelable);

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.First().Process(error);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void Should_AddForException_Action_WithCancellationType_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>(_ => invoked = true, CancellationType.Precancelable);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_WithCancellationType_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(_ => { }, CancellationType.Precancelable);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Action<TException, CancellationToken>) Tests

        [Test]
        public void Should_AddForException_Action_WithCancellationToken_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;
            CaptureToken tokenValue = default;

            sut.AddForException<InvalidOperationException>((_, ct) =>
            {
                invoked = true;
                tokenValue = ct;
            });

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                sut.First().Process(error, cancellationToken: token);

                Assert.That(invoked, Is.True);
                Assert.That(tokenValue.Value, Is.EqualTo(token));
            }
        }

        [Test]
        public void Should_AddForException_Action_WithCancellationToken_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException((InvalidOperationException _, CancellationToken __) => invoked = true);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_WithCancellationToken_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException((InvalidOperationException _, CancellationToken __) => { });

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, Task>) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;
            InvalidOperationException captured = null;

            sut.AddForException<InvalidOperationException>(async ex =>
            {
                await Task.Delay(1);
                invoked = true;
                captured = ex;
            });

            Assert.That(sut.Count, Is.EqualTo(1));

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.True);
            Assert.That(captured, Is.SameAs(error));
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>(async _ =>
            {
                await Task.Delay(1);
                invoked = true;
            });

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(async _ => await Task.CompletedTask);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, Task>, CancellationType) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithCancellationType_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;

            sut.AddForException<InvalidOperationException>(async _ =>
            {
                await Task.Delay(1);
                invoked = true;
            }, CancellationType.Precancelable);

            Assert.That(sut.Count, Is.EqualTo(1));

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithCancellationType_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>(async _ =>
            {
                await Task.Delay(1);
                invoked = true;
            }, CancellationType.Precancelable);

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_WithCancellationType_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(async _ => await Task.CompletedTask, CancellationType.Precancelable);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, CancellationToken, Task>) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithCancellationToken_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            bool invoked = false;
            CaptureToken tokenValue = default;

            sut.AddForException<InvalidOperationException>(async (_, ct) =>
            {
                await Task.Delay(1);
                invoked = true;
                tokenValue = ct;
            });

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                await sut.First().ProcessAsync(error, cancellationToken: token);

                Assert.That(invoked, Is.True);
                Assert.That(tokenValue.Value, Is.EqualTo(token));
            }
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithCancellationToken_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException(async (InvalidOperationException _, CancellationToken __) =>
            {
                await Task.Delay(1);
                invoked = true;
            });

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_WithCancellationToken_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException(async (InvalidOperationException _, CancellationToken __) => await Task.CompletedTask);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Action<TException, ProcessingErrorInfo<TContext>>) Tests

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            const int contextParam = 42;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, contextParam));

            bool invoked = false;
            InvalidOperationException captured = null;
            ProcessingErrorInfo<int> capturedInfo = null;

            sut.AddForException<InvalidOperationException>((ex, info) =>
            {
                invoked = true;
                captured = ex;
                capturedInfo = info;
            });

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.First().Process(error, errorInfo);

            Assert.That(invoked, Is.True);
            Assert.That(captured, Is.SameAs(error));
            Assert.That(capturedInfo, Is.SameAs(errorInfo));
            Assert.That(capturedInfo.Param, Is.EqualTo(contextParam));
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException((InvalidOperationException _, ProcessingErrorInfo<int> __) => invoked = true);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException((InvalidOperationException _, ProcessingErrorInfo<int> __) => { });

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Action<TException, ProcessingErrorInfo<TContext>>, CancellationType) Tests

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_AndCancellationType_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            const int contextParam = 99;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Retry, new ProcessingErrorContext<int>(PolicyAlias.Retry, contextParam));

            bool invoked = false;

            sut.AddForException<InvalidOperationException>((_, __) => invoked = true, CancellationType.Precancelable);

            Assert.That(sut.Count, Is.EqualTo(1));

            sut.First().Process(error, errorInfo);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_AndCancellationType_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>((_, __) => invoked = true, CancellationType.Precancelable);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_AndCancellationType_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>((_, __) => { }, CancellationType.Precancelable);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Action<TException, ProcessingErrorInfo<TContext>, CancellationToken>) Tests

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_WithCancellationToken_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            const int contextParam = 7;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Fallback, new ProcessingErrorContext<int>(PolicyAlias.Fallback, contextParam));

            bool invoked = false;
            InvalidOperationException captured = null;
            ProcessingErrorInfo<int> capturedInfo = null;
            CaptureToken capturedToken = default;
            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                sut.AddForException<InvalidOperationException>((ex, info, ct) =>
                {
                    invoked = true;
                    captured = ex;
                    capturedInfo = info;
                    capturedToken = ct;
                });

                sut.First().Process(error, errorInfo, token);

                Assert.That(invoked, Is.True);
                Assert.That(captured, Is.SameAs(error));
                Assert.That(capturedInfo, Is.SameAs(errorInfo));
                Assert.That(capturedToken.Value, Is.EqualTo(token));
            }
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_AndCancellationToken_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>((_, __, ___) => invoked = true);

            sut.First().Process(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_Action_WithErrorInfo_AndCancellationToken_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>((_, __, ___) => { });

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, ProcessingErrorInfo<TContext>, Task>) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
			const int contextParam = 88;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Simple, new ProcessingErrorContext<int>(PolicyAlias.Simple, contextParam));

            bool invoked = false;
            InvalidOperationException captured = null;
            ProcessingErrorInfo<int> capturedInfo = null;

            sut.AddForException<InvalidOperationException>(async (ex, info) =>
            {
                await Task.Delay(1);
                invoked = true;
                captured = ex;
                capturedInfo = info;
            });

            Assert.That(sut.Count, Is.EqualTo(1));

            await sut.First().ProcessAsync(error, errorInfo);

            Assert.That(invoked, Is.True);
            Assert.That(captured, Is.SameAs(error));
            Assert.That(capturedInfo, Is.SameAs(errorInfo));
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException(
            async (InvalidOperationException _, ProcessingErrorInfo<int> __) =>
            {
                await Task.Delay(1);
                invoked = true;
            });

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_WithErrorInfo_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException(async (InvalidOperationException _, ProcessingErrorInfo<int> __) => await Task.CompletedTask);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, ProcessingErrorInfo<TContext>, Task>, CancellationType) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationType_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
			const int contextParam = 55;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Retry, new ProcessingErrorContext<int>(PolicyAlias.Retry, contextParam));

            bool invoked = false;

            sut.AddForException<InvalidOperationException>(async (_, __) =>
            {
                await Task.Delay(1);
                invoked = true;
            }, CancellationType.Precancelable);

            Assert.That(sut.Count, Is.EqualTo(1));

            await sut.First().ProcessAsync(error, errorInfo);

            Assert.That(invoked, Is.True);
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationType_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>(async (_, __) =>
            {
                await Task.Delay(1);
                invoked = true;
            }, CancellationType.Precancelable);

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationType_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(async (_, __) => await Task.CompletedTask, CancellationType.Precancelable);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region AddForException(Func<TException, ProcessingErrorInfo<TContext>, CancellationToken, Task>) Tests

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationToken_HandleMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
			const int contextParam = 33;
            ProcessingErrorInfo<int> errorInfo = new ProcessingErrorInfo<int>(PolicyAlias.Fallback, new ProcessingErrorContext<int>(PolicyAlias.Fallback, contextParam));

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;

                bool invoked = false;
                InvalidOperationException captured = null;
                ProcessingErrorInfo<int> capturedInfo = null;
                CaptureToken capturedToken = default;

                sut.AddForException<InvalidOperationException>(async (ex, info, ct) =>
                {
                    await Task.Delay(1);
                    invoked = true;
                    captured = ex;
                    capturedInfo = info;
                    capturedToken = ct;
                });

                await sut.First().ProcessAsync(error, errorInfo, cancellationToken: token);

                Assert.That(invoked, Is.True);
                Assert.That(captured, Is.SameAs(error));
                Assert.That(capturedInfo, Is.SameAs(errorInfo));
                Assert.That(capturedToken.Value, Is.EqualTo(token));
            }
        }

        [Test]
        public async Task Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationToken_NotHandleNonMatchingException()
        {
            var sut = new PipelineErrorProcessors<int>();
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var error = new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool invoked = false;

            sut.AddForException<InvalidOperationException>(async (_, __, ___) =>
            {
                await Task.Delay(1);
                invoked = true;
            });

            await sut.First().ProcessAsync(error);

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void Should_AddForException_AsyncFunc_WithErrorInfo_AndCancellationToken_ReturnSameInstance()
        {
            var sut = new PipelineErrorProcessors<int>();
            var returned = sut.AddForException<InvalidOperationException>(async (_, __, ___) => await Task.CompletedTask);

            Assert.That(returned, Is.SameAs(sut));
        }

        #endregion

        #region Fluent chaining tests

        [Test]
        public void Should_AddForException_Allow_Fluent_Chaining()
        {
            var sut = new PipelineErrorProcessors<int>();

            sut.AddForException<InvalidOperationException>(_ => { })
                .AddForException<ArgumentException>(async _ => await Task.CompletedTask)
                .AddForException((FormatException _, CancellationToken __) => { })
                .AddForException(async (TypeAccessException _, CancellationToken __) => await Task.CompletedTask);

            Assert.That(sut.Count, Is.EqualTo(4));
        }

        [Test]
        public void Should_AddForException_Multiple_Processors_On_Same_Exception_Type()
        {
            var sut = new PipelineErrorProcessors<int>();
            var error = new InvalidOperationException("test");
            int invocationCount = 0;

            sut.AddForException<InvalidOperationException>(_ => invocationCount++)
                .AddForException<InvalidOperationException>(_ => invocationCount++)
                .AddForException((InvalidOperationException _, CancellationToken __) => invocationCount++);

            Assert.That(sut.Count, Is.EqualTo(3));

            sut.First().Process(error);

            // First two processors (Action<TException>) should handle it; third (Action<TException, ProcessingErrorInfo<int>>) also handles it
            Assert.That(invocationCount, Is.GreaterThan(0));
        }

        [Test]
        public void Should_AddForException_Processor_Count_Isolated_Per_Instance()
        {
            var sut1 = new PipelineErrorProcessors<int>();
            var sut2 = new PipelineErrorProcessors<string>();

            sut1.AddForException<InvalidOperationException>(_ => { });
            sut2.AddForException<ArgumentException>(_ => { });

            Assert.That(sut1.Count, Is.EqualTo(1));
            Assert.That(sut2.Count, Is.EqualTo(1));
        }

        #endregion
    }

    internal struct CaptureToken : IEquatable<CaptureToken>
    {
        public static implicit operator CaptureToken(CancellationToken ct) => new CaptureToken { Value = ct };
        public static implicit operator CancellationToken(CaptureToken ct) => ct.Value;
        public bool Equals(CaptureToken other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is CaptureToken other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public CancellationToken Value { get; set; }
    }
}
