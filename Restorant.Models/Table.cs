namespace Restorant.Models
{
    public class Table : BaseModel
    {
        public string? Name { get; set; }
        public int? Capacity { get; set; }
        public string? Status { get; set; }
        public DateTime? ReservationDate { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
