using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using System.Security.Claims;

namespace Restorant.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles ="Admin")]
        public IActionResult OrderArchive()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Garson"), HttpPost]
        public IActionResult Create(Order order)
        {
            _unitOfWork.Orders.Add(order);
            _unitOfWork.Save();
            return Ok(order.Id);
        }


        [Authorize]
        public IActionResult GetAll()
        {
            return Json(new
            {
                data = _unitOfWork.Orders.GetAll(o => o.IsDeleted == false).Select(o => new
                {
                    Id = o.Id,
                    Table = o.Table.Id,
                    AppUser = o.AppUser.FirstName +" " + o.AppUser.LastName,
                    Status = o.Status,
                    TotalPrice = o.TotalPrice + " TL",
                    Payments = o.Payments,
                    OrderProducts = o.OrderProducts
                })
            });
        }


        [Authorize(Roles ="Admin, Garson"), HttpPatch]
        public IActionResult ChangeTable(int orderId, int oldTableId, int newTableId) 
        {
            if(orderId != 0 && oldTableId != 0 && newTableId != 0)
            {
                Table oldTable = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == oldTableId);
                Table newTable = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == newTableId);
                Order order = _unitOfWork.Orders.GetFirstOrDefault(o => o.Id == orderId);
                order.TableId = newTableId;

                oldTable.Status = "Boş";
                newTable.Status = "Dolu";

                _unitOfWork.Tables.Update(newTable);
                _unitOfWork.Tables.Update(oldTable);
                _unitOfWork.Orders.Update(order);
                _unitOfWork.Save();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [Authorize(Roles = "Admin, Garson"), HttpPost]
        public IActionResult JoinTables(int firstTableId, int firstTableOrderId, int secondTableId)
        {
            if (firstTableId != 0 && firstTableOrderId != 0  && secondTableId != 0)
            {
                Order firstTableOrder = _unitOfWork.Orders.GetAll(o => o.Id == firstTableOrderId).Include(o=>o.OrderProducts).First();
                Table firstTable = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == firstTableId);
                Table secondTable = _unitOfWork.Tables.GetAll(t => t.Id == secondTableId).Include(t=>t.Orders.Where(o=>o.Status == "Aktif")).ThenInclude(o=>o.OrderProducts.Where(op=>op.IsDeleted == false)).First();

                if(secondTable.Status == "Boş")
                {
                    return BadRequest();
                }

                var orderProducts = secondTable.Orders.First().OrderProducts;
                if (orderProducts != null) 
                {
                    foreach (var item in orderProducts)
                    {
                        orderProducts.Remove(item);
                        item.OrderId = firstTableOrderId;
                        firstTableOrder.OrderProducts.Add(item);
                    }
                }
                
                

                firstTableOrder.TotalPrice = firstTableOrder.TotalPrice + secondTable.Orders.First().TotalPrice;
                secondTable.Orders.First().TotalPrice = 0;

                _unitOfWork.Tables.Update(firstTable);
                _unitOfWork.Tables.Update(secondTable);
                _unitOfWork.Orders.Update(firstTableOrder);
                _unitOfWork.Save();
                return Ok("");
            }
            else
            {
                return BadRequest();
            }
        }


        [Authorize]
        public IActionResult GetAllPayments(int id)
        {
            return Json(new { data = _unitOfWork.Orders.GetAll(o => o.Id == id).Include(o => o.Payments) });
        }


        [Authorize]
        public IActionResult GetAllWithOrderProduct(int? id)
        {
            if (id.HasValue)
            {
                return Json(new { Data = _unitOfWork.Orders.GetAll(o => o.Id == id ).Include(o => o.OrderProducts.Where(op => op.IsDeleted == false)).ThenInclude(op => op.Product).First() });
            }
            else
            {
                return Json(new { Data = _unitOfWork.Orders.GetAll(o => o.Status != "Tamamlandı" && o.IsDeleted == false).Include(o => o.OrderProducts.Where(op => op.IsDeleted == false)).ThenInclude(op => op.Product).First() });
            }
        }


        [Authorize]
        public IActionResult GetById(int id)
        {

            var order = _unitOfWork.Orders.GetAll(o => o.Id == id)
                .Select(o => new
                {
                    Order = new
                    {
                        o.Id,
                    },
                    OrderProducts = o.OrderProducts
                        .Where(op => !op.IsDeleted)
                        .Select(op => new
                        {
                            op.Portion,
                            op.Quantity,
                            Product = new
                            {
                                op.Product.Portion05Price,
                                op.Product.Portion1Price,
                                op.Product.Portion15Price,
                                op.Product.Portion2Price
                            }
                        })
                })
                .FirstOrDefault();

            if (order == null)
            {
                return NotFound(); // İlgili sipariş bulunamazsa HTTP 404 Not Found döndürülür.
            }

            return Json(order);


            //Order order = _unitOfWork.Orders.GetAll(o => o.Id == id).Include(o=>o.OrderProducts.Where(op=>op.IsDeleted == false)).ThenInclude(op=>op.Product).First();
            //return Json(order);
        }


        [Authorize(Roles = "Admin,Garson"), HttpPatch]
        public IActionResult UpdateTotalPrice(int id, decimal totalPrice)
        {
            Order asil = _unitOfWork.Orders.GetFirstOrDefault(o => o.Id == id);


            asil.TotalPrice = totalPrice;
            _unitOfWork.Orders.Update(asil);
            _unitOfWork.Save();
            return Ok();
        }


        [Authorize(Roles = "Admin,Garson"), HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.Orders.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }


        [Authorize(Roles ="Admin"), HttpGet]
        public IActionResult GetReceivedOrderCountByUsers(DateTime? startDate, DateTime? endDate) 
        {
            if (startDate == null && endDate == null)
            {

                DateTime day = DateTime.Today.Date.AddDays(-30);

                List<AppUser> waiters = _unitOfWork.AppUsers.GetAll(u => u.Position.Name == "Admin" || u.Position.Name == "Garson").Include(o => o.Orders.Where(o => o.IsDeleted == false && o.DateCreated >= day)).ToList();

                Dictionary<string, int> combinedData = new Dictionary<string, int>();
                string name = "";
                int orderCount = 0;



                foreach (var waiter in waiters)
                {
                    name = waiter.FirstName + " " + waiter.LastName;
                    orderCount = waiter.Orders.Count();
                    combinedData.Add(name, orderCount);
                }

                return Json(combinedData);
            }
            else if (startDate != null && endDate != null)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date;

                if (start <= end)
                {
                    List<AppUser> waiters = _unitOfWork.AppUsers.GetAll(u => u.Position.Name == "Admin" || u.Position.Name == "Garson").Include(o => o.Orders.Where(o => o.IsDeleted == false && o.DateCreated >= start && o.DateCreated <= end)).ToList();

                    Dictionary<string, int> combinedData = new Dictionary<string, int>();
                    string name = "";
                    int orderCount = 0;

                    foreach (var waiter in waiters)
                    {
                        name = waiter.FirstName + " " + waiter.LastName;
                        orderCount = waiter.Orders.Count();
                        combinedData.Add(name, orderCount);
                    }

                    return Json(combinedData);
                }
                else
                {
                    return BadRequest("Başlangıç tarihi, bitiş tarihinden sonra olamaz.");
                }
            }
            else
            {
                return BadRequest();
            }

        }


    }
}
