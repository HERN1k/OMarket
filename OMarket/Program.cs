using OMarket.Configurators;

namespace OMarket
{
    public class Program
    {
        public static DateTime StartupTime { get; private set; }

        public static void Main(string[] args)
        {
            ApplicationConfigurator conf = new(WebApplication.CreateBuilder(args));

            StartupTime = DateTime.UtcNow;

            conf.WebApplication.Run();
        }
    }
}