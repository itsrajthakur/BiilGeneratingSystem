public class BillItem
{
    public int Id { get; set; }
    public int BillId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public virtual Bill Bill { get; set; }
    public virtual Product Product { get; set; }
}