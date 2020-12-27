namespace NScatterGather.Recipients.Factories
{
    internal interface IRecipientFactory
    {
        object Get();

        IRecipientFactory Clone();
    }
}
