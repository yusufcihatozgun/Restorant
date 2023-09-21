namespace Restorant.Models
{
    public class Order : BaseModel
    {
        public int? TableId { get; set; }
        public virtual Table? Table { get; set; }
        public int? AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }
        public string? Status { get; set; }
        public decimal? TotalPrice { get; set; }
        public virtual ICollection<OrderProduct>? OrderProducts { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
    }
}