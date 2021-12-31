using System;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PopulationBot
{
    class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5021881078:AAF-WkRD3hlEd9ml3_dgKEPH3vpvfr0xtwU");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Population Bot";
            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "/help" or "/start" => help(botClient, message),
                _ => getPopulation(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {


                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text:
                                                                  "/help - Get help\n" +
                                                                  "Type the name of the country to find out its population"
                                                            );
            }



            static async Task<Message> getPopulation(ITelegramBotClient botClient, Message message)
            {

                string recivedmsg = message.Text;

                StreamReader r = new StreamReader("H://cp.json");
                string jsonString = r.ReadToEnd();
                var Deserialized = JsonConvert.DeserializeObject<List<Root>>(jsonString);
                //var myObj = vv[0];
                foreach (var i in Deserialized)
                {

                    if (recivedmsg.ToUpper().Equals(i.country.ToUpper()))
                    {
                        Console.WriteLine("cc");
                        return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text:
                                                                      "Population of " + i.country + ": " + i.population.ToString()
                                                                );
                    }

                }
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                           text:
                                                                 "The country is not found. Please make sure to write the name correctly\n" +
                                                                 "/help - Get help\n");


            }
        }



        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }


    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Root
{
    public string country { get; set; }
    public int population { get; set; }
}
}