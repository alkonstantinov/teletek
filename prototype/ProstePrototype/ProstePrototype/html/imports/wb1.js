//#region VARIABLES
let reloading = false;
const BUTTON_COLORS = {
    IRIS: 'fire',
    ECLIPSE: 'normal',
    TTE: 'grasse',
};

let darkModeStylesheetId = "ssDarkMode";

let CONFIG_CONST = {};

function setConfigConst(config_const) {
    CONFIG_CONST = config_const;
}
//#endregion VARIABLES

//#region Language Funcs
class T {
    getLangs = () => {
        return Translations.languages;
    }

    t = (lang, key) => {
        let k = Translations.translations[key];
        if (!k) {
            return "Not found translation key";
        }
        let t = k[lang];
        if (!t) {
            return "Not found translation language";
        }
        return t;
    }
}

const newT = new T();

function toggleLang(key) {
    setLang(key);
    // reload
    // document.location.reload();

    // shoud clear the document.getElementById('panelsList').innerHTML = "";
    document.getElementById('panelsList').innerHTML = "";

    reloading = true;
    // request the previous content of panels with boundAsync.panelsInLeftBrowser
    boundAsync.panelsInLeftBrowser().then(res => {
        if (res) {
            let panelArray = JSON.parse(res);
            panelArray.forEach(panel => {
                receiveMessageWPF(JSON.stringify(panel));
                
            });
        } else {
            alert("System error: No response from 'panelsInLeftBrowser'");
        }
        // when all the panels are loaded we are back to normal
        reloading = false;
    }).catch(err => alert(err));
}

function setLang(key) {
    if (Translations.languages.map(x => x.key).includes(key)) {
        localStorage.setItem('lang', Translations.languages.find(x => x.key === key).id);
    } else if (Translations.languages.map(x => x.id).includes(key)) {
        localStorage.setItem('lang', key);
    } else {
        localStorage.setItem('lang', 'en');
    }
}
//#endregion

//#region WPF Communication
function sendMessageWPF(json, comm = {}) {
    try {
        //alert(`1`);
        CefSharp.PostMessage(JSON.stringify(json));
        //alert(`2`);
    } catch (e) { console.log('CefSharp error', e); }
}

function receiveMessageWPF(jsonTxt) {
    var json = JSON.parse(jsonTxt);
    if (Object.keys(json) === 0) return; // guard for empty json

    let body; // Main, Devices, Menu
    switch (true) {
        case !!document.getElementById('divMain'): // setting the body element
            //alert('divMain')

            break;
        case !!document.getElementById('divDevices'):
            /* case index-page */
            body = document.getElementById('divDevices');
            let divD = document.createElement('div');
            divD.classList = "row m2 no-gutter";
            devices = json["pageName"]["wb2"];
            for (let i = 0; i < devices.length; i++) {
                addButton(devices[i].title, devices[i].schema.toLowerCase(), divD, devices[i]);
                body.appendChild(divD);
            }
            break;
        case !!document.getElementById("divPDevices"):

            break;
        case !!document.getElementById("divLDevices"):

            break;
        default:
            // case menu-page
            body = document.getElementById('panelsList');
            let id = json["~panel_id"].replaceAll("-", "_");

            // check if there is a div with similar id;
            if (document.getElementById(id)) {
                if (!document.getElementById(id).firstElementChild.classList.contains('collapsed')) return; // it is already active
                selectNewPanel(id); // else we set it as active
            }
            else {
                // else case: add new panel
                // 1. find the new panel type
                let jsonKeys = reloading ?
                    Object.keys(json["pages"]) :
                    Object.keys(json)
                        .filter(x => x !== "~panel_id")
                        .filter(y => y !== "~panel_name")
                        .filter(y => y !== "~path");

                let panelIcon;
                switch (true) {
                    case jsonKeys[0].toLowerCase().startsWith("iris"):
                        panelIcon = '<span class="material-icons-outlined fire">local_fire_department</span>'
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("eclipse"):
                        panelIcon = '<span class="material-icons-outlined normal">notifications</span >'
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("tte"):
                        panelIcon = '<span class="material-icons-outlined grasse">wifi</span > '
                        break;
                    default: break;
                }
                let panelName = json["~panel_name"]; // [0].toUpperCase() + json["~panel_name"].slice(1).toLowerCase();
                                
                // 2. create the new panel-item
                let panelItem = document.createElement('div');
                panelItem.classList = "accordion-item m-4";
                panelItem.insertAdjacentHTML(
                    'afterbegin', 
                    `<h2 class="accordion-header" id="${id}" onclick="javascript: selectNewPanel(this.id)">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse${id}" aria-expanded="true" aria-controls="collapse${id}">
                            ${panelIcon} ${panelName}
                        </button>
                    </h2>
                    <div id="collapse${id}" class="accordion-collapse collapse show" aria-labelledby="${id}" data-bs-parent="#panelsList"></div>`);
                
                panelCreationHandler(panelItem, json);

                body.appendChild(panelItem);

                setTimeout(() => openAccordionItem(id), 100);
            }
            break;
    }
    $(function () {
        // DOM Ready - do your stuff
        pagePreparation();
    });
}
//#endregion

// addButton function for creating the main panels buttons
const addButton = (title, schemaKey, div, localJSON = {}) => {
    let indexFlag = Object.keys(localJSON).length > 0;
    // clean all digits from used schema
    let key = schemaKey.toLowerCase().trim().replaceAll(' ', '_').replace(/[0-9]/g, '');

    // button color definition
    let color = indexFlag ? localJSON.deviceType : "";

    if (CONFIG_CONST[key] && CONFIG_CONST[key].breadcrumbs.includes('iris')) { color = "fire"; }
    else if (CONFIG_CONST[key] && CONFIG_CONST[key].breadcrumbs.includes('tte')) { color = "grasse"; }

    // title definition
    var titleTranslated = newT.t(localStorage.getItem('lang'), title.trim().replaceAll(" ", "_").toLowerCase());

    let el = `<a href="javascript: showBackDrop(); sendMessageWPF({'Command': 'NewSystem','Params': '${schemaKey}'})" onclick="javascript: addActive();" class="col-sm-3 minw" id="${schemaKey}">
                <div class="btnStyle ${color}">
                    <i class="fa-solid ${CONFIG_CONST[key].picture} fa-3x p15">
                        <br /><span class="someS">
                            <span class="h5">
                                ${titleTranslated}
                            </span>
                            ${indexFlag ? localJSON.interface : ""}
                        </span>
                    </i>
                </div>
            </a>`;

    div.insertAdjacentHTML('beforeend', el);
};

function showBackDrop() {
    document.body.classList.add('backdrop');
}

async function selectNewPanel(id) {
    if (document.getElementById(id).firstElementChild.classList.contains('collapsed')) return;
    // asking the back-end to provide me with id for the new panel
    let backEndId = id.replaceAll("_", "-");
    let newPanelId = await boundAsync.setActivePanel(backEndId);
    // after
    if (newPanelId) {
        //openAccordionItem(id);
    }
};

function openAccordionItem(id) {
    $('.accordion-button').addClass('collapsed');
    $('.accordion-button').attr('aria-expanded', false);
    $('.accordion-collapse').removeClass('show');
    var el = document.getElementById(id);
    if (el) {
        el.firstElementChild.classList.remove('collapsed');
        el.nextElementSibling.classList.add('show');
        $(el).find('button').attr('aria-expanded', true);
    }
}

const panelCreationHandler = (panelItem, jsonAtLevel) => {
    if (reloading) jsonAtLevel = jsonAtLevel["pages"];
    let div = document.createElement('div');
    div.classList = "accordion-body";
    if (!jsonAtLevel) return;
    var elementKeys = Object.keys(jsonAtLevel);

    const pathStr = "~path";
    let cleanKeys = reloading ?
        elementKeys.filter(x => jsonAtLevel[x]["breadcrumbs"].length !== 1) : // for the reloading case
        elementKeys.filter(x => x !== pathStr && x !== "~panel_id" && x !== "~panel_name"); // for the usual new panel adding

    let innerbutton = document.createElement('div');
    innerbutton.class = 'accordion-item';
    cleanKeys.forEach(field => {
        // guard for null value of jsonAtLevel[field]
        if (jsonAtLevel[field] && jsonAtLevel[field].title) {
            let title = jsonAtLevel[field].title;
            addAccordeonButton(title, field, div);
        }
    });

    panelItem.lastChild.appendChild(div);
}

const addAccordeonButton = (title, page, div) => {
    // clean all digits from used schema
    let key = page.toLowerCase().trim().replaceAll(' ', '_').replace(/[0-9]/g, '');
    // button color definition
    let color = "";
    if (CONFIG_CONST[key] && CONFIG_CONST[key].breadcrumbs.includes('iris')) { color = "fire"; }
    else if (CONFIG_CONST[key] && CONFIG_CONST[key].breadcrumbs.includes('tte')) { color = "grasse"; }

    // title definition
    var titleTranslated = newT.t(localStorage.getItem('lang'), title.trim().replaceAll(" ", "_").toLowerCase().replace(/[/*.?!#]/g, ''));

    let el = `<a href="javascript:sendMessageWPF({'Command': 'LoadPage','Params': '${page}'})" onclick="javascript: addActive()" class="" id="${page}">
                    <span class="${color} mr10"><i class="fa-solid ${CONFIG_CONST[key].picture}"></i></span>${titleTranslated}
            </a>`;

    div.insertAdjacentHTML('beforeend', el);
};


//#region pagePreparation, contextMenu, toggleDarkMode
function pagePreparation() {
    $(() => { // $(function() {}) equivalent to $(document).ready(function () {})
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle

        let searchParams = new URLSearchParams(window.location.search)

        // position the selected btn and selecting it
        if (searchParams.has('highlight')) {
            elem = document.getElementById(searchParams.get('highlight')).children[0];
            if (elem) {
                $(elem).addClass('active');
            }
            elem.scrollIntoView({ behavior: 'auto', block: 'center' });
        }

        // searching for menu context menu on the page - beginning of contextMenu part
        menuEl = document.getElementById("ctxMenu");
        if (menuEl) {
            var elems = document.querySelectorAll('h2');
            for (var i = 0; i < elems.length; i++) {
                elems[i].oncontextmenu = function (e) {
                    return showContextMenu(e, this);
                }
            }
        }
    });
}

function showContextMenu(event, el) {
    event.preventDefault();

    var ctxMenu = document.getElementById("ctxMenu");
    // Modify the textContent of the menuitem elements
    var menuItems = ctxMenu.getElementsByTagName("menuitem");
    for (var i = 0; i < menuItems.length; i++) {
        menuItems[i].title = newT.t(localStorage.getItem('lang'), `${menuItems[i].getAttribute('topic')}`);
    }
    ctxMenu.setAttribute('element', `${el.id}`);
    ctxMenu.className = el.children[0].className.split(" ")[1];
    ctxMenu.style.display = "block";
    ctxMenu.style.left = (event.pageX - 10) + "px";
    ctxMenu.style.top = (event.pageY - 10) + "px";
    ctxMenu.onmouseleave = () => ctxMenu.style.display = "none";
    ctxMenu.onmouseup = () => ctxMenu.style.display = "none";
    return false;
}

function sendMsg(self) {
    if (self.getAttribute('topic') === "Rename") {
        const panelId = self.parentNode.getAttribute('element');
        const el = document.getElementById(panelId);
        let currentNode = el.firstElementChild.childNodes[2];
        let inner = el.firstElementChild.innerHTML;
        let idx = inner.lastIndexOf(">");
        let currentName = inner.slice(idx + 1, idx + 11).replaceAll('\n', "");
        let inp = document.createElement("input");
        inp.id = "newNameId";
        inp.maxLength = "15";
        inp.type = "text";
        inp.size = "10";
        inp.placeholder = currentName;
        let replacing = false; // flag for the focusout event
        inp.addEventListener('focusout', (e) => {
            if (!replacing) {
                setNewName(e, currentName, inp.id, currentNode);
            }
        }, false);
        inp.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                replacing = true;
                setNewName(e, currentName, inp.id, currentNode);
            } else if (e.key === 'Escape') {
                replacing = true;
                inp.parentElement.replaceChild(document.createTextNode(currentName), inp);
            } else if (e.key == " " ||
                e.code == "Space" ||
                e.key == 32
            ) {
                e.preventDefault();
                inp.value += ' ';
            }
        }, false);
        el.firstElementChild.replaceChild(inp, currentNode);
        inp.focus();
    } else {
        sendMessageWPF({ 'Command': 'MainMenuBtn', 'Function': self.getAttribute('topic') });
    }
}

function setNewName(event, currName, inpId) {
    let newName = event.target.value;
    const panelId = event.target.parentElement.parentElement.id.replaceAll("_", "-");
    let inpEl = document.getElementById(inpId);    
    if (!newName) inpEl.parentElement.replaceChild(document.createTextNode(currName), inpEl);
    else {
        inpEl.parentElement.replaceChild(document.createTextNode(newName), inpEl);
        sendMessageWPF({ 'Command': 'MainMenuBtn', 'Function': 'Rename', 'newName': newName, '~panel_id': panelId });
    }
}
// finish of the contextMenu part

// activate dark mode
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

function addActive() {
    $('body').on('click', 'a:has(> span:first-child)', function () {
        $('a:has(> span:first-child)').removeClass('active'); // Remove the active class from all anchor tags that contain a span as their first child
        $(this).addClass('active'); // Add the active class to the clicked anchor tag
    });
}
//#endregion
