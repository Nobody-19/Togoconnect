// ============================================================
//  TogoConnect – Couche Modèles (Entités)
//  Fichier : Models/Utilisateur.cs
// ============================================================
using System;
using System.ComponentModel.DataAnnotations;

namespace TogoConnect.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(8, ErrorMessage = "Minimum 8 caractères")]
        public string MotDePasse { get; set; }

        [Required]
        public string Role { get; set; }  // "etudiant" | "entreprise"

        public bool EstActif { get; set; } = true;
        public DateTime DateInscription { get; set; } = DateTime.Now;

        public string NomComplet => $"{Prenom} {Nom}";
    }

    public class ProfilEtudiant
    {
        public int Id { get; set; }
        public int IdUtilisateur { get; set; }
        public string Filiere { get; set; }
        public string Etablissement { get; set; }
        public string Biographie { get; set; }
        public string CheminCV { get; set; }
        public DateTime? DateNaissance { get; set; }
        public string Ville { get; set; }
        public string Telephone { get; set; }

        // Navigation
        public Utilisateur Utilisateur { get; set; }
    }

    public class ProfilEntreprise
    {
        public int Id { get; set; }
        public int IdUtilisateur { get; set; }

        [Required(ErrorMessage = "La raison sociale est obligatoire")]
        public string RaisonSociale { get; set; }
        public string Secteur { get; set; }
        public string Description { get; set; }
        public string SiteWeb { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string CheminLogo { get; set; }

        // Navigation
        public Utilisateur Utilisateur { get; set; }
    }

    public class Offre
    {
        public int Id { get; set; }
        public int IdEntreprise { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        public string Titre { get; set; }

        [Required(ErrorMessage = "Le type est obligatoire")]
        public string TypeOffre { get; set; }  // emploi | stage | formation | evenement

        [Required(ErrorMessage = "La description est obligatoire")]
        public string Description { get; set; }

        public string Competences { get; set; }
        public string Localisation { get; set; }

        [Required(ErrorMessage = "La date limite est obligatoire")]
        public DateTime DateLimite { get; set; }

        public string Statut { get; set; } = "active";  // active | expiree | archivee
        public DateTime DatePublication { get; set; } = DateTime.Now;

        // Navigation
        public ProfilEntreprise Entreprise { get; set; }
        public bool EstExpiree => DateLimite < DateTime.Today;
    }

    public class Candidature
    {
        public int Id { get; set; }
        public int IdEtudiant { get; set; }
        public int IdOffre { get; set; }
        public string LettreMotivation { get; set; }
        public string Statut { get; set; } = "en_attente";  // en_attente | acceptee | refusee
        public DateTime DateDepot { get; set; } = DateTime.Now;

        // Navigation
        public ProfilEtudiant Etudiant { get; set; }
        public Offre Offre { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }
        public int IdExpediteur { get; set; }
        public int IdDestinataire { get; set; }

        [Required(ErrorMessage = "Le message ne peut pas être vide")]
        public string Contenu { get; set; }

        public bool Lu { get; set; } = false;
        public DateTime DateEnvoi { get; set; } = DateTime.Now;

        // Navigation
        public Utilisateur Expediteur { get; set; }
        public Utilisateur Destinataire { get; set; }
    }
}
