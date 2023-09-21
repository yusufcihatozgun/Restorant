using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class SubCategory : BaseModel
    {
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public string? Name { get; set; }
        public ICollection<Product>? Products { get; set; }

    }
}
