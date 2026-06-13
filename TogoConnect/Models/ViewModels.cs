// ============================================================
//  TogoConnect – ViewModels
//  Fichier : Models/ViewModels.cs
// ============================================================
using System.Collections.Generic;
using TogoConnect.Models;

namespace TogoConnect.ViewModels
{
    public class DashboardVM
    {
        public string Role            { get; set; }
        public string NomUtilisateur  { get; set; }
        public int    MessagesNonLus  { get; set; }

        // Étudiant
        public int              TotalCandidatures   { get; set; }
        public int              EnAttente           { get; set; }
        public int              Acceptees           { get; set; }
        public int              Refusees            { get; set; }
        public List<Candidature> CandidaturesRecentes { get; set; }
        public List<Offre>      DernieresOffres     { get; set; }

        // Entreprise
        public int          TotalOffres              { get; set; }
        public int          OffresActives            { get; set; }
        public int          TotalCandidaturesRecues  { get; set; }
        public List<Offre>  DernieresOffresEnt       { get; set; }
    }
}

// ============================================================
//  Fichier : App_Start/RouteConfig.cs
// ============================================================
namespace TogoConnect
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

// ============================================================
//  Fichier : Global.asax.cs
// ============================================================
namespace TogoConnect
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }
    }

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
