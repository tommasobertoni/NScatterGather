namespace NScatterGather
{
    public abstract class Invocation
    {
        public RecipientDescription Recipient { get; }

        protected Invocation(RecipientDescription recipient)
        {
            Recipient = recipient;
        }
    }
}
