// ============================================================
//  TogoConnect – Couche Données (DAL)
//  Fichier : DAL/OffreDAL.cs
// ============================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TogoConnect.Models;

namespace TogoConnect.DAL
{
    public class OffreDAL
    {
        private Offre MapRow(DataRow row)
        {
            return new Offre
            {
                Id              = (int)row["Id"],
                IdEntreprise    = (int)row["IdEntreprise"],
                Titre           = row["Titre"].ToString(),
                TypeOffre       = row["TypeOffre"].ToString(),
                Description     = row["Description"].ToString(),
                Competences     = row["Competences"] != DBNull.Value ? row["Competences"].ToString() : "",
                Localisation    = row["Localisation"] != DBNull.Value ? row["Localisation"].ToString() : "",
                DateLimite      = (DateTime)row["DateLimite"],
                Statut          = row["Statut"].ToString(),
                DatePublication = (DateTime)row["DatePublication"],
                Entreprise = row.Table.Columns.Contains("NomEntreprise") ? new ProfilEntreprise
                {
                    RaisonSociale = row["NomEntreprise"].ToString(),
                    Secteur       = row["SecteurEntreprise"].ToString(),
                    CheminLogo    = row["LogoEntreprise"] != DBNull.Value ? row["LogoEntreprise"].ToString() : ""
                } : null
            };
        }

        /// <summary>Retourne toutes les offres actives avec filtre optionnel.</summary>
        public List<Offre> ObtenirOffresActives(string type = null, string localisation = null, string recherche = null)
        {
            string sql = @"
                SELECT * FROM VW_OffresAvecEntreprise
                WHERE Statut = 'active' AND DateLimite >= GETDATE()
                AND (@Type IS NULL OR TypeOffre = @Type)
                AND (@Localisation IS NULL OR Localisation LIKE '%' + @Localisation + '%')
                AND (@Recherche IS NULL OR Titre LIKE '%' + @Recherche + '%'
                     OR Description LIKE '%' + @Recherche + '%')
                ORDER BY DatePublication DESC";

            var p = new[]
            {
                new SqlParameter("@Type",        (object)type ?? DBNull.Value),
                new SqlParameter("@Localisation",(object)localisation ?? DBNull.Value),
                new SqlParameter("@Recherche",   (object)recherche ?? DBNull.Value)
            };

            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Offre>();
            foreach (DataRow row in dt.Rows) liste.Add(MapRow(row));
            return liste;
        }

        /// <summary>Retourne les offres d'une entreprise.</summary>
        public List<Offre> ObtenirParEntreprise(int idEntreprise)
        {
            string sql = @"
                SELECT * FROM VW_OffresAvecEntreprise
                WHERE IdEntreprise = @IdEntreprise
                ORDER BY DatePublication DESC";

            var p = new[] { new SqlParameter("@IdEntreprise", idEntreprise) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var liste = new List<Offre>();
            foreach (DataRow row in dt.Rows) liste.Add(MapRow(row));
            return liste;
        }

        /// <summary>Retourne une offre par son Id.</summary>
        public Offre ObtenirParId(int id)
        {
            string sql = "SELECT * FROM VW_OffresAvecEntreprise WHERE Id = @Id";
            var p = new[] { new SqlParameter("@Id", id) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            return dt.Rows.Count > 0 ? MapRow(dt.Rows[0]) : null;
        }

        /// <summary>Publie une nouvelle offre.</summary>
        public bool Publier(Offre o)
        {
            string sql = @"
                INSERT INTO Offres (IdEntreprise, Titre, TypeOffre, Description, Competences, Localisation, DateLimite)
                VALUES (@IdEntreprise, @Titre, @TypeOffre, @Description, @Competences, @Localisation, @DateLimite)";

            var p = new[]
            {
                new SqlParameter("@IdEntreprise", o.IdEntreprise),
                new SqlParameter("@Titre",        o.Titre),
                new SqlParameter("@TypeOffre",    o.TypeOffre),
                new SqlParameter("@Description",  o.Description),
                new SqlParameter("@Competences",  (object)o.Competences  ?? DBNull.Value),
                new SqlParameter("@Localisation", (object)o.Localisation ?? DBNull.Value),
                new SqlParameter("@DateLimite",   o.DateLimite)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Modifie une offre existante.</summary>
        public bool Modifier(Offre o)
        {
            string sql = @"
                UPDATE Offres SET
                    Titre = @Titre, TypeOffre = @TypeOffre, Description = @Description,
                    Competences = @Competences, Localisation = @Localisation, DateLimite = @DateLimite
                WHERE Id = @Id AND IdEntreprise = @IdEntreprise";

            var p = new[]
            {
                new SqlParameter("@Id",           o.Id),
                new SqlParameter("@IdEntreprise", o.IdEntreprise),
                new SqlParameter("@Titre",        o.Titre),
                new SqlParameter("@TypeOffre",    o.TypeOffre),
                new SqlParameter("@Description",  o.Description),
                new SqlParameter("@Competences",  (object)o.Competences  ?? DBNull.Value),
                new SqlParameter("@Localisation", (object)o.Localisation ?? DBNull.Value),
                new SqlParameter("@DateLimite",   o.DateLimite)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Supprime une offre.</summary>
        public bool Supprimer(int id, int idEntreprise)
        {
            string sql = "DELETE FROM Offres WHERE Id = @Id AND IdEntreprise = @IdEntreprise";
            var p = new[]
            {
                new SqlParameter("@Id",           id),
                new SqlParameter("@IdEntreprise", idEntreprise)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Archive les offres expirées automatiquement.</summary>
        public int ArchiverExpirees()
        {
            string sql = "UPDATE Offres SET Statut = 'expiree' WHERE DateLimite < GETDATE() AND Statut = 'active'";
            return DatabaseHelper.ExecuteNonQuery(sql);
        }
    }
}
