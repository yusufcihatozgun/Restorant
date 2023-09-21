using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using System.Data;

namespace Restorant.Web.Controllers
{
    public class SubCategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubCategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult Create(SubCategory subCategory)
        {
            if (_unitOfWork.SubCategories.GetAll().Any(sc=>sc.Id == subCategory.Id))
            {
                return UnprocessableEntity("Bu alt kategori adı zaten kullanıyor.");
            }
            else
            {
                _unitOfWork.SubCategories.Add(subCategory);
                _unitOfWork.Save();
                return Ok();
            }
        }


        [Authorize(Roles ="Admin")]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.SubCategories.GetAll().Include(sc => sc.Category) });
        }


        //public IActionResult GetAllWithProducts(int id)
        //{
        //    return Json(_unitOfWork.SubCategories.GetAll(sc=>sc.Id == id).Include(sc=>sc.Products));
        //}


        [Authorize(Roles = "Admin"), HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0 )
            {
                return UnprocessableEntity("Alt kategori bulunamadı.");
            }
            else
            {
                _unitOfWork.SubCategories.Remove(id);
                _unitOfWork.Save();
                return Ok();
            }
           
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            return Json(_unitOfWork.SubCategories.GetFirstOrDefault(c => c.Id == id));
        }


        [Authorize(Roles = "Admin"), HttpPatch]
        public IActionResult Update(SubCategory subCategory)
        {
            SubCategory asil = _unitOfWork.SubCategories.GetFirstOrDefault(sc => sc.Id == subCategory.Id);

            if (subCategory == null || asil == null)
            {
                return UnprocessableEntity("Alt kategori bulunamadı.");
            }
            else
            {
                if (_unitOfWork.SubCategories.GetAll(sc=>sc.Id != subCategory.Id).Any(sc=>sc.Name == subCategory.Name))
                {
                    return UnprocessableEntity("Bu alt kategori adı zaten kullanılıyor.");
                }
                else
                {
                    asil.Name = subCategory.Name;
                    asil.CategoryId = subCategory.CategoryId;

                    _unitOfWork.SubCategories.Update(asil);
                    _unitOfWork.Save();
                    return Ok();
                }
            }
        }


        [Authorize(Roles = "Admin"), HttpPatch]
        public IActionResult DisconnectSubCategoryFromCategory(int id)
        {
            SubCategory asil = _unitOfWork.SubCategories.GetFirstOrDefault(sc => sc.Id == id);

            if (id == 0 || asil == null)
            {
                return UnprocessableEntity("Alt kategori bulunamadı.");
            }
            else
            {
                asil.CategoryId = null;
                _unitOfWork.SubCategories.Update(asil);
                _unitOfWork.Save();
                return Ok();
            }

        }

    }
}
