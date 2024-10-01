using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary1;
using MainClasses;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MainClasses;

public class HelperMethods
{
    public static bool Filter { get; set; } = false;
    public static bool Flag { get; set; } = false;
    public static bool Sort { get; set; } = false;
    public static bool SortDone { get; set; } = false;
    public static bool FilterDone { get; set; } = false;
    public static bool Receive { get; set; } = false;
    public static string FilePath { get; set; } = "";
    public static string Choice { get; set; } = "";
    public static string AdmArea { get; set; } = "";
    public static long CoverageArea = 0;
    public static bool CoverageHave { get; set; } = false;
    public static List<WifiParks> Data { get; set; } = new List<WifiParks>();
    public static List<WifiParks> Result { get; set; } = new List<WifiParks>();
    
    /// <summary>
    /// Основное меню. 
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="botClient"></param>
    public static async Task ShowMainMenu(long chatId, ITelegramBotClient botClient)
    {
        var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "1. Загрузить CSV/JSON файл" },
            new KeyboardButton[] { "2. Произвести выборку" },
            new KeyboardButton[] { "3. Отсортировать данные" }
        })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(chatId, "Выберите действие:", replyMarkup: replyKeyboardMarkup);
    }
    
    /// <summary>
    /// Реализация основных пунктов из меню.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cts"></param>
    public static async Task ChoiceMenu(ITelegramBotClient botClient, Update update, CancellationToken cts)
    {
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            // Вызов меню через /start.
            if (message.Text == "/start")
            {
                await ShowMainMenu(message.Chat.Id, botClient);
            }
            // Часть обработки файла: чтение файла.
            else if (message.Type == MessageType.Document && Filter == false && Sort == false)
            {
                var doc = message.Document;
                if (doc.FileName.EndsWith(".csv") | doc.FileName.EndsWith(".json"))
                {
                    FilePath = doc.FileName.EndsWith(".csv") ? "data.csv" : "data.json";
                    var fileId = doc.FileId;
                    var Stream = new MemoryStream();

                    await botClient.GetInfoAndDownloadFileAsync(fileId, Stream, cts);

                    try
                    {
                        if (FilePath.EndsWith(".csv"))
                        {
                            Data = CsvProcessing.Read(Stream);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл был скачан!");
                            await ShowMainMenu(message.Chat.Id, botClient);
                        }
                        else if (FilePath.EndsWith(".json"))
                        {
                            Data = JsonProcessing.Read(Stream);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Файл был скачан!");
                            await ShowMainMenu(message.Chat.Id, botClient);
                        }
                    }
                    catch
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Возникла ошибка с файлом, повторите попытку!", cancellationToken: cts);
                        await ShowMainMenu(message.Chat.Id, botClient);
                    }
                }
            }
            
            // Проверяется случай, если пользователь отправил файл не csv/json.
            else if (message.Type == MessageType.Document && !(message.Document.FileName.EndsWith(".csv") |
                                                               message.Document.FileName.EndsWith(".json")))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Полученный файл имеет не то расширение, отправьте файл с @.csv или @.json");
            }
            
            // Выгрузка и отправка файла пользователю.
            else if (Flag)
            {
                switch (message.Text)
                {
                    case "CSV":
                        using (MemoryStream stream = CsvProcessing.Write(Result))
                        {
                            stream.Position = 0;
                            await botClient.SendDocumentAsync(message.Chat.Id,
                                InputFile.FromStream(stream: stream, "DownloadedFile.csv"), caption: "Вот скачанный файл!");
                        }
                        await ShowMainMenu(message.Chat.Id, botClient);
                        Flag = false;
                        break;
                    case "JSON":
                        using (MemoryStream stream = JsonProcessing.Write(Result))
                        {
                            stream.Position = 0;
                            await botClient.SendDocumentAsync(message.Chat.Id,
                                InputFile.FromStream(stream: stream, "DownloadedFile.json"), caption: "Вот скачанный файл!");
                        }
                        await ShowMainMenu(message.Chat.Id, botClient);
                        Flag = false;
                        break;
                }
            }
            
            // Реализация меню.
            else if ((Receive == false & message.Type == MessageType.Text) ||
                     (Receive & Sort == false & Filter == false))
            {
                switch (message.Text)
                {
                    case "1. Загрузить CSV/JSON файл":
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте CSV/JSON файл на обработку",
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "2. Произвести выборку":
                        Receive = true;
                        Filter = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Выберите, по какому полю производить выборку",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "1. CoverageArea" }, new KeyboardButton[] { "2. ParkName" },
                                new KeyboardButton[] { "3. AdmArea и CoverageArea" }
                            }) { ResizeKeyboard = true });
                        break;
                    case "3. Отсортировать данные":
                        Receive = true;
                        Sort = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите, какое поле отсортировать",
                            replyMarkup: new ReplyKeyboardMarkup(new[]
                            {
                                new KeyboardButton[] { "1. Name по алфавиту" },
                                new KeyboardButton[] { "2. CoverageArea по возрастанию" }
                            }) { ResizeKeyboard = true });
                        break;
                    default:
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Нет такой команды, повторите попытку!");
                        break;
                }
            }
            
            // Реализация сортировки после загрузки файла.
            else if (Receive & Sort)
            {
                switch (message.Text)
                {
                    case "1. Name по алфавиту":
                        Result = (from park in Data orderby park.Name select park).ToList();
                        Sort = false;
                        SortDone = false;
                        Filter = false;
                        FilterDone = false;
                        Receive = false;
                        Flag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Данные были отсортированы по полю Name!");
                        await SaveQuestion(botClient, message);
                        break;
                    case "2. CoverageArea по возрастанию":
                        Result = (from park in Data orderby park.CoverageArea select park).ToList();
                        Sort = false;
                        SortDone = false;
                        Filter = false;
                        FilterDone = false;
                        Receive = false;
                        Flag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Данные были отсортированы по полю CoverageArea!");
                        await SaveQuestion(botClient, message);
                        break;
                    default:
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Нет такого поля, введите существующее поле");
                        break;
                }
            }
            
            // Реализация выборки после отправки файла.
            else if (Receive & Filter & FilterDone == false)
            {
                switch (message.Text)
                {
                    case "1. CoverageArea":
                        FilterDone = true;
                        Choice = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите значение для поля CoverageArea",
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "2. ParkName":
                        FilterDone = true;
                        Choice = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите значение для поля ParkName",
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                    case "3. AdmArea и CoverageArea":
                        FilterDone = true;
                        Choice = message.Text;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите значение для поля AdmArea",
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                    default:
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Нет такого поля, введите существующее поле");
                        break;
                }
            }
            
            // Релизация выборки по полям, кроме AdmArea и CoverageArea (одновременно)!
            else if (Receive & Filter & FilterDone & Choice != "3. AdmArea и CoverageArea")
            {
                switch (Choice)
                {
                    case "1. CoverageArea":
                        Result = (from park in Data
                            where park.CoverageArea.ToString().Contains(message.Text)
                            select park).ToList();
                        Sort = false;
                        SortDone = false;
                        Filter = false;
                        FilterDone = false;
                        Receive = false;
                        Flag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Данные были отфильтрованы по полю CoverageArea!");
                        await SaveQuestion(botClient, message);
                        break;
                    case "2. ParkName":
                        Result = (from park in Data
                            where park.CoverageArea.ToString().Contains(message.Text)
                            select park).ToList();
                        Sort = false;
                        SortDone = false;
                        Filter = false;
                        FilterDone = false;
                        Receive = false;
                        Flag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Данные были отфильтрованы по полю ParkName!");
                        await SaveQuestion(botClient, message);
                        break;
                }
            }
            
            // Реализация выборки по полям AdmArea и CoverageArea.
            else if (Receive & Filter & FilterDone & Choice == "3. AdmArea и CoverageArea" & CoverageHave == false)
            {
                AdmArea = message.Text ?? "";
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введите значение для поля CoverageArea");
                CoverageHave = true;
            }
            else if (Receive & Filter & FilterDone & Choice == "3. AdmArea и CoverageArea" & CoverageHave & long.TryParse(message.Text, out CoverageArea))
            {
                Result = (from park in Data
                    where park.CoverageArea.ToString().Contains(CoverageArea.ToString()) &
                          park.AdmArea.Contains(AdmArea)
                    select park).ToList();
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Данные были отфильтрованы по полям AdmArea и CoverageArea!");
                Receive = false;
                Filter = false;
                FilterDone = false;
                CoverageHave = false;
                SortDone = false;
                Sort = false;
                Flag = true;
                await SaveQuestion(botClient, message);
            }
            
            // Проверяется сообщение на число в связи с выборкой по поле CoverageArea.
            else if (!long.TryParse(message.Text, out CoverageArea))
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Неверный ввод, введите любое число");
            }
        }
    }

    // Спрашвиается у пользователя, в каком формате ему хочется сохранить файл.
    public async static Task SaveQuestion(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Какой файл вы хотите скачать?",
            replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton[] { "CSV", "JSON" }) { ResizeKeyboard = true });
    }
}