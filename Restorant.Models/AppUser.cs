using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restorant.Models
{
    public class AppUser : BaseModel
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public virtual Position? Position { get; set; }
        public int? PositionId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRememberMe { get; set; } = false;
        public int FailedLoginAttempts { get; set; }

    }
}
