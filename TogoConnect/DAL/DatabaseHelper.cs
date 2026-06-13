// ============================================================
//  TogoConnect – Couche Données (DAL)
//  Fichier : DAL/DatabaseHelper.cs
//  Rôle    : Gestion centralisée de la connexion ADO.NET
// ============================================================
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TogoConnect.DAL
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["TogoConnectDB"].ConnectionString;

        /// <summary>Ouvre et retourne une connexion SQL active.</summary>
        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>Exécute une requête INSERT/UPDATE/DELETE, retourne le nombre de lignes affectées.</summary>
        public static int ExecuteNonQuery(string sql, SqlParameter[] parametres = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parametres != null) cmd.Parameters.AddRange(parametres);
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Exécute une requête et retourne la première valeur (scalaire).</summary>
        public static object ExecuteScalar(string sql, SqlParameter[] parametres = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parametres != null) cmd.Parameters.AddRange(parametres);
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>Exécute une requête SELECT et retourne un DataTable.</summary>
        public static DataTable ExecuteQuery(string sql, SqlParameter[] parametres = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parametres != null) cmd.Parameters.AddRange(parametres);
                var dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(dt);
                return dt;
            }
        }
    }
}
