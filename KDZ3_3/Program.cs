using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MainClasses;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static string filePath;
    private static TelegramBotClient botClient;

    static async Task Main()
    {
        try
        {
            // Оставлю на всякий случай тег бота - @CSV_KDZ_Bot.
            botClient = new TelegramBotClient("6366863055:AAGfSKWKxSCZQOGraXD5M5-tok46Vq3Nocw");
            var me = await botClient.GetMeAsync();
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            using var cts = new CancellationTokenSource();

            botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);
            Console.WriteLine($"Hello, my name is {me.FirstName}");
            Console.ReadLine();
            cts.Cancel();
        }
        catch
        {
            Console.WriteLine("Возникла ошибка!");
        }
    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
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

    async static Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        try
        {
            await HelperMethods.ChoiceMenu(botClient, update, token);
        }
        catch
        {
            Console.WriteLine("Ошибка");
        }
    }
}