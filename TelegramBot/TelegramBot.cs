using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    internal class TelegramBot
    {
        private TelegramBotClient botClient { get; set; }
        private CancellationTokenSource cts { get; set; }

        public TelegramBot()
        {
            botClient = new TelegramBotClient("Token");
            cts = new CancellationTokenSource();
        }

        public async void SendMessage(string messageText, long chatId)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: messageText,
                cancellationToken: cts.Token);
        }

        public async Task RunBot()
        {
            var api = new Api();
            api.RunAutomatically();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

            var me = await botClient.GetMeAsync();

            await Task.Delay(TimeSpan.FromMilliseconds(-1));

            cts.Cancel();

            Console.WriteLine(me.FirstName);

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Type != UpdateType.Message) return;

                if (update.Message!.Type != MessageType.Text) return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                Console.WriteLine("Received a new message!");
                switch (messageText)
                {
                    case "/start":
                        SendMessage(messageText: "УПРАВЛЕНИЕ:\n" +
                            "Show - Показать используемую для отслеживания запись в базе данных\n" +
                            "Delete - Удалить текущую запись из базы данных\n" +
                            "ФОРМАТ ВВОДА:\n" +
                            "Координаты вводятся через запятую, точки начала и конца разделены пробелом. " +
                            "Все маршруты отправляются одним сообщением, где пара КОНЕЦ-НАЧАЛО введены через пробел\n" +
                            "Пример ввода:\n" +
                            "55.725625,37.647208 55.724231,37.652903\n" +
                            "55.725625,37.647208 55.717736,37.633464",
                            chatId: chatId);
                        break;

                    case "Delete":
                        try
                        {
                            DbHandler.Delete(chatId);
                            SendMessage("Запись удалена", chatId);
                        }
                        catch (Exception e)
                        {
                            SendMessage(e.Message, chatId);
                        }
                        break;

                    case "Get now":
                        try
                        {
                            await api.PrivateHandler(chatId);
                        }
                        catch (Exception e)
                        {
                            SendMessage(e.Message, chatId);
                        }
                        break;

                    case "Check":
                        SendMessage("Ok!", chatId);
                        break;

                    default:
                        DbHandler.Save(chatId, messageText);
                        break;
                }
            }

            Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
        }
    }
}
