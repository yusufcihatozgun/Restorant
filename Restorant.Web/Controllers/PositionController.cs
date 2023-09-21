using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;

namespace Restorant.Web.Controllers
{
    public class PositionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PositionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles ="Admin")]
        public IActionResult GetAll()
        {
            return Json(_unitOfWork.Positions.GetAll());
        }
    }
}
