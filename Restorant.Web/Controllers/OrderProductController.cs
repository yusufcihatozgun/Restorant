using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using System.Data;
using System.Security.Claims;

namespace Restorant.Web.Controllers
{
    public class OrderProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = "Admin,Aşçı,Garson")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles ="Admin,Garson,Aşçı")]
        public IActionResult GetAll()
        {
            DateTime dateTime = DateTime.Now.AddHours(-1);

            return Json(new 
            {
                data = _unitOfWork.OrderProducts.GetAll(op=>op.DateCreated > dateTime || op.Status != "Teslim Edildi").Select(op => new
                {
                    orderProductId = op.Id,
                    productName = op.Product.Name,
                    orderProductPortion = op.Portion,
                    orderProductQuantity = op.Quantity,
                    orderProductStatus = op.Status,
                    tableName = op.Order.Table.Name
                })
            });
        }

        [Authorize(Roles = "Admin,Aşçı,Garson"), HttpPost]
        public IActionResult ChangeStatus(int orderProductId, string status)
        {
            var orderProduct = _unitOfWork.OrderProducts.GetFirstOrDefault(op => op.Id == orderProductId);

            if (orderProduct == null)
            {
                return BadRequest("Sipariş bulunamadı");
            }
            else
            {
                orderProduct.Status = status;
                _unitOfWork.OrderProducts.Update(orderProduct);
                _unitOfWork.Save();
                return Ok();
            }
        }

        [Authorize(Roles = "Admin,Garson"), HttpPost]
        public IActionResult Create(OrderProduct orderProduct)
        {
            if (orderProduct == null || orderProduct.ProductId == null)
            {
                return UnprocessableEntity("Lütfen ürün seçiniz");            
            }
            else if(orderProduct.Portion == null)
            {
                return UnprocessableEntity("Porsiyon bilgisi boş bırakılamaz");
            }
            else
            {
                _unitOfWork.OrderProducts.Add(orderProduct);
                _unitOfWork.Save();
                return Ok();
            }
        }


        [Authorize(Roles = "Admin,Garson"), HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.OrderProducts.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }
    }
}
