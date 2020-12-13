using System;

namespace NScatterGather.Recipients
{
    internal class InstanceRecipient : TypeRecipient
    {
        public static InstanceRecipient Create(
            object instance,
            string? name = null)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            return new InstanceRecipient(instance, name);
        }

        protected InstanceRecipient(object instance, string? name = null)
            : base(instance.GetType(), factory: () => instance, name)
        {
        }
    }
}
