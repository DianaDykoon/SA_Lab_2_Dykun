﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ConsoleBot
{
    public class BotService
    {
        TelegramBotClient botClient = new TelegramBotClient("6622796762:AAHQvOC1UhpobDFga4Js-JHnQVjSzzdCHVI");
        List<Stock> stocks = new List<Stock>();
        Dictionary<long, List<Stock>> userSavedStockProducts = new Dictionary<long, List<Stock>>();

        Stock testStock = new Stock("Holika Holika",
        "BB-крем з перлинною пудрою для надання сатинового сяйва" +
        " Holika Holika Shimmering Petit BB Cream SPF45 / PA +++, 30 мл", 257.00f, 204.00f, "Eva", "https://eva.ua/ua/pr105252/",
        "https://github.com/DianaDykoon/Lab_7_Dykun/raw/master/images/HolikaPetitBB.jpg");


        Stock testStock2 = new Stock("Maybelline New York",
            "Туш для вій Maybelline New York Volum' Express Classic, екстра чорна, 10 мл",
            189.00f, 151.00f, "makeup", "https://makeup.com.ua/ua/product/524823/",
            "https://github.com/DianaDykoon/Lab_7_Dykun/raw/master/images/MaybelineNewYorkMascara.jpg");

        public BotService()
        {
            botClient.StartReceiving(OnUpdate, OnError);
            stocks.Add(testStock);
            stocks.Add(testStock2);
        }

        private Task OnError(ITelegramBotClient client, Exception exception, CancellationToken token)
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

        private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken token)
        {
            var message = update.Message;
            var messageText = message?.Text;

            long chatId;
            if (messageText != null)
                chatId = message!.Chat.Id;
            else
                chatId = update.CallbackQuery!.Message!.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message?.Chat.FirstName}.");

            if (!userSavedStockProducts.ContainsKey(chatId))
            {
                // Створення списку збережених акційних товарів
                // Для нового користувача
                userSavedStockProducts[chatId] = new List<Stock>();
            }

            // Обробка повідомлень від користувача
            if (update.Type == UpdateType.CallbackQuery)
            {
                Console.WriteLine($"Inline callback data: {update.CallbackQuery?.Data}");
                await HandleInlineKeyboardButtonAsync(chatId, update.CallbackQuery!, token);
            }
            else if (messageText!.Contains("/"))
            {
                await HandleCommandsAsync(message!, token);
            }
            else
            {
                await HandleButtonMessageAsync(message!, token);
            }
        }

        // Обробка натискання на команди з меню команд
        private async Task HandleCommandsAsync(Message message, CancellationToken cancellationToken)
        {
            switch (message.Text)
            {
                case "/start":
                    await SendStartMessageAsync(message, message.Chat, cancellationToken);
                    break;
                case "/quit":
                    await SendQuitMessageAsync(message.Chat, cancellationToken);
                    break;
                case "/help":
                    await SendHelpMessageAsync(message.Chat, cancellationToken);
                    break;
                default:
                    await SendUnknownCommandMessageAsync(message.Chat, cancellationToken);
                    break;
            }
        }

        // Обробка натискання на кнопки
        private async Task HandleButtonMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "Акції ❤️", "Збережені" } });
            replyKeyboardMarkup.ResizeKeyboard = true;

            switch (message.Text)
            {
                case "Акції ❤️":
                    await SendStockMessageAsync(message.Chat, cancellationToken);
                    break;

                case "Eva 💚":
                    await SendStockFromEvaMessageAsync(message.Chat, cancellationToken);
                    break;
                case "makeup \U0001f5a4":
                    await SendStockFromMakeupMessageAsync(message.Chat, cancellationToken);
                    break;
                case "⬅️ Повернутись":
                    await SendBackMessageAsync(message.Chat, cancellationToken);
                    break;

                case "Збережені ✅":
                    await SendSavedMessageAsync(message.Chat, cancellationToken);
                    break;
                default:
                    var message1 = await botClient.SendTextMessageAsync(message.Chat,
                        "Я тебе не розумію... виберіть команду будь ласка!",
                        cancellationToken: cancellationToken);
                    break;
            }
        }

        // Натискання на команду '/start'
        private async Task SendStartMessageAsync(Message message, ChatId chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { "Акції ❤️", "Збережені ✅" }
        });
            replyKeyboardMarkup.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(chatId,
                $"Доброго дня 👋, {message.Chat.FirstName}, виберіть команду.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        // Натискання на команду '/quit'
        private async Task SendQuitMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "/start" }, });
            replyKeyboardMarkup.ResizeKeyboard = true;

            var message = await botClient.SendTextMessageAsync(chatId,
                "До побачення!",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        // Натискання на команду '/help'
        private async Task SendHelpMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var message = await botClient.SendTextMessageAsync(chatId, "Для початку роботи вам потрібно натиснути на кнопку під полем вводу повідомлення!", cancellationToken: cancellationToken);
        }

        // Користувач вводить невідому команду
        private async Task SendUnknownCommandMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var message = await botClient.SendTextMessageAsync(chatId,
                "Виберіть команду із запропонованих.",
                cancellationToken: cancellationToken);
        }

        // Натискання на кнопку "Акції"
        private async Task SendStockMessageAsync(Chat chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { "Eva 💚", "makeup 🖤" },
            new KeyboardButton[] { "⬅️ Повернутись" },
        });
            replyKeyboardMarkup.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(chatId,
                "Оберіть магазин, для перегляду акцій. 😉",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        // Натискання на кнопку "Eva"
        private async Task SendStockFromEvaMessageAsync(Chat chatId, CancellationToken cancellationToken)
        {
            string messageStock = "";
            foreach (var stock in stocks)
            {
                if (stock.Store == "Eva")
                {
                    messageStock = stock.ToString();
                    Message message = await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromUri(stock.PhotoUrl),
                    caption: messageStock,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
                }
            }

            var rows = new List<InlineKeyboardButton[]>();
            var currentRow = new List<InlineKeyboardButton>();

            foreach (var stock in stocks)
            {
                if (stock.Store == "Eva")
                {
                    var button = InlineKeyboardButton.WithCallbackData(text: $"{stock.Name}" + " " + $"-{stock.Sale:F1}%",
                        callbackData: $"stock_{stock.Id}");

                    currentRow.Add(button);

                    rows.Add(currentRow.ToArray());
                    currentRow.Clear();
                }
            }

            InlineKeyboardMarkup inlineKeyboard = rows.ToArray();

            var message2 = await botClient.SendTextMessageAsync(chatId,
                "Виберіть акцію, яку хочете зберігти:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // Натискання на кнопку "makeup"
        private async Task SendStockFromMakeupMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            string messageStock = "";
            foreach (var stock in stocks)
            {
                if (stock.Store == "makeup")
                {
                    messageStock = stock.ToString();
                    Message message = await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromUri(stock.PhotoUrl),
                    caption: messageStock,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
                }
            }

            var rows = new List<InlineKeyboardButton[]>();
            var currentRow = new List<InlineKeyboardButton>();

            foreach (var stock in stocks)
            {
                if (stock.Store == "makeup")
                {
                    var button = InlineKeyboardButton.WithCallbackData(text: $"{stock.Name}" + " " + $"-{stock.Sale:F1}%",
                        callbackData: $"stock_{stock.Id}");

                    currentRow.Add(button);

                    rows.Add(currentRow.ToArray());
                    currentRow.Clear();
                }
            }

            InlineKeyboardMarkup inlineKeyboard = rows.ToArray();

            var message2 = await botClient.SendTextMessageAsync(chatId,
                "Виберіть акцію, яку хочете зберігти:",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        // Натискання на кнопку "Повернутись"
        private async Task SendBackMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { "Акції ❤️", "Збережені ✅" }
        });
            replyKeyboardMarkup.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(chatId,
                $"Виберіть команду:",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
        }

        // Натискання на кнопку "Збережені"
        private async Task SendSavedMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            if (userSavedStockProducts.ContainsKey((long)chatId.Identifier) && userSavedStockProducts[(long)chatId.Identifier].Count != 0)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
                { new KeyboardButton[] { "Акції ❤️", "Збережені ✅" } });
                replyKeyboardMarkup.ResizeKeyboard = true;

                float sum = 0;
                string messageSaved = " Ваші збережені товари\n";
                foreach (var product in userSavedStockProducts[(long)chatId.Identifier])
                {
                    messageSaved += $"\n\n👉 <b>{product.Name}</b> | {product.NewPrice}\n {product.Description}";
                    sum += product.NewPrice;
                }
                messageSaved += "\n\n";
                messageSaved += $"Вартість всіх збережених товарів: {sum:F2}₴";

                var message = await botClient.SendTextMessageAsync(
                    chatId,
                    messageSaved,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyKeyboardMarkup);
            }
            else
            {
                var message = await botClient.SendTextMessageAsync(chatId, "На даний момент у вас не має збережених товарів");
            }
        }

        // Вибір акційного товару на кнопки
        private async Task HandleInlineKeyboardButtonAsync(long chatId, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            { new KeyboardButton[] { "Акції ❤️", "Збережені ✅" } });
            replyKeyboardMarkup.ResizeKeyboard = true;

            botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            string data = callbackQuery.Data!;

            int stockId = int.Parse(data.Split("_")[1]);
            var stock = stocks.FirstOrDefault(p => p.Id == stockId);


            if (stock != null)
            {
                if (userSavedStockProducts[chatId].Contains(stock))
                {
                    Message sendInfoMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    $"Ви вже зберігли цей товар, оберіть інший 😅",
                    parseMode: ParseMode.Html,
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                }

                else
                {
                    userSavedStockProducts[chatId].Add(stock);

                    Message sendMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    $"Товар {stock.Name} успішно збережено",
                    parseMode: ParseMode.Html,
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
                }
            }
            else
            {
                Message sendMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                "Товар не знайдено",
                parseMode: ParseMode.Html,
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
            }
        }
    }
}
