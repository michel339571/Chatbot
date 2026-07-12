
using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatbotCommercial
{
	public static class ServiceBot
	{
		// Dictionnaire = comme un tableau mais avec une clé personnalisée
		// Clé = chatId du client, Valeur = son état actuel
		private static Dictionary<long, string> etatsClients = new();

		// Dictionnaire imbriqué = stocke plusieurs infos par client
		// Ex: client 123 → { "produit": "Pizza", "quantite": "2" }
		private static Dictionary<long, Dictionary<string, string>> donneesClients = new();

		// Fonction principale appelée à chaque message reçu
		// "async Task" = fonction asynchrone qui peut attendre
		public static async Task HandleUpdateAsync(
			ITelegramBotClient bot, Update update, CancellationToken ct)
		{
			// Si le message n'est pas un texte → on ignore
			if (update.Message is not { Text: { } messageText } message) return;

			long chatId = message.Chat.Id;

			// ToLower() = met en minuscule pour faciliter la comparaison
			// Trim() = enlève les espaces au début et à la fin
			string texte = messageText.ToLower().Trim();
			string prenom = message.Chat.FirstName ?? "cher client";

			Console.WriteLine($"📩 {prenom} ({chatId}) : {messageText}");

			// Déterminer la salutation selon l'heure
			int heure = DateTime.Now.Hour;
			string salutation;
			if (heure >= 5 && heure < 12)
				salutation = "Bonjour";
			else if (heure >= 12 && heure < 18)
				salutation = "Bon après-midi";
			else if (heure >= 18 && heure <= 23)
				salutation = "Bonsoir";
			else
				salutation = "Bonne nuit";

			// Ouvrir la connexion à SQL Server
			using SqlConnection con = new SqlConnection(configuration.ConnectionString);
			con.Open();

			// Récupérer l'état actuel du client
			// ContainsKey = vérifie si ce client existe dans le dictionnaire
			string etat = etatsClients.ContainsKey(chatId)
				? etatsClients[chatId]  // si oui → prendre son état
				: "ACCUEIL";            // si non → état par défaut = ACCUEIL

			// switch = comme plusieurs if/else mais plus lisible
			switch (etat)
			{
				case "ACCUEIL":
					await GererAccueil(bot, chatId, texte, prenom, salutation, con);
					break;

				case "ATTENTE_PRODUIT":
					await GererChoixProduit(bot, chatId, messageText, con);
					break;

				case "ATTENTE_QUANTITE":
					await GererChoixQuantite(bot, chatId, texte);
					break;

				case "ATTENTE_TYPE_COMMANDE":
					await GererTypeCommande(bot, chatId, texte);
					break;

				case "ATTENTE_ADRESSE":
					donneesClients[chatId]["adresse"] = messageText;
					etatsClients[chatId] = "ATTENTE_NOM";
					await bot.SendMessage(chatId, "👤 Quel est votre nom ?");
					break;

				case "ATTENTE_NOM":
					donneesClients[chatId]["nom"] = messageText;
					etatsClients[chatId] = "ATTENTE_PRENOM";
					await bot.SendMessage(chatId, "👤 Quel est votre prénom ?");
					break;

				case "ATTENTE_PRENOM":
					donneesClients[chatId]["prenom"] = messageText;
					etatsClients[chatId] = "ATTENTE_TELEPHONE";
					await bot.SendMessage(chatId, "📞 Votre numéro de téléphone ?");
					break;

				case "ATTENTE_TELEPHONE":
					await GererTelephone(bot, chatId, messageText, con);
					break;
			}
		}

		// Gère l'état ACCUEIL
		private static async Task GererAccueil(ITelegramBotClient bot, long chatId,
			string texte, string prenom, string salutation, SqlConnection con)
		{
			if (texte.Contains("bonjour") || texte.Contains("salut") ||
				texte.Contains("bonsoir") || texte.Contains("hello") ||
				texte.Contains("bonne nuit") || texte.Contains("bon après-midi"))
			{
				await bot.SendMessage(chatId,
					$"👋 {salutation} {prenom} ! Bienvenue au Restaurant Le Congo 🍽️\n\n" +
					$"Que souhaitez-vous faire ?\n" +
					$"1️⃣ Passer une commande\n" +
					$"2️⃣ Me renseigner");
			}
			else if (texte.Contains("commande"))
			{
				await TraiterCommande(bot, chatId, con);
			}
			else if (texte.Contains("renseign") || texte.Contains("info") ||
					 texte.Contains("horaire"))
			{
				string infos = ServiceData.ObtenirInfosRestaurant(con);
				await bot.SendMessage(chatId, infos);
			}
			else if (texte.Contains("merci") || texte.Contains("thank"))
			{
				await bot.SendMessage(chatId,
					"😊 De rien ! C'est avec plaisir.\n" +
					"N'hésitez pas à revenir pour une prochaine commande !");
			}
			else if (texte.Contains("ok") || texte.Contains("d'accord") ||
					 texte.Contains("parfait") || texte.Contains("très bien") || texte.Contains("Cool"))
			{
				await bot.SendMessage(chatId,
					"👍 Parfait ! N'hésitez pas si vous avez besoin d'autre chose.");
			}
			else if (texte.Contains("au revoir") || texte.Contains("bye") ||
					 texte.Contains("à bientôt"))
			{
				await bot.SendMessage(chatId,
					"👋 Au revoir ! À bientôt chez Restaurant Le Congo 🍽️");
			}
			else if (texte.Contains("super") || texte.Contains("bien") ||
					 texte.Contains("excellent") || texte.Contains("bravo"))
			{
				await bot.SendMessage(chatId,
					"😊 Merci ! On fait de notre mieux pour vous satisfaire !");
			}
			else
			{
				await bot.SendMessage(chatId,
					$"🤖 {salutation} {prenom} !\n" +
					"Tapez 'commande' ou 'renseignement'.");
			}
		}

		// Gère le choix du produit
		private static async Task GererChoixProduit(ITelegramBotClient bot,
			long chatId, string messageText, SqlConnection con)
		{
			if (!ServiceData.ProduitExiste(con, messageText))
			{
				await bot.SendMessage(chatId,
					"❌ Désolé, ce produit ne figure pas dans notre menu.\n\n" +
					"Voici notre menu :\n" + ServiceData.ObtenirMenu(con) +
					"\nVeuillez choisir un produit du menu.");
				return;
			}
			donneesClients[chatId]["produit"] = messageText;
			etatsClients[chatId] = "ATTENTE_QUANTITE";
			await bot.SendMessage(chatId, "📦 Quelle quantité souhaitez-vous ?");
		}

		// Gère le choix de la quantité
		private static async Task GererChoixQuantite(ITelegramBotClient bot,
			long chatId, string texte)
		{
			// TryParse = essaie de convertir le texte en nombre
			// Si ça échoue → demande de réessayer
			if (!int.TryParse(texte, out int quantite))
			{
				await bot.SendMessage(chatId,
					"⚠️ Veuillez entrer un nombre valide (ex: 1, 2, 3)");
				return;
			}
			donneesClients[chatId]["quantite"] = quantite.ToString();
			etatsClients[chatId] = "ATTENTE_TYPE_COMMANDE";
			await bot.SendMessage(chatId,
				"🚀 Comment souhaitez-vous récupérer votre commande ?\n\n" +
				"1️⃣ Livraison à domicile\n" +
				"2️⃣ Commander sur place");
		}

		// Gère le type de commande (livraison ou sur place)
		private static async Task GererTypeCommande(ITelegramBotClient bot,
			long chatId, string texte)
		{
			if (texte.Contains("1") || texte.Contains("livraison"))
			{
				donneesClients[chatId]["type"] = "livraison";
				etatsClients[chatId] = "ATTENTE_ADRESSE";
				await bot.SendMessage(chatId,
					"📍 Quelle est votre adresse de livraison ?");
			}
			else if (texte.Contains("2") || texte.Contains("place"))
			{
				donneesClients[chatId]["type"] = "sur_place";
				donneesClients[chatId]["adresse"] = "Sur place";
				etatsClients[chatId] = "ATTENTE_NOM";
				await bot.SendMessage(chatId, "👤 Quel est votre nom ?");
			}
			else
			{
				await bot.SendMessage(chatId,
					"⚠️ Tapez '1' pour livraison ou '2' pour sur place.");
			}
		}

		// Gère la réception du téléphone et finalise la commande
		private static async Task GererTelephone(ITelegramBotClient bot,
			long chatId, string messageText, SqlConnection con)
		{
			donneesClients[chatId]["telephone"] = messageText;

			var donnees = donneesClients[chatId];
			string code = ServiceData.EnregistrerCommande(
				con,
				donnees["nom"],
				donnees["prenom"],
				donnees["telephone"],
				donnees["produit"],
				int.Parse(donnees["quantite"]),
				donnees["type"],
				donnees["adresse"]
			);

			if (code == "ERREUR")
			{
				await bot.SendMessage(chatId,
					"⚠️ Erreur lors de l'enregistrement. Tapez 'commande' pour réessayer.");
			}
			else
			{
				string typeMsg = donnees["type"] == "livraison"
					? $"🛵 Livraison à : {donnees["adresse"]}"
					: "🏠 Sur place";

				await bot.SendMessage(chatId,
					$"✅ Commande enregistrée avec succès !\n\n" +
					$"👤 Client : {donnees["prenom"]} {donnees["nom"]}\n" +
					$"🍽️ Produit : {donnees["produit"]}\n" +
					$"📦 Quantité : {donnees["quantite"]}\n" +
					$"{typeMsg}\n" +
					$"🔑 Code : {code}\n\n" +
					$"Merci pour votre commande !");

				await RapportService.EnvoyerRapportSiNecessaire(bot, con);
			}

			// Réinitialiser l'état du client
			etatsClients[chatId] = "ACCUEIL";
			donneesClients.Remove(chatId);
		}

		// Lance le traitement d'une commande
		private static async Task TraiterCommande(ITelegramBotClient bot,
			long chatId, SqlConnection con)
		{
			if (!ServiceData.EstOuvert(con))
			{
				await bot.SendMessage(chatId,
					"😔 Désolé, nous sommes actuellement fermés.\n" +
					"Consultez nos horaires et revenez nous voir !");
				return;
			}

			string menu = ServiceData.ObtenirMenu(con);
			await bot.SendMessage(chatId,
				"📋 Voici notre menu :\n\n" + menu +
				"\n\nQuel produit souhaitez-vous commander ?");

			etatsClients[chatId] = "ATTENTE_PRODUIT";
			donneesClients[chatId] = new Dictionary<string, string>();
		}

		// Gère les erreurs Telegram
		public static async Task HandleErrorAsync(ITelegramBotClient bot,
			Exception ex, CancellationToken ct)
		{
			Console.WriteLine($"❌ Erreur : {ex.Message}");
		}
	}
}
