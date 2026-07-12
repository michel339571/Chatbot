USE ChatbotCommercial;
GO
ALTER TABLE Commande
ADD TypeCommande VARCHAR(50), -- 'Livraison' ou 'sur_place'
	AdresseLivraison TEXT;