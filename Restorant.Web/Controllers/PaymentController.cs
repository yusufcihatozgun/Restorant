using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;

namespace Restorant.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [Authorize(Roles = "Admin,Kasiyer")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Statistics()
        {
            return View();
        }


        [Authorize(Roles = "Admin,Kasiyer")]
        public IActionResult EndOfTheDay()
        {
            DateTime todayStart = DateTime.Today;
            DateTime todayEnd = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);

            var totalCardPaymentAmounts = _unitOfWork.Payments.GetAll(p => p.IsDeleted == false && p.PaymentType == "Kredi Kartı" && p.DateCreated >= todayStart && p.DateCreated <= todayEnd).Sum(p => p.Amount);
            var totalTicketPaymentAmounts = _unitOfWork.Payments.GetAll(p => p.IsDeleted == false && p.PaymentType == "Yemek Çeki" && p.DateCreated >= todayStart && p.DateCreated <= todayEnd).Sum(p => p.Amount);
            var totalCashPaymentAmounts = _unitOfWork.Payments.GetAll(p => p.IsDeleted == false && p.PaymentType == "Nakit" && p.DateCreated >= todayStart && p.DateCreated <= todayEnd).Sum(p => p.Amount);
            var totalPaymentAmounts = totalCardPaymentAmounts + totalTicketPaymentAmounts + totalCashPaymentAmounts;

            if (totalCardPaymentAmounts != null && totalTicketPaymentAmounts != null && totalCashPaymentAmounts != null && totalPaymentAmounts != null)
            {
                object[] payments = new object[]
                {
                    new { Name = "totalCashPaymentAmounts", Amount = totalCashPaymentAmounts },
                    new { Name = "totalCardPaymentAmounts", Amount = totalCardPaymentAmounts },
                    new { Name = "totalTicketPaymentAmounts", Amount = totalTicketPaymentAmounts },
                    new { Name = "totalPaymentAmounts", Amount = totalPaymentAmounts }
                };

                return Json(payments);
            }
            else
            {
                return BadRequest();
            }
        }


        [Authorize(Roles = "Admin"),HttpPost]
        public IActionResult GetDailySalesData(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null && endDate == null)
            {
                Dictionary<DateTime, decimal?> combinedData = new Dictionary<DateTime, decimal?>();

                DateTime day = DateTime.Today.Date.AddDays(-30);

                for (int i = 1; i <= 30; i++)
                {
                    decimal? dailyTotalSalesAmount = _unitOfWork.Payments.GetAll(p => p.DateCreated.Date == day.Date).Sum(p => p.Amount);

                    combinedData.Add(day, dailyTotalSalesAmount);

                    day = day.AddDays(1);
                }

                return Json(combinedData);
            }
            else if (startDate != null && endDate != null)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date;

                if (start <= end) 
                {
                    Dictionary<DateTime, decimal?> combinedData = new Dictionary<DateTime, decimal?>();

                    for (DateTime date = start; date <= end; date = date.AddDays(1))
                    {
                        decimal? dailyTotalSalesAmount = _unitOfWork.Payments
                            .GetAll(p => p.DateCreated.Date == date.Date)
                            .Sum(p => p.Amount);

                        combinedData.Add(date, dailyTotalSalesAmount);
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


        [Authorize(Roles = "Admin,Kasiyer")]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Payments.GetAll() });
        }


        [Authorize(Roles = "Admin,Kasiyer")]
        public IActionResult Create(Payment payment)
        {
            if (payment != null && payment.Amount != null)
            {
                List<Payment> all = _unitOfWork.Payments.GetAll(p => p.OrderId == payment.OrderId).ToList();
                decimal? totalAmount = 0;
                foreach (var item in all)
                {
                    totalAmount += item.Amount;
                }
                Order order = _unitOfWork.Orders.GetFirstOrDefault(o => o.Id == payment.OrderId);


                if (order.TotalPrice >= (order.TotalPrice - totalAmount))
                {

                    _unitOfWork.Payments.Add(payment);
                    _unitOfWork.Save();
                    return Ok();

                }
            }
            return BadRequest();
        }
    }
}
