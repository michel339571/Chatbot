using Microsoft.Data.SqlClient;
namespace ChatbotCommercial
{
	public static class ServiceData
		{
			public static bool ProduitExiste(SqlConnection connexion, string nomProduit)
			{
				string requete = "SELECT COUNT(*) FROM Produit WHERE NomProduit LIKE '%' + @nom + '%'";

				using SqlCommand cmd = new SqlCommand(requete, connexion);

				cmd.Parameters.AddWithValue("@nom", nomProduit);

				int count = (int)cmd.ExecuteScalar();


				return count > 0;
			}

			
			public static bool EstOuvert(SqlConnection connexion)
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

			public static string ObtenirMenu(SqlConnection connexion)
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

			public static string ObtenirInfosRestaurant(SqlConnection connexion)
			{
				string requete = "SELECT TOP 1 Nom, Adress, Telephone, Email, Livraison, ModePaiement FROM Restaurant";
				using SqlCommand cmd = new SqlCommand(requete, connexion);
				using SqlDataReader reader = cmd.ExecuteReader();

				string infos = "";

				if (reader.Read())
				{
					
					infos = $"🏠 Restaurant : {reader.GetString(0)}\n" +
							$"📍 Adresse    : {reader.GetString(1)}\n" +
							$"📞 Téléphone  : {reader.GetString(2)}\n" +
							$"📧 Email      : {reader.GetString(3)}\n" +
							$"🛵 Livraison  : {(reader.GetBoolean(4) ? "Oui" : "Non")}\n" +
							$"💳 Paiement   : {reader.GetString(5)}\n\n" +
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

			public static string EnregistrerCommande(SqlConnection connexion, string nom,
				string prenom, string telephone, string nomProduit,
				int quantite, string type, string adresse)
			{
				
				string requeteClient = "INSERT INTO Client (Nom, Prenom, Telephone) OUTPUT INSERTED.IdClient VALUES (@nom, @prenom, @tel)";
				using SqlCommand cmdClient = new SqlCommand(requeteClient, connexion);
				cmdClient.Parameters.AddWithValue("@nom", nom);
				cmdClient.Parameters.AddWithValue("@prenom", prenom);
				cmdClient.Parameters.AddWithValue("@tel", telephone);

				int idClient = (int)cmdClient.ExecuteScalar();

				
				string requeteProduit = "SELECT TOP 1 IdProduit FROM Produit WHERE NomProduit LIKE '%' + @nomProduit + '%'";
				using SqlCommand cmdProduit = new SqlCommand(requeteProduit, connexion);
				cmdProduit.Parameters.AddWithValue("@nomProduit", nomProduit);
				object? resultatProduit = cmdProduit.ExecuteScalar();

				
				if (resultatProduit == null) return "ERREUR";
				int idProduit = (int)resultatProduit;

				
				string requeteCommande = @"INSERT INTO Commande 
                (IdClient, IdProduit, Quantite, DateCommande, TypeCommande, AdresseLivraison) 
                VALUES (@idClient, @idProduit, @quantite, @date, @type, @adresse)";
				using SqlCommand cmdCommande = new SqlCommand(requeteCommande, connexion);
				cmdCommande.Parameters.AddWithValue("@idClient", idClient);
				cmdCommande.Parameters.AddWithValue("@idProduit", idProduit);
				cmdCommande.Parameters.AddWithValue("@quantite", quantite);
				cmdCommande.Parameters.AddWithValue("@date", DateTime.Now);
				cmdCommande.Parameters.AddWithValue("@type", type);
				cmdCommande.Parameters.AddWithValue("@adresse", adresse);

				
				cmdCommande.ExecuteNonQuery();

				
				return "CMD-" + DateTime.Now.Year + "-" + idClient + idProduit;
			}

		
			public static List<(string nom, string prenom, string tel,
				string produit, int qte, string type, string adresse, DateTime date)>
				ObtenirCommandesPeriode(SqlConnection connexion, DateTime debut, DateTime fin)
			{
				string requete = @"
                SELECT c.Nom, c.Prenom, c.Telephone,
                       p.NomProduit, co.Quantite,
                       co.TypeCommande, co.AdresseLivraison,
                       co.DateCommande
                FROM Commande co
                JOIN Client c ON co.IdClient = c.IdClient
                JOIN Produit p ON co.IdProduit = p.IdProduit
                WHERE co.DateCommande BETWEEN @debut AND @fin
                ORDER BY co.TypeCommande, co.DateCommande";

				using SqlCommand cmd = new SqlCommand(requete, connexion);
				cmd.Parameters.AddWithValue("@debut", debut);
				cmd.Parameters.AddWithValue("@fin", fin);
				using SqlDataReader reader = cmd.ExecuteReader();

				var commandes = new List<(string, string, string, string, int, string, string, DateTime)>();

				while (reader.Read())
				{
					commandes.Add((
						reader.GetString(0),
						reader.GetString(1),
						reader.GetString(2),
						reader.GetString(3),
						reader.GetInt32(4),
						reader.IsDBNull(5) ? "sur_place" : reader.GetString(5),
						reader.IsDBNull(6) ? "Sur place" : reader.GetString(6),
						reader.GetDateTime(7)
					));
				}

				return commandes;
			}
	}
}


