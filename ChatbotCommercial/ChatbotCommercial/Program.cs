
using ChatbotCommercial;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;


var botClient = new TelegramBotClient(configuration.TokenTelegram);

Console.WriteLine("✅ Bot Telegram démarré !");
Console.WriteLine("📱 Écris à @AngelMichaelBot sur Telegram !");


botClient.StartReceiving(
	BotService.HandleUpdateAsync,
	BotService.HandleErrorAsync,
	new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() }
);


Console.ReadLine();