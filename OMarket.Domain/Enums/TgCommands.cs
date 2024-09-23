namespace OMarket.Domain.Enums
{
    public enum TgCommands
    {
        NONE = 0,
        DEV = 2,
        START = 4,
        SAVECONTACT = 8,
        SAVESTOREADDRESS = 16,
        MAINMENU = 32,
        MENUPRODUCTTYPES = 64,
        MENUPRODUCTUNDERTYPE = 128,
        UPDATESTOREADDRESS = 256,
        MENUPRODUCTSLIST = 512
    }

    public static class TgCommandExtensions
    {
        public static TgCommands GetTelegramCommand(string command)
        {
            if (int.TryParse(command, out int number))
            {
                if (Enum.IsDefined(typeof(TgCommands), number))
                {
                    return (TgCommands)number;
                }
                else
                {
                    return TgCommands.NONE;
                }
            }

            if (Enum.TryParse(command, out TgCommands result))
            {
                return result;
            }
            else
            {
                return TgCommands.NONE;
            }
        }
    }
}