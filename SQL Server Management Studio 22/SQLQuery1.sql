
USE ChatbotCommercial;
GO
CREATE TABLE Client (
	IdClient INT PRIMARY KEY IDENTITY(1,1),
	Nom VARCHAR(50),
	Prenom VARCHAR(50),
	Telephone VARCHAR(50),
	Email VARCHAR(100),
);
GO
CREATE TABLE Produit(
	IdProduit INT PRIMARY KEY IDENTITY(1,1),
	NomProduit VARCHAR(100),
	Prix DECIMAL(10,2),
	Stock INT
);
GO
CREATE TABLE Chat(
	IdChat INT PRIMARY KEY IDENTITY(1,1),
	IdClient INT,
	MessageClient TEXT,
	ReponseChatbot TEXT,
	DATEDiscussion DATETIME DEFAULT GETDATE(),

	FOREIGN KEY (IdClient)
	REFERENCES Client (IdClient)
);
GO
CREATE TABLE Commande(
	IdCommande INT PRIMARY KEY IDENTITY(1,1),
	IdClient INT,
	IdProduit INT,
	Quantite INT,
	DateCommande DATETIME DEFAULT GETDATE(),

	FOREIGN KEY (IdClient)
	REFERENCES Client(IdClient),

	FOREIGN KEY(IdProduit)
	REFERENCES Produit (IdProduit)
);
GO
INSERT INTO Client VALUES(
'Michel', 'Olivier', '064858589', 'Angelolivier@gmail.com'
);
GO
INSERT INTO Produit VALUES
	('Pizza', 50000, 30),
	('Hamburger', 10000, 50);
GO
INSERT INTO Chat VALUES(
1, 
'Bonjour je cherche une pizza',
'Nous avons des pizzas disponibles',
GETDATE()
);
INSERT INTO Commande VALUES(
	1,
	2,
	1,
	GETDATE()
);
SELECT * FROM Client;
SELECT * FROM Produit;
SELECT * FROM Chat;
SELECT * FROM Commande;