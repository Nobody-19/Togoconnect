// ============================================================
//  TogoConnect – Couche Présentation (Controllers)
//  Fichier : Controllers/DashboardController.cs
// ============================================================
using System.Web.Mvc;
using TogoConnect.BLL;
using TogoConnect.DAL;
using System.Data.SqlClient;

namespace TogoConnect.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly OffreService       _offreService = new OffreService();
        private readonly CandidatureService _candService  = new CandidatureService();
        private readonly MessageService     _msgService   = new MessageService();

        private int    UserId => (int)Session["UserId"];
        private string Role   => Session["UserRole"].ToString();

        public ActionResult Index()
        {
            var vm = new ViewModels.DashboardVM();
            vm.Role          = Role;
            vm.NomUtilisateur = Session["UserNom"].ToString();
            vm.MessagesNonLus = _msgService.CompterNonLus(UserId);

            if (Role == "etudiant")
            {
                var candidatures = _candService.ObtenirParEtudiant(UserId);
                vm.TotalCandidatures  = candidatures.Count;
                vm.CandidaturesRecentes = candidatures.GetRange(0, System.Math.Min(5, candidatures.Count));
                vm.EnAttente = candidatures.FindAll(c => c.Statut == "en_attente").Count;
                vm.Acceptees = candidatures.FindAll(c => c.Statut == "acceptee").Count;
                vm.Refusees  = candidatures.FindAll(c => c.Statut == "refusee").Count;
                vm.DernieresOffres = _offreService.ObtenirOffresActives().GetRange(0, System.Math.Min(4, _offreService.ObtenirOffresActives().Count));
            }
            else if (Role == "entreprise")
            {
                string sql = "SELECT Id FROM ProfilsEntreprises WHERE IdUtilisateur = @Id";
                var dt = DatabaseHelper.ExecuteQuery(sql, new[] { new SqlParameter("@Id", UserId) });
                if (dt.Rows.Count > 0)
                {
                    int idEnt = (int)dt.Rows[0]["Id"];
                    var offres = _offreService.ObtenirParEntreprise(idEnt);
                    vm.TotalOffres        = offres.Count;
                    vm.OffresActives      = offres.FindAll(o => o.Statut == "active").Count;
                    vm.DernieresOffresEnt = offres.GetRange(0, System.Math.Min(5, offres.Count));

                    // Candidatures reçues pour toutes les offres
                    int totalCand = 0;
                    foreach (var offre in offres)
                        totalCand += _candService.ObtenirParOffre(offre.Id).Count;
                    vm.TotalCandidaturesRecues = totalCand;
                }
            }

            return View(vm);
        }
    }

    // ============================================================
    //  Fichier : Controllers/MessagesController.cs
    // ============================================================
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly MessageService     _msgService  = new MessageService();
        private readonly UtilisateurService _userService = new UtilisateurService();

        private int UserId => (int)Session["UserId"];

        // GET: /Messages
        public ActionResult Index()
        {
            var interlocuteurs = _msgService.ObtenirInterlocuteurs(UserId);
            return View(interlocuteurs);
        }

        // GET: /Messages/Conversation/5
        public ActionResult Conversation(int id)
        {
            var interlocuteur = _userService.ObtenirParId(id);
            if (interlocuteur == null) return HttpNotFound();

            var messages = _msgService.ObtenirConversation(id, UserId);
            ViewBag.Interlocuteur = interlocuteur;
            return View(messages);
        }

        // POST: /Messages/Envoyer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Envoyer(int idDestinataire, string contenu)
        {
            var msg = new Models.Message
            {
                IdExpediteur   = UserId,
                IdDestinataire = idDestinataire,
                Contenu        = contenu
            };

            var (succes, message) = _msgService.Envoyer(msg);
            if (!succes) TempData["Erreur"] = message;

            return RedirectToAction("Conversation", new { id = idDestinataire });
        }
    }

    // ============================================================
    //  Fichier : Controllers/CandidaturesController.cs
    // ============================================================
    [Authorize]
    public class CandidaturesController : Controller
    {
        private readonly CandidatureService _service = new CandidatureService();

        private int    UserId => (int)Session["UserId"];
        private string Role   => Session["UserRole"].ToString();

        // GET: /Candidatures (liste pour l'étudiant)
        public ActionResult Index()
        {
            if (Role != "etudiant") return RedirectToAction("Index", "Dashboard");
            var candidatures = _service.ObtenirParEtudiant(UserId);
            return View(candidatures);
        }

        // GET: /Candidatures/PourOffre/5 (pour l'entreprise)
        public ActionResult PourOffre(int id)
        {
            if (Role != "entreprise") return RedirectToAction("Index", "Dashboard");
            var candidatures = _service.ObtenirParOffre(id);
            ViewBag.IdOffre = id;
            return View(candidatures);
        }

        // POST: /Candidatures/ChangerStatut
        [HttpPost]
        public ActionResult ChangerStatut(int id, string statut)
        {
            if (Role != "entreprise") return Json(new { succes = false });
            var (succes, message) = _service.ChangerStatut(id, statut);
            return Json(new { succes, message });
        }

        // POST: /Candidatures/Retirer/5
        [HttpPost]
        public ActionResult Retirer(int id)
        {
            if (Role != "etudiant") return Json(new { succes = false });
            var (succes, message) = _service.Retirer(id, UserId);
            return Json(new { succes, message });
        }
    }
}
