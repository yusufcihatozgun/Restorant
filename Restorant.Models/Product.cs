using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class Product : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? SubCategoryId { get; set; }
        public virtual SubCategory? SubCategory { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<OrderProduct>? OrderProducts { get; set; }
        public ICollection<CustomerReview>? CustomerReviews { get; set; }
        public decimal? Portion05Price { get; set; }
        public decimal? Portion1Price { get; set; }
        public decimal? Portion15Price { get; set; }
        public decimal? Portion2Price { get; set; }
    }
}