// UPI integration
$(document).ready(function() {
    // Generate QR code button click handler
    $('#generateQrBtn').click(function() {
        $('#qrcode-container').removeClass('d-none');
        $(this).addClass('d-none');
        generateUpiQrCode();
    });

    // Confirm payment button click handler
    $('#confirmUpiPayment').click(function() {
        console.log("UPI payment confirmation clicked");
        
        // Add UPI payment confirmation to form data
        $('<input>').attr({
            type: 'hidden',
            name: 'UpiPaymentConfirmed',
            value: 'true'
        }).appendTo('#billForm');
        
        // Add payment method info if not already in the form
        if ($('#PaymentMethod').val() !== 'UPI') {
            $('#PaymentMethod').val('UPI');
        }
        
        // Submit the form to create the bill
        $('#billForm').submit();
        
        // Show a confirmation message
        alert("UPI Payment confirmed! Processing your bill...");
    });
});

function generateUpiQrCode() {
    // Get the bill total
    var amount = parseFloat($('#billTotal').text());
    
    // Generate UPI payment URL
    // Format: upi://pay?pa=UPI_ID&pn=NAME&am=AMOUNT&cu=CURRENCY&tn=DESCRIPTION
    var upiId = $('#upiIdDisplay').text(); 
    var name = "Bill Payment";
    var description = "Bill Payment for " + $('#CustomerName').val();
    var upiUrl = "upi://pay?pa=" + encodeURIComponent(upiId) + 
                 "&pn=" + encodeURIComponent(name) + 
                 "&am=" + amount + 
                 "&cu=INR" + 
                 "&tn=" + encodeURIComponent(description);
    
    // Clear previous QR code
    $('#qrcode').empty();
    
    // Generate QR code using QRCode.js
    new QRCode(document.getElementById("qrcode"), {
        text: upiUrl,
        width: 200,
        height: 200,
        colorDark: "#000000",
        colorLight: "#ffffff",
        correctLevel: QRCode.CorrectLevel.H
    });
}