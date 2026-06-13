-- ============================================================
--  TogoConnect – Script de création de la base de données
--  SGBD      : Microsoft SQL Server
--  Projet    : ECT 330 – Développement Avancé d'Applications Internet
--  Filière   : Génie Logiciel (GL) – Licence 2
--  Année     : 2025 – 2026
-- ============================================================

-- ────────────────────────────────────────────────────────────
--  0. CRÉATION DE LA BASE DE DONNÉES
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TogoConnect')
BEGIN
    CREATE DATABASE TogoConnect
    COLLATE French_CI_AS;
    PRINT 'Base de données TogoConnect créée avec succès.';
END
GO

USE TogoConnect;
GO

-- ────────────────────────────────────────────────────────────
--  1. TABLE : Utilisateurs
--  Stocke tous les comptes (étudiants et entreprises)
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Utilisateurs', 'U') IS NOT NULL DROP TABLE dbo.Utilisateurs;
GO

CREATE TABLE dbo.Utilisateurs (
    Id               INT           IDENTITY(1,1) PRIMARY KEY,
    Nom              NVARCHAR(100) NOT NULL,
    Prenom           NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(200) NOT NULL UNIQUE,
    MotDePasse       NVARCHAR(256) NOT NULL,           -- Hash SHA-256
    Role             NVARCHAR(20)  NOT NULL             -- 'etudiant' | 'entreprise'
        CONSTRAINT CK_Role CHECK (Role IN ('etudiant', 'entreprise')),
    EstActif         BIT           NOT NULL DEFAULT 1,
    DateInscription  DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ────────────────────────────────────────────────────────────
--  2. TABLE : ProfilsEtudiants
--  Détails du profil pour les comptes étudiants
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.ProfilsEtudiants', 'U') IS NOT NULL DROP TABLE dbo.ProfilsEtudiants;
GO

CREATE TABLE dbo.ProfilsEtudiants (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    IdUtilisateur   INT           NOT NULL UNIQUE,
    Filiere         NVARCHAR(150) NULL,
    Etablissement   NVARCHAR(200) NULL,
    Biographie      NVARCHAR(MAX) NULL,
    CheminCV        NVARCHAR(500) NULL,               -- Chemin vers le fichier PDF
    DateNaissance   DATE          NULL,
    Ville           NVARCHAR(100) NULL,
    Telephone       NVARCHAR(20)  NULL,
    CONSTRAINT FK_ProfilsEtudiants_Utilisateurs
        FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(Id)
        ON DELETE CASCADE
);
GO

-- ────────────────────────────────────────────────────────────
--  3. TABLE : ProfilsEntreprises
--  Détails du profil pour les comptes entreprises
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.ProfilsEntreprises', 'U') IS NOT NULL DROP TABLE dbo.ProfilsEntreprises;
GO

CREATE TABLE dbo.ProfilsEntreprises (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    IdUtilisateur   INT           NOT NULL UNIQUE,
    RaisonSociale   NVARCHAR(200) NOT NULL,
    Secteur         NVARCHAR(150) NULL,
    Description     NVARCHAR(MAX) NULL,
    SiteWeb         NVARCHAR(300) NULL,
    Adresse         NVARCHAR(300) NULL,
    Telephone       NVARCHAR(20)  NULL,
    CheminLogo      NVARCHAR(500) NULL,
    CONSTRAINT FK_ProfilsEntreprises_Utilisateurs
        FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(Id)
        ON DELETE CASCADE
);
GO

-- ────────────────────────────────────────────────────────────
--  4. TABLE : Offres
--  Annonces publiées par les entreprises
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Offres', 'U') IS NOT NULL DROP TABLE dbo.Offres;
GO

CREATE TABLE dbo.Offres (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    IdEntreprise    INT           NOT NULL,
    Titre           NVARCHAR(250) NOT NULL,
    TypeOffre       NVARCHAR(50)  NOT NULL
        CONSTRAINT CK_TypeOffre CHECK (TypeOffre IN ('emploi', 'stage', 'formation', 'evenement')),
    Description     NVARCHAR(MAX) NOT NULL,
    Competences     NVARCHAR(500) NULL,
    Localisation    NVARCHAR(200) NULL,
    DateLimite      DATE          NOT NULL,
    Statut          NVARCHAR(20)  NOT NULL DEFAULT 'active'
        CONSTRAINT CK_StatutOffre CHECK (Statut IN ('active', 'expiree', 'archivee')),
    DatePublication DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Offres_Entreprises
        FOREIGN KEY (IdEntreprise) REFERENCES dbo.ProfilsEntreprises(Id)
        ON DELETE CASCADE
);
GO

-- ────────────────────────────────────────────────────────────
--  5. TABLE : Candidatures
--  Lien entre un étudiant et une offre
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Candidatures', 'U') IS NOT NULL DROP TABLE dbo.Candidatures;
GO

CREATE TABLE dbo.Candidatures (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    IdEtudiant        INT           NOT NULL,
    IdOffre           INT           NOT NULL,
    LettreMotivation  NVARCHAR(MAX) NULL,
    Statut            NVARCHAR(20)  NOT NULL DEFAULT 'en_attente'
        CONSTRAINT CK_StatutCandidature CHECK (Statut IN ('en_attente', 'acceptee', 'refusee')),
    DateDepot         DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Candidatures_Etudiants
        FOREIGN KEY (IdEtudiant) REFERENCES dbo.ProfilsEtudiants(Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Candidatures_Offres
        FOREIGN KEY (IdOffre) REFERENCES dbo.Offres(Id)
        ON DELETE NO ACTION,
    CONSTRAINT UQ_Candidature_Unique
        UNIQUE (IdEtudiant, IdOffre)             -- Un étudiant ne peut postuler qu'une seule fois
);
GO

-- ────────────────────────────────────────────────────────────
--  6. TABLE : Messages
--  Messagerie interne entre utilisateurs
-- ────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.Messages', 'U') IS NOT NULL DROP TABLE dbo.Messages;
GO

CREATE TABLE dbo.Messages (
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    IdExpediteur    INT           NOT NULL,
    IdDestinataire  INT           NOT NULL,
    Contenu         NVARCHAR(MAX) NOT NULL,
    Lu              BIT           NOT NULL DEFAULT 0,
    DateEnvoi       DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Messages_Expediteur
        FOREIGN KEY (IdExpediteur) REFERENCES dbo.Utilisateurs(Id)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Messages_Destinataire
        FOREIGN KEY (IdDestinataire) REFERENCES dbo.Utilisateurs(Id)
        ON DELETE NO ACTION,
    CONSTRAINT CK_Messages_Different
        CHECK (IdExpediteur <> IdDestinataire)   -- On ne peut pas se écrire à soi-même
);
GO

-- ────────────────────────────────────────────────────────────
--  7. INDEX pour optimiser les performances
-- ────────────────────────────────────────────────────────────
CREATE INDEX IX_Offres_Type       ON dbo.Offres(TypeOffre);
CREATE INDEX IX_Offres_Statut     ON dbo.Offres(Statut);
CREATE INDEX IX_Offres_DateLimite ON dbo.Offres(DateLimite);
CREATE INDEX IX_Candidatures_Etudiant ON dbo.Candidatures(IdEtudiant);
CREATE INDEX IX_Candidatures_Offre    ON dbo.Candidatures(IdOffre);
CREATE INDEX IX_Messages_Destinataire ON dbo.Messages(IdDestinataire);
GO

-- ────────────────────────────────────────────────────────────
--  8. VUES UTILES
-- ────────────────────────────────────────────────────────────

-- Vue : Offres avec nom de l'entreprise
CREATE OR ALTER VIEW dbo.VW_OffresAvecEntreprise AS
SELECT
    o.Id,
    o.Titre,
    o.TypeOffre,
    o.Description,
    o.Competences,
    o.Localisation,
    o.DateLimite,
    o.Statut,
    o.DatePublication,
    pe.RaisonSociale  AS NomEntreprise,
    pe.Secteur        AS SecteurEntreprise,
    pe.CheminLogo     AS LogoEntreprise
FROM dbo.Offres o
INNER JOIN dbo.ProfilsEntreprises pe ON o.IdEntreprise = pe.Id;
GO

-- Vue : Candidatures avec détail étudiant + offre
CREATE OR ALTER VIEW dbo.VW_CandidaturesDetail AS
SELECT
    c.Id,
    c.DateDepot,
    c.Statut,
    u.Nom           AS NomEtudiant,
    u.Prenom        AS PrenomEtudiant,
    u.Email         AS EmailEtudiant,
    pe.Filiere,
    pe.CheminCV,
    o.Titre         AS TitreOffre,
    o.TypeOffre,
    ent.RaisonSociale AS NomEntreprise
FROM dbo.Candidatures c
INNER JOIN dbo.ProfilsEtudiants pe  ON c.IdEtudiant = pe.Id
INNER JOIN dbo.Utilisateurs u       ON pe.IdUtilisateur = u.Id
INNER JOIN dbo.Offres o             ON c.IdOffre = o.Id
INNER JOIN dbo.ProfilsEntreprises ent ON o.IdEntreprise = ent.Id;
GO

-- Vue : Messages non lus par destinataire
CREATE OR ALTER VIEW dbo.VW_MessagesNonLus AS
SELECT
    m.Id,
    m.Contenu,
    m.DateEnvoi,
    m.IdDestinataire,
    u.Nom    AS NomExpediteur,
    u.Prenom AS PrenomExpediteur
FROM dbo.Messages m
INNER JOIN dbo.Utilisateurs u ON m.IdExpediteur = u.Id
WHERE m.Lu = 0;
GO

-- ────────────────────────────────────────────────────────────
--  9. DONNÉES DE TEST (optionnel)
-- ────────────────────────────────────────────────────────────

-- Compte étudiant (mot de passe : "Etudiant123" hashé en SHA-256)
INSERT INTO dbo.Utilisateurs (Nom, Prenom, Email, MotDePasse, Role)
VALUES ('Kokou', 'Amavi', 'amavi.kokou@etudiant.tg',
        '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'etudiant');

INSERT INTO dbo.ProfilsEtudiants (IdUtilisateur, Filiere, Etablissement, Biographie, Ville)
VALUES (1, 'Génie Logiciel', 'Université de Lomé',
        'Étudiant passionné de développement web et mobile.', 'Lomé');

-- Compte entreprise (mot de passe : "Entreprise123" hashé en SHA-256)
INSERT INTO dbo.Utilisateurs (Nom, Prenom, Email, MotDePasse, Role)
VALUES ('Tech', 'Togo', 'contact@techtogo.tg',
        'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'entreprise');

INSERT INTO dbo.ProfilsEntreprises (IdUtilisateur, RaisonSociale, Secteur, Description, SiteWeb, Adresse)
VALUES (2, 'TechTogo SARL', 'Technologies de l''Information',
        'Entreprise togolaise spécialisée dans le développement de solutions numériques.',
        'https://techtogo.tg', 'Avenue de la Libération, Lomé');

-- Offres de test
INSERT INTO dbo.Offres (IdEntreprise, Titre, TypeOffre, Description, Competences, Localisation, DateLimite)
VALUES
(1, 'Développeur Web Full Stack', 'emploi',
 'Nous recherchons un développeur web pour rejoindre notre équipe. Mission : développement et maintenance d''applications web.',
 'HTML, CSS, JavaScript, ASP.NET, SQL Server', 'Lomé, Togo', DATEADD(MONTH, 2, GETDATE())),

(1, 'Stage en Développement Mobile', 'stage',
 'Stage de 3 mois pour participer au développement de notre application mobile.',
 'Android, Java, ou Flutter', 'Lomé, Togo', DATEADD(MONTH, 1, GETDATE())),

(1, 'Formation Cybersécurité', 'formation',
 'Formation de 5 jours sur les fondamentaux de la cybersécurité pour les développeurs.',
 'Réseaux, Linux, bases en sécurité', 'Lomé, Togo', DATEADD(WEEK, 3, GETDATE()));

-- Candidature de test
INSERT INTO dbo.Candidatures (IdEtudiant, IdOffre, LettreMotivation)
VALUES (1, 1, 'Madame, Monsieur, je souhaite vivement rejoindre votre équipe...');

-- Message de test
INSERT INTO dbo.Messages (IdExpediteur, IdDestinataire, Contenu)
VALUES (2, 1, 'Bonjour, nous avons bien reçu votre candidature. Nous reviendrons vers vous rapidement.');

GO

-- ────────────────────────────────────────────────────────────
--  10. VÉRIFICATION FINALE
-- ────────────────────────────────────────────────────────────
PRINT '============================================';
PRINT '  TogoConnect – Base de données créée !';
PRINT '============================================';
SELECT 'Utilisateurs'      AS [Table], COUNT(*) AS [Lignes] FROM dbo.Utilisateurs   UNION ALL
SELECT 'ProfilsEtudiants'  AS [Table], COUNT(*) AS [Lignes] FROM dbo.ProfilsEtudiants UNION ALL
SELECT 'ProfilsEntreprises'AS [Table], COUNT(*) AS [Lignes] FROM dbo.ProfilsEntreprises UNION ALL
SELECT 'Offres'            AS [Table], COUNT(*) AS [Lignes] FROM dbo.Offres          UNION ALL
SELECT 'Candidatures'      AS [Table], COUNT(*) AS [Lignes] FROM dbo.Candidatures    UNION ALL
SELECT 'Messages'          AS [Table], COUNT(*) AS [Lignes] FROM dbo.Messages;
GO
