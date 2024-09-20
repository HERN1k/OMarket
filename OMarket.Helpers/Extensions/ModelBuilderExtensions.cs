using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OMarket.Helpers.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static PropertyBuilder HasMinLength(this PropertyBuilder propertyBuilder, int minLength) =>
            propertyBuilder.HasAnnotation("MinLength", minLength);
    }
}