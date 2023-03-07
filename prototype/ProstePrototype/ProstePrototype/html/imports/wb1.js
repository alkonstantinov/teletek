//#region VARIABLES

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
    document.location.reload();
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
    if (Object.keys(comm).length > 0) {
        try {
            switch (comm['funcName']) {
                case 'changeStyleDisplay':
                    eval(`${comm['funcName']}("${comm['params']['goToId']}", "${comm['params']['id']}")`);
                    break;
                case 'addElement':
                    eval(`${comm['funcName']}("${comm['params']['id']}", "${comm['params']['elementType']}")`);
                    break;
                default:
                    eval(`${comm['funcName']}("${comm['params']}")`);
                    break;
            }
        } catch (e) {
            console.log('error', e);
        }
    }
    //alert(JSON.stringify(json));
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
            body = document.getElementById('panelsList');;
            let id = json["~panel_id"].replaceAll("-", "_");

            // check if there is a div with similar id;
            if (document.getElementById(id)) {
                if (!document.getElementById(id).firstElementChild.classList.contains('collapsed')) return; // it is already active
                selectNewPanel(id); // else we set it as active
            }
            else {
                // else case: add new panel
                // 1. find the new panel type
                let jsonKeys = Object.keys(json).filter(x => x !== "~panel_id").filter(y => y !== "~path");
                let panelIcon;
                switch (true) {
                    case jsonKeys[0].toLowerCase().startsWith("iris"):
                        panelIcon = '<span class="material-icons-outlined fire">local_fire_department</span>'
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("eclipse"):
                        panelIcon = '<span class="material-icons-outlined normal">notifications</span >'
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("tte"):
                        panelIcon = '<span class="material-icons-outlined grasse">nest_remote_comfort_sensor</span > '
                        break;
                    default: break;
                }
                let panelName = jsonKeys[0].split("_")[0].toUpperCase();
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

    let el = `<a href="javascript: sendMessageWPF({'Command': 'NewSystem','Params': '${schemaKey}'})" onclick="javascript: addActive()" class="col-sm-3 minw" id="${schemaKey}">
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
    let div = document.createElement('div');
    div.classList = "accordion-body";
    if (!jsonAtLevel) return;
    var elementKeys = Object.keys(jsonAtLevel);
    const pathStr = "~path";
    let cleanKeys = elementKeys.filter(x => x !== pathStr).filter(y => y !== '~panel_id');

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

    let el = `<a href="javascript:sendMessageWPF({'Command': 'LoadPage','Params': '${page}'})" onclick="javascript: addActive()" class="col-sm-3 minw" id="${page}">
                    <span class="${color} mr10"><i class="fa-solid ${CONFIG_CONST[key].picture}"></i></span>${titleTranslated}
            </a>`;

    div.insertAdjacentHTML('beforeend', el);
};


//#region pagePreparation, contextMenu, toggleDarkMode
function pagePreparation() {
    $(document).ready(() => {
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
            var elems = document.querySelectorAll('a');
            for (var i = 0; i < elems.length; i++) {
                elems[i].oncontextmenu = function (e) {
                    return showContextMenu(this);
                }
            }
        }
    });
}
//pagePreparation();
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

function addActive(doc = document) {
    $(doc).on('click', '.btnStyle', function () {
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle fire
        $(this).addClass('active');// here apply selected class on clicked btnStyle fire
    });
}
//#endregion