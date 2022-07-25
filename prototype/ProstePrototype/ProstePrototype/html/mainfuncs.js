let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json) {
    CefSharp.PostMessage(JSON.stringify(json));
}


function toggleDarkMode(show, filename) {
    if (show) {
        let head = document.head;
        let link = document.createElement("link");
        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = filename;
        link.id = darkModeStylesheetId;
        head.appendChild(link);
        
    }
    else {
        let ss = document.getElementById("ssDarkMode");
        ss.parentNode.removeChild(ss);
    }
}