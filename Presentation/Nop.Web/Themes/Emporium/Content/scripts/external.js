function loaderShow() {
    $("div.ajax-loading-block-window").attr("style", "display:block!important;");
}

function loaderHide() {
    $("div.ajax-loading-block-window").attr("style", "display:none!important;");
}

function clearConsumoSigoCreditos() {
    $("#montoConsumo").val("");
    $("#pinConsumo").val("");
}


function inhabilitar(valor) {

    if (parseInt(valor) == 0) {
        console.log("inhabilitar: " + valor);

        window.oncontextmenu = function () { return false; }
        document.onkeydown = function (e) {

            if (event.keyCode == 123 || e.button == 2) { // Prevent F12
                return false;
            } else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) { // Prevent Ctrl+Shift+I        
                return false;
            }

        }
        document.addEventListener("contextmenu", function (e) {
            e.preventDefault();
        }, false);

    }
    else {
        console.log("inhabilitar: " + valor);
        window.oncontextmenu = function () { return true; }
        document.onkeydown = function (e) {
            if (event.keyCode == 123 || e.button == 2) { // Prevent F12
                return true;
            } else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) { // Prevent Ctrl+Shift+I        
                return true;
            }
        }
        document.addEventListener("contextmenu", function (e) {
            e.preventDefault();
        }, true);
    } 
  
    
}