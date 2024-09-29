namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IAdminsRepository
    {
        Task<Guid?> GetAdmin(long id, CancellationToken token);
    }
}