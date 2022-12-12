let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json) {
    //alert(JSON.stringify(json));
    CefSharp.PostMessage(JSON.stringify(json));
}

function receiveMessageWPF(jsonTxt) {
    var json = JSON.parse(jsonTxt);

    let body; // Main, Devices, Menu
    switch (true) {
        case !!document.getElementById('divMain'): // setting the body element
            body = document.getElementById('divMain');
            if (body.firstElementChild.tagName === 'FIELDSET') {
                body = body.firstElementChild;
            }
            // getting keys and creating element for each
            keys = Object.keys(json);
            console.log(keys)
            keys.filter(k => k !== '~path').forEach(k => {
                let divLevel = json[k];
                if (k.includes('~')) {
                    let div = document.createElement('div');
                    div.classList = "row align-items-center m-2";
                    console.log('diclevel', divLevel);
                    elementsCreationHandler(div, divLevel);

                    body.appendChild(div);
                } else {
                    console.log('k', k, 'divlevel', divLevel)
                    const { input_name, input_id } = {
                        input_name: divLevel.name,
                        input_id: divLevel.name.toLowerCase().replaceAll(' ', '_')
                    }
                    var inside = `<button class="fire collapsible ml-1 collapsible_${input_id}">${input_name}</button>
                <div class="collapsible-content col-12">
                    <div class="row align-items-center m-2" id="${input_id}"></div>
                </div>`;
                    body.insertAdjacentHTML('beforeend', inside);

                    let div = body.querySelector(`#${input_id}`);
                    elementsCreationHandler(div, divLevel.fields);

                    collapsible(`collapsible_${input_id}`)
                }
            });
            break;
        case !!document.getElementById('divDevices'):
            body = document.getElementById('divDevices');
            let divD = document.createElement('div');
            divD.classList = "row m2 no-gutter";
            devices = json["pageName"]["wb2"];
            for (let i = 0; i < devices.length; i++) {
                const src = "pages-dynamic.js";
                if (document.querySelectorAll(`script[src*="${src}"]`).length === 0) {
                    loadScript(() => addButton(devices[i].title, devices[i].title.toLowerCase(), divD, devices[i]), src);
                } else {
                    window.setTimeout(() => addButton(devices.title, devices.title.toLowerCase(), divD, devices[i]), 50);
                }
                body.appendChild(divD);
            }
            break;
        default:
            // case divIRIS, divTTE, divECLIPSE
            body = document.body;
            let div = document.createElement('div');
            div.classList = "row m2 no-gutter";

            elementsCreationHandler(div, json);

            body.appendChild(div);
            break;

    }

}

// creating elements on div level
const elementsCreationHandler = (div, jsonAtLevel) => {
    if (!jsonAtLevel) return;
    var elementKeys = Object.keys(jsonAtLevel);
    const pathStr = "~path";
    const cleanKeys = elementKeys.filter(x => x !== pathStr);

    cleanKeys.forEach(field => {
        if (jsonAtLevel[field]['@TYPE']) {
            if (jsonAtLevel[field]['@TYPE'] === "AND") {
                elementsCreationHandler(div, jsonAtLevel[field]["PROPERTIES"]);
                return;
            }

            const innerString = transformGroupElement(jsonAtLevel[field]);
            const newElement = document.createElement('div');
            let className = `col-xs-12 col-md-${jsonAtLevel[field]['@TYPE'] === "EMAC" ? "12" : "6"} mt-1`;
            newElement.classList = className;
            newElement.innerHTML = innerString;
            div.appendChild(newElement);
        } else if (jsonAtLevel[field].title) {
            let title = jsonAtLevel[field].title;
            const src = "pages-dynamic.js";
            if (document.querySelectorAll(`script[src*="${src}"]`).length === 0) {
                loadScript(() => addButton(title, field, div), src);
            } else {
                window.setTimeout(() => addButton(title, field, div), 50);
            }
        }
    });
}

const addButton = (title, fieldKey, div, localJSON = {}) => {
    let indexFlag = Object.keys(localJSON).length > 0;
    let color = indexFlag ? localJSON.deviceType : "";
    if (CONFIG_CONST[fieldKey].breadcrumbs.includes('iris')) { color = "fire"; }
    else if (CONFIG_CONST[fieldKey].breadcrumbs.includes('tte')) { color = "grasse"; }
    console.log(CONFIG_CONST);
    let el = `<a href="javascript:sendMessageWPF({'Command': 'LoadPage','Params':'${fieldKey}'${!indexFlag ? ", 'Highlight':'${fieldKey}'" : ""}})" onclick="javascript: addActive()" class="col-sm-3 minw" id="${fieldKey}">
                <div class="btnStyle ${color} active">
                    <i class="fa-solid ${CONFIG_CONST[fieldKey].picture} fa-3x p15">
                        <br /><span class="someS">
                            <span class="h5">
                                ${title}
                            </span>
                            ${indexFlag ? localJSON.interface : ""}
                        </span>
                    </i>
                </div>
            </a>`;
    div.insertAdjacentHTML('afterbegin', el);
};

// transforming object function
const transformGroupElement = (elementJson) => {
    let { type, input_name, input_id, max, min, maxTextLength, placeHolderText, bytesData, lengthData, readOnly, input_name_on, input_name_off, checked, path } = {
        type: elementJson['@TYPE'],
        input_name: elementJson['@TEXT'], //.charAt(0).toUpperCase() + elementJson['@TEXT'].slice(1),
        input_id: elementJson['@TEXT'].toLowerCase().replaceAll(' ', '_'),
        max: elementJson['@MAX'],
        min: elementJson['@MIN'],
        maxTextLength: elementJson['@LENGTH'],
        placeHolderText: elementJson['@PLACEHOLDER'],
        bytesData: elementJson['@BYTE'],
        lengthData: elementJson['@LEN'],
        readOnly: !!(+elementJson['@READONLY']),
        input_name_on: elementJson['@YESVAL'],
        input_name_off: elementJson['@NOVAL'],
        checked: !!(+elementJson['@CHECKED']),
        path: elementJson['~path'],
    };

    let attributes = { type, input_name, input_id, max, min, maxTextLength, placeHolderText, bytesData, lengthData, readOnly, input_name_on, input_name_off, checked, path };

    switch (type) {
        case 'INT':
            return getNumberInput({ ...attributes });

        case 'TEXT':
            return getTextInput({ ...attributes });

        case 'SLIDER':
            if (/\bon\b/i.test(input_name)) {
                attributes.input_name_on = 'ON';
                attributes.input_name_off = 'OFF';
                attributes.input_name = input_name.replace(/\bon\b/ig, "").trim();
            }
            if (attributes.input_name.toLowerCase().includes('enable')) {
                attributes.input_name = '';
                attributes.input_name_on = 'Enabled';
                attributes.input_name_off = 'Disabled';
            }
            return getSliderInput({ ...attributes });

        case 'CHECK':
            return getCheckboxInput({ ...attributes });

        case 'LIST':
            attributes.selectList = elementJson.ITEMS.ITEM.map(o => {
                return {
                    value: o['@VALUE'],
                    label: o['@NAME'],
                };
            });
            return getSelectInput({ ...attributes })

        case 'IP':
            src = "../imports/jquery.inputmask.min.js";
            const found_in_script_tags = document.querySelectorAll(`script[src*="${src}"]`).length > 0;
            if (!found_in_script_tags) {
                loadScript(() => ipMaskActivation());
            }
            attributes.ip = true;
            return getTextInput({ ...attributes });
        case 'EMAC':
            return getEmacInput({ ...attributes });
        default: break;
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
        coll[i].addEventListener("click", handleClick);
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
    // position the selected btn and selecting it
    if (searchParams.has('highlight')) {
        elem = document.getElementById(searchParams.get('highlight')).children[0];
        if (elem) {
            $(elem).addClass('active');
        }
        elem.scrollIntoView({ behavior: 'auto', block: 'center' });
    }
});

// checking a hex value function
function checkHexRegex(event) {
    let val = event.target.value;
    let regEx = /^([0-9A-Fa-f]{1,2})$/;
    let isHex = regEx.test(val);
    if (!isHex) {
        document.getElementById(event.target.id).value = val.slice(0, -1);
    }
}

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

// hsow changed articles
function addVisitedBackground() {
    $('.form-item input, .form-item select').change(function () {
        this.classList.add('visited-bgd');
    });
}

// ip mask activation
function ipMaskActivation(src = "../imports/jquery.inputmask.min.js") {
    if (document.querySelectorAll(`script[src*="${src}"]`).length > 0) {
        $('[ip="yes"]').inputmask({
            alias: "ip",
            greedy: false //The initial mask shown will be "" instead of "-____".
        });
    }
}

function loadScript(callback, src = "../imports/jquery.inputmask.min.js") {
    var body = document.getElementsByTagName('body')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = src;

    script.onreadystatechange = callback;
    script.onload = callback;
    body.appendChild(script);
}

/////////////////////----- COMMON Funcs -----////////////////////////////////////////////
const getTextInput = ({ type, input_name, input_id, maxTextLength, placeHolderText, bytesData, lengthData, readOnly, RmBtn = false, ip = false, path = '' }) => {
    return `<div class="form-item roww flex">
                ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                    <i class="fa-solid fa-square-minus fa-2x"></i>
                </button>` : ""}
                <label for="${input_id}">${input_name}</label>
                <input type="${type === 'passInput' ? 'password' : 'text'}" 
                       id="${input_id}" name="${input_id}" 
                       ${maxTextLength ? `maxlength="${maxTextLength}"` : (ip ? `maxlength = "15"` : "")}
                       ${ip ? `ip="yes"` : ""}
                       ${placeHolderText ? `placeholder="${placeHolderText}"` : (ip ? `placeholder = " 0 . 0 . 0 . 0 "` : "")} 
                       onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})"
                       ${bytesData ? `bytes="${bytesData}"` : ""} 
                       ${lengthData ? `length="${lengthData}"` : ""} 
                       ${readOnly ? "disabled" : ''}/>
            </div>`
}

const getCheckboxInput = ({ input_name, input_id, bytesData, lengthData, readOnly, checked = false, RmBtn = false, path = '' }) => {
    return `<div class="form-item roww">
                ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                    <i class="fa-solid fa-square-minus fa-2x"></i>
                </button>` : ""}
                <label for="${input_id}">${input_name}</label>
                <input type="checkbox" id="${input_id}" class="ml10"
                    ${bytesData ? `bytes="${bytesData}"` : ""} 
                    ${lengthData ? `length="${lengthData}"` : ""} 
                    ${readOnly ? "disabled" : ''}
                    ${checked ? "checked" : ''}
                    onchange="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})" />
            </div>`
}

const getSliderInput = ({ input_name, input_name_off, input_name_on, input_id, bytesData, lengthData, readOnly, checked = false, RmBtn = false, path = '' }) => {
    return `<div class="form-item roww fire">
                ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                    <i class="fa-solid fa-square-minus fa-2x"></i>
                </button>` : ""}
                ${input_name && `<label for="${input_id}">${input_name}</label>`}
                <p class="fire${input_name ? ' bordered' : ''}">
                    ${input_name_off}
                    <label class="switch">
                        <input type="checkbox" id="${input_id}" name="${input_id}" 
                            ${bytesData ? `bytes="${bytesData}"` : ""} 
                            ${lengthData ? `length="${lengthData}"` : ""} 
                            ${readOnly ? "disabled" : ''} 
                            ${checked ? "checked" : ''} 
                            onchange="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})"/>
                        <span class="slider"></span>
                    </label>
                    ${input_name_on}
                </p>
            </div>`
}

const getSelectInput = ({ input_name, input_id, selectList, placeHolderText, bytesData, lengthData, readOnly, RmBtn = false, path = "" }) => {
    let str = `<div class="form-item roww mt-1">
                    ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                        <i class="fa-solid fa-square-minus fa-2x"></i>
                    </button>` : ""}
                    <label for="${input_id}">${input_name}</label>
                    <div class="select">
                        <select id="${input_id}" name="${input_id}"
                            ${bytesData ? `bytes="${bytesData}"` : ""} 
                            ${lengthData ? `length="${lengthData}"` : ""} 
                            ${readOnly ? "disabled" : ''} 
                            onchange="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})" >
                            <option value="" disabled selected>${placeHolderText || "Select your option"}</option>`;
    if (selectList.length > 0) selectList.map(o => str += `<option value="${o.value}">${o.label}</option>`);
    str += `</select></div></div>`
    return str;
}

const getNumberInput = ({ input_name, input_id, max, min, bytesData, lengthData, readOnly, RmBtn = false, path = "" }) => {
    return `<div class="form-item roww">
                ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                    <i class="fa-solid fa-square-minus fa-2x"></i>
                </button>` : ""}
                <label for="${input_id}">${input_name}</label>
                <input class="ml10"
                        type="number"
                        id="${input_id}"
                        name="${input_id}"
                        ${max ? `data-maxlength="${`${max}`.length}"` : ""}
                        oninput="this.value=this.value.slice(0,this.dataset.maxlength)"
                        onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})"
                        ${min ? `min="${min}"` : ""} ${max ? `max="${max}"` : ""}
                        ${bytesData ? `bytes="${bytesData}"` : ""} 
                        ${lengthData ? `length="${lengthData}"` : ""} 
                        ${readOnly ? "disabled" : ''} />
            </div>`;
}

const getEmacInput = ({ input_id, input_name, readOnly, RmBtn = false, path = "" }) => {
    return `<div class="form-item roww" macInput>
                ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                    <i class="fa-solid fa-square-minus fa-2x"></i>
                </button>` : ""}
                <label for="${input_id}">${input_name}</label>
                <div class="row m0" id="${input_id}" >
                    <input class="col-1 mr-1"
                            type="text"
                            id="${input_id}0"
                            name="${input_id}0"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                    <div>:</div>
                    <input class="col-1 mr-1"
                            type="text"
                            id="${input_id}1"
                            name="${input_id}1"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                    <div>:</div>
                    <input type="text"
                            class="col-1 mr-1"
                            id="${input_id}2"
                            name="${input_id}2"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                    <div>:</div>
                    <input type="text"
                            class="col-1 mr-1"
                            id="${input_id}3"
                            name="${input_id}3"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                    <div>:</div>
                    <input type="text"
                            class="col-1 mr-1"
                            id="${input_id}4"
                            name="${input_id}4"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                    <div>:</div>
                    <input type="text"
                            class="col-1 mr-1"
                            id="${input_id}5"
                            name="${input_id}5"
                            placeholder="00" ${readOnly ? "disabled" : ''}
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                </div>
            </div>`;
}
