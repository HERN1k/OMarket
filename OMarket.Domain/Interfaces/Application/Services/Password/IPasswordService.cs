namespace OMarket.Domain.Interfaces.Application.Services.Password
{
    public interface IPasswordService
    {
        string Generate(string password);

        bool Verify(string password, string hashedPassword);
    }
}