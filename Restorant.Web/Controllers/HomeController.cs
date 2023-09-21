using Microsoft.AspNetCore.Mvc;
using Restorant.Models;
using System.Diagnostics;

namespace Restorant.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}