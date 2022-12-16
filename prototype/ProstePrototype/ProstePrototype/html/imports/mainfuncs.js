//try {
//    boundAsync.showMessage("me").then(text => alert(text));
//} catch (e) {
//    alert(e);
//}

let darkModeStylesheetId = "ssDarkMode";
function sendMessageWPF(json, comm = {}) {
    if (Object.keys(comm).length > 0) {
        try {
            switch (comm['funcName']) {
                case 'changeStyleDisplay':
                    eval(`${comm['funcName']}("${comm['params']['goToId']}", "${comm['params']['id']}")`);
                    break;
                case 'addElement':                    
                    eval(`${comm['funcName']}("${comm['params']['id']}", "${ comm['params']['elementType']}")`);
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
        CefSharp.PostMessage(JSON.stringify(json));
    } catch (e) { console.log('CefSharp error', e); }
}

function receiveMessageWPF(jsonTxt) {
    var json = JSON.parse(jsonTxt);
    if (Object.keys(json) === 0) return; // guard for empty json

    let body; // Main, Devices, Menu
    switch (true) {
        case !!document.getElementById('divMain'): // setting the body element
            //alert('divMain')
            body = document.getElementById('divMain');
            if (body.firstElementChild?.tagName === 'FIELDSET') {
                body = body.firstElementChild;
            }
            // getting keys and creating element for each
            keys = Object.keys(json);
            
            keys.filter(k => k !== '~path').forEach(k => {
                let divLevel = json[k];
                
                if (k.includes('~')) { // ~noname cases
                    let div = document.createElement('div');
                    div.classList = "row align-items-center m-2";
                    elementsCreationHandler(div, divLevel);

                    body.appendChild(div);
                } else if (divLevel.name === "Company Info") { // unique case "Company Info"
                    let fieldset = document.createElement('fieldset');
                    var insideRows = `<legend>${divLevel.name}</legend>`;
                    for (field in divLevel.fields) {
                        if (field.includes("~")) continue;
                        let pl = divLevel.fields[field]["@TEXT"];
                        let id = pl.replaceAll(" ", "_");
                        let m = divLevel.fields[field]["@LENGTH"];
                        insideRows += `<div class="form-item p0">
                            <input type="text" maxlength="${m}" name="${id}" id="${id}" placeholder="${pl}">
                        </div>`
                    }
                    fieldset.insertAdjacentHTML('afterbegin', insideRows);
                    body.appendChild(fieldset);
                } else if (!divLevel["@TYPE"] && !divLevel.name) {
                    for (let i = 0; i < +divLevel["@MIN"]; i++) lst.push(i + 1);
                    elements = divLevel["@MAX"] && (+divLevel["@MAX"] + 1);
                    console.log('divLevel', divLevel, 'k', k, 'lst', lst, 'elements', elements);
                    let btnDiv = document.getElementById("buttons");
                    // adding the button
                    btnDiv.insertAdjacentHTML(
                        'afterbegin',
                        `<button style="display: inline-flex;" 
                            type="button"
                            onclick="javascript:addElement('element', '${k}')" 
                            id="_btn" class="btn-round btn-border-black">
                            <i class="fa-solid fa-plus 5x"></i> Add New ${k.split('_').slice(1).join(' ')}
                        </button>`);
                    if (k.toUpperCase().includes('ZONE') && !k.toUpperCase().includes('EVAC')) {
                        let divCol9 = document.getElementsByClassName("col-9")[0];
                        console.log('divCol9', divCol9)
                        divCol9.classList = 'col-7';
                        divCol9.insertAdjacentHTML(
                            'beforebegin',
                            `<div class="col-2 bl fire scroll">
                                Devices:
                                <div id="attached_devices"></div>
                            </div>`
                        );
                    }
                    if (k.toUpperCase().includes('INPUT_GROUP')) {
                        const oldEl = document.querySelector('.row.pt-2.fullHeight');
                        console.log(oldEl)
                        const newEl = document.createElement("div");
                        newEl.id = 'new';
                        newEl.classList = "row scroll";
                        oldEl.replaceWith(newEl);
                    }
                } else { // collapsible parts
                    const { input_name, input_id } = {
                        input_name: divLevel.name,
                        input_id: divLevel.name.toLowerCase().replaceAll(' ', '_').replace(/["\\]/g, '\\$&').replaceAll('/', '_')
                    }
                    var inside = `<button class="fire collapsible ml-1 collapsible_${input_id}">${input_name}</button>
                <div class="collapsible-content col-12">
                    <div class="row align-items-center m-2" id="${input_id}"></div>
                </div>`;
                    body.insertAdjacentHTML('beforeend', inside);
                    console.log('clean data', input_id)
                    let div = body.querySelector(`#${input_id}`);
                    elementsCreationHandler(div, divLevel.fields);

                    collapsible(`collapsible_${input_id}`)
                }
            });
            break;
        case !!document.getElementById('divDevices'):
            //alert('divDevices')
            body = document.getElementById('divDevices');
            let divD = document.createElement('div');
            divD.classList = "row m2 no-gutter";
            devices = json["pageName"]["wb2"];
            for (let i = 0; i < devices.length; i++) {
                const src = "pages-dynamic.js";
                if (document.querySelectorAll(`script[src*="${src}"]`).length === 0) {
                    loadScript(() => addButton(devices[i].title, devices[i].title.toLowerCase(), divD, devices[i]), src);
                } else {
                    window.setTimeout(() => addButton(devices[i].title, devices[i].title.toLowerCase(), divD, devices[i]), 50);
                }
                body.appendChild(divD);
            }
            break;
        default:
            // case divIRIS, divTTE, divECLIPSE
            /*alert('default')*/
            body = document.body;
            let div = document.createElement('div');
            div.classList = "row m2 no-gutter";

            elementsCreationHandler(div, json, reverse = false);

            body.appendChild(div);
            
            break;
    }
    pagePreparation();
}

// creating elements on div level
const elementsCreationHandler = (div, jsonAtLevel, reverse = false) => {
    if (!jsonAtLevel) return;
    var elementKeys = Object.keys(jsonAtLevel);
    const pathStr = "~path";
    let cleanKeys = elementKeys.filter(x => x !== pathStr);
    
    if (reverse)
        cleanKeys = cleanKeys.reverse();
    cleanKeys.forEach(field => {
        // guard for null value of jsonAtLevel[field]
        if (jsonAtLevel[field] && jsonAtLevel[field]['@TYPE']) {
            if (jsonAtLevel[field]['@TYPE'] === "AND") {
                elementsCreationHandler(div, jsonAtLevel[field]["PROPERTIES"]);
                return;
            }

            const innerString = transformGroupElement(jsonAtLevel[field]);
            if (jsonAtLevel[field]['@TYPE'] === "WEEK") {
                div.parentNode.parentNode.insertAdjacentHTML('beforeend', innerString);
                return;
            }
            const newElement = document.createElement('div');
            let className = `col-xs-12 col-md-${jsonAtLevel[field]['@TYPE'] === "EMAC" ? "12" : "6"} mt-1`;
            newElement.classList = className;
            newElement.innerHTML = innerString;
            div.appendChild(newElement);
        } else if (jsonAtLevel[field] && jsonAtLevel[field].title) {
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

    let el = `<a href="javascript:sendMessageWPF({'Command': 'LoadPage','Params':'${fieldKey}'${!indexFlag ? ", 'Highlight':'${fieldKey}'" : ""}})" onclick="javascript: addActive()" class="col-sm-3 minw" id="${fieldKey}">
                <div class="btnStyle ${color}">
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
    div.insertAdjacentHTML('beforeend', el);
};

// transforming object function
const transformGroupElement = (elementJson) => {
    let attributes = {
        type: elementJson['@TYPE'],
        input_name: elementJson['@ID'] ? elementJson['@ID'] : elementJson['@TEXT'], //.charAt(0).toUpperCase() + elementJson['@TEXT'].slice(1),
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
        value: elementJson['@VALUE'],
    };

    switch (attributes.type) {
        case 'INT':
            return getNumberInput({ ...attributes });

        case 'TEXT':
            return getTextInput({ ...attributes });

        case 'SLIDER':
            if (/\bon\b/i.test(attributes.input_name)) {
                attributes.input_name = attributes.input_name.replace(/\bon\b/ig, "").trim();
            }
            if (attributes.input_name.toLowerCase().includes('enable')) {
                attributes.input_name = '';
                attributes.input_name_on = 'Enabled';
                attributes.input_name_off = 'Disabled';
            } else {
                attributes.input_name_on = 'ON';
                attributes.input_name_off = 'OFF';
            }
            return getSliderInput({ ...attributes });

        case 'CHECK':
            return getCheckboxInput({ ...attributes });

        case 'LIST':
            attributes.selectList = elementJson.ITEMS.ITEM.map(o => {
                return {
                    value: o['@VALUE'],
                    label: o['@NAME'],
                    selected: !!(+o['@DEFAULT']),
                    link: o['ScheduleKey'],
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

        case 'WEEK':
            attributes.input_id = attributes.input_name.replaceAll(' ', '');
            return getWeekInput({ ...attributes });

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

const pagePreparation = () => {
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
}
pagePreparation();
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
const getTextInput = ({ type, input_name, input_id, maxTextLength, placeHolderText, bytesData, lengthData, readOnly, RmBtn = false, ip = false, path = '', value }) => {
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
                       ${value ? `value="${value}"` : ""}
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
    let link = selectList.filter(x => x.link !== undefined).length > 0 && selectList.filter(x => x.link !== undefined)[0].link;
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
                            onchange="javascript:sendMessageWPF(
                                {'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}}
                                ${link.length > 0 ? `, {'funcName': 'changeStyleDisplay', 'params': { 'goToId': '${link}', 'id': '${input_id}' }}` : ""}
                            )" >`;
    if (selectList.length > 0) {
        let isDefaultValue = selectList.map(v => v.selected).reduce((prevValue, currValue) => (prevValue || currValue), false);
        str += `<option value="" disabled ${isDefaultValue ? "" : "selected"}>${placeHolderText || "Select your option"}</option>`;
        selectList.map(o => str += `<option value="${o.value}" ${o.selected ? "selected" : ""}>${o.label}</option>`);
    }
    str += `</select></div></div>`
    return str;
}

const getNumberInput = ({ input_name, input_id, max, min, bytesData, lengthData, readOnly, RmBtn = false, path = "", value }) => {
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
                        ${value ? `value="${value}"` : ""}
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

const getWeekInput = ({ input_id, input_name, readOnly, RmBtn = false, path = "" }) => {
    return `<div style="display: none" id="${input_id}-schedule">
            <fieldset>
                <legend>${input_name}</legend>
                <div class="row">
                    <fieldset class="col-xs-12 col-md-3 col-lg bn">
                        <legend>Monday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-m-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-m-${input_id}"
                                   name="activate-m-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-m-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-m-${input_id}"
                                   name="deactivate-m-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-3 col-lg bn">
                        <legend>Thuesday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-t-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-t-${input_id}"
                                   name="activate-t-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-t-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-t-${input_id}"
                                   name="deactivate-t-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-3 col-lg bn">
                        <legend>Wednesday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-w-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-w-${input_id}"
                                   name="activate-w-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-w-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-w-${input_id}"
                                   name="deactivate-w-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-3 col-lg bn">
                        <legend>Thursday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-th-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-th-${input_id}"
                                   name="activate-th-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-th-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-th-${input_id}"
                                   name="deactivate-th-${input_id}"
                                   data-maxlength="5"
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-4 col-lg bn">
                        <legend>Friday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-f-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-f-${input_id}"
                                   name="activate-f-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-f-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-f-${input_id}"
                                   name="deactivate-f-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-4 col-lg bn">
                        <legend>Saturday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-s-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-s-${input_id}"
                                   name="activate-s-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-s-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-s-${input_id}"
                                   name="deactivate-s-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                    <fieldset class="col-xs-12 col-md-4 col-lg bn">
                        <legend>Sunday</legend>
                        <div class="form-item roww mw">
                            <label for="activate-su-${input_id}">Activate</label>
                            <input class="ml10p"
                                   type="text"
                                   id="activate-su-${input_id}"
                                   name="activate-su-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                        <div class="form-item roww mw">
                            <label for="deactivate-su-${input_id}">Deactivate</label>
                            <input class="ml10"
                                   type="text"
                                   id="deactivate-su-${input_id}"
                                   name="deactivate-su-${input_id}"
                                   data-maxlength="5"
                                   ${readOnly ? "disabled" : ''}
                                   oninput="javascript: myFunction3(this.id)"
                                   placeholder="00:00" />
                        </div>
                    </fieldset>
                </div>
            </fieldset>
        </div>`
}

/////////////////////----- MYFUNCTION FUNCS Funcs -----////////////////////////////////////////////

function myFunction(id) {
    var element = document.getElementById(id);
    element.value = element.value.slice(0, element.dataset.maxlength);
}
function myFunction2(id) {
    var element = document.getElementById(id);
    if (+element.value > +element.max) {
        element.value = element.max;
    }
}
function myFunction3(id) {
    var element = document.getElementById(id);
    let len = element.value.length;
    let regex;
    if (len < 2) {
        regex = new RegExp("^([0-1]?[0-9]|2[0-3]):?");
    }
    else if (!element.value.includes(":")) {
        element.value += ":";
        regex = new RegExp("^([0-1]?[0-9]|2[0-3]):([0-5][0-9]?)?$");
    } else {
        regex = new RegExp("^([0-1]?[0-9]|2[0-3]):([0-5][0-9]?)?$");
    }
    if (!regex.test(element.value)) {
        if (element.value.includes(":") && len == 2) element.value = element.value.slice(0, -2);
        else element.value = element.value.slice(0, -1);
    }
}

function changeStyleDisplay(goToId, id) {
    var val = document.getElementById(id).value;
    var divId = goToId + "-schedule";
    var element = document.getElementById(divId);
    if (val === "2") {
        if (element.style.display === "block") {
            element.style.display = "none";
        } else {
            element.style.display = "block";
        }
    } else {
        element.style.display = "none";
    }
}

function addActive() {
    $(document).on('click', '.btnStyle', function () {
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle fire
        $(this).addClass('active');// here apply selected class on clicked btnStyle fire
    });
}

// adding button elements function
function addElement(id, elementType = "") {
    if (id === "element") {
        var last = 0;
        for (i = 1; i <= elements; i++) {
            if (lst.includes(i)) {
                continue;
            } else {
                last = i;
                break;
            }
        }
        if (last === 0 || lst.includes(last)) return;

        let color = Object.keys(BUTTON_COLORS).find(c => elementType.toUpperCase().includes(c));
        let elType = Object.keys(BUTTON_IMAGES).find(im => elementType.toUpperCase().includes(im));
        
        sendMessageWPF({ 'Command': 'AddingElement', 'Params': { 'elementType': `'${elementType}'`, 'elementNumber': `${last}` } });
        const newUserElement = !elementType.toUpperCase().includes('INPUT_GROUP') ? ( //if the element is not INPUT_GROUP
            `<div class="col-12" id=${last}>
                <div class="row">
                    <div class="col-11 pr-1">
                        <a href="javascript:showElement('${last}', '${elementType}')" onclick="javascript:addActive()">
                            <div class="btnStyle ${BUTTON_COLORS[color]}">
                                <i class="${BUTTON_IMAGES[elType].im} fa-3x p15">
                                    <br /><span class="someS">
                                        <span class="h5">
                                            ${BUTTON_IMAGES[elType].sign} ${last}
                                        </span>
                                    </span>
                                </i>
                                                            
                            </div>
                        </a>
                    </div>
                    <div class="col-1 p-0 m-0" onclick="javascript:sendMessageWPF({'Command':'RemovingElement', 'Params': { 'elementType':'${elementType}', 'elementNumber': '${last}' }}, comm = { 'funcName': 'addElement', 'params': {'id' : '${last}', 'elementType': '' }})">
                        <i class="fa-solid fa-xmark fire"></i>
                    </div>
                </div>
            </div>`) : ( //if the element is INPUT_GROUP
                `<div id="${last}" class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                    <div class="row">
                        <fieldset style="min-width: 200px;">
                            <legend>Input Group ${last}</legend>
                                <label for="gr_input_logic_${last}">Input logic</label>:
                                <p class="fire">
                                    AND
                                    <label class="switch">
                                        <input type="checkbox" id="gr_input_logic_${last}"/>
                                        <span class="slider"></span>
                                    </label>
                                    OR
                                </p>

                        </fieldset>
                        <div onclick="javascript:addInputGroup(${last})" class="mt-2 ml-1">
                            <i class="fa-solid fa-xmark fire"></i>
                        </div>
                    </div>
                </div>`);
        var element = document.getElementById("new");
        var new_inner = `
                            ${element.innerHTML}
                            ${newUserElement}
                        `;
        lst.push(last);
        element.innerHTML = new_inner;

        // reordering
        var main = document.getElementById('new');

        [].map.call(main.children, Object).sort(function (a, b) {
            return +a.id.match(/\d+/) - +b.id.match(/\d+/);
        }).forEach(function (elem) {
            main.appendChild(elem);
        });

        // button display check
        if (lst.length === elements) {
            let button = document.getElementById("_btn");
            button.style.display = "none";
        }
    } else {
        if (lst.includes(+id)) {
            var elem = document.getElementById(`${id}`);
            elem.parentNode.removeChild(elem);
            lst = lst.filter(function (item) {
                return item !== +id
            })
            var el = document.getElementById(`id_${id}`);
            if (el) el.parentNode.removeChild(el);

            let button = document.getElementById("_btn");
            if (lst.length < element && button.style.display === "none") {
                button.style.display = "block";
            }
        } else {
            return;
        }
    }
}

// showing element function
function showElement(id, elementType) {
    let returnedJson;
    try {
        boundAsync.getJsonForElement(elementType, +id).then(res => {
            if (res.length > 0) {
                returnedJson = JSON.parse(res);
                if (Object.keys(returnedJson).length > 0) {
                    var el = document.getElementById("selected_area");
                    id = parseInt(id);
                    var target = `<fieldset id="id_${id}">
                                    <legend>Panel ${id}</legend>
                                        <div class="row align-items-center">
                                            <div class="col">
                                                <div class="form-item roww flex">
                                                    <label for="panelip_${id}">Panel IP</label>
                                                    <input type="text"
                                                            id="panelip_${id}"
                                                            name="panelip_${id}"
                                                            minlength="7"
                                                            maxlength="15"
                                                            size="15"
                                                            pattern="^(\s*(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\s*\.){3}(\s*\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\s*$"
                                                            value=" 0 . 0 . 0 . 0 " />
                                                </div>
                                            </div>
                                        </div>

                                        <button class="fire collapsible ml-1">Parameters</button>
                                        <div class="collapsible-content fire">
                                            <div class="row align-items-center m-1">
                                                <div class="col-6">
                                                    <div class="form-item roww disabled">
                                                        <label for="state_${id}">State</label>
                                                        <div class="select">
                                                            <select id="state_${id}" name="state">
                                                                <option value="loc">Normal</option>
                                                            </select>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="col">
                                                    <div class="form-item roww disabled">
                                                        <label for="status_${id}">Status</label>
                                                        <div class="select">
                                                            <select id="status_${id}" name="status">
                                                                <option value="loc">none</option>
                                                            </select>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="row align-items-center m-1">
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="receive_msg_${id}">Receive messages</label>
                                                        <input type="checkbox" id="receive_msg_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="receive_cmd_${id}">Receive commands</label>
                                                        <input type="checkbox" id="receive_cmd_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="send_commands_${id}">Send commands</label>
                                                        <input type="checkbox" id="send_commands_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                            </div>

                                        </div>

                                        <button class="fire collapsible ml-1">Panel Outputs</button>
                                        <div class="collapsible-content fire">
                                            <div class="row align-items-center m-1">
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="r_sounder_${id}">Repeat sounder</label>
                                                        <input type="checkbox" id="r_sounder_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="r_fire_brigade_${id}">Repeat Fire bigrade</label>
                                                        <input type="checkbox" id="r_fire_brigade_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="r_fault_output_${id}">Repeat Fault output</label>
                                                        <input type="checkbox" id="r_fault_output_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div class="form-item roww">
                                                        <label for="r_fire_protection_${id}">Repeat Fire protection</label>
                                                        <input type="checkbox" id="r_fire_protection_${id}" class="ml10" />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </fieldset>`;
                    el.innerHTML = target;
                    collapsible();
                    addVisitedBackground();
                }
            }
        })
    } catch (e) {
        console.log('Error', e);
    }
}
