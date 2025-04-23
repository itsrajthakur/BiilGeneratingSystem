using BillGeneratingSystem.Models;

namespace BillGeneratingSystem.ViewModels
{
    public class CreateBillViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerContact { get; set; }
        public string PaymentMethod { get; set; }
        public List<BillItemViewModel> BillItems { get; set; } = new List<BillItemViewModel>();
    }

    public class BillItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class EditBillViewModel
    {
        public int Id { get; set; }
        public string BillNumber { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerContact { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public List<BillItemViewModel> BillItems { get; set; } = new List<BillItemViewModel>();
    }
}