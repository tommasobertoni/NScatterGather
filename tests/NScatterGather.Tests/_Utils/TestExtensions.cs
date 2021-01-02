using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Collection.Scope;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather
{
    internal static class TestExtensions
    {
        // Helper method for adding a new TypeRecipient to a RecipientsScope.
        public static void AddTypeRecipient<TRecipients>(this RecipientsScope scope)
            where TRecipients : new()
        {
            scope.AddRange(new[]
            {
                TypeRecipient.Create(
                    new TypeInspectorRegistry(),
                    () => new TRecipients(),
                    name: null,
                    Lifetime.Transient,
                    IgnoreRecipient)
            });
        }
    }
}
