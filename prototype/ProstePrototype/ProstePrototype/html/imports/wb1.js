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
    // reload
    // document.location.reload();

    // shoud clear the document.getElementById('ram_sidebar_menu').innerHTML = "";
    document.getElementById('ram_sidebar_menu').innerHTML = "";

    // request the previous content of panels with boundAsync.panelsInLeftBrowser
    boundAsync.panelsInLeftBrowser().then(res => {
        if (res) {
            //alert(typeof (res) + 'is the type;  ' + res);
            let panelArray = JSON.parse(res);
            panelArray.forEach(panel => {
                receiveMessageWPF(JSON.stringify(panel));                
            });
        } else {
            alert("System error: No response from 'panelsInLeftBrowser'");
        }
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
            document.body.style.backgroundColor = "#E6ECF4";
            body = document.getElementById('divDevices');
            const devicesPerType = json["pageName"]["wb2"];
            Object.keys(devicesPerType).forEach(deviceType => {
                let divD = document.createElement('div');
                divD.classList = "row m-2 g10 no-gutter border-top";
                let h4 = document.createElement('h4');
                h4.classList = "h4";
                h4.innerText = newT.t(localStorage.getItem('lang'), deviceType);
                divD.appendChild(h4);
                const devices = devicesPerType[deviceType];
                for (let i = 0; i < devices.length; i++) {
                    addButton(devices[i].title, divD, i, devices[i]);
                    body.appendChild(divD);
                }
            });
            break;
        case !!document.getElementById("divPDevices"):

            break;
        case !!document.getElementById("divLDevices"):

            break;
        default:
            // case menu-page
            body = document.getElementById('ram_sidebar_menu');
            let id = json["~panel_id"].replaceAll("-", "_");

            // check if there is a div with similar id;
            if (document.getElementById(id)) {
                if (!document.getElementById(id).firstElementChild.classList.contains('collapsed')) return; // it is already active
                selectNewPanel(id); // else we set it as active
            }
            else {
                // else case: add new panel
                // 1. find the new panel type
                let jsonKeys = Object.keys(json)
                        .filter(x => x !== "~panel_id")
                        .filter(y => y !== "~panel_name")
                        .filter(y => y !== "~path");

                let panelIcon, pageType, color;
                switch (true) {
                    case jsonKeys[0].toLowerCase().startsWith("iris"):
                        panelIcon = '<i class="ram_icon fireicon fire"></i>';
                        pageType = "iris";
                        color = "fire";
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("eclipse"):
                        panelIcon = '<i class="ram_icon bell normal"></i>';
                        pageType = "eclipse";
                        color = "normal";
                        break;
                    case jsonKeys[0].toLowerCase().startsWith("tte"):
                        panelIcon = '<i class="ram_icon signall grasse"></i>';
                        pageType = "tte";
                        color = "grasse";
                        break;
                    default: break;
                }
                let panelName = json["~panel_name"]; // [0].toUpperCase() + json["~panel_name"].slice(1).toLowerCase();
                                
                // 2. create the new panel-item
                let panelItem = document.createElement('div');
                panelItem.classList.add("accordion-item", color);
                panelItem.insertAdjacentHTML(
                    'afterbegin', 
                    `<h2 class="accordion-header" id="${id}" onclick="javascript: selectNewPanel(this.id, this)" pageType="${pageType}">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse${id}" aria-expanded="true" aria-controls="collapse${id}">
                            ${panelIcon} <span class="ram_btn_title">${panelName}</span>
                        </button>
                    </h2>
                    <div id="collapse${id}" class="accordion-collapse collapse show" aria-labelledby="${id}" data-bs-parent="#ram_sidebar_menu"></div>`);
                
                panelCreationHandler(color, panelItem, json);

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
const addButton = (title, div, index, localJSON = {}) => {
    let indexFlag = Object.keys(localJSON).length > 0;
    // button color definition
    let color = indexFlag ? localJSON.deviceType : "";
    if (color === 'guard') color = 'normal'; // options for color: "normal", "fire", "grasse"

    // clean all digits from used deviceType
    let key;
    switch (color) {
        case 'fire': key = 'iris'; break;
        case 'grasse': key = 'tte'; break;
        default: key = 'eclipse';
    }

    // title definition
    const titleTranslated = newT.t(localStorage.getItem('lang'), title.trim().replaceAll(" ", "_").toLowerCase());
    let img = DEVICES_CONSTS[title].im ? `<img src="${DEVICES_CONSTS[title].im}" alt="${DEVICES_CONSTS[title].sign}">` : `<i class="ram_icon ${CONFIG_CONST[key].picture}"></i>`
    let localJSONString = JSON.stringify(localJSON).replaceAll("\"", "'");
    let el = `<a href="javascript: showBackDrop(); sendMessageWPF({'Command': 'NewSystem','Params': ${localJSONString}})" onclick="javascript: addActive();" class="col-sm-3 minw" id="${index}_${localJSON.schema}">
                <div class="${color} ram_card">
                    <div class="ram_card_img">
                        ${img}
                    </div>
                    <div class="ram_card_body">
                        <h5 class="ram_title">
                            ${titleTranslated}
                            <span class="h5">${indexFlag ? localJSON.interface : ""}</span>
                        </h5>      
                    </div>
                </div>
            </a>`;

    div.insertAdjacentHTML('beforeend', el);
};

function showBackDrop() {
    document.body.classList.add('backdrop');
}

async function selectNewPanel(id, self) {
    if (document.getElementById(id).firstElementChild.classList.contains('collapsed')) return;
    // asking the back-end to provide me with id for the new panel
    let backEndId = id.replaceAll("_", "-");
    let newPanelId = await boundAsync.setActivePanel(backEndId);
    // after
    if (newPanelId) {        
        $('a:has(> span:first-child)').removeClass('active');
        sendMessageWPF({ 'Command': 'LoadPage', 'Params': self.getAttribute("pageType") });
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

const panelCreationHandler = (color, panelItem, jsonAtLevel) => {
    // create the accordion-body div
    let div = document.createElement('div');
    div.classList = "accordion-body";
    if (!jsonAtLevel) return;
    var elementKeys = Object.keys(jsonAtLevel);
    // create the ram_list_group ul
    let ul = document.createElement('ul');
    ul.classList = "ram_list_group";

    const pathStr = "~path";
    let cleanKeys = elementKeys.filter(x => x !== pathStr && x !== "~panel_id" && x !== "~panel_name"); // for the usual new panel adding

    cleanKeys.forEach(field => {
        // guard for null value of jsonAtLevel[field]
        if (jsonAtLevel[field] && jsonAtLevel[field].title) {
            let title = jsonAtLevel[field].title;
            addAccordeonButton(title, field, ul);
        }
    });

    div.appendChild(ul);

    panelItem.lastChild.appendChild(div);
}

const addAccordeonButton = (title, page, ul_element) => {
    // clean all digits from used schema
    let key = page.toLowerCase().trim().replaceAll(' ', '_').replace(/[0-9]/g, '');

    // title definition
    var titleTranslated = newT.t(localStorage.getItem('lang'), title.trim().replaceAll(" ", "_").toLowerCase().replace(/[/*.?!#]/g, ''));

    let el = `<li class="ram_list_group_item" onclick="javascript:sendMessageWPF({'Command': 'LoadPage','Params': '${page}'}); addActive()" id="${page}">
                   <div class="ram_list_item_content">
                       <i class="${CONFIG_CONST[key].picture.startsWith("fa-") ? "fa-solid" : "ram_icon"} ${CONFIG_CONST[key].picture}"></i>
                       <span>${titleTranslated}</span>
                   </div>`;
    if (false) el += `<i class="ram_icon add_device"></i>`;
    el += `</li>`;
    // old el: 
    //<a href="javascript:sendMessageWPF({'Command': 'LoadPage','Params': '${page}'})" onclick="javascript: addActive()" class="" id="${page}">
    //    <span class="${color} mr10"><i class="fa-solid ${CONFIG_CONST[key].picture}"></i></span>${titleTranslated}
    //</a>
    ul_element.insertAdjacentHTML('beforeend', el);
};

function alertScanFinished(show) {
    if (show === 'alert') {
        var el = document.getElementsByClassName("active")[0];
        if (el)
            el.click();
        else {
            el = document.getElementsByClassName("show");
            if (!el[0]) {
                el = document.querySelectorAll('h2')[0];
            } else {
                el = el[0].previousElementSibling;
            }
            const pageType = el.getAttribute("pageType");
            sendMessageWPF({ 'Command': 'LoadPage', 'Params': pageType });
        }
    }
}


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
    ctxMenu.className = el.parentElement.className.split(" ")[1];
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
    $('body').on('click', 'li.ram_list_group_item', function () {
        $('li.ram_list_group_item').removeClass('active'); // Remove the active class from all anchor tags that contain a span as their first child
        if (this.parentElement.parentElement.parentElement.classList.contains("show")) {
            $(this).addClass('active'); // Add the active class to the clicked anchor tag
        }
    });
}

function toggleClosedClass(action = 'close') {
    const el = document.getElementById("ram_sidebar");
    if (action === 'close')
        el.classList.add("closed");
    else {
        el.classList.remove("closed");
    }
}

function transformToTooltip(spanContent) {
    boundAsync.transformToTooltip(spanContent)
        .then(() => { })
        .catch(err => console.log(err));
}
//#endregion
