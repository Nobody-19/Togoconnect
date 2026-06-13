// ============================================================
//  TogoConnect – Couche Présentation (Controllers)
//  Fichier : Controllers/AccountController.cs
// ============================================================
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TogoConnect.BLL;
using TogoConnect.Models;

namespace TogoConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly UtilisateurService _service = new UtilisateurService();

        // GET: /Account/Connexion
        public ActionResult Connexion()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // POST: /Account/Connexion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Connexion(string email, string motDePasse, bool rememberMe = false)
        {
            var (succes, utilisateur, message) = _service.Connecter(email, motDePasse);
            if (!succes)
            {
                ViewBag.Erreur = message;
                return View();
            }

            // Stocker infos en session
            Session["UserId"]   = utilisateur.Id;
            Session["UserNom"]  = utilisateur.NomComplet;
            Session["UserRole"] = utilisateur.Role;

            FormsAuthentication.SetAuthCookie(utilisateur.Email, rememberMe);
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/Inscription
        public ActionResult Inscription()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // POST: /Account/Inscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inscription(Utilisateur u, string confirmerMotDePasse)
        {
            if (u.MotDePasse != confirmerMotDePasse)
            {
                ViewBag.Erreur = "Les mots de passe ne correspondent pas.";
                return View(u);
            }

            if (!ModelState.IsValid) return View(u);

            var (succes, message) = _service.Inscrire(u);
            if (!succes)
            {
                ViewBag.Erreur = message;
                return View(u);
            }

            ViewBag.Succes = message;
            return RedirectToAction("Connexion");
        }

        // GET: /Account/Deconnexion
        public ActionResult Deconnexion()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Connexion");
        }
    }

    // ============================================================
    //  Fichier : Controllers/OffresController.cs
    // ============================================================
    [Authorize]
    public class OffresController : Controller
    {
        private readonly OffreService      _offreService = new OffreService();
        private readonly CandidatureService _candService  = new CandidatureService();

        private int UserId   => (int)Session["UserId"];
        private string Role  => Session["UserRole"].ToString();

        // GET: /Offres
        public ActionResult Index(string type = null, string localisation = null, string recherche = null)
        {
            var offres = _offreService.ObtenirOffresActives(type, localisation, recherche);
            ViewBag.Filtres = new { type, localisation, recherche };
            return View(offres);
        }

        // GET: /Offres/Details/5
        public ActionResult Details(int id)
        {
            var offre = _offreService.ObtenirParId(id);
            if (offre == null) return HttpNotFound();
            return View(offre);
        }

        // GET: /Offres/Creer (entreprise seulement)
        public ActionResult Creer()
        {
            if (Role != "entreprise") return RedirectToAction("Index");
            return View();
        }

        // POST: /Offres/Creer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Creer(Offre o)
        {
            if (Role != "entreprise") return RedirectToAction("Index");
            if (!ModelState.IsValid) return View(o);

            // Récupérer l'IdEntreprise de l'utilisateur connecté
            var dalEnt = new DAL.DatabaseHelper();
            string sql = "SELECT Id FROM ProfilsEntreprises WHERE IdUtilisateur = @Id";
            var dt = DAL.DatabaseHelper.ExecuteQuery(sql,
                new[] { new System.Data.SqlClient.SqlParameter("@Id", UserId) });

            if (dt.Rows.Count == 0) { ViewBag.Erreur = "Profil entreprise introuvable."; return View(o); }
            o.IdEntreprise = (int)dt.Rows[0]["Id"];

            var (succes, message) = _offreService.Publier(o);
            if (!succes) { ViewBag.Erreur = message; return View(o); }

            TempData["Succes"] = message;
            return RedirectToAction("MesOffres");
        }

        // GET: /Offres/MesOffres
        public ActionResult MesOffres()
        {
            if (Role != "entreprise") return RedirectToAction("Index");
            string sql = "SELECT Id FROM ProfilsEntreprises WHERE IdUtilisateur = @Id";
            var dt = DAL.DatabaseHelper.ExecuteQuery(sql,
                new[] { new System.Data.SqlClient.SqlParameter("@Id", UserId) });
            if (dt.Rows.Count == 0) return View(new System.Collections.Generic.List<Offre>());

            int idEntreprise = (int)dt.Rows[0]["Id"];
            var offres = _offreService.ObtenirParEntreprise(idEntreprise);
            return View(offres);
        }

        // POST: /Offres/Supprimer/5
        [HttpPost]
        public ActionResult Supprimer(int id)
        {
            if (Role != "entreprise") return Json(new { succes = false });
            string sql = "SELECT Id FROM ProfilsEntreprises WHERE IdUtilisateur = @Id";
            var dt = DAL.DatabaseHelper.ExecuteQuery(sql,
                new[] { new System.Data.SqlClient.SqlParameter("@Id", UserId) });
            if (dt.Rows.Count == 0) return Json(new { succes = false });

            int idEntreprise = (int)dt.Rows[0]["Id"];
            var (succes, message) = _offreService.Supprimer(id, idEntreprise);
            return Json(new { succes, message });
        }

        // POST: /Offres/Postuler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Postuler(int idOffre, string lettreMotivation)
        {
            if (Role != "etudiant") { TempData["Erreur"] = "Seuls les étudiants peuvent postuler."; return RedirectToAction("Details", new { id = idOffre }); }

            string sql = "SELECT Id FROM ProfilsEtudiants WHERE IdUtilisateur = @Id";
            var dt = DAL.DatabaseHelper.ExecuteQuery(sql,
                new[] { new System.Data.SqlClient.SqlParameter("@Id", UserId) });
            if (dt.Rows.Count == 0) { TempData["Erreur"] = "Complétez votre profil étudiant d'abord."; return RedirectToAction("Details", new { id = idOffre }); }

            int idEtudiant = (int)dt.Rows[0]["Id"];
            var c = new Candidature { IdEtudiant = idEtudiant, IdOffre = idOffre, LettreMotivation = lettreMotivation };
            var (succes, message) = _candService.Deposer(c);

            TempData[succes ? "Succes" : "Erreur"] = message;
            return RedirectToAction("Details", new { id = idOffre });
        }
    }
}
