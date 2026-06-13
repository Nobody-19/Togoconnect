// ============================================================
//  TogoConnect – Couche Données (DAL)
//  Fichier : DAL/CandidatureDAL.cs
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TogoConnect.Models;

namespace TogoConnect.DAL
{
    public class CandidatureDAL
    {
        private Candidature MapRow(DataRow row)
        {
            return new Candidature
            {
                Id               = (int)row["Id"],
                IdEtudiant       = (int)row["IdEtudiant"],
                IdOffre          = (int)row["IdOffre"],
                LettreMotivation = row["LettreMotivation"] != DBNull.Value ? row["LettreMotivation"].ToString() : "",
                Statut           = row["Statut"].ToString(),
                DateDepot        = (DateTime)row["DateDepot"],
                Etudiant = row.Table.Columns.Contains("NomEtudiant") ? new ProfilEtudiant
                {
                    Utilisateur = new Utilisateur
                    {
                        Nom    = row["NomEtudiant"].ToString(),
                        Prenom = row["PrenomEtudiant"].ToString(),
                        Email  = row["EmailEtudiant"].ToString()
                    },
                    Filiere  = row["Filiere"] != DBNull.Value ? row["Filiere"].ToString() : "",
                    CheminCV = row["CheminCV"] != DBNull.Value ? row["CheminCV"].ToString() : ""
                } : null,
                Offre = row.Table.Columns.Contains("TitreOffre") ? new Offre
                {
                    Titre     = row["TitreOffre"].ToString(),
                    TypeOffre = row["TypeOffre"].ToString(),
                    Entreprise = new ProfilEntreprise { RaisonSociale = row["NomEntreprise"].ToString() }
                } : null
            };
        }

        /// <summary>Dépose une candidature.</summary>
        public bool Deposer(Candidature c)
        {
            string sql = @"
                INSERT INTO Candidatures (IdEtudiant, IdOffre, LettreMotivation)
                VALUES (@IdEtudiant, @IdOffre, @LettreMotivation)";

            var p = new[]
            {
                new SqlParameter("@IdEtudiant",       c.IdEtudiant),
                new SqlParameter("@IdOffre",          c.IdOffre),
                new SqlParameter("@LettreMotivation", (object)c.LettreMotivation ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Retourne les candidatures d'un étudiant.</summary>
        public List<Candidature> ObtenirParEtudiant(int idEtudiant)
        {
            string sql = @"
                SELECT * FROM VW_CandidaturesDetail
                WHERE IdEtudiant = (SELECT Id FROM ProfilsEtudiants WHERE IdUtilisateur = @IdUtil)
                ORDER BY DateDepot DESC";

            var p = new[] { new SqlParameter("@IdUtil", idEtudiant) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Candidature>();
            foreach (DataRow row in dt.Rows) liste.Add(MapRow(row));
            return liste;
        }

        /// <summary>Retourne les candidatures reçues pour une offre (côté entreprise).</summary>
        public List<Candidature> ObtenirParOffre(int idOffre)
        {
            string sql = "SELECT * FROM VW_CandidaturesDetail WHERE IdOffre = @IdOffre ORDER BY DateDepot DESC";
            var p = new[] { new SqlParameter("@IdOffre", idOffre) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Candidature>();
            foreach (DataRow row in dt.Rows) liste.Add(MapRow(row));
            return liste;
        }

        /// <summary>Change le statut d'une candidature (acceptee / refusee).</summary>
        public bool ChangerStatut(int id, string statut)
        {
            string sql = "UPDATE Candidatures SET Statut = @Statut WHERE Id = @Id";
            var p = new[]
            {
                new SqlParameter("@Id",     id),
                new SqlParameter("@Statut", statut)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Retire une candidature (étudiant).</summary>
        public bool Retirer(int id, int idEtudiant)
        {
            string sql = "DELETE FROM Candidatures WHERE Id = @Id AND IdEtudiant = @IdEtudiant";
            var p = new[]
            {
                new SqlParameter("@Id",         id),
                new SqlParameter("@IdEtudiant", idEtudiant)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Vérifie si un étudiant a déjà postulé à une offre.</summary>
        public bool DejaPostule(int idEtudiant, int idOffre)
        {
            string sql = "SELECT COUNT(*) FROM Candidatures WHERE IdEtudiant = @IdEtudiant AND IdOffre = @IdOffre";
            var p = new[]
            {
                new SqlParameter("@IdEtudiant", idEtudiant),
                new SqlParameter("@IdOffre",    idOffre)
            };
            return (int)DatabaseHelper.ExecuteScalar(sql, p) > 0;
        }
    }

    // ============================================================
    //  Fichier : DAL/MessageDAL.cs
    // ============================================================
    public class MessageDAL
    {
        private Message MapRow(DataRow row)
        {
            return new Message
            {
                Id             = (int)row["Id"],
                IdExpediteur   = (int)row["IdExpediteur"],
                IdDestinataire = (int)row["IdDestinataire"],
                Contenu        = row["Contenu"].ToString(),
                Lu             = (bool)row["Lu"],
                DateEnvoi      = (DateTime)row["DateEnvoi"],
                Expediteur = row.Table.Columns.Contains("NomExpediteur") ? new Utilisateur
                {
                    Nom    = row["NomExpediteur"].ToString(),
                    Prenom = row["PrenomExpediteur"].ToString()
                } : null
            };
        }

        /// <summary>Envoie un message.</summary>
        public bool Envoyer(Message m)
        {
            string sql = @"
                INSERT INTO Messages (IdExpediteur, IdDestinataire, Contenu)
                VALUES (@IdExpediteur, @IdDestinataire, @Contenu)";

            var p = new[]
            {
                new SqlParameter("@IdExpediteur",   m.IdExpediteur),
                new SqlParameter("@IdDestinataire", m.IdDestinataire),
                new SqlParameter("@Contenu",        m.Contenu)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Retourne la conversation entre deux utilisateurs.</summary>
        public List<Message> ObtenirConversation(int idUser1, int idUser2)
        {
            string sql = @"
                SELECT m.*, u.Nom AS NomExpediteur, u.Prenom AS PrenomExpediteur
                FROM Messages m
                INNER JOIN Utilisateurs u ON m.IdExpediteur = u.Id
                WHERE (m.IdExpediteur = @U1 AND m.IdDestinataire = @U2)
                   OR (m.IdExpediteur = @U2 AND m.IdDestinataire = @U1)
                ORDER BY m.DateEnvoi ASC";

            var p = new[]
            {
                new SqlParameter("@U1", idUser1),
                new SqlParameter("@U2", idUser2)
            };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Message>();
            foreach (DataRow row in dt.Rows) liste.Add(MapRow(row));
            return liste;
        }

        /// <summary>Compte les messages non lus d'un utilisateur.</summary>
        public int CompterNonLus(int idDestinataire)
        {
            string sql = "SELECT COUNT(*) FROM Messages WHERE IdDestinataire = @Id AND Lu = 0";
            var p = new[] { new SqlParameter("@Id", idDestinataire) };
            return (int)DatabaseHelper.ExecuteScalar(sql, p);
        }

        /// <summary>Marque tous les messages d'une conversation comme lus.</summary>
        public void MarquerLus(int idExpediteur, int idDestinataire)
        {
            string sql = @"
                UPDATE Messages SET Lu = 1
                WHERE IdExpediteur = @IdExp AND IdDestinataire = @IdDest AND Lu = 0";
            var p = new[]
            {
                new SqlParameter("@IdExp",  idExpediteur),
                new SqlParameter("@IdDest", idDestinataire)
            };
            DatabaseHelper.ExecuteNonQuery(sql, p);
        }

        /// <summary>Retourne la liste des interlocuteurs d'un utilisateur.</summary>
        public List<Utilisateur> ObtenirInterlocuteurs(int idUtilisateur)
        {
            string sql = @"
                SELECT DISTINCT u.Id, u.Nom, u.Prenom, u.Email, u.Role
                FROM Messages m
                INNER JOIN Utilisateurs u ON (
                    CASE WHEN m.IdExpediteur = @Id THEN m.IdDestinataire
                         ELSE m.IdExpediteur END = u.Id)
                WHERE m.IdExpediteur = @Id OR m.IdDestinataire = @Id";

            var p = new[] { new SqlParameter("@Id", idUtilisateur) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Utilisateur>();
            foreach (DataRow row in dt.Rows)
                liste.Add(new Utilisateur
                {
                    Id     = (int)row["Id"],
                    Nom    = row["Nom"].ToString(),
                    Prenom = row["Prenom"].ToString(),
                    Email  = row["Email"].ToString(),
                    Role   = row["Role"].ToString()
                });
            return liste;
        }
    }
}
