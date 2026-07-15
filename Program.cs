using System;
using System.Collections.Generic;

class Programme
{
    // Structure simple pour représenter un plat
    struct Plat
    {
        public string Categorie;
        public string Nom;
        public string Prix;

        public Plat(string categorie, string nom, string prix)
        {
            Categorie = categorie;
            Nom = nom;
            Prix = prix;
        }
    }

    static void Main(string[] args)
    {
        Menu(PasserCommande, Renseignements, Aide);
    }

    // ---- Fonction Menu avec 3 paramètres (délégués Action) ----
    static void Menu(Action commande, Action renseignements, Action aide)
    {
        Console.WriteLine("\n===== MENU =====");
        Console.WriteLine("1. Passer une commande");
        Console.WriteLine("2. Renseignements");
        Console.WriteLine("3. Aide");
        Console.WriteLine("4. Quitter");
        Console.Write("Votre choix (1-4) : ");

        string choix = Console.ReadLine();

        switch (choix)
        {
            case "1":
                commande();
                break;
            case "2":
                renseignements();
                break;
            case "3":
                aide();
                break;
            case "4":
                Console.WriteLine("Au revoir !");
                return;
            default:
                Console.WriteLine("Choix invalide, réessayez.");
                Menu(commande, renseignements, aide);
                break;
        }
    }

    // ---- 1er paramètre : affiche une "autre face" -> les menus du jour ----
    static void PasserCommande()
    {
        // Liste des menus du jour (déclarée ici, uniquement utilisée dans cette fonction)
        List<Plat> menusDuJour = new List<Plat>
        {
            new Plat("Entrée", "Velouté de courgette", "6,50 €"),
            new Plat("Plat", "Suprême de volaille rôti", "14,90 €"),
            new Plat("Plat", "Risotto aux champignons", "13,50 €"),
            new Plat("Dessert", "Tarte fine aux pommes", "6,00 €")       
        };

        Console.WriteLine("\n----- MENUS DU JOUR -----");
        for (int i = 0; i < menusDuJour.Count; i++)
        {
            Plat p = menusDuJour[i];
            Console.WriteLine($"{i + 1}. [{p.Categorie}] {p.Nom} — {p.Prix}");
        }

        Console.WriteLine("5. Aucun plat ne me convient, je veux passer ma propre commande ");
        Console.WriteLine("\nSi aucun plat ne vous plait , tapez 5 pour passer votre propre commande");
        Console.Write("\nTapez le numéro du plat à commander (0 pour revenir) : ");
        string choix = Console.ReadLine();

        int num;
        bool estValide = int.TryParse(choix, out num);

        if (!estValide || num == 0)
        {
            RetourMenu();
            return;
        }

         if (num == 5)
        {
            Console.Write("\nDites-nous ce que vous souhaitez commander : ");
            string demandePersonnalisee = Console.ReadLine();
            Console.WriteLine($"\n✔ Votre demande a bien été enregistrée : \"{demandePersonnalisee}\"");
            Console.WriteLine("Notre équipe va l'examiner et reviendra vers vous.");
        }

        if (num >= 1 && num <= menusDuJour.Count)
        {
            Plat plat = menusDuJour[num - 1];
            Console.WriteLine($"\n✔ Commande enregistrée : {plat.Nom} ({plat.Prix})");
        }
        else
        {
            Console.WriteLine("\nNuméro invalide.");
        }

        RetourMenu();
    }

    // ---- 2e paramètre ----
    static void Renseignements()
    {
        Console.WriteLine("\n----- RENSEIGNEMENTS -----");
        Console.WriteLine("Adresse : 12 rue des Artisans, 75011 Paris");
        Console.WriteLine("Horaires : du mardi au dimanche, 12h-14h30 et 19h-22h30");
        Console.WriteLine("Téléphone : 06 699 43 25");
        RetourMenu();
    }

    // ---- 3e paramètre ----
    static void Aide()
    {
        Console.WriteLine("\n----- AIDE -----");
        Console.WriteLine("Contactez-nous au 06 699 43 25 ou par mail à chatBOOT@gmail.com");
        RetourMenu();
    }

    // Revenir au menu principal après chaque action
    static void RetourMenu()
    {
        Menu(PasserCommande, Renseignements, Aide);
    }
}
