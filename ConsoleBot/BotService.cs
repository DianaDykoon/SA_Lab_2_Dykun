using System;
using System.Collections.Generic;
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

        public BotService()
        {
            botClient.StartReceiving(OnUpdate, OnError);
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
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (update.Message is not { } message )
               return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {message?.Chat.FirstName}.");

            // Обробка повідомлень від користувача
            if (messageText.Contains("/"))
            {
                await HandleCommandsAsync(message!, token);
            }

            // Echo received message text
            //Message sentMessage = await botClient.SendTextMessageAsync(
            //    chatId: chatId,
            //    text: "> " + messageText,
            //    cancellationToken: token
            //    );
            //if (messageText == "photo")
            //{
            //    Message phMessage = await botClient.SendPhotoAsync(
            //    chatId: chatId,
            //    photo: InputFile.FromUri("https://github.com/TelegramBots/book/raw/master/src/docs/photo-ara.jpg"),
            //    cancellationToken: token
            //   );
            //}
        }

        // Обробка натискання на команди з меню команд
        private async Task HandleCommandsAsync(Message message, CancellationToken cancellationToken)
        {
            switch (message.Text)
            {
                case "/start":
                    await SendStartMessageAsync(message.Chat, cancellationToken);
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

        // Натискання на команду '/start'
        private async Task SendStartMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Акції ❤️", "Збережені" }
            });
            replyKeyboardMarkup.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(chatId,
                "Доброго дня 👋, виберіть команду.", 
                replyMarkup: replyKeyboardMarkup, 
                cancellationToken: cancellationToken);
        }

        // Натискання на команду '/quit'
        private async Task SendQuitMessageAsync(ChatId chatId, CancellationToken cancellationToken)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new[] { new KeyboardButton[] { "/start" }, });
            replyKeyboardMarkup.ResizeKeyboard = true;
            var message = await botClient.SendTextMessageAsync(chatId, "Гарного вам дня, ще побачимось!", replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
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
    }
}
