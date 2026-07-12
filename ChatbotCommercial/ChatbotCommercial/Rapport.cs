// RapportService.cs
// Génère le fichier Excel et l'envoie au patron
// Séparé car c'est une responsabilité distincte

using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ChatbotCommercial
{
	public static class RapportService
	{
		// Envoie le rapport Excel au patron si nécessaire
		// "async" = cette fonction peut attendre sans bloquer le programme
		public static async Task EnvoyerRapportSiNecessaire(
			ITelegramBotClient bot, SqlConnection connexion)
		{
			int heureActuelle = DateTime.Now.Hour;

			// Si on a déjà envoyé un rapport cette heure → on ne renvoie pas
			if (heureActuelle == configuration.DerniereHeureRapport) return;
			configuration.DerniereHeureRapport = heureActuelle;

			// Récupérer les commandes de la dernière heure
			DateTime debut = DateTime.Now.AddHours(-1);
			DateTime fin = DateTime.Now;

			var commandes = ServiceData.ObtenirCommandesPeriode(connexion, debut, fin);

			// Pas de commandes → pas de rapport
			if (commandes.Count == 0) return;

			// Créer le fichier Excel
			using var package = new ExcelPackage();
			var feuille = package.Workbook.Worksheets.Add("Commandes");

			// En-têtes du tableau (ligne 1)
			feuille.Cells[1, 1].Value = "Nom";
			feuille.Cells[1, 2].Value = "Prénom";
			feuille.Cells[1, 3].Value = "Téléphone";
			feuille.Cells[1, 4].Value = "Produit";
			feuille.Cells[1, 5].Value = "Quantité";
			feuille.Cells[1, 6].Value = "Type";
			feuille.Cells[1, 7].Value = "Adresse";
			feuille.Cells[1, 8].Value = "Heure";

			// Remplir les données à partir de la ligne 2
			int ligne = 2;
			foreach (var c in commandes) // foreach = parcourt chaque commande
			{
				feuille.Cells[ligne, 1].Value = c.nom;
				feuille.Cells[ligne, 2].Value = c.prenom;
				feuille.Cells[ligne, 3].Value = c.tel;
				feuille.Cells[ligne, 4].Value = c.produit;
				feuille.Cells[ligne, 5].Value = c.qte;
				feuille.Cells[ligne, 6].Value = c.type;
				feuille.Cells[ligne, 7].Value = c.adresse;
				feuille.Cells[ligne, 8].Value = c.date.ToString("HH:mm");
				ligne++;
			}

			// Ajuster automatiquement la largeur des colonnes
			feuille.Cells.AutoFitColumns();

			// Sauvegarder le fichier sur le disque
			string cheminFichier = $"rapport_{DateTime.Now:yyyy-MM-dd_HH}.xlsx";
			File.WriteAllBytes(cheminFichier, package.GetAsByteArray());

			// Envoyer au patron si son ID est configuré
			if (configuration.ChatIdPatron != 0)
			{
				int surPlace = commandes.Count(c => c.type == "sur_place");
				int livraison = commandes.Count(c => c.type == "livraison");

				// Envoyer le résumé texte
				await bot.SendMessage(configuration.ChatIdPatron,
					$"📊 Rapport horaire ({debut:HH:mm} - {fin:HH:mm})\n" +
					$"Total commandes : {commandes.Count}\n" +
					$"🏠 Sur place : {surPlace}\n" +
					$"🛵 Livraison : {livraison}");

				// Envoyer le fichier Excel
				using var stream = File.OpenRead(cheminFichier);
				await bot.SendDocument(configuration.ChatIdPatron,
					new InputFileStream(stream, cheminFichier));
			}
		}
	}
}