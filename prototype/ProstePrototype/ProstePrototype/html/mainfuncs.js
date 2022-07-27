let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json) {

    CefSharp.PostMessage(JSON.stringify(json));
}

function addActive() {
    $(document).on('click', '.btnStyle', function () {
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle
        $(this).addClass('active');// here apply selected class on clicked btnStyle
    });
}

function toggleDarkMode(show, filename) {
    if (show) {
        let ss = document.getElementById(darkModeStylesheetId);
        if (ss) {
            return;
        }
        let head = document.head;
        let link = document.createElement("link");
        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = filename;
        link.id = darkModeStylesheetId;
        head.appendChild(link);
        
    }
    else {
        let ss = document.getElementById(darkModeStylesheetId);
        ss.parentNode.removeChild(ss);
    }
}