using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;

namespace Restorant.Web.Controllers
{
    public class TableController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TableController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Garson")]
        public IActionResult Tables()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Create(Table table)
        {
            if (_unitOfWork.Tables.GetAll().Any(t => t.Name == table.Name))
            {
                return UnprocessableEntity("Bu masa numarası zaten kullanılıyor.");
            }
            else
            {
                table.Status = "Boş";
                _unitOfWork.Tables.Add(table);
                _unitOfWork.Save();
                return Ok();
            }

        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Json(new { Data = _unitOfWork.Tables.GetAll() });
        }


        [Authorize(Roles = "Admin,Garson")]
        public IActionResult GetAllClosedTables()
        {
            return Json(_unitOfWork.Tables.GetAll(t => t.Status == "Boş").Select(t=> new
            {
                t.Id,
                t.Name,
                t.Status,
                orders = t.Orders
            }));
        }


        [HttpGet]
        public IActionResult GetTablesUsingProjection(string? page)
        {
            IQueryable<Table> query;

            if (page == "payment")
            {
                query = _unitOfWork.Tables.GetAll(t => t.Status == "Dolu");
            }
            else
            {
                query = _unitOfWork.Tables.GetAll();
            }

            var tables = query.Select(t => new
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status,
                Orders = t.Orders.Where(o=>o.Status == "Aktif").Select(o => new
                {
                    Id = o.Id,
                    AppUser = new
                    {
                        FirstName = o.AppUser.FirstName,
                        LastName = o.AppUser.LastName
                    },
                    TotalPrice = o.TotalPrice,
                    OrderProducts = o.OrderProducts.Select(op => new
                    {
                        Product = new
                        {
                            Name = op.Product.Name,
                            Portion05Price = op.Product.Portion05Price,
                            Portion1Price = op.Product.Portion1Price,
                            Portion15Price = op.Product.Portion15Price,
                            Portion2Price = op.Product.Portion2Price,
                        }
                    }).ToList()
                }).ToList()
            }).ToList();

            return Json(tables);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            Table table = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == id);
            return Json(table);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Update(Table table)
        {
            Table asil = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == table.Id);

            if (asil == null)
            {
                return UnprocessableEntity("Masa bulunamadı");

            }
            else
            {
                if (_unitOfWork.Tables.GetAll(t => t.Id != table.Id).Any(t => t.Name == table.Name))
                {
                    return UnprocessableEntity("Bu masa numarası zaten kullanılıyor.");
                }
                else
                {

                    table.DateCreated = asil.DateCreated;
                    table.ReservationDate = asil.ReservationDate;
                    table.Status = asil.Status;
                    table.IsDeleted = asil.IsDeleted;
                    table.Orders = asil.Orders;

                    _unitOfWork.Tables.Update(table);
                    _unitOfWork.Save();
                    return Ok();
                }
            }



        }


        [Authorize(Roles = "Admin,Kasiyer")]
        public IActionResult CloseTable(int id)
        {
            Table table = _unitOfWork.Tables.GetAll(t => t.Id == id).Include(t=>t.Orders).First();
            Order Order = table.Orders.FirstOrDefault(o => o.Status == "Aktif");

            if (table == null)
            {
                return UnprocessableEntity("Masa bulunamadı");

            }
            else
            {
                table.Status = "Boş";
                Order.Status = "Tamamlandı";
                _unitOfWork.Tables.Update(table);
                _unitOfWork.Save();
                return Ok();
            }

        }


        [Authorize(Roles = "Admin,Garson"),HttpPost]
        public IActionResult OpenTable(int tableId)
        {
            if (tableId == 0)
            {
                return UnprocessableEntity("Masa Bulunamadı");
            }
            else
            {
                Table table = _unitOfWork.Tables.GetFirstOrDefault(t => t.Id == tableId);

                table.Status = "Dolu";
                _unitOfWork.Tables.Update(table);
                _unitOfWork.Save();
                return Ok();
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _unitOfWork.Tables.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }
    }
}
