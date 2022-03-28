using System.Threading.Tasks;

namespace TelegramBot
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var telegramBot = new TelegramBot();
            await telegramBot.RunBot();
        }
    }
}
