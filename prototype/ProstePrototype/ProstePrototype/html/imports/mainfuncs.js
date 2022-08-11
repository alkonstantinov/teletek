let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json) {
    CefSharp.PostMessage(JSON.stringify(json));
}

$(document).ready(() => {
    $('.btnStyle').removeClass('active');// here remove class active from all btnStyle
    let searchParams = new URLSearchParams(window.location.search)
    if (searchParams.has('highlight')) {
        elem = document.getElementById(searchParams.get('highlight')).children[0];
        if (elem) {
            $(elem).addClass('active');
        }
        elem.scrollIntoView({ behavior: 'auto', block: 'center' });
    }
});

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
