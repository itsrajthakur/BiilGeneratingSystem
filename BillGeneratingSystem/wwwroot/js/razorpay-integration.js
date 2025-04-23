// Razorpay integration
$(document).ready(function() {
    $('#razorpayBtn').click(function() {
        initializeRazorpay();
    });
});

function initializeRazorpay() {
    var options = {
        "key": razorpayKey, // This comes from the _CardPayment.cshtml
        "amount": parseFloat($('#billTotal').text()) * 100, // Amount in paise
        "currency": "INR",
        "name": "Bill Payment",
        "description": "Bill Payment for " + $('#CustomerName').val(),
        "image": "https://www.shutterstock.com/image-vector/thin-line-easy-contactless-payment-600nw-1707796087.jpg", // Update with your logo path
        "handler": function (response) {
            // On successful payment
            alert("Payment Successful! Payment ID: " + response.razorpay_payment_id);
            
            // Add payment ID to form data
            $('<input>').attr({
                type: 'hidden',
                name: 'RazorpayPaymentId',
                value: response.razorpay_payment_id
            }).appendTo('#billForm');
            
            // Submit the form to create the bill
            $('#billForm').submit();
        },
        "prefill": {
            "name": $('#CustomerName').val(),
            "contact": $('#CustomerContact').val()
        },
        "theme": {
            "color": "#3399cc"
        }
    };
    
    var rzp = new Razorpay(options);
    rzp.open();
}