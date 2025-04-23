// Bill form handling
let rowIndex = 0;

function updateItemTotal(row) {
    var quantity = parseInt(row.find('.quantity').val()) || 0;
    var unitPrice = parseFloat(row.find('.unit-price').text().replace(/[^0-9.-]+/g, '')) || 0;
    var total = quantity * unitPrice;
    row.find('.item-total').text(total.toFixed(2));
    row.find('.total-price-input').val(total.toFixed(2));
    updateBillTotal();
}

function updateBillTotal() {
    var billTotal = 0;
    $('.item-total').each(function() {
        billTotal += parseFloat($(this).text().replace(/[^0-9.-]+/g, '')) || 0;
    });
    $('#billTotal').text(billTotal.toFixed(2));
    $('#cashAmountDisplay').text(billTotal.toFixed(2));
    $('#qrAmountDisplay').text(billTotal.toFixed(2));
    $('#pendingAmountDisplay').text(billTotal.toFixed(2));
}

function updateRowIndexes() {
    $('.item-row').each(function(index) {
        $(this).find('[name^="BillItems["]').each(function() {
            var name = $(this).attr('name');
            var newName = name.replace(/\[\d+\]/, '[' + index + ']');
            $(this).attr('name', newName);
        });
    });
}

$(document).ready(function () {
    // Product selection change
    $(document).on('change', '.product-select', function() {
        var row = $(this).closest('tr');
        var price = parseFloat($(this).find(':selected').data('price')) || 0;
        row.find('.unit-price').text(price.toFixed(2));
        updateItemTotal(row);
    });

    // Quantity change
    $(document).on('change', '.quantity', function() {
        var row = $(this).closest('tr');
        updateItemTotal(row);
    });

    // Add item button
    $("#addItem").click(function () {
        rowIndex++;
        var newRow = $(".item-row:first").clone();
        newRow.find("input").val("1");
        newRow.find("select").val("");
        newRow.find(".unit-price, .item-total").text("0.00");
        newRow.find('[name^="BillItems["]').each(function() {
            var name = $(this).attr('name');
            var newName = name.replace(/\[\d+\]/, '[' + rowIndex + ']');
            $(this).attr('name', newName);
        });
        $("#itemsTable tbody").append(newRow);
    });

    // Remove item button
    $(document).on("click", ".remove-item", function () {
        if ($(".item-row").length > 1) {
            $(this).closest("tr").remove();
            updateBillTotal();
            updateRowIndexes();
        }
    });

    // Payment method change
    $('#paymentMethod').change(function() {
        // Hide all payment containers
        $('.payment-method-container').addClass('d-none');
        
        // Show the selected payment container
        var method = $(this).val();
        if (method === 'Card') {
            $('#card-payment-container').removeClass('d-none');
        } else if (method === 'UPI') {
            $('#upi-payment-container').removeClass('d-none');
        } else if (method === 'Cash') {
            $('#cash-payment-container').removeClass('d-none');
        }
    });

    // Form validation
    $("#billForm").submit(function(e) {
        var isValid = true;
        if ($(".product-select").filter(function() { return !this.value; }).length > 0) {
            alert("Please select all products");
            isValid = false;
        }
        if ($(".quantity").filter(function() { return !this.value || this.value < 1; }).length > 0) {
            alert("Please enter valid quantities");
            isValid = false;
        }

        // Check if this is a UPI confirmation submission
        var isUpiConfirmation = $('input[name="UpiPaymentConfirmed"]').length > 0;
        
        // For Cash, proceed with normal form submission
        // For Card, the razorpay-integration.js will handle submission
        // For UPI confirmation, allow the form to submit
        if (isValid && $('#paymentMethod').val() === 'UPI' && !isUpiConfirmation) {
            e.preventDefault();
            // If UPI is selected but not yet confirmed, show the QR code
            if ($('#qrcode-container').hasClass('d-none')) {
                $('#generateQrBtn').click();
            }
        }

        return isValid;
    });
});