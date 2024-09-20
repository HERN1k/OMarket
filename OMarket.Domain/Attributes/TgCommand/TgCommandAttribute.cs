using OMarket.Domain.Enums;

namespace OMarket.Domain.Attributes.TgCommand
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TgCommandAttribute : Attribute
    {
        public TgCommands Command { get; init; }

        public TgCommandAttribute(TgCommands command)
        {
            Command = command;
        }
    }
}