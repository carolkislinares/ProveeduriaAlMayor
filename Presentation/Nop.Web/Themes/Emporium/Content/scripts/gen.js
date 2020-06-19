//<script src="https://www.paypal.com/sdk/js?client-id=AfbVJuARzopmc5Yqw6LnuMZGsvgUbjW2Z4omie4CRjBVsJ3lXV5tcrrVA1VTPi7SplXpK2nu6poae-qp&currency=USD"
//    data-sdk-integration-source="button-factory">
//</script>

function gen() {
    var pageElement = document.createElement("script");  // create a script DOM node
    var goTo = "https://www.paypal.com/sdk/js?client-id=AfbVJuARzopmc5Yqw6LnuMZGsvgUbjW2Z4omie4CRjBVsJ3lXV5tcrrVA1VTPi7SplXpK2nu6poae-qp&currency=USD";// set its src to the provided URL
    pageElement.setAttribute("src", goTo);
    pageElement.setAttribute("data-sdk-integration-source", "button-factory");
    document.head.appendChild(pageElement);  // add it to the end of the head section of the page (could change 'head' to 'body' to add it to the end of the body section instead)
    document.cookie = "_ga=GA1.2.876021233.1592419914; expires=Thu, 11 Jun 2070 11:11:11 UTC; path=/; SameSite=None; Secure";

    setTimeout(function () { return pButtons(); }, 5000);
    //return pButtons();
}

function pButtons() {
    //setTimeout(function () {
        paypal.Buttons({
            onInit: function (data, actions) {
                // Disable the buttons
                actions.disable();
                
                document.querySelector('#AddBalanceModel_TransactionAmount')
                    .addEventListener('blur', function (event) {
                        // Enable or disable the button when it is checked or unchecked
                        if (event.target.value > 0) {
                            actions.enable();
                        } else {
                            actions.disable();
                        }
                    });
            },
            style: {
                shape: 'rect', color: 'silver', layout: 'horizontal', label: 'checkout',
            },
            createOrder: function (data, actions) {
                // Valor = $("#Valor").val();
                Valor = $("#AddBalanceModel_TransactionAmount").val();
                total = parseFloat((parseFloat(Valor) + parseFloat(0.3) +
                    (parseFloat(Valor) * parseFloat(0.034))));

                return actions.order.create({
                    purchase_units: [{
                        amount:
                            { value: total }
                    }]
                });
            },
            onApprove: function (data, actions) {
            return actions.order.capture().then(function (details) {
                if (details.status.toUpperCase() === "COMPLETED".toUpperCase()) {
                    //alert('Transaccion Exitosa ' + details.payer.name.given_name + '!');
                    $('#paypalTransaction .modal-body h5').text('Transaccion Exitosa ' + details.payer.name.given_name + '!');
                    $('#paypalTransaction').modal('show');
                    setTimeout(function () { $('#paypalTransaction').modal('hide'); }, 5000);
                    var g_id = details.id.toString();
                    $("#@Html.IdFor(model => model.IdTransaccion)").val(g_id);
                    console.log(g_id);
                    $('form').attr('asp-route-pIdTransaccion', g_id);
                    $("#save-info-buttonA").click();
                } else {
                    //alert('Transaction ' + details.status + '!');
                    $('#paypalTransaction .modal-body h5').text('Transaction ' + details.status + '!');
                    $('#paypalTransaction').modal('show');
                }
            });
        }
        }).render('#paypal-button-container');
    //}, 5000);
}