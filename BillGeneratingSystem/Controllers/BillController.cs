using BillGeneratingSystem.Data;
using BillGeneratingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BillGeneratingSystem.ViewModels;

public class BillController : Controller
{
    private readonly ApplicationDbContext _context;

    public BillController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Bills.Include(b => b.BillItems).ToListAsync());
    }

    public IActionResult Create()
    {
        ViewBag.Products = _context.Products.ToList();
        return View(new CreateBillViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBillViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var bill = new Bill
                {
                    BillDate = DateTime.Now,
                    BillNumber = GenerateBillNumber(),
                    CustomerName = viewModel.CustomerName,
                    CustomerContact = viewModel.CustomerContact,
                    PaymentMethod = viewModel.PaymentMethod,
                    PaymentStatus = viewModel.PaymentMethod == "Pending" ? "Pending" : "Paid",
                    BillItems = new List<BillItem>()
                };

                foreach (var item in viewModel.BillItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for product: {product.Name}");
                            ViewBag.Products = _context.Products.ToList();
                            return View(viewModel);
                        }

                        var billItem = new BillItem
                        {
                            ProductId = product.Id,
                            Product = product,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            TotalPrice = product.Price * item.Quantity
                        };
                        bill.BillItems.Add(billItem);

                        // Update product stock
                        product.Stock -= item.Quantity;
                        _context.Update(product);
                    }
                }

                if (!bill.BillItems.Any())
                {
                    ModelState.AddModelError("", "At least one valid product must be added to the bill.");
                    ViewBag.Products = _context.Products.ToList();
                    return View(viewModel);
                }

                bill.TotalAmount = bill.BillItems.Sum(item => item.TotalPrice);

                _context.Add(bill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the bill.");
            }
        }

        ViewBag.Products = _context.Products.ToList();
        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bill = await _context.Bills
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bill == null)
        {
            return NotFound();
        }

        var viewModel = new EditBillViewModel
        {
            Id = bill.Id,
            BillNumber = bill.BillNumber,
            BillDate = bill.BillDate,
            CustomerName = bill.CustomerName,
            CustomerContact = bill.CustomerContact,
            PaymentMethod = bill.PaymentMethod,
            PaymentStatus = bill.PaymentStatus,
            TotalAmount = bill.TotalAmount,
            BillItems = bill.BillItems.Select(bi => new BillItemViewModel
            {
                ProductId = bi.ProductId,
                Quantity = bi.Quantity,
                UnitPrice = bi.UnitPrice,
                TotalPrice = bi.TotalPrice
            }).ToList()
        };

        ViewBag.Products = _context.Products.ToList();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBillViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingBill = await _context.Bills
                    .Include(b => b.BillItems)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (existingBill == null)
                {
                    return NotFound();
                }

                // Update basic bill information
                existingBill.CustomerName = viewModel.CustomerName;
                existingBill.CustomerContact = viewModel.CustomerContact;
                existingBill.PaymentStatus = viewModel.PaymentStatus;
                existingBill.PaymentMethod = viewModel.PaymentMethod;
                existingBill.TotalAmount = viewModel.TotalAmount;

                // Store old quantities for stock adjustment
                var oldQuantities = existingBill.BillItems
                    .ToDictionary(bi => bi.ProductId, bi => bi.Quantity);

                // Remove existing bill items
                _context.BillItems.RemoveRange(existingBill.BillItems);
                existingBill.BillItems.Clear();

                // Add updated bill items
                foreach (var item in viewModel.BillItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Calculate stock adjustment
                        var oldQuantity = oldQuantities.GetValueOrDefault(item.ProductId);
                        var quantityDifference = item.Quantity - oldQuantity;

                        // Check if enough stock is available
                        if (product.Stock - quantityDifference < 0)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for product: {product.Name}");
                            ViewBag.Products = _context.Products.ToList();
                            return View(viewModel);
                        }

                        // Update product stock
                        product.Stock -= quantityDifference;
                        _context.Update(product);

                        var billItem = new BillItem
                        {
                            ProductId = product.Id,
                            Product = product,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            TotalPrice = product.Price * item.Quantity
                        };
                        existingBill.BillItems.Add(billItem);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillExists(viewModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        ViewBag.Products = _context.Products.ToList();
        return View(viewModel);
    }

    private bool BillExists(int id)
    {
        return _context.Bills.Any(e => e.Id == id);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var bill = await _context.Bills
            .Include(b => b.BillItems)
            .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bill == null)
        {
            return NotFound();
        }

        return View(bill);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bill = await _context.Bills
            .Include(b => b.BillItems)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bill != null)
        {
            _context.BillItems.RemoveRange(bill.BillItems);
            _context.Bills.Remove(bill);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private string GenerateBillNumber()
    {
        return "BILL-" + DateTime.Now.ToString("yyyyMMddHHmmss");
    }
}