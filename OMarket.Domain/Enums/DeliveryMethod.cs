namespace OMarket.Domain.Enums
{
    public enum DeliveryMethod
    {
        NONE = 0,
        DELIVERY = 2,
        SELFPICKUP = 4
    }

    public static class DeliveryMethodExtensions
    {
        public static DeliveryMethod GetDeliveryMethod(string method)
        {
            if (Enum.TryParse(method, out DeliveryMethod result))
            {
                return result;
            }
            else
            {
                return DeliveryMethod.NONE;
            }
        }
    }
}