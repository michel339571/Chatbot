USE ChatbotCommercial;
GO
CREATE TABLE Horaires(
	Id INT PRIMARY KEY IDENTITY,
	JourSemaine VARCHAR(20),
	HeureOuverture TIME,
	HeureFermeture TIME,
	EstFerme BIT DEFAULT 0 -- 1 = fermé ce jour
	);
GO
CREATE TABLE Restaurant(
	Id INT PRIMARY KEY IDENTITY,
	Nom VARCHAR(200),
	Adress TEXT,
	Telephone VARCHAR(40),
	Email VARCHAR(200),
	Livraison BIT DEFAULT 0,
	ModePaiement VARCHAR(200)
);
GO
CREATE TABLE JoursFerie(
	Id INT PRIMARY KEY IDENTITY,
	DateFerie DATE,
	Description VARCHAR(200)
);