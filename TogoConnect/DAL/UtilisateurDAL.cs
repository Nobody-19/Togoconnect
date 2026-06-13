// ============================================================
//  TogoConnect – Couche Données (DAL)
//  Fichier : DAL/UtilisateurDAL.cs
// ============================================================
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using TogoConnect.Models;

namespace TogoConnect.DAL
{
    public class UtilisateurDAL
    {
        /// <summary>Crée un nouveau compte utilisateur.</summary>
        public bool Inscrire(Utilisateur u)
        {
            string sql = @"
                INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasse, Role)
                VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Role)";

            var p = new[]
            {
                new SqlParameter("@Nom",        u.Nom),
                new SqlParameter("@Prenom",     u.Prenom),
                new SqlParameter("@Email",      u.Email),
                new SqlParameter("@MotDePasse", HashPassword(u.MotDePasse)),
                new SqlParameter("@Role",       u.Role)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
        }

        /// <summary>Vérifie les identifiants et retourne l'utilisateur ou null.</summary>
        public Utilisateur Connecter(string email, string motDePasse)
        {
            string sql = @"
                SELECT Id, Nom, Prenom, Email, Role, DateInscription
                FROM Utilisateurs
                WHERE Email = @Email AND MotDePasse = @MotDePasse AND EstActif = 1";

            var p = new[]
            {
                new SqlParameter("@Email",      email),
                new SqlParameter("@MotDePasse", HashPassword(motDePasse))
            };

            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new Utilisateur
            {
                Id              = (int)row["Id"],
                Nom             = row["Nom"].ToString(),
                Prenom          = row["Prenom"].ToString(),
                Email           = row["Email"].ToString(),
                Role            = row["Role"].ToString(),
                DateInscription = (System.DateTime)row["DateInscription"]
            };
        }

        /// <summary>Vérifie si un email est déjà enregistré.</summary>
        public bool EmailExiste(string email)
        {
            string sql = "SELECT COUNT(*) FROM Utilisateurs WHERE Email = @Email";
            var p = new[] { new SqlParameter("@Email", email) };
            return (int)DatabaseHelper.ExecuteScalar(sql, p) > 0;
        }

        /// <summary>Retourne un utilisateur par son Id.</summary>
        public Utilisateur ObtenirParId(int id)
        {
            string sql = "SELECT Id, Nom, Prenom, Email, Role FROM Utilisateurs WHERE Id = @Id";
            var p = new[] { new SqlParameter("@Id", id) };
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new Utilisateur
            {
                Id     = (int)row["Id"],
                Nom    = row["Nom"].ToString(),
                Prenom = row["Prenom"].ToString(),
                Email  = row["Email"].ToString(),
                Role   = row["Role"].ToString()
            };
        }

        /// <summary>Hash SHA-256 du mot de passe.</summary>
        public static string HashPassword(string motDePasse)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(motDePasse));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
