using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class CustomerReview : BaseModel
    {
        public int? ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerComment { get; set; }
        public int? ReviewScore { get; set; }
        public bool? IsApproved { get; set; } = false;
        public string? AvatarNo { get; set; }
    }
}
