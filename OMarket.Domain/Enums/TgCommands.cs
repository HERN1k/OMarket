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
        MENUPRODUCTSLIST = 512,
        QUANTITYSELECTIONPRODUCT = 1024,
        ADDPRODUCTTOCART = 2048,
        CART = 4096,
        EDITCART = 8192,
        PRODUCTSEARCHBYNAME = 16_384,
        STARTSEARCH = 32_768,
        ENDSEARCH = 65_536,
        VIEWSEARCHPRODUCT = 131_072,
        SEARCHPRODUCTADDCART = 262_144,
        STORELOCATION = 524_288,
        SENDSTORELOCATION = 1_048_576,
        STORECONTACTS = 2_097_152,
        SENDSTORECONTACTS = 4_194_304,
        PROFILE = 8_388_608
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