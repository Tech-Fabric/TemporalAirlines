using Microsoft.AspNetCore.Mvc;

namespace TemporalAirlinesConcept.Web.Controllers;

public class HomeController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}
