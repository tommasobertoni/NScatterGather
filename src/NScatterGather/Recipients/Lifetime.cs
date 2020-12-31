namespace NScatterGather
{
    public enum Lifetime
    {
        /// <summary>
        /// A new instance is created each time.
        /// </summary>
        Transient,

        /// <summary>
        /// Only one instance is created for each Aggregator instance.
        /// </summary>
        Scoped,

        /// <summary>
        /// Only one instance is created for each registration.
        /// </summary>
        Singleton,
    }
}
