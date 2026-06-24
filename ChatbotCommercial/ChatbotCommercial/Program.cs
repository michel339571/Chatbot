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
	else if(reponse.Contains("renseign")){
		Console.WriteLine("Bien sur, Posez toutes vos questions.");

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
