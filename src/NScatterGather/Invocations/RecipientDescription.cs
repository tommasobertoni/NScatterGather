using System;

namespace NScatterGather
{
    public class RecipientDescription
    {
        public Guid Id { get; }

        public string? Name { get; }

        public Type? Type { get; }

        public Lifetime Lifetime { get; }

        public CollisionStrategy CollisionStrategy { get; }

        internal RecipientDescription(
            Guid id,
            string? name,
            Type? type,
            Lifetime lifetime,
            CollisionStrategy collisionStrategy)
        {
            Id = id;
            Name = name;
            Type = type;
            Lifetime = lifetime;
            CollisionStrategy = collisionStrategy;
        }

        public void Deconstruct(
            out Guid id,
            out string? name,
            out Type? type,
            out Lifetime lifetime,
            out CollisionStrategy collisionStrategy)
        {
            id = Id;
            name = Name;
            type = Type;
            lifetime = Lifetime;
            collisionStrategy = CollisionStrategy;
        }
    }
}
