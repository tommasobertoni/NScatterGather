using System;
using System.Threading.Tasks;
using NScatterGather.Inspection;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Recipients
{
    public class TypeRecipientTests
    {
        [Fact]
        public void Recipient_can_be_created_from_type()
        {
            _ = TypeRecipient.Create(
                registry: new TypeInspectorRegistry(),
                () => new SomeType(),
                name: null,
                lifetime: Lifetime.Transient,
                IgnoreRecipient);
        }

        [Fact]
        public void Error_if_registry_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = TypeRecipient.Create(
                    registry: (null as TypeInspectorRegistry)!,
                    () => new SomeType(),
                    name: null,
                    lifetime: Lifetime.Transient,
                    IgnoreRecipient);
            });
        }

        [Fact]
        public void Error_if_no_factory_method()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = TypeRecipient.Create(
                    registry: new TypeInspectorRegistry(),
                    (null as Func<SomeType>)!,
                    name: null,
                    lifetime: Lifetime.Transient,
                    IgnoreRecipient);
            });
        }

        [Fact]
        public void Error_if_invalid_lifetime()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ = TypeRecipient.Create(
                    registry: new TypeInspectorRegistry(),
                    () => new SomeType(),
                    name: null,
                    lifetime: (Lifetime)42,
                    IgnoreRecipient);
            });
        }

        [Fact]
        public void Recipient_has_a_name()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: "My name is", Lifetime.Transient, IgnoreRecipient);

            Assert.NotNull(recipient.Name);
            Assert.NotEmpty(recipient.Name);
        }

        [Fact]
        public async Task Recipient_accepts_request()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var input = 42;
            var runners = recipient.Accept(input);
            var runner = runners[0];
            await runner.Start();

            Assert.Equal(input.ToString(), runner.Result);
        }

        [Fact]
        public async Task Recipient_accepts_request_and_replies_with_task()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeAsyncType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var input = 42;
            var runners = recipient.Accept(input);
            var runner = runners[0];
            await runner.Start();

            Assert.Equal(input.ToString(), runner.Result);
        }

        [Fact]
        public void Recipient_accepts_input_parameter_type()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            bool canAccept = recipient.CanAccept(typeof(int));

            Assert.True(canAccept);
        }

        [Fact]
        public void Recipient_type_is_visible()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Same(typeof(SomeType), recipient.Type);
        }

        [Fact]
        public void Recipient_replies_with_response_type()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            bool canAccept = recipient.CanReplyWith(typeof(int), typeof(string));

            Assert.True(canAccept);
        }

        [Fact]
        public void Recipient_replies_with_async_response_type()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeAsyncType(), name: null, Lifetime.Transient, IgnoreRecipient);

            bool canAccept = recipient.CanReplyWith(typeof(int), typeof(string));

            Assert.True(canAccept);
        }

        [Fact]
        public void Error_if_request_or_response_types_are_null()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(typeof(int), null!));
            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(null!, typeof(string)));
        }

        [Fact]
        public void Error_if_request_type_not_supported()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Throws<InvalidOperationException>(() => recipient.Accept(Guid.NewGuid()));
        }

        [Fact]
        public void Error_if_response_type_not_supported()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Throws<InvalidOperationException>(() => recipient.ReplyWith<Guid>(42));
        }

        [Fact]
        public async Task Recipient_replies_with_response()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var input = 42;
            var runners = recipient.ReplyWith<string>(input);
            var runner = runners[0];
            await runner.Start();

            Assert.Equal(input.ToString(), runner.Result);
        }

        [Fact]
        public async Task Recipient_can_reply_with_task()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeAsyncType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var input = 42;
            var runners = recipient.ReplyWith<string>(input);
            var runner = runners[0];
            await runner.Start();

            Assert.Equal(input.ToString(), runner.Result);
        }

        [Fact]
        public void Recipient_must_return_something()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeComputingType(), name: null, Lifetime.Transient, IgnoreRecipient);

            bool accepts = recipient.CanAccept(typeof(int));

            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_void_returning()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeComputingType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Throws<InvalidOperationException>(() => recipient.Accept(42));
        }

        [Fact]
        public void Recipient_must_return_something_async()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeAsyncComputingType(), name: null, Lifetime.Transient, IgnoreRecipient);

            bool accepts = recipient.CanReplyWith(typeof(int), typeof(Task));
            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_returning_task_without_result()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeAsyncComputingType(), name: null, Lifetime.Transient, IgnoreRecipient);

            Assert.Throws<InvalidOperationException>(() => recipient.ReplyWith<Task>(42));
        }

        [Fact]
        public void Can_be_cloned()
        {
            var registry = new TypeInspectorRegistry();

            var recipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var clone = recipient.Clone();

            Assert.NotNull(clone);
            Assert.IsType<TypeRecipient>(clone);
            Assert.Equal(recipient.Name, clone.Name);
            Assert.Equal(recipient.Lifetime, clone.Lifetime);
            Assert.Equal(recipient.Type, (clone as TypeRecipient)!.Type);
        }

        [Fact]
        public void Has_expected_lifetime()
        {
            var registry = new TypeInspectorRegistry();

            var transientRecipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Transient, IgnoreRecipient);

            var scopedRecipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Scoped, IgnoreRecipient);

            var singletonRecipient = TypeRecipient.Create(
                registry, () => new SomeType(), name: null, Lifetime.Singleton, IgnoreRecipient);

            Assert.Equal(Lifetime.Transient, transientRecipient.Lifetime);
            Assert.Equal(Lifetime.Scoped, scopedRecipient.Lifetime);
            Assert.Equal(Lifetime.Singleton, singletonRecipient.Lifetime);
        }
    }
}
