using Restorant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class OrderProduct : BaseModel
    {
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public decimal? Portion { get; set; }
        public int? Quantity { get; set; }
        public string? Status { get; set;}
        public bool? IsReviewed { get; set; }
        public int? CustomerReviewCount { get; set; }
    }
}
