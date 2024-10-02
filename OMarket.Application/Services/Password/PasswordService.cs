using OMarket.Domain.Interfaces.Application.Services.Password;

namespace OMarket.Application.Services.Password
{
    public class PasswordService : IPasswordService
    {
        private const int _workFactor = 12;

        private const BCrypt.Net.HashType _hashType = BCrypt.Net.HashType.SHA256;

        public PasswordService() { }

        public string Generate(string password) =>
            BCrypt.Net.BCrypt.EnhancedHashPassword(password, _workFactor, _hashType);

        public bool Verify(string password, string hashedPassword) =>
            BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword, _hashType);
    }
}