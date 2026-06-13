// ============================================================
//  TogoConnect – Couche Métier (BLL)
//  Fichier : BLL/UtilisateurService.cs + OffreService.cs
//  Rôle    : Logique métier, validation, règles applicatives
// ============================================================
using System;
using System.Collections.Generic;
using System.Web;
using TogoConnect.DAL;
using TogoConnect.Models;

namespace TogoConnect.BLL
{
    // ────────────────────────────────────────────────────────────
    //  Service : UtilisateurService
    // ────────────────────────────────────────────────────────────
    public class UtilisateurService
    {
        private readonly UtilisateurDAL _dal = new UtilisateurDAL();

        public (bool succes, string message) Inscrire(Utilisateur u)
        {
            if (_dal.EmailExiste(u.Email))
                return (false, "Cet email est déjà utilisé.");

            if (u.MotDePasse.Length < 8)
                return (false, "Le mot de passe doit contenir au moins 8 caractères.");

            bool ok = _dal.Inscrire(u);
            return ok
                ? (true, "Inscription réussie ! Vous pouvez vous connecter.")
                : (false, "Une erreur est survenue lors de l'inscription.");
        }

        public (bool succes, Utilisateur utilisateur, string message) Connecter(string email, string motDePasse)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(motDePasse))
                return (false, null, "Email et mot de passe sont obligatoires.");

            var u = _dal.Connecter(email, motDePasse);
            if (u == null)
                return (false, null, "Email ou mot de passe incorrect.");

            return (true, u, "Connexion réussie.");
        }

        public Utilisateur ObtenirParId(int id) => _dal.ObtenirParId(id);
    }

    // ────────────────────────────────────────────────────────────
    //  Service : OffreService
    // ────────────────────────────────────────────────────────────
    public class OffreService
    {
        private readonly OffreDAL _dal = new OffreDAL();

        public List<Offre> ObtenirOffresActives(string type = null, string localisation = null, string recherche = null)
        {
            _dal.ArchiverExpirees();  // Archivage automatique avant affichage
            return _dal.ObtenirOffresActives(type, localisation, recherche);
        }

        public List<Offre> ObtenirParEntreprise(int idEntreprise)
            => _dal.ObtenirParEntreprise(idEntreprise);

        public Offre ObtenirParId(int id) => _dal.ObtenirParId(id);

        public (bool succes, string message) Publier(Offre o)
        {
            if (o.DateLimite <= DateTime.Today)
                return (false, "La date limite doit être dans le futur.");

            bool ok = _dal.Publier(o);
            return ok
                ? (true, "Offre publiée avec succès.")
                : (false, "Erreur lors de la publication.");
        }

        public (bool succes, string message) Modifier(Offre o)
        {
            if (o.DateLimite <= DateTime.Today)
                return (false, "La date limite doit être dans le futur.");

            bool ok = _dal.Modifier(o);
            return ok
                ? (true, "Offre mise à jour.")
                : (false, "Erreur lors de la mise à jour.");
        }

        public (bool succes, string message) Supprimer(int id, int idEntreprise)
        {
            bool ok = _dal.Supprimer(id, idEntreprise);
            return ok
                ? (true, "Offre supprimée.")
                : (false, "Impossible de supprimer cette offre.");
        }
    }

    // ────────────────────────────────────────────────────────────
    //  Service : CandidatureService
    // ────────────────────────────────────────────────────────────
    public class CandidatureService
    {
        private readonly CandidatureDAL _dal = new CandidatureDAL();

        public (bool succes, string message) Deposer(Candidature c)
        {
            if (_dal.DejaPostule(c.IdEtudiant, c.IdOffre))
                return (false, "Vous avez déjà postulé à cette offre.");

            bool ok = _dal.Deposer(c);
            return ok
                ? (true, "Candidature déposée avec succès !")
                : (false, "Erreur lors du dépôt de candidature.");
        }

        public List<Candidature> ObtenirParEtudiant(int idEtudiant)
            => _dal.ObtenirParEtudiant(idEtudiant);

        public List<Candidature> ObtenirParOffre(int idOffre)
            => _dal.ObtenirParOffre(idOffre);

        public (bool succes, string message) ChangerStatut(int id, string statut)
        {
            var statutsValides = new[] { "acceptee", "refusee", "en_attente" };
            if (!Array.Exists(statutsValides, s => s == statut))
                return (false, "Statut invalide.");

            bool ok = _dal.ChangerStatut(id, statut);
            return ok ? (true, "Statut mis à jour.") : (false, "Erreur de mise à jour.");
        }

        public (bool succes, string message) Retirer(int id, int idEtudiant)
        {
            bool ok = _dal.Retirer(id, idEtudiant);
            return ok
                ? (true, "Candidature retirée.")
                : (false, "Impossible de retirer cette candidature.");
        }
    }

    // ────────────────────────────────────────────────────────────
    //  Service : MessageService
    // ────────────────────────────────────────────────────────────
    public class MessageService
    {
        private readonly MessageDAL _dal = new MessageDAL();

        public (bool succes, string message) Envoyer(Message m)
        {
            if (string.IsNullOrWhiteSpace(m.Contenu))
                return (false, "Le message ne peut pas être vide.");

            if (m.Contenu.Length > 2000)
                return (false, "Le message est trop long (max 2000 caractères).");

            bool ok = _dal.Envoyer(m);
            return ok ? (true, "Message envoyé.") : (false, "Erreur d'envoi.");
        }

        public List<Message> ObtenirConversation(int idUser1, int idUser2)
        {
            _dal.MarquerLus(idUser1, idUser2);  // Marquer comme lus à l'ouverture
            return _dal.ObtenirConversation(idUser1, idUser2);
        }

        public int CompterNonLus(int idUtilisateur) => _dal.CompterNonLus(idUtilisateur);

        public List<Utilisateur> ObtenirInterlocuteurs(int idUtilisateur)
            => _dal.ObtenirInterlocuteurs(idUtilisateur);
    }
}
