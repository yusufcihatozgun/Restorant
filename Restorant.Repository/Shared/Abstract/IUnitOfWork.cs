using Restorant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Repository.Shared.Abstract
{
    public interface IUnitOfWork
    {
        IRepository<Category> Categories { get; }
        IRepository<AppUser> AppUsers { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderProduct> OrderProducts { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Product> Products { get; }
        IRepository<Table> Tables { get; }
        IRepository<SubCategory> SubCategories { get; }
        IRepository<Position> Positions { get; }
        IRepository<CustomerReview> CustomerReviews { get; }

        void Save();
    }
}
