let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json) {
    CefSharp.PostMessage(JSON.stringify(json));
}

function receiveMessageWPF(jsonTxt) {
    var json = JSON.parse(jsonTxt);
    switch (true) {
        case Object.keys(json).includes("alert"):
            alert(`alerting json ${json.alert}`);
            break;
        case Object.keys(json).includes("pageName"):
            alert(`current pageName ${json.pageName}`);
            break;
        default:
            alert("No recognised keys in the json");
            break;
    }
}

// collapsible part
function collapsible(param) {
    var coll = param ? document.getElementsByClassName(param) : document.getElementsByClassName("collapsible");
    var i;

    function handleClick() {
        this.classList.toggle("cactive");
        var content = this.nextElementSibling;
        if (content.style.maxHeight) {
            content.style.maxHeight = null;
        } else {
            content.style.maxHeight = content.scrollHeight + 54 + "px";
        }
    };

    for (i = 0; i < coll.length; i++) {
        //if (coll[i].getAttribute('isclick') == 'true') {
        //    coll[i].removeEventListener("click", handleClick);
        //}
        coll[i].addEventListener("click", handleClick);
        //coll[i].setAttribute('isclick', 'true');
    }

    for (i = 0; i < coll.length; i++) {
        coll[i].click();
    }
}
collapsible();

// searching for menu context menu on the page - beginning of contextMenu part
menuEl = document.getElementById("ctxMenu");
if (menuEl) {
    var elems = document.querySelectorAll('a');
    for (var i = 0; i < elems.length; i++) {
        elems[i].oncontextmenu = function (e) {
            return showContextMenu(this);
        }
    }
}

function showContextMenu(el) {
    event.preventDefault();
    let s = JSON.parse(el.href.slice(26, -1).replaceAll('\'', '\"'));
    s.Command = "MainMenuBtn";
    var ctxMenu = document.getElementById("ctxMenu");
    ctxMenu.setAttribute('sendMessage', JSON.stringify(s));
    ctxMenu.className = el.children[0].className.split(" ")[1];
    ctxMenu.style.display = "block";
    ctxMenu.style.left = (event.pageX - 10) + "px";
    ctxMenu.style.top = (event.pageY - 10) + "px";
    ctxMenu.onmouseleave = () => ctxMenu.style.display = "none";
    return false;
}

function sendMsg(el) {
    //console.log('event', event);
    var json = JSON.parse(el.parentNode.getAttribute("sendMessage"));
    json["Function"] = el.title;
    sendMessageWPF(json);

}
// finish of the contextMenu part

$(document).ready(() => {
    addVisitedBackground();
    $('.btnStyle').removeClass('active');// here remove class active from all btnStyle
    let searchParams = new URLSearchParams(window.location.search)
    if (searchParams.has('highlight')) {
        elem = document.getElementById(searchParams.get('highlight')).children[0];
        console.log('elem', elem);
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

// hsow changed articles
function addVisitedBackground() {
    $('.form-item input, .form-item select').change(function () {
        this.classList.add('visited-bgd');
    });
}
