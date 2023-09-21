using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using Restorant.Repository.Shared.Concrete;
using System.Data;

namespace Restorant.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Menu()
        {
            return View();
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult Create(Product product)
        {
            if (_unitOfWork.Products.GetAll().Any(p => p.Name == product.Name))
            {
                return UnprocessableEntity("Bu ürün adı zaten kullanılıyor.");
            }
            else
            {
                _unitOfWork.Products.Add(product);
                _unitOfWork.Save();
                return Ok();
            }
        }


        public IActionResult GetAll([FromQuery(Name = "subCategoryId")] int? subCategoryId)
        {
            if (subCategoryId == null)
            {
                var products = _unitOfWork.Products.GetAll().Select(p => new
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SubCategoryId = p.SubCategoryId,
                    SubCategory = p.SubCategory,
                    ImageUrl = p.ImageUrl,
                    Portion05Price = p.Portion05Price,
                    Portion1Price = p.Portion1Price,
                    Portion15Price = p.Portion15Price,
                    Portion2Price = p.Portion2Price,
                    CustomerReviewScores = p.CustomerReviews.Select(cr=>cr.ReviewScore),
                    AverageCustomerReviewScore = p.CustomerReviews.Select(cr => cr.ReviewScore).Average(),
                    OrderCount = p.OrderProducts.Where(op => op.DateCreated > DateTime.Now.AddDays(-30)).Sum(orderProduct => orderProduct.Quantity),
                });
                return Json(products);

            }
            else
            {
                var productsWithOrderCount = _unitOfWork.Products.GetAll(p => p.SubCategoryId == subCategoryId).Select(p => new
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    SubCategoryId = p.SubCategoryId,
                    SubCategory = p.SubCategory,
                    ImageUrl = p.ImageUrl,
                    Portion05Price = p.Portion05Price,
                    Portion1Price = p.Portion1Price,
                    Portion15Price = p.Portion15Price,
                    Portion2Price = p.Portion2Price,
                    CustomerReviewScores = p.CustomerReviews.Select(cr => cr.ReviewScore),
                    AverageCustomerReviewScore = p.CustomerReviews.Select(cr => cr.ReviewScore).Average(),
                    OrderCount = p.OrderProducts.Where(op => op.DateCreated > DateTime.Now.AddDays(-30)).Sum(orderProduct => orderProduct.Quantity),
                });

                return Json(productsWithOrderCount);
            }
        }


        public IActionResult GetAllWithProjection()
        {
            
            var query = _unitOfWork.Products.GetAll()
            .Select(p => new
            {
                productId = p.Id,
                Name = p.Name,
            })
            .ToList();

            return Json(query);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetAllForDatatables()
        {
            return Json(new
            {
                data = _unitOfWork.Products.GetAll(p => p.IsDeleted == false).Include(p => p.SubCategory).Where(sc => sc.IsDeleted == false).Select(product => new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    subCategory = product.SubCategory.Name,
                    product.ImageUrl,
                    product.OrderProducts,
                    product.Portion05Price,
                    product.Portion1Price,
                    product.Portion15Price,
                    product.Portion2Price
                })
            });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            return Json(_unitOfWork.Products.GetFirstOrDefault(p => p.Id == id));
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult SaveImage(IFormFile image, int id)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Dosya yüklenemedi.");
            }
            else
            {
                Product product = _unitOfWork.Products.GetFirstOrDefault(p => p.Id == id);

                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "assets/images/product/");

                // Rastgele bir dosya adı oluşturma (dosyanın üzerine yazmamak için)
                string uniqueFileName = Guid.NewGuid().ToString() + image.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                // Dosyayı klasöre kaydetme
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    // Eğer mevcut resim dosyası varsa ve silinebilirse siliyoruz
                    if (System.IO.File.Exists(uploadFolder + product.ImageUrl))
                    {
                        System.IO.File.Delete(uploadFolder + product.ImageUrl);
                    }
                    image.CopyTo(fileStream);
                }

                // Dosya yolunu ve adını kombine ederek Product modeline kaydetme
                if (product != null)
                {
                    product.ImageUrl = uniqueFileName;
                    _unitOfWork.Products.Update(product);
                    _unitOfWork.Save();
                }

                return Ok();
            }
        }


        [Authorize(Roles = "Admin"), HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.Products.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult Update(Product product)
        {
            Product asil = _unitOfWork.Products.GetFirstOrDefault(p => p.Id == product.Id);

            if (product == null || asil == null)
            {
                return UnprocessableEntity("Ürün Bulunamadı");
            }
            else
            {
                if (_unitOfWork.Products.GetAll(p => p.Id != product.Id).Any(p => p.Name == product.Name))
                {
                    return UnprocessableEntity("Bu ürün adı zaten kullanılıyor.");
                }
                else
                {
                    asil.Name = product.Name;
                    asil.Portion05Price = product.Portion05Price;
                    asil.Portion1Price = product.Portion1Price;
                    asil.Portion15Price = product.Portion15Price;
                    asil.Portion2Price = product.Portion2Price;
                    asil.Description = product.Description;
                    asil.SubCategoryId = product.SubCategoryId;

                    _unitOfWork.Products.Update(asil);
                    _unitOfWork.Save();
                    return Ok();
                }
            }
        }
    }
}
