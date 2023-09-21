using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class Payment : BaseModel
    {
        public int? OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentType { get; set; }
    }
}
