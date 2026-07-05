using Microsoft.Data.SqlClient;
string connectionString = "Server=localhost\\SQLEXPRESS;Database=ChatbotCommercial;Trusted_Connection=true;TrustServerCertificate=true;";
using SqlConnection con = new SqlConnection(connectionString);
try{
	con.Open();
	Console.WriteLine("Connecté à SQL Server avec Succès !");
	Conversation(con);

}
catch(Exception ex){
	Console.WriteLine("Erreur");
}
void Conversation(SqlConnection connexion){
	if(!EstOuvert(connexion)){
		Console.WriteLine("Bot: Désolé, nous sommes actuellement fermés.");
		Console.WriteLine("Bot: Consultez nos heures d'ouverture et revenez nous voir !");
		return;
	}
	Console.WriteLine("Bot: Bonjour ! Voulez-vous vous renseigner ou passer une commande??");
	Console.Write("Vous: ");
	string reponse = Console.ReadLine()?.ToLower() ?? "";
	if(reponse.Contains("commande")){
		Console.WriteLine("\n Bot: Bien sur ! Voici notre menu :\n");
		AfficherMenu(connexion);
		Console.WriteLine("\n Bot: Quel produit voulez-vous commander???\n");
		Console.Write("Vous: ");
		string nomProduit = Console.ReadLine() ?? "";
		Console.WriteLine("\n Bot: Quelle quantité ???\n");
		Console.Write("Vous: ");
		int  quantite = Convert.ToInt32(Console.ReadLine() ?? "");
		Console.WriteLine("\n Bot: Quel est votre nom?\n");
		Console.Write("Vous: ");
		string nom = Console.ReadLine() ?? "";
		Console.WriteLine("\n Bot: Prénom\n");
		Console.Write("Vous: ");
		string prenom = Console.ReadLine() ?? "";
		Console.WriteLine("\n Bot: Numéro de Téléphone?\n");
		Console.Write("Vous: ");
		string telephone = Console.ReadLine() ?? "";
		string code = EnregistrerCommande(connexion, nom, prenom, telephone, nomProduit, quantite);
		Console.WriteLine($"\n Bot: Commande Enregistrée avec succès !");
		Console.WriteLine($"Bot: Votre code de commande est : {code}");
		Console.WriteLine("Bot: Présentez ce code au restaurant pour récupérer votre commande");
	}
	else if(reponse.Contains("renseigner") || reponse.Contains("info") || reponse.Contains("question")){
		Console.WriteLine("\n Bot: Voici toutes les informations du restaurant :\n");
		AfficherInfosRestaurant(connexion);

	}
	else{
		Console.WriteLine("Désolé cette option ne figure pas dans mes capacités.");
	}
}
void AfficherMenu(SqlConnection connexion){
	string requete = "SELECT NomProduit, Prix, Stock FROM Produit";
	using SqlCommand cmd = new SqlCommand(requete, connexion);
	using SqlDataReader reader = cmd.ExecuteReader();
	while(reader.Read()){
		string nom = reader.GetString(0);
		decimal prix = reader.GetDecimal(1);
		int stock = reader.GetInt32(2);
		Console.WriteLine($"{nom} - {prix} FCFA (Stock: {stock})");
	}
}
string EnregistrerCommande(SqlConnection connexion, string nom, string prenom, string telephone, string nomProduit, int quantite) {
	string requeteClient = "INSERT INTO Client(Nom, Prenom, Telephone) OUTPUT INSERTED.IdClient VALUES(@nom, @prenom, @tel)";
	using SqlCommand cmdClient = new SqlCommand(requeteClient, connexion);
	cmdClient.Parameters.AddWithValue("@nom", nom);
	cmdClient.Parameters.AddWithValue("@prenom", prenom);
	cmdClient.Parameters.AddWithValue("@tel", telephone);
	int idClient = (int)cmdClient.ExecuteScalar();
	string requeteProduit = "SELECT TOP 1 IdProduit FROM Produit WHERE NomProduit= @nomProduit";
	using SqlCommand cmdProduit = new SqlCommand(requeteProduit, connexion);
	cmdProduit.Parameters.AddWithValue("@nomProduit", nomProduit);
	object? resultatProduit = cmdProduit.ExecuteScalar();
	if(resultatProduit==null){
		Console.WriteLine("Produit non trouvé, Veuillez réessayer");
		return "ERREUR";
	}
	int idProduit = (int)resultatProduit;
	string requeteCommande = "INSERT INTO Commande(IdClient, IdProduit, Quantite, DateCommande) VALUES (@idClient, @idProduit, @quantite, @date)";
	using SqlCommand cmdCommande = new SqlCommand(requeteCommande, connexion);
	cmdCommande.Parameters.AddWithValue("@idClient", idClient);
	cmdCommande.Parameters.AddWithValue("@idProduit", idProduit);
	cmdCommande.Parameters.AddWithValue("@quantite", quantite);
	cmdCommande.Parameters.AddWithValue("@date", DateTime.Now);
	cmdCommande.ExecuteNonQuery();
	string code = "CMD-" + DateTime.Now.Year + "-" + idClient + "" + idProduit;
	return code;

}
bool EstOuvert(SqlConnection connexion){
	DateTime maintenant = DateTime.Now;
	string jourActuel = maintenant.ToString("dddd");
	TimeSpan heureActuelle = maintenant.TimeOfDay;
	string requeteFerie = "SELECT COUNT(*) FROM JoursFerie WHERE DateFerie= @date";
	using SqlCommand cmd = new SqlCommand(requeteFerie, connexion);
	cmd.Parameters.AddWithValue("@date", maintenant.Date);
	int estFerie = (int)cmd.ExecuteScalar();
	if (estFerie > 0) return false;
	string requeteHoraire = "SELECT HeureOuverTure, HeureFermeture, EstFerme FROM Horaires WHERE JourSemaine =@jour";
	using SqlCommand cmd2 = new SqlCommand(requeteHoraire, connexion);
	cmd2.Parameters.AddWithValue("@jour", jourActuel);
	using SqlDataReader reader = cmd2.ExecuteReader();
	if(reader.Read()){
		bool estFerme = reader.GetBoolean(2);
		if (estFerme) return false;
		TimeSpan ouverture = reader.GetTimeSpan(0);
		TimeSpan fermeture = reader.GetTimeSpan(1);
		return heureActuelle >= ouverture && heureActuelle <= fermeture;
	}
	return false;
}
void AfficherInfosRestaurant(SqlConnection connexion){
	string requete = "SELECT TOP 1 Nom, Adress, Telephone, Email, Livraison, ModePaiement FROM Restaurant";
	using SqlCommand cmd = new SqlCommand(requete, connexion);
	using SqlDataReader reader = cmd.ExecuteReader();
	if(reader.Read()){
		string nom = reader.GetString(0);
		string adress = reader.GetString(1);
		string telephone = reader.GetString(2);
		string email = reader.GetString(3);
		bool livraison = reader.GetBoolean(4);
		string paiement = reader.GetString(5);

		Console.WriteLine($"\n Restaurant: {nom}");
		Console.WriteLine($"\n Adresse: {adress}");
		Console.WriteLine($"\n Téléphone: {telephone}");
		Console.WriteLine($"\n Email: {email}");
		Console.WriteLine($"\n Livraison: {(livraison? "Oui": "Non")} ");
		Console.WriteLine($"\n Paiement: {paiement}");
		Console.WriteLine($"\n Horaires d'ouverture : ");
		reader.Close();
		string requeteHoraires = "SELECT JourSemaine, HeureOuverture, HeureFermeture, EstFerme FROM Horaires";
		using SqlCommand cmd2 = new SqlCommand(requeteHoraires, connexion);
		using SqlDataReader reader2 = cmd2.ExecuteReader();
		while(reader2.Read()){
			string jour = reader2.GetString(0);
			bool ferme = reader2.GetBoolean(3);
			if (ferme)
				Console.WriteLine($" {jour}: Fermé");
			else{
				TimeSpan ouverture = reader2.GetTimeSpan(1);
				TimeSpan fermeture = reader2.GetTimeSpan(2);
				Console.WriteLine($" {jour}: {ouverture:hh\\:mm} - {fermeture:hh\\:mm}");

			}
		}
	}
	else{
		Console.WriteLine("Bot: Informations du restaurant non disponibles");
	}
}