using Medo;

namespace OMarket.Domain.Interfaces.Domain.Entities
{
    public interface IEntity
    {
        Guid Id { get; init; }

        public static Guid CreateUuidV7ToGuid() =>
            Uuid7.NewUuid7().ToGuid(matchGuidEndianness: true);
    }
}