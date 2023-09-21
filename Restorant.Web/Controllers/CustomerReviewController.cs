using Microsoft.AspNetCore.Mvc;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Xml;

namespace Restorant.Web.Controllers
{
    public class CustomerReviewController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerReviewController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CustomerReview(int tableId)
        {
            var DeliveredOrderProducts = _unitOfWork.Tables
                .GetAll(t => t.Id == tableId)
                .SelectMany(t => t.Orders).Where(o => o.Status == "Aktif").Select(o => new
                {
                    OrderProducts = o.OrderProducts.Where(op => op.Status == "Teslim Edildi"),
                });

            if (DeliveredOrderProducts.Count() > 0)
            {
                return View();
            }

            return View("Siparişiniz mayasa ulaşmadan yorum ekleyemezsiniz");
        }

       
        public IActionResult GetAll(int productId)
        {
            return Json(new { data = _unitOfWork.CustomerReviews.GetAll(cr=>cr.ProductId == productId && cr.IsApproved == true) });
        }

        [Authorize(Roles ="Admin")]
        public IActionResult Delete(int id)
        {
            if (id != 0)
            {
                _unitOfWork.CustomerReviews.Remove(id);
                _unitOfWork.Save();
                return Ok();

            }
            return BadRequest();

        }


        public IActionResult Create(CustomerReview customerReview, int tableId)
        {
            if (customerReview != null)
            {
                Table table = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == tableId);
                if (table != null)
                {
                    Order order = _unitOfWork.Orders.GetFirstOrDefault(o => o.Id == tableId && o.Status != "Tamamlandı");
                    if(order != null)
                    {
                        List<OrderProduct> orderProducts = _unitOfWork.OrderProducts.GetAll(op => op.OrderId == order.Id).ToList();
                        if (orderProducts != null)
                        {
                            if(orderProducts.FirstOrDefault(op => op.ProductId == customerReview.ProductId).Quantity >= orderProducts.FirstOrDefault(op => op.ProductId == customerReview.ProductId).CustomerReviewCount)
                            {
                                _unitOfWork.CustomerReviews.Add(customerReview);
                                _unitOfWork.Save();
                                return Ok();
                            }
                            else
                            {
                                return BadRequest("Daha fazla yorum yapamazsınız");
                            }
                        }
                        else
                        {
                            return BadRequest("Sipariş bulunamadı");

                        }
                    }
                    else
                    {
                        return BadRequest("Adisyon bulunamadı");
                    }

                }
                else
                {
                    return BadRequest("Masa bulunamadı");
                }

            }
            return BadRequest();
        }
    }
}

