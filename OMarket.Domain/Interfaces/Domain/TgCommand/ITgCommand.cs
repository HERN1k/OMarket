namespace OMarket.Domain.Interfaces.Domain.TgCommand
{
    public interface ITgCommand
    {
        Task InvokeAsync(CancellationToken token);
    }
}