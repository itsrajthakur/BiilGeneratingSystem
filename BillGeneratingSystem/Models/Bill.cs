public class Bill
{
    public int Id { get; set; }
    public string BillNumber { get; set; }
    public DateTime BillDate { get; set; }
    public string CustomerName { get; set; }
    public string CustomerContact { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; }
    public string PaymentMethod { get; set; }
    public virtual ICollection<BillItem> BillItems { get; set; }
}