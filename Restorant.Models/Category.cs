using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class Category : BaseModel
    {
        public string? Name { get; set; }
        public ICollection<SubCategory>? SubCategories { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
