using Restorant.Data;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using Restorant.Repository.Shared.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Repository.Shared.Concrete
{
    public class UnitOfWork : IUnitOfWork
    {
        public IRepository<Category> Categories { get; private set; }

        public IRepository<AppUser> AppUsers { get; private set; }

        public IRepository<Order> Orders { get; private set; }

        public IRepository<OrderProduct> OrderProducts { get; private set; }

        public IRepository<Payment> Payments { get; private set; }

        public IRepository<Product> Products { get; private set; }

        public IRepository<Table> Tables { get; private set; }

        public IRepository<SubCategory> SubCategories { get; private set; }

        public IRepository<Position> Positions { get; private set; }

        public IRepository<CustomerReview> CustomerReviews { get; private set; }

        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Categories = new Repository<Category>(db);
            AppUsers = new Repository<AppUser>(db);
            Orders = new Repository<Order>(db);
            OrderProducts = new Repository<OrderProduct>(db);
            Payments = new Repository<Payment>(db);
            Products = new Repository<Product>(db);
            Tables = new Repository<Table>(db);
            SubCategories = new Repository<SubCategory>(db);
            Positions = new Repository<Position>(db);
            CustomerReviews = new Repository<CustomerReview>(db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
