USE ChatbotCommercial;
GO
INSERT INTO Horaires(JourSemaine, HeureOuverture, HeureFermeture, EstFerme)
VALUES
('lundi', '08:00', '22:00', 0),
('mardi', '08:00', '22:00', 0),
('mercredi', '08:00', '22:00', 0),
('jeudi', '08:00', '22:00', 0),
('vendredi', '08:00', '22:00', 0),
('samedi', '08:00', '17:00', 0),
('dimanche', '08:00', '20:00', 0);
GO
SELECT * FROM Horaires;
GO