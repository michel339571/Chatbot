using Microsoft.Data.SqlClient;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

string token = "8821571363:AAFX2oZKYdQeyDVqztJlr9hFtaaIDUtPkUo";
string connectionString = "Server=localhost\\SQLEXPRESS;Database=ChatbotCommercial;Trusted_Connection=true;TrustServerCertificate=true;";

var botClient = new TelegramBotClient(token);

Console.WriteLine("✅ Bot Telegram démarré !");
Console.WriteLine("📱 Cherche @AngelMichaelBot sur Telegram et écris-lui !");

botClient.StartReceiving(
	HandleUpdateAsync,
	HandleErrorAsync,
	new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() }
);

Console.ReadLine();

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
{
	if (update.Message is not { Text: { } messageText } message) return;

	long chatId = message.Chat.Id;
	string texte = messageText.ToLower();
	string prenom = message.Chat.FirstName ?? "cher client";

	Console.WriteLine($"📩 Message reçu de {prenom} : {messageText}");

	int heure = DateTime.Now.Hour;
	string salutation;
	if (heure >= 5 && heure < 12)
		salutation = "Bonjour";
	else if (heure >= 12 && heure < 18)
		salutation = "Bon après-midi";
	else if (heure >= 18 && heure < 22)
		salutation = "Bonsoir";
	else
		salutation = "Bonne nuit";

	using SqlConnection con = new SqlConnection(connectionString);
	con.Open();

	if (texte.Contains("bonjour") || texte.Contains("salut") ||
		texte.Contains("bonsoir") || texte.Contains("hello") ||
		texte.Contains("bonne nuit") || texte.Contains("bon après-midi") ||
		texte.Contains("bonne après-midi"))
	{
		await bot.SendMessage(chatId,
			$"👋 {salutation} {prenom} ! Bienvenue au Restaurant Le Congo 🍽️\n\n" +
			$"Que souhaitez-vous faire ?\n" +
			$"1️⃣ Passer une commande\n" +
			$"2️⃣ Me renseigner");
	}
	else if (texte.Contains("commande"))
	{
		if (!EstOuvert(con))
		{
			await bot.SendMessage(chatId,
				"😔 Désolé, nous sommes actuellement fermés.\n" +
				"Consultez nos horaires et revenez nous voir !");
			return;
		}

		string menu = ObtenirMenu(con);
		await bot.SendMessage(chatId,
			"📋 Voici notre menu :\n\n" + menu +
			"\n\nQuel produit voulez-vous commander ? (tapez le nom exact)");
	}
	else if (texte.Contains("renseign") || texte.Contains("info"))
	{
		string infos = ObtenirInfosRestaurant(con);
		await bot.SendMessage(chatId, infos);
	}
	else
	{
		await bot.SendMessage(chatId,
			$"🤖 Je n'ai pas compris {prenom}.\n" +
			"Tapez 'commande' ou 'renseignement'.");
	}
}

async Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
{
	Console.WriteLine($"❌ Erreur : {ex.Message}");
}




bool EstOuvert(SqlConnection connexion)
{
	DateTime maintenant = DateTime.Now;
	string jourActuel = maintenant.ToString("dddd").ToLower();
	TimeSpan heureActuelle = maintenant.TimeOfDay;

	string requeteFerie = "SELECT COUNT(*) FROM JoursFerie WHERE DateFerie = @date";
	using SqlCommand cmd = new SqlCommand(requeteFerie, connexion);
	cmd.Parameters.AddWithValue("@date", maintenant.Date);
	int estFerie = (int)cmd.ExecuteScalar();
	if (estFerie > 0) return false;

	string requeteHoraire = "SELECT HeureOuverture, HeureFermeture, EstFerme FROM Horaires WHERE JourSemaine = @jour";
	using SqlCommand cmd2 = new SqlCommand(requeteHoraire, connexion);
	cmd2.Parameters.AddWithValue("@jour", jourActuel);
	using SqlDataReader reader = cmd2.ExecuteReader();

	if (reader.Read())
	{
		bool estFerme = reader.GetBoolean(2);
		if (estFerme) return false;

		TimeSpan ouverture = reader.GetTimeSpan(0);
		TimeSpan fermeture = reader.GetTimeSpan(1);
		return heureActuelle >= ouverture && heureActuelle <= fermeture;
	}
	return false;
}

string ObtenirMenu(SqlConnection connexion)
{
	string requete = "SELECT DISTINCT NomProduit, Prix FROM Produit";
	using SqlCommand cmd = new SqlCommand(requete, connexion);
	using SqlDataReader reader = cmd.ExecuteReader();

	string menu = "";
	while (reader.Read())
	{
		string nom = reader.GetString(0);
		decimal prix = reader.GetDecimal(1);
		menu += $"🍽️ {nom} - {prix} FCFA\n";
	}
	return menu;
}

string ObtenirInfosRestaurant(SqlConnection connexion)
{
	string requete = "SELECT TOP 1 Nom, Adress, Telephone, Email, Livraison, ModePaiement FROM Restaurant";
	using SqlCommand cmd = new SqlCommand(requete, connexion);
	using SqlDataReader reader = cmd.ExecuteReader();

	string infos = "";

	if (reader.Read())
	{
		string nom = reader.GetString(0);
		string adresse = reader.GetString(1);
		string telephone = reader.GetString(2);
		string email = reader.GetString(3);
		bool livraison = reader.GetBoolean(4);
		string paiement = reader.GetString(5);

		infos = $"🏠 Restaurant : {nom}\n" +
				$"📍 Adresse    : {adresse}\n" +
				$"📞 Téléphone  : {telephone}\n" +
				$"📧 Email      : {email}\n" +
				$"🛵 Livraison  : {(livraison ? "Oui" : "Non")}\n" +
				$"💳 Paiement   : {paiement}\n\n" +
				$"🕐 Horaires d'ouverture :\n";
	}

	reader.Close();
	string requeteHoraires = "SELECT JourSemaine, HeureOuverture, HeureFermeture, EstFerme FROM Horaires";
	using SqlCommand cmd2 = new SqlCommand(requeteHoraires, connexion);
	using SqlDataReader reader2 = cmd2.ExecuteReader();

	while (reader2.Read())
	{
		string jour = reader2.GetString(0);
		bool ferme = reader2.GetBoolean(3);

		if (ferme)
			infos += $"   📅 {jour} : Fermé\n";
		else
		{
			TimeSpan ouverture = reader2.GetTimeSpan(1);
			TimeSpan fermeture = reader2.GetTimeSpan(2);
			infos += $"   📅 {jour} : {ouverture:hh\\:mm} - {fermeture:hh\\:mm}\n";
		}
	}

	return infos == "" ? "Informations non disponibles." : infos;
}