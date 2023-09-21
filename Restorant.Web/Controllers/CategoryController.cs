using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;

namespace Restorant.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult Create(Category category, List<SubCategory> subCategories)
        {


            if (_unitOfWork.Categories.GetAll().Any(c => c.Name == category.Name))
            {
                return UnprocessableEntity("Bu kategori adı zaten kullanılıyor.");
            }
            else
            {
                _unitOfWork.Categories.Add(category);
                _unitOfWork.Save();

                Category addedCategory = _unitOfWork.Categories.GetAll(c => c.Name == category.Name).Include(c => c.SubCategories).First();
                addedCategory.SubCategories = subCategories;


                _unitOfWork.Categories.Update(addedCategory);
                _unitOfWork.Save();
                return Ok();
            }

        }

        public IActionResult GetAll()
        {
            return Json(new { Data = _unitOfWork.Categories.GetAll().Include(c => c.SubCategories) });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            return Json(_unitOfWork.Categories.GetAll(c=>c.Id == id).Include(c => c.SubCategories).First() );
        }


        [Authorize(Roles = "Admin"), HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.Categories.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }


        [Authorize(Roles = "Admin"), HttpPatch]
        public IActionResult Update(Category category, List<int> subCategoryIds)
        {
            Category asil = _unitOfWork.Categories.GetFirstOrDefault(c=>c.Id == category.Id);

            List<SubCategory> subCategories = subCategoryIds.Select(id => _unitOfWork.SubCategories.GetFirstOrDefault(sc => sc.Id == id)).ToList();

            if (category == null || asil == null || category.Id == 0)
            {
                return UnprocessableEntity("Kategori Bulunamadı");
            }
            else
            {
                if (_unitOfWork.Categories.GetAll(c=>c.Id != asil.Id).Any(c => c.Name == category.Name))
                {
                    return UnprocessableEntity("Bu kategori adı zaten kullanılıyor.");
                }
                else
                {

                    asil.SubCategories = subCategories;
                    asil.Name = category.Name;

                    _unitOfWork.Categories.Update(asil);
                    _unitOfWork.Save();
                    return Ok();
                }
            }
        }

        //menu sayfası için
        public IActionResult GetAllWithSubCategory()
        {
            return Json(_unitOfWork.Categories.GetAll().Include(c => c.SubCategories).ThenInclude(sc=>sc.Products));
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult AddSubCategoryToCategory(int categoryId, List<int> subCategoryIds)
        {
            Category asil = _unitOfWork.Categories.GetFirstOrDefault(c => c.Id == categoryId);

            List<SubCategory> subCategories = subCategoryIds.Select(id => _unitOfWork.SubCategories.GetFirstOrDefault(sc => sc.Id == id)).ToList();

            if (asil == null || categoryId == 0)
            {
                return UnprocessableEntity("Kategori Bulunamadı");
            }
            else if(subCategoryIds == null)
            {
                return UnprocessableEntity("Alt Kategori Seçilmedi");
            }
            else
            {
                    asil.SubCategories = subCategories;
                    _unitOfWork.Categories.Update(asil);
                    _unitOfWork.Save();
                    return Ok();
            }
        }


        [Authorize(Roles = "Admin"), HttpPatch]
        public IActionResult RemoveSubCategoryFromCategory(int subCategoryId) 
        {
            SubCategory deletedSubCategory = _unitOfWork.SubCategories.GetFirstOrDefault(sc => sc.Id == subCategoryId);

            if (subCategoryId == 0)
            {
                return UnprocessableEntity("Kategori Bulunamadı");
            }
            else
            {
                deletedSubCategory.CategoryId = null;
                _unitOfWork.SubCategories.Update(deletedSubCategory);
                _unitOfWork.Save();
                return Ok();
            }

        }
    }
}
