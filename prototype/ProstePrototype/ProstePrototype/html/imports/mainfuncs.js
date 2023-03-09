//#region VARIABLES

const BUTTON_COLORS = {
    IRIS: 'fire',
    ECLIPSE: 'normal',
    TTE: 'grasse',
};

let darkModeStylesheetId = "ssDarkMode";

let CONFIGURED_IO = {};

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
            body = document.getElementById('divMain');
            if (body.firstElementChild?.tagName === 'FIELDSET') {
                body = body.firstElementChild;
            }

            let path = json["~path"];
            let color;
            if (path) color = Object.keys(BUTTON_COLORS).find(x => path.toUpperCase().includes(x));

            drawFields(body, json, color ? BUTTON_COLORS[color] : '');

            break;
        case !!document.getElementById('divFBF'): 
            fatFbfFunc(json);
            break;
        case !!document.getElementById("divPDevices"):
            body = document.getElementById('divPDevices');

            drawWithModal(body, json);
            break;
        case !!document.getElementById("divLDevices"):
            if (!elements && Object.keys(json)) {
                mainKey = Object.keys(json)
                    .find(currKey => currKey !== '~path' && currKey !== '~panel_id'); // currKey should not be ~path, nor ~panel_id
                elements = json[mainKey]['@MAX']; // case of LDevices
                minElements = 0;
            }

            body = document.getElementById('divLDevices');
            body.querySelector('#new').insertAdjacentHTML('afterbegin', `<p>${newT.t(localStorage.getItem('lang'), "add_up_to")} ${elements} ${newT.t(localStorage.getItem('lang'), 'loops')}</p>`)

            getLoops();
            //drawWithModal(body, json);
            break;
        default:
            // case divIRIS, divTTE, divECLIPSE
            body = document.body;
            let div = document.createElement('div');
            div.classList = "row m2 no-gutter";

            elementsCreationHandler(div, json, reverse = false);

            body.appendChild(div);

            break;
    }
    $(function () {
        // DOM Ready - do your stuff
        pagePreparation();
    });
}
//#endregion

const drawWithModal = (body, json) => {
    // getting keys and adding elements to modal for each
    keys = Object.keys(json).filter(k => k !== '~path' && k !== '~panel_id');

    lst = [0];
    elements = keys.length + 1; // for the case of PDevices
    minElements = 0;

    var deviceList = body.querySelector('#deviceList');
    var modalList = deviceList.querySelector('#list-tab');

    for (k of keys) {
        if (k.split('_').pop() === 'NONE') continue;

        let elType = Object.keys(BUTTON_IMAGES).find(x => k.includes(x));

        modalList.insertAdjacentHTML('beforeend', `<button type="button"
                                class="list-group-item col"
                                id="${k.toLowerCase()}_btn"
                                onclick="javascript: addElement('element', '${k}'); $('#deviceList').modal('hide');"
                                data-toggle="tab"
                                role="tab"
                                aria-selected="false">
                            <div class="btnStyle fire">
                                <i class="${BUTTON_IMAGES[elType].im} fa-3x p-2"></i>
                                <div class="someS">
                                    <h5>${k.split('_').slice(1).join(' ').toUpperCase()}</h5>
                                </div>
                            </div>
                        </button>`);

        getAvailableElements(k.toUpperCase());
    };
}

function drawFields(body, json, inheritedColor = '') {
    if (!json) return;
    // getting keys and creating element for each
    keys = Object.keys(json);
    keys.filter(k => k !== '~path' && k !== '~panel_id').forEach(k => {
        let divLevel = json[k];

        if (k.startsWith('~noname')) { // ~noname cases
            let div = document.createElement('div');
            div.classList = "row align-items-center m-2";
            elementsCreationHandler(div, divLevel);
            body.appendChild(div);
        } else if (divLevel.name === "Company Info") { // unique case "Company Info"
            let fieldset = document.createElement('fieldset');
            var insideRows = `<legend>${newT.t(localStorage.getItem('lang'), "company_info")}</legend>`;
            for (field in divLevel.fields) {
                if (field.includes("~")) continue;
                let pl = newT.t(localStorage.getItem('lang'), divLevel.fields[field]["@LNGID"]);
                let id = divLevel.fields[field]["@LNGID"];
                let m = divLevel.fields[field]["@LENGTH"];
                let value = divLevel.fields[field]["~value"];
                insideRows += `<div class="form-item p0">
                            <input type="text" maxlength="${m}" name="${id}" id="${id}" placeholder="${pl}" value="${value || ''}">
                        </div>`
            }
            fieldset.insertAdjacentHTML('afterbegin', insideRows);
            body.appendChild(fieldset);
        } else if (!divLevel["@TYPE"] && !divLevel.name) {
            minElements = +divLevel["@MIN"];
            for (let i = 0; i < minElements; i++) {
                if (!lst.includes(i)) lst.push(i);
            }
            elements = +divLevel["@MAX"]; // case of divMain
            let btnDiv = document.getElementById("buttons");
            // adding the button
            btnDiv.insertAdjacentHTML(
                'afterbegin',
                `<button style="display: inline-flex;" 
                            type="button"
                            onclick="javascript:addElement('element', '${k}')" 
                            id="_btn" class="btn-round btn-border-black">
                            <i class="fa-solid fa-plus 5x"></i> ${newT.t(localStorage.getItem('lang'), 'add_new')} ${k.split('_').slice(1).join(' ')}
                        </button>`);
            if (k.toUpperCase().includes('ZONE') && !k.toUpperCase().includes('EVAC')) {
                deviceNmbr = 0;
                let divCol9 = document.getElementsByClassName("col-9")[0];
                divCol9.classList = 'col-7';

                var colors = Object.keys(BUTTON_COLORS).find(x => k.includes(x));

                divCol9.insertAdjacentHTML(
                    'beforebegin',
                    `<div class="col-2 bl ${inheritedColor || (colors ? BUTTON_COLORS[colors] : '')} scroll">
                        <div id="attached_device" class="row">                            
                        </div>
                            </div>`
                );
            }
            if (k.toUpperCase().includes('INPUT_GROUP')) {
                const oldEl = document.querySelector('.row.pt-2');
                const newEl = document.createElement("div");
                newEl.id = 'new';
                newEl.classList = "row scroll";
                oldEl.replaceWith(newEl);
            }

            getAvailableElements(k.toUpperCase());
        } else { // collapsible parts
            const { input_name, input_id } = {
                input_name: divLevel.name,
                input_id: divLevel.name.toLowerCase().trim().replaceAll(' ', '_').replace(/["\\]/g, '\\$&').replace(/[/]/g, '')
            }

            var colors = Object.keys(BUTTON_COLORS).find(x => k.includes(x));

            var inside = `<button class="${inheritedColor || (colors ? BUTTON_COLORS[colors] : '')} collapsible ml-1 collapsible_${input_id}">${input_name}</button>
                <div class="collapsible-content col-12">
                    <div class="row align-items-center m-2" id="${input_id}"></div>
                </div>`;

            body.insertAdjacentHTML('beforeend', inside);
            let div = body.querySelector(`#${input_id}`);

            elementsCreationHandler(div, divLevel.fields);

            collapsible(`collapsible_${input_id}`)
        }
    });
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

            if (jsonAtLevel[field]["@TYPE"] === "PAD") return; // another guard

            const innerString = transformGroupElement(jsonAtLevel[field]);
            if (!innerString) return;
            if (jsonAtLevel[field]['@TYPE'] === "WEEK") {
                div.parentNode.parentNode.insertAdjacentHTML('beforeend', innerString);
                return;
            } else if (jsonAtLevel[field]['@TYPE'] === "INTLIST") {
                // inserting in the div of the last element of the collapsible div
                div.lastElementChild.insertAdjacentHTML('beforeend', innerString);
                return;
            }
            const newElement = document.createElement('div');
            let className = `col-12 col-lg-${jsonAtLevel[field]['@TYPE'] === "EMAC" || jsonAtLevel[field]['@TYPE'] === "TAB" ? "12" : "6"} mt-1`;
            newElement.classList = className;
            if (jsonAtLevel[field]['@TYPE'] === "TAB") newElement.style.minHeight = "6em";

            if (typeof (innerString) !== "string") {
                const [inner, jsFunc] = innerString;
                newElement.innerHTML = inner;
                div.appendChild(newElement);
                const [el1, el2, el3, el4] = jsFunc();
                // alert(`el1 -> ${el1}, el2 -> ${el2}, el3 -> ${el3}, el4 -> ${el4}`)
                setTimeout(() => {
                    let el = document.getElementById(el1);
                    loadDiv(el, el2, el3, el4);
                }, 200);
            } else {
                newElement.innerHTML = innerString;
                div.appendChild(newElement);
            }

        } else if (jsonAtLevel[field] && jsonAtLevel[field].title) {
            let title = jsonAtLevel[field].title;
            //const src = "pages-dynamic.js";
            //if (document.querySelectorAll(`script[src*="${src}"]`).length === 0) {
            //    loadScript(() => addButton(title, field, div), src);
            //    //doSleep(50);
            //} else {
            //    //window.setTimeout(() =>
            addAccordeonButton(title, field, div);
            //    //, 50);
            //}
        } else if (jsonAtLevel[field] && Object.keys(jsonAtLevel[field]).length > 0 && typeof (jsonAtLevel[field]) !== 'string') {
            elementsCreationHandler(div, jsonAtLevel[field])
        }
    });
}

//async function doSleep(time) {
//    // Sleep for 0.05 seconds
//    await new Promise(r => setTimeout(() => { r(); }, time));
//}

const appendInnerToElement = (innerString, element, elementJSON) => {
    if (!Array.isArray(elementJSON)) return;
    const newElement = document.createElement('div');
    let andArray = elementJSON.filter(f => f["@TYPE"] === "AND").map(e => e["PROPERTIES"]["PROPERTY"]).flatMap(el => el);
    let len2 = andArray.filter(f => f["@TYPE"] !== "HIDDEN").length || 0;
    let len1 = elementJSON.filter(f => f["@TYPE"] !== "HIDDEN").filter(f => f["@TYPE"] !== "AND").length;
    let className = innerString.slice(0, 9) === '<fieldset' ? "col-12 mt-2 mb-2" : `col-12 col-lg-${(len1 + len2 > 4) ? 6 : (12 / (len1 + len2))} mt-1`;
    newElement.classList = className;
    newElement.innerHTML = innerString;
    element.appendChild(newElement);
}

//#region Zone Device functions
function getZoneDevices(elementNumber) {
    var el = document.getElementById('attached_device');
    if (!el) return;
    el.innerHTML = `<button class="btn-small btn-border-black" onclick="javacript: calculateZoneDevices(${elementNumber})" id="calculateDevices"
                        data-toggle="modal" data-target="#showDevicesListModal" >
                            ${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}
                    </button>`; // reset of the attached to the zone device list

    boundAsync.zoneDevices(elementNumber).then(r => {
        if (!r) return;        
        let zoneDevicesJson = JSON.parse(r);

        //update the nuber of devices in zone
        deviceNmbr = zoneDevicesJson.length;
        let showBtn = document.getElementById("calculateDevices");
        showBtn.innerHTML = `${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}`;        

        zoneDevicesJson.forEach(device => {
            let address = device["~devaddr"];
            let deviceName = device["~device"];
            let loopType = device["~loop"];
            let loopNumber = device["~loop_nom"];
            let key = device["~device"].split("_").slice(1).join("_");
            let showName = device["~devname"] || DEVICES_CONSTS[key].sign;
            //('${loopType}', '${deviceName}', '${loopNumber}', '${address}')
            // create a new button
            const newDeviceInner = `<div class="col-12" id='${deviceName}_${address}'>
                                        <a href="javascript:sendMessageWPF({ 'Command': 'GoToDeviceInLoop', 'Params': { 'loopType': '${loopType}', 'loopNumber': '${loopNumber}', 'elementType': '${deviceName}', 'elementNumber': '${address}' } });" > 
                                            <div class="btnStyle ${BUTTON_COLORS[deviceName.split("_")[0]]}">
                                                <img src="${DEVICES_CONSTS[key].im}" alt="" width="25" height="25" class="m15" />
                                                <div class="someS">
                                                    <div class="h5">${address + '. ' + showName}</div>
                                                </div>
                                            </div>
                                        </a>
                                    </div>`;          

            // inserting
            el.insertAdjacentHTML('beforeend', newDeviceInner);
            
            // reordering
            [].map.call(el.children, Object).sort(function (a, b) {
                return +a.id.match(/\d+$/) - +b.id.match(/\d+$/);
            }).forEach(function (elem) {
                el.appendChild(elem);
            });
        });
    }).catch(e => console.log('e', e));
};

function calculateZoneDevices(elementNumber) {
    let modal = $(document.getElementById("showDevicesListModal"));

    let modalTitle = modal.find('.modal-title')[0];
    modalTitle.innerHTML = `${new T().t(localStorage.getItem('lang'), 'number_of_devices_per_zone')}:`;

    let modalContent = modal.find('.modal-body')[0];

    const deviceMap = new Map();
    boundAsync.zoneDevices(elementNumber).then(res => {
        if (res) {
            let deviceListJSON = JSON.parse(res);
            deviceListJSON.forEach(e => {
                let device = e["~device"].split('_').slice(1).join('_');
                if (deviceMap.has(device)) {
                    deviceMap.set(device, deviceMap.get(device) + 1)
                } else {
                    deviceMap.set(device, 1)
                }
            });

            let innerModalContent = `<div class="table-responsive"><table class="table table-striped"><thead><tr>
                                <th scope="col">${new T().t(localStorage.getItem('lang'), "device_type")}</th>
                                <th scope="col">${new T().t(localStorage.getItem('lang'), "count")}</th>
                              </tr></thead><tbody>`;
            deviceMap.forEach((value, key, map) => {
                innerModalContent += `<tr>
                                <td >${key}</td>
                                <td>${value}</td>
                              </tr>`;
            })
            innerModalContent += `</tbody></table></div>`;
            modalContent.innerHTML = innerModalContent;
        }
    }).catch(err => alert(err));
}
//#endregion

//#region FAT_FBF
function fatFbfFunc(json) {
    let elementNames = Object.keys(json).filter(name => !name.startsWith("~"));
    elementNames.forEach(el => {
        boundAsync.getElement(el).then(response => {
            if (response) {
                let jObj = JSON.parse(response);

                const { input_name, input_id } = {
                    input_name: jObj["@PRODUCTNAME"], //new T().t(localStorage.getItem("lang"), jObj["@LNGID"]),
                    input_id: jObj["@PRODUCTNAME"].replaceAll(" ", "_")
                }

                var inside = `<button class="fire collapsible ml-1 collapsible_${input_id}">${input_name}</button>
                <div class="collapsible-content col-12">
                    <div class="row align-items-center m-2" id="${input_id}"></div>
                </div>`;

                const mainDiv = document.getElementById("divFBF");
                mainDiv.insertAdjacentHTML('beforeend', inside);
                let div = mainDiv.querySelector(`#${input_id}`);

                elementsCreationHandler(div, jObj["PROPERTIES"]);

                collapsible(`collapsible_${input_id}`)
            }
        }).catch(error => console.log(error));
    });
}
//#endregion

//#region Loop Type
function showLoopType(level, type, key, showDivId, selectDivId) {
    const showDiv = document.getElementById(showDivId);
    const selectDiv = document.getElementById(selectDivId);
    let title = "Loop";
    let nextFunc = `showLoopType(2, '${type}', this.value, '${showDivId}', '${selectDivId}')`;
    let dataUsed = [];

    let firstDiv = document.getElementById(`${showDivId + "-Loop"}`);
    let lowerDiv = document.getElementById(`${showDivId + "-Device"}`);
    let lowestDiv = document.getElementById(`${showDivId + "-" + type}`);
    switch (level) {
        case 1:
            // remove lower menus if any
            if (firstDiv) showDiv.removeChild(firstDiv);
            if (lowerDiv) showDiv.removeChild(lowerDiv);
            if (lowestDiv) showDiv.removeChild(lowestDiv);

            dataUsed = Object.keys(CONFIGURED_IO).map(loop => {
                return { value: loop, label: loop.split("_").slice(1).join("_"), selected: CONFIGURED_IO[loop]["selected"] }
            });
            break;
        case 2:
            title = "Device";
            // remove lower menus if any
            if (lowerDiv) showDiv.removeChild(lowerDiv.parentNode.parentNode);
            if (lowestDiv) showDiv.removeChild(lowestDiv.parentNode.parentNode);

            nextFunc = `showLoopType(3, '${type}', '${key}' + '+' + this.value, '${showDivId}', '${selectDivId}')`;
            dataUsed = Object.keys(CONFIGURED_IO[key])
                .filter(deviceName => deviceName !== "selected")
                .map(deviceName => {
                    let nameLst = deviceName.split('/');
                    let name = !(nameLst[2]) ? nameLst[0].split("_").slice(1).join("_") : nameLst[1];
                    let address = nameLst.at(-1);
                    let currentDeviceNameObject = CONFIGURED_IO[key][deviceName];
                    let checkedJson = Object.keys(currentDeviceNameObject)
                        .map(channel => currentDeviceNameObject[channel]["uses"])
                        .filter(uses => uses === null || (Array.isArray(uses) && uses.length === 0));
                    if (checkedJson.length === 0) {
                        return {
                            value: deviceName, label: `${address}. ${name} - ${newT.t(localStorage.getItem('lang'), 'all_channels_used')}`, selected: CONFIGURED_IO[key][deviceName]["selected"]
                        };
                    }
                    return { value: deviceName, label: address + '. ' + name, selected: CONFIGURED_IO[key][deviceName]["selected"] };
                });
            break;
        case 3:
            title = type;
            // remove lower menus if any
            if (lowestDiv) lowestDiv.parentNode.parentNode.parentNode.removeChild(lowestDiv.parentNode.parentNode);
            let jsonAtLevel3 = CONFIGURED_IO[key.split("+")[0]][key.split("+")[1]];
            dataUsed = Object.keys(jsonAtLevel3)
                .filter(ch => ch !== "selected")
                .map(ch => {
                    let nameLst = ch.split('/');
                    let name = nameLst[0] ? nameLst[0] : nameLst[1];
                    if (jsonAtLevel3[ch]["uses"] && Array.isArray(jsonAtLevel3[ch]["uses"]) && jsonAtLevel3[ch]["uses"].length > 0) {
                        return { value: ch, label: `${name} - ${newT.t(localStorage.getItem('lang'), 'used')}`, selected: jsonAtLevel3[ch]["selected"] };
                    }
                    return { value: ch, label: name, selected: jsonAtLevel3[ch]["selected"] };
                });
            nextFunc = `showLoopType(4, '${type}', '${key}' + '+' + this.value, '${showDivId}', '${selectDivId}')`;
            break;
        case 4:
            sendMessageWPF({ 'Command': 'changedValue', 'Params': { 'path': CONFIGURED_IO[key.split("+")[0]][key.split("+")[1]][key.split("+")[2]]["path"], 'newValue': CONFIGURED_IO[key.split("+")[0]][key.split("+")[1]][key.split("+")[2]]["channel_path"] } });
            return;
    }

    let selectId = showDivId + "-" + title;
    let inner = `
            <div class="form-item roww">
                <label for="${selectId}">${newT.t(localStorage.getItem('lang'), title.toLowerCase())}</label>
                <div class="select">
                    <select id="${selectId}" name="${selectId}"
                        onchange="javascript: ${nextFunc}" >
                        <option value="" disabled ${dataUsed.some(x => x["selected"]) ? "" : "selected"} >${newT.t(localStorage.getItem('lang'), 'select_an_option')}</option>`;
    dataUsed.map(o => {
        let disabled = ""; let tooltip = "";
        if (level >= 2) {
            if (type.toLowerCase() === "output" && (o["label"].includes(` - ${newT.t(localStorage.getItem('lang'), 'used')}`) || o["label"].includes(` - ${newT.t(localStorage.getItem('lang'), 'all_channels_used')}`))) {
                disabled = 'disabled style="color: red"'
            }
            if (o["label"].includes(` - ${newT.t(localStorage.getItem('lang'), 'used')}`)) {
                let againJsonAtLevel3 = CONFIGURED_IO[key.split("+")[0]][key.split("+")[1]];
                tooltip = `title="Used in: ${againJsonAtLevel3[o["value"]]["uses"]}"`;
            }
            if (o["label"].includes(` - ${newT.t(localStorage.getItem('lang'), 'all_channels_used')}`)) {
                let jsonAtLevel2 = CONFIGURED_IO[key][o["value"]];
                tooltip += `title="${newT.t(localStorage.getItem('lang'), 'used_in')}: {`;
                for (let channel in jsonAtLevel2) {
                    let channelInfo = channel.split("/");
                    tooltip += `{ ${channelInfo[0] ? channelInfo[0] : channelInfo[1]} -> ${jsonAtLevel2[channel]["uses"]} }`;
                }
                tooltip += `}"`;
            }
        }

        inner += `<option value="${o["value"]}" ${disabled} ${tooltip} ${o["selected"] ? "selected" : ""}>${o["label"]}</option>`
    });
    inner += `</select>
                </div>`;

    showDiv.insertAdjacentHTML('beforeend', inner);

    adjustCollapsibleHeight(selectDiv);
    addVisitedBackground();
    setupSelFixer();

    if (dataUsed.some(x => x["selected"])) {
        if (level + 1 === 3) {
            showLoopType(level + 1, type, `${key}+${dataUsed.find(x => x["selected"]).value}`, showDivId, selectDivId)
        } else if (level + 1 < 3) {
            showLoopType(level + 1, type, dataUsed.find(x => x["selected"]).value, showDivId, selectDivId)
        }
    }
}

//createLoopTypeMenu
function createLoopTypeMenu(selectDiv, showDiv, path) {
    let fieldset = document.createElement("fieldset");
    fieldset.id = "loop_type-" + showDiv.id;
    fieldset.insertAdjacentHTML('afterbegin', `<legend>${newT.t(localStorage.getItem('lang'), 'loop_type')}</legend>`);
    let pathFound = path, channel_path = "", loop_number = "", device = "", address = "";
    /*  value reading: the whole is the channel path
     * "IRIS8_TTELOOP1/IRIS8_TTENONE#IRIS8_MIO22.ELEMENTS.IRIS8_MIO22.PROPERTIES.PROPERTY[9].~index~1"
     * --------loop number----------#--device---.-----------------channel-------------------.-address-
     * 
     * "IRIS_LOOP2/IRIS_SNONE#IRIS_S6200.ELEMENTS.IRIS_S6200"
     * -----loop number------#--device--.
     * */

    if (Array.isArray(path)) { // it should be an array of [~path, ~value]
        pathFound = path[0];
        channel_path = path[1];
    }
    if (channel_path) {
        loop_number = channel_path.split("/")[0];
        device = channel_path.split("#")[1].split(".")[0];
        address = channel_path.split("~").pop(); // if not ~ in channel_path will return channel_path
    }
    let type = pathFound.split(".").find(e => e.includes("putType") || e.includes("FAT_FBF_"));
    switch (type) {
        case "FAT_FBF_OUT1":
        case "FAT_FBF_OUT2":
        case "FAT_FBF_OUT3":
        case "FAT_FBF_OUT4":
        case "OutputType":
            boundAsync.loopsOutputs(pathFound).then(result => {
                CONFIGURED_IO = JSON.parse(result);
                addElementToCONFIGURED_IO(loop_number, device, address, channel_path);

                showLoopType(1, "Output", "", fieldset.id, selectDiv.id);

            }).catch(err => alert("Error" + err)); break;
        case "FAT_FBF_IN1":
        case "FAT_FBF_IN2":
        case "FAT_FBF_IN3":
        case "InputType":
            boundAsync.loopsInputs(pathFound).then(result => {
                CONFIGURED_IO = JSON.parse(result);
                addElementToCONFIGURED_IO(loop_number, device, address, channel_path);

                showLoopType(1, "Input", "", fieldset.id, selectDiv.id);

            }).catch(err => alert("Error" + err)); break;
        default:
            alert("something is wrong");
    }

    showDiv.replaceChildren(fieldset);
}
// adding ["selected"] flag to CONFIGURED_IO for pre-configured Loop Type based on predefined loop, device, deviceAddress and channel
function addElementToCONFIGURED_IO(loop_number, device, deviceAddress, channel_path) {
    if (!channel_path || !CONFIGURED_IO) return;
    for (let loopProp in CONFIGURED_IO) {
        if (loopProp === loop_number) {
            CONFIGURED_IO[loopProp]["selected"] = true;
            for (let deviceProp in CONFIGURED_IO[loopProp]) {
                if (deviceProp.includes(device)) {
                    if (deviceAddress && deviceProp.endsWith(deviceAddress)) {
                        CONFIGURED_IO[loopProp][deviceProp]["selected"] = true;
                        for (let channelProp in CONFIGURED_IO[loopProp][deviceProp]) {
                            if (CONFIGURED_IO[loopProp][deviceProp][channelProp]["channel_path"] === channel_path) {
                                CONFIGURED_IO[loopProp][deviceProp][channelProp]["selected"] = true;
                            }
                        }
                    }
                    else if (deviceAddress === channel_path)
                    {
                        CONFIGURED_IO[loopProp][deviceProp]["selected"] = true;
                        CONFIGURED_IO[loopProp][deviceProp]["device"]["selected"] = true;
                    }
                }
            }
        }
    }
}
//#endregion

// transforming object function
const transformGroupElement = (elementJson) => {
    let attributes = {
        type: elementJson['@TYPE'],
        input_name: newT.t(localStorage.getItem('lang'), elementJson['@LNGID']), //(elementJson['@TEXT'] ? elementJson['@TEXT'] : (elementJson['@ID'] && elementJson['@ID'] !== 'SUBTYPE' && elementJson['@TYPE'] !== 'AND') ? elementJson['@ID'] : elementJson['@TEXT']).trim().replaceAll(" ", "_").toLowerCase().replace(/[/*.?!#]/g, '')), //.charAt(0).toUpperCase() + elementJson['@TEXT'].slice(1),
        input_id: elementJson['@TEXT'] ? elementJson['@TEXT'].toLowerCase().replaceAll(' ', '_') : elementJson['@LNGID'], //.replaceAll("-", "_"),
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
        size: elementJson['@SIZE'],
        value: elementJson["~value"] ? elementJson["~value"] : (elementJson['@VALUE'] ? elementJson['@VALUE'] : (elementJson['@MIN'] ? elementJson['@MIN'] : "")),
    };

    switch (attributes.type) {
        case 'AND':
            let andElementsList = elementJson["PROPERTIES"] && elementJson["PROPERTIES"]["PROPERTY"];
            if (!Array.isArray(andElementsList)) return '';
            let fs = document.createElement('fieldset');
            fs.id = attributes.input_id;
            fs.insertAdjacentHTML('afterbegin', `<legend>${attributes.input_name}</legend>`);
            let ds = document.createElement('div');
            ds.classList = 'row align-items-center';

            for (el in andElementsList) {
                let andElDiv = document.createElement('div');
                andElDiv.classList = 'col-12 col-lg-' + (andElementsList.length > 4 ? 3 : 12 / andElementsList.length);
                andElDiv.insertAdjacentHTML('beforeend', transformGroupElement(andElementsList[el]));
                ds.appendChild(andElDiv);
            }
            fs.appendChild(ds);
            return fs.outerHTML;
        case 'HIDDEN': return '';
        case 'TAB':
            let tabs = elementJson['TABS'];
            if (tabs["TAB"]) tabs = tabs["TAB"];
            // input_name = "Input Type", input_id = "input_name"
            let tabsKeys = Object.keys(tabs);
            //console.log('tabsKeys', tabsKeys, 'tabs', tabs);
            // create selectField and append it
            let inner = `<div class="form-item roww">
                            <label for="${attributes.input_id}">${attributes.input_name}</label>
                            <div class="select">
                                <select id="${attributes.input_id}" name="${attributes.input_id}" 
                                        onchange="javascript: sendMessageWPF({'Command': 'changedValue','Params':{'path':'${attributes.path}','newValue': this.value}});
                                                              loadDiv(this, 'showDiv_${attributes.input_id}', this.value, this.options[this.selectedIndex].getAttribute('checktype'));" >`;
            tabsKeys.map(o => {
                let disabled = false; // todo
                let checkType = "";
                if (tabs[o].hasOwnProperty("~enabled")) {// if exists such field ["~enabled"]
                    //alert(attributes.input_name + ' - ' + tabs[o]["@NAME"] + ' - showing only if exists: ' + tabs[o].hasOwnProperty("~enabled") + ' and the data is ' + tabs[o]["~enabled"]);
                    disabled = !tabs[o]["~enabled"];
                    if (tabs[o]["~enabled"]) {
                        checkType = `checktype='${tabs[o]["~path"]}'`;
                    }
                }
                let value = `${tabs[o]['@VALUE']}_${o}`;  // (tabs[o].hasOwnProperty("~enabled")) ? tabs[o]['@VALUE'] : `${tabs[o]['@VALUE']}_${o}`;
                let selected;
                if (attributes.value) {
                    selected = attributes.value === value ? "selected" : "";
                } else {
                    selected = !!(+tabs[o]['@DEFAULT']) ? "selected" : "";
                }
                inner += `<option ${checkType} value="${value}" ${selected} ${disabled ? "disabled" : ""} >${newT.t(localStorage.getItem('lang'), tabs[o]['@LNGID'])} </option>`
            });
            inner += `</select>
                            </div>
                        </div>
                        <div id="showDiv_${attributes.input_id}"></div>`;

            let additionalOnChangeCommand = "";
            let loopTypePath = "";
            // add additional div with display=none
            tabsKeys.forEach(key => {
                if (!tabs[key]["~enabled"]) {
                    let tabDiv = document.createElement('div');
                    tabDiv.id = tabs[key]['@VALUE'] + '_' + key;
                    tabDiv.style.display = 'none';

                    let tabFlag = (tabs[key]["PROPERTIES"] && Array.isArray(tabs[key]["PROPERTIES"]["PROPERTY"]));

                    let fieldsetDiv = document.createElement(tabFlag ? 'div' : 'fieldset');
                    fieldsetDiv.classList = 'row align-items-center';

                    let elementJSON;
                    if (tabs[key]["PROPERTIES"]) {
                        if (!tabFlag) {
                            fieldsetDiv.insertAdjacentHTML('afterbegin', `<legend>${newT.t(localStorage.getItem('lang'), tabs[key]["PROPERTIES"]["PROPERTY"]["@LNGID"])}</legend>`);
                        }
                        elementJSON = tabs[key]["PROPERTIES"]["PROPERTY"];

                        for (elIndex in elementJSON) {
                            if (tabFlag) { // if it is an array => tabFlag
                                if (elementJSON[elIndex]['@TYPE'] === 'HIDDEN') continue;
                                let innerString;
                                if (elementJSON[elIndex]['@TYPE'] === "AND") {
                                    let andElements = elementJSON[elIndex]["PROPERTIES"]["PROPERTY"];
                                    if (Array.isArray(andElements)) {
                                        for (andEl in andElements) {
                                            innerString = transformGroupElement(andElements[andEl]);
                                            if (innerString && typeof (innerString) === "string") //NB!! Might lead to bug
                                                appendInnerToElement(innerString, fieldsetDiv, elementJSON);
                                            innerString = '';
                                        }
                                    } else {
                                        innerString = transformGroupElement(andElements);
                                    }
                                } else {
                                    innerString = transformGroupElement(elementJSON[elIndex]);
                                }
                                if (innerString && typeof (innerString) === "string") {
                                    appendInnerToElement(innerString, fieldsetDiv, elementJSON);
                                } else if (innerString && typeof (innerString) !== "string") {
                                    const [inner, jsFunc] = innerString;
                                    appendInnerToElement(inner, fieldsetDiv, elementJSON);
                                    console.log("should've reached reached the end of the three"); //NB!! Might lead to bug
                                }
                                if (elementJSON.filter(f => f["@TYPE"] !== "HIDDEN").filter(f => f["@TYPE"] !== "AND").length === 0) {
                                    fieldsetDiv.classList.add('justify-content-around');
                                }
                            }
                            else {
                                if (elementJSON['@TYPE'] === 'HIDDEN') continue;
                                const newElement = document.createElement('div');
                                let className = `col-12 col-lg-${elementJSON['@TYPE'] === "EMAC" || elementJSON['@TYPE'] === "TAB" ? "12" : "6"} mt-1`;
                                newElement.classList = className;

                                const innerString = transformGroupElement(elementJSON);
                                if (typeof (innerString) !== "string") {
                                    const [inner, jsFunc] = innerString;
                                    newElement.innerHTML = inner;
                                    fieldsetDiv.appendChild(newElement);
                                    const [el1, el2, el3, el4] = jsFunc();
                                    // alert(`el1 -> ${el1}, el2 -> ${el2}, el3 -> ${el3}, el4 -> ${el4}`)
                                    setTimeout(() => {
                                        let el = document.getElementById(el1);
                                        loadDiv(el, el2, el3, el4);
                                    }, 200);
                                } else {
                                    newElement.innerHTML = innerString;
                                    fieldsetDiv.appendChild(newElement);
                                }
                                break;
                            }
                        }
                        tabDiv.appendChild(fieldsetDiv);
                    } else { // tabs[key] without properties but with tabs, so the select must incude the sendMessageWPF funcitonality 
                        additionalOnChangeCommand = `sendMessageWPF({'Command': 'changedValue','Params': {'path':'${attributes.path}','newValue': this.value}})`;
                    }
                    inner += tabDiv.outerHTML;
                } else if (tabs[key]["~value"]) {
                    loopTypePath = [tabs[key]["~path"], tabs[key]["~value"]];
                }
            });
            if (additionalOnChangeCommand && inner.indexOf('" ><option value="0') !== -1) {
                inner = inner.substring(0, inner.indexOf('" ><option value="0')) +
                    additionalOnChangeCommand + inner.substring(inner.indexOf('" ><option value="0'));
            }
            if (attributes.value && (!elementJson["@ID"] || elementJson["@ID"] !== "SUBTYPE")) {
                // elementJson and attributes are those of Input Type
                const jsFunc = () => [`${attributes.input_id}`, `showDiv_${attributes.input_id}`, `${attributes.value}`, loopTypePath];

                return [inner, jsFunc];
            }
            return inner;
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
                attributes.input_name_on = newT.t(localStorage.getItem('lang'), 'enabled');
                attributes.input_name_off = newT.t(localStorage.getItem('lang'), 'disabled');
            } else {
                attributes.input_name_on = newT.t(localStorage.getItem('lang'), 'on');
                attributes.input_name_off = newT.t(localStorage.getItem('lang'), 'off');
            }
            if (attributes.value === "1" || attributes.value === "True") attributes.checked = true;
            else if (attributes.value === "0" || attributes.value === "False") attributes.checked = false;
            return getSliderInput({ ...attributes });

        case 'CHECK':
            if (attributes.value === "1" || attributes.value === "True") attributes.checked = true;
            else if (attributes.value === "0" || attributes.value === "False") attributes.checked = false;
            return getCheckboxInput({ ...attributes });

        case 'LIST':
            if (elementJson["addChannelHelp"]) attributes.addButton = true;
            attributes.selectList = elementJson.ITEMS.ITEM.map((o) => {
                let sel;
                if (attributes.value) {
                    sel = (attributes.value === o['@VALUE'])
                } else {
                    sel = !!(o['@DEFAULT']);
                }
                let label = newT.t(localStorage.getItem('lang'), o['@LNGID']) === "Not found translation key" ?
                                o['@NAME'] :
                                newT.t(localStorage.getItem('lang'), o['@LNGID']);
                let tmp = {
                    value: o['@VALUE'],
                    label: label,
                    selected: sel
                };
                if (o['ScheduleKey']) tmp.link = o['ScheduleKey'];
                if (o['@IMAGE']) tmp.imageKey = o['@IMAGE'].split('.')[0];
                return tmp;
            });
            if (!attributes.selectList.map(e => e.selected).some(e => !!e)
                && elementJson['@AND']
                && attributes.selectList.map(e => e.value).some(e => e === elementJson['@AND'])) {
                attributes.selectList.find(e => e.value === elementJson['@AND']).selected = true;
            };
            attributes.modal = elementJson["openModalFirst"];
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
            if (attributes.value)
                attributes.value = attributes.value.replaceAll(":", " : ");
            attributes.input_name = elementJson["@TEXT"];
            return getEmacInput({ ...attributes });

        case 'WEEK':
            attributes.input_id = elementJson['@TEXT'].replaceAll(' ', '');
            return getWeekInput({ ...attributes });

        case 'INTLIST':
            attributes.value = attributes.value.split(',');
            return getIntListInput({ ...attributes });
        default: break;
    }
}

//#region collapsible part
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
//#endregion

//#region pagePreparation, contextMenu, toggleDarkMode
function pagePreparation() {
    $(() => { // $(function() {}) equivalent to $(document).ready(function () {})
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
        setupSelFixer();

        // URLSearchParams: loopType, loopNumber, elementType, elementNumber (address)
        // etape 1: showLoop('1', 'IRIS_TTELOOP1')
        if (searchParams.has('loopNumber') && searchParams.has('loopType')) {
            let loopNumber = searchParams.get('loopNumber');
            let loopType = searchParams.get('loopType');
            showLoop(loopNumber, loopType);

            //etape 2: showDevice('IRIS_TTELOOP1', 'IRIS_MIO22', '1', '1')
            if (searchParams.has('elementType') && searchParams.has('elementNumber')) {
                let address = searchParams.get('elementNumber');;
                let deviceName = searchParams.get('elementType');;
                showDevice(loopType, deviceName, loopNumber, address);
                setTimeout(() => $(`#${address}_${deviceName}`).addClass('active'), 200);
            }
        }
    });
}
//pagePreparation();
function showContextMenu(event, el) {
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
//#endregion

//#region UTILS
// select dropdownmenu limitation to 5 rows
function setupSelFixer(contain = $("body")) {
    if (!window.IsLocal) {
        contain.find("select").on("mousedown", function (ev) {
            //console.log("selFixer mouseDown.");
            var _this = $(this);
            //this.focus();
            var size = 5;
            if (_this.hasClass("sf6")) {
                size = 6;
            } else if (_this.hasClass("sf3")) {
                size = 3;
            } else if (_this.hasClass("sf8")) {
                size = 8;
            } else if (_this.hasClass("sf12")) {
                size = 12;
            }
            //console.log("ht:", this.style.height);
            if (this.options.length > size) {
                this.size = size;
                this.style.height = `${size}rem`;
                this.style.marginBottom = (-(size - 1)) + "rem";
                this.style.position = "relative";
                this.style.zIndex = 10009;
            }
        });
        //onchange
        contain.find("select").on("change select", function () {
            //console.log("selFixer Change.");
            resetSelFixer(this);
        });
        //onblur
        contain.find("select").on("blur", function () {
            resetSelFixer(this);
        });
        function resetSelFixer(el) {
            el.size = 0;
            el.style.height = "auto";
            el.style.marginBottom = "0rem";
            el.style.zIndex = 1;
        }
    }
}

// checking a hex value function
function checkHexRegex(event) {
    let val = event.target.value;
    let regEx = /^([0-9A-Fa-f]{1,2})$/;
    let isHex = regEx.test(val);
    if (!isHex) {
        document.getElementById(event.target.id).value = val.slice(0, -1);
    }
}

// hsow changed articles
function addVisitedBackground() {
    $('.form-item input, .form-item select').on('change', function () {
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
    var body = document.body;
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = src;

    script.onreadystatechange = callback;
    script.onload = callback;
    body.appendChild(script);
}

function weekChangedValueHandler(scheduleId, index, newValue, path) {
    let oldValues = document.getElementById(`${scheduleId}`).getAttribute("value").split(" ");
    let newValues = [...oldValues];
    newValues[+index] = newValue;
    //console.log(newValues.join(" "));
    sendMessageWPF({ 'Command': 'changedValue', 'Params': { 'path': path, 'newValue': newValues.join(" ") } });
}

function intListChangedValueHandler(index, newValue, path, input_id, size) {
    let valueArray = [];
    for (var i = 0; i < +size; i++) {
        var e = document.getElementById(`${input_id}_${i}`);
        if (i === index) {
            valueArray.push(+newValue);
            e.value = +newValue;
        } else {
            valueArray.push(e.value ? e.value : 0);
        }
    }
    sendMessageWPF({ 'Command': 'changedValue', 'Params': { 'path': path, 'newValue': valueArray.join(',') } });
}
//#endregion

//#region //----- COMMON Funcs -----////////////////////////////////////////////
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
                    onchange="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.checked}})" />
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
                            onchange="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.checked }})"/>
                        <span class="slider"></span>
                    </label>
                    ${input_name_on}
                </p>
            </div>`
}

const getSelectInput = ({ input_name, input_id, selectList, placeHolderText, bytesData, lengthData, readOnly, modal, addButton = false, RmBtn = false, path = "" }) => {
    let link = selectList.filter(x => x.link !== undefined).length > 0 && selectList.filter(x => x.link !== undefined)[0].link;
    let image = selectList.filter(x => x.imageKey).length > 0;
    let str = `<${image ? "fieldset" : "div"} class="form-item roww mt-1">
                    ${image ? `<legend>${input_name}</legend>` : ""}
                    ${RmBtn ? `<button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)">
                        <i class="fa-solid fa-square-minus fa-2x"></i>
                    </button>` : ""}
                    ${image ? "" : `<label for="${input_id}">${input_name}</label>`}
                    ${addButton ? `<button 
                                        id="popoverData_${input_id}" class="btn"
                                        data-content="Popover with data - trigger"
                                        onmouseover="javascript: showChannelInfo(this, '${path}');"
                                        data-placement="top" data-original-title="Used in:">
                                            <i class="fa-solid fa-circle-question"></i>
                                    </button>`: ""}
                    <div class="select">
                        <select id="${input_id}" name="${input_id}"
                            ${bytesData ? `bytes="${bytesData}"` : ""} 
                            ${lengthData ? `length="${lengthData}"` : ""} 
                            ${readOnly ? "disabled" : ''}                            
                            onchange="javascript: ${modal ? modal : `sendMessageWPF(
                                {'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}}
                                ${link.length > 0 ? `, {'funcName': 'changeStyleDisplay', 'params': { 'goToId': '${link}', 'id': '${input_id}' }}` : ""})
                                ${image ? `; loadDiv(this,'showSchema_${input_id}', this.value + '_function');` : ""}
                            `}" >`;
    if (selectList.length > 0) {
        let isDefaultValue = selectList.map(v => v.selected).reduce((prevValue, currValue) => (prevValue || currValue), false);
        str += `<option value="" disabled ${isDefaultValue ? "" : "selected"}>${placeHolderText || newT.t(localStorage.getItem('lang'), 'select_an_option')}</option>`;
        selectList.map(o => str += `<option value="${o.value}" ${o.selected ? "selected" : ""}>${o.label}</option>`);
    }
    str += '</select>';
    if (link.length > 0)
        str += `<img src onerror="javascript: changeStyleDisplay('${link}', '${input_id}')" />`; // dirty workaround
    str += `</div>${image ? "" : "</div>"}`;

    if (image) {
        str += `<div id="showSchema_${input_id}" class="image col-9" style="margin-left: auto; margin-right: auto">`
        let selectedImageEl = selectList.find(o => o.selected);
        if (selectedImageEl) {
            str += `<img src="${BUTTON_IMAGES[selectedImageEl.imageKey].im}" alt="${selectedImageEl.imageKey}" />`
        }
        str += `</div>`;

        selectList.map(o => str += `<div style="display: none" id="${o["value"] + "_function"}">
            <img src="${BUTTON_IMAGES[o.imageKey].im}" alt="${o.imageKey}" />
        </div>`);
        str += "</fieldset>"
    }
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
                        onchange"javascript: myFunction2(this.id)"
                        onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}','newValue': this.value}})"
                        ${min ? `min="${min}"` : ""} ${max ? `max="${max}"` : ""}
                        ${value ? `value="${value}"` : ""}
                        ${bytesData ? `bytes="${bytesData}"` : ""} 
                        ${lengthData ? `length="${lengthData}"` : ""} 
                        ${readOnly ? "disabled" : ''} />
            </div>`;
}

const getEmacInput = ({ input_id, input_name, readOnly, value, RmBtn = false, path = "" }) => {
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
                            placeholder="00 : 00 : 00 : 00 : 00 : 00" ${readOnly ? "disabled" : ''}
                            value="${value}"
                            oninput="javascript: checkHexRegex(event)" maxlength="2"
                            onblur="javascript:sendMessageWPF({'Command': 'changedValue','Params':{'path':'${path}0','newValue': this.value,'position':this.id.replace('${input_id}','')}})"
                            />
                 </div>
            </div>`;
}

const getWeekInput = ({ input_id, input_name, readOnly, value, size, RmBtn = false, path = "" }) => {
    //value = "04:04 03:03 01:00 00:01 01:02 02:01 00:00 00:00 00:00 00:00 00:00 00:00 00:00 00:00"; // for test purposes
    let fields = [["Sunday", "-su-"], ["Moday", "-m-"], ["Thuesday", "-t-"], ["Wednesday", "-w-"], ["Thursday", "-th-"], ["Friday", "-f-"], ["Saturday", "-s-"]];
    let data = value.split(" ");
    let inner = `<div style="display: none" id="${input_id}-schedule" value="${value}">
            <fieldset>
                <legend>${input_name}</legend>
                <div class="row">`;
    for (let i = 0; i < size / 2; i++) {
        if (i % 2 === 0) inner += `<fieldset class="col-12 col-md-3 col-lg bn"><legend>${fields[Math.floor(i / 2)][0]}</legend>`;
        inner += ` <div class="form-item roww mw">
                        <label for="${i % 2 === 0 ? "activate" : "deactivate"}${fields[Math.floor(i / 2)][1]}${input_id}">${i % 2 === 0 ? newT.t(localStorage.getItem('lang'), 'activate') : newT.t(localStorage.getItem('lang'), 'deactivate')}</label>
                        <input class="ml10${i % 2 === 0 ? 'p' : ''}"
                                type="text"
                                id="${i % 2 === 0 ? "activate" : "deactivate"}${fields[Math.floor(i / 2)][1]}${input_id}"
                                name="${i % 2 === 0 ? "activate" : "deactivate"}${fields[Math.floor(i / 2)][1]}${input_id}"
                                data-maxlength="5"
                                ${readOnly ? "disabled" : ''}
                                value="${data[i] || ''}"
                                oninput="javascript: myFunction3(this.id)"
                                onblur="javascript: weekChangedValueHandler('${input_id}-schedule', ${i}, this.value, '${path}')"
                                placeholder="00:00" />
                    </div>`
        if (i % 2 !== 0) inner += `</fieldset>`;
    }
    inner += "</div></fieldset></div>"
    return inner;
}

const getIntListInput = ({ input_id, input_name, max, min, RmBtn = false, path = "", value, size }) => {
    attributes = [ input_id, input_name, max, min, RmBtn, path, value, size ];
    let inner = `<fieldset id="${input_id}">
                <legend>${input_name}</legend>
                <div class="form-item roww row mr-1">`;
    for (let i = 0; i < size; i++) {
        inner += `<div class="col-3 p-0">
                        <input
                            type="number" id="${input_id}_${i}" name="${input_id}_${i}" 
                            data-maxlength="${max.length}" 
                            oninput="javascript: myFunction(this.id); myFunction2(this.id);"
                            onblur="javascript: intListChangedValueHandler(${i}, this.value, '${path}', '${input_id}', ${size});"
                            value="${value[i]}" min="${min}" max="${max}">
                    </div>`;
    }
    inner += `</div>
            </fieldset >`;
    return inner;
}
//#endregion

//#region //----- MYFUNCTION FUNCS Funcs -----////////////////////////////////////////////

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

function addActive(doc = document) {
    $(doc).on('click', '.btnStyle', function () {
        $('.btnStyle').removeClass('active');// here remove class active from all btnStyle fire
        $(this).addClass('active');// here apply selected class on clicked btnStyle fire
    });
}

const getAvailableElements = (elementType) => {
    boundAsync.getElements(elementType).then(r => {
        if (!r) return;
        let elementList = JSON.parse(r);
        // creating the founded elements
        Object.keys(elementList).forEach(key => {
            addConcreteElement(key, elementType);
        });
    }).catch(err => alert(err));
}

// adding pre-defined elements function
function addConcreteElement(id, elementType = "") {
    if (lst.includes(+id)) {
        return;
    } else {
        last = parseInt(id);
        createElementButton(last, elementType);
    }
}

// adding button elements function
function addElement(id, elementType = "") {

    if (id === "element") {

        callAddressModal(elementType); // going through modal for defining the address of the element
        
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

function callAddressModal(elementType, current) {
    let modal = document.getElementById("addressModal");
    document.getElementById("addressModalLabel").innerText = newT.t(localStorage.getItem('lang'), "element_address");
    modal.querySelector(".btn.btn-secondary").innerText = newT.t(localStorage.getItem('lang'), "MenuClose");
    let innerSelectText = `<div class="form-item roww mt-1">
                                    <label for="select_address">${newT.t(localStorage.getItem('lang'), "set_address")} ${elementType}</label>
                                    <div class="select">
                                        <select id="select_address" name="select_address">`;

    for (var i = minElements; i <= elements; i++) {
        let currentId = i === +current ? "selected" : "";
        let disabled = lst.includes(i) ? "disabled style='background: #aaa;'" : ""
        innerSelectText += `<option value="${i}" ${disabled} ${currentId}>${i}</option>`;
    }

    modal.querySelector(".modal-body").innerHTML = innerSelectText;

    modal.querySelector(".btn.btn-primary").addEventListener('click', function handler(){
        if (!lst.includes(modal.querySelector("select").value)) {
            if (!current) // guard if new
                addElementAtAddress(elementType, modal.querySelector("select").value); // setting the element at the given address            
            else { // else it is not a new item, so modify the old one
                modifyElementCurrentAddress(current, elementType, modal.querySelector("select").value)
            }
        }
        $(modal).modal('hide');
        // removing the event listener function
        this.removeEventListener('click', handler);
    });
    $(modal).modal('show');
}

function addElementAtAddress(elementType, last) {
    if (elementType === "INPUT_GROUP") {
        boundAsync.addingElementSync(`${elementType}`, +last).then(r => {
            if (r === "added") {
                createElementButton(last, elementType); // creating the fieldset of the INPUT_GROUP element
            }
        }).catch((e) => console.log(e));
    } else {
        sendMessageWPF({ 'Command': 'AddingElement', 'Params': { 'elementType': `'${elementType}'`, 'elementNumber': `${last}` } });
        createElementButton(last, elementType); // creating the button of the element
    }
}

function modifyElementCurrentAddress(oldAddress, elementType, newAddress) {
    boundAsync.modifyElementAddress(oldAddress, elementType, newAddress).then(r => {
        if (r) {
            // recreate button
            var oldButton = document.getElementById(oldAddress);
            oldButton.parentNode.removeChild(oldButton);
            createElementButton(newAddress, elementType);
            lst = lst.filter(address => address !== +oldAddress);
            // recreate fieldset------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            showElement(newAddress, elementType);
            document.getElementById(newAddress).addClass('active');
        }
    });
}

async function createElementButton(last, elementType) {
    let color = Object.keys(BUTTON_COLORS).find(c => elementType.toUpperCase().includes(c));
    let elType = Object.keys(BUTTON_IMAGES).find(im => elementType.toUpperCase().includes(im));

    let newUserElement;
    if (!elementType.toUpperCase().includes('INPUT_GROUP')) {
        //if the element is not INPUT_GROUP
        newUserElement = `<div class="col-12" id=${last}>
                <div class="row">
                    <div class="col-11 pr-1">
                        <a href="javascript:showElement('${last}', '${elementType}')" onclick="javascript:addActive()">
                            <div class="btnStyle ${BUTTON_COLORS[color]}">
                                <i class="${BUTTON_IMAGES[elType].im} fa-3x p15">
                                    <br /><span class="someS">
                                        <span class="h5">
                                            ${BUTTON_IMAGES[elType].sign || elementType.split('_').slice(1).join(' ')} ${last}
                                        </span>
                                    </span>
                                </i>
                                                            
                            </div>
                        </a>
                    </div>
                    <div class="col-1 p-0" onclick="javascript:sendMessageWPF({'Command':'RemovingElement', 'Params': { 'elementType':'${elementType}', 'elementNumber': '${last}' }}, comm = { 'funcName': 'addElement', 'params': {'id' : '${last}', 'elementType': '' }})">
                        <i class="fa-solid fa-xmark ${BUTTON_COLORS[color]}"></i>
                    </div>
                </div>
            </div>`;
    } else { //if the element is INPUT_GROUP
        newUserElement = await inputGroupTextGenerator(last, elementType);       
    }
    var element = document.getElementById("new");
    var new_inner = `
                        ${element.innerHTML}
                        ${newUserElement}
                    `;
    lst.push(+last);
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
}

//#region Input Group Handlers
const inputGroupHandler = (checked, trueValue, falseValue, path) => {
    let newValue = checked ? trueValue : falseValue;
    sendMessageWPF({ 'Command': 'changedValue', 'Params': { 'path': `${path}`, 'newValue': newValue } })
};

async function inputGroupTextGenerator(last, elementType) {
    let result;
    try {
        result = await boundAsync.getJsonForElement(elementType, +last);
        if (result) {
            let returnedJson = JSON.parse(result);
            let currentJSON = returnedJson["~noname"]["fields"]["Input_Logic"];
            let isChecked = currentJSON["~value"] ? currentJSON["~value"] === currentJSON["ITEMS"]["ITEM"][1]["@VALUE"] : currentJSON["ITEMS"]["ITEM"][1].hasOwnProperty("@DEFAULT");
            let trans = newT.t(localStorage.getItem('lang'), currentJSON["@LNGID"]);
            let legend = (trans.length + `${last}.`.length) > 14 ? trans.substring(0, 8) + '...' : trans;
            return `<div id="${last}" class="col-12 col-sm-6 col-md-4 col-lg-3">
                        <div class="row">
                            <fieldset style="min-width: 200px;" class="col-10">
                                <legend>${last}. ${legend}</legend> 
                                <button onclick="javascript: callAddressModal('${elementType}', '${last}')" type="button" class="btn btn-position-right h5">${newT.t(localStorage.getItem('lang'), modif_address)}</button>
                                <p class="fire">
                                    ${newT.t(localStorage.getItem('lang'), currentJSON["ITEMS"]["ITEM"][0]["@LNGID"])}
                                    <label class="switch">
                                        <input type="checkbox" id="gr_input_logic_${last}"
                                                ${isChecked ? "checked" : ""}
                                                onchange="javascript: inputGroupHandler(this.checked, ${currentJSON["ITEMS"]["ITEM"][1]["@VALUE"]}, ${currentJSON["ITEMS"]["ITEM"][0]["@VALUE"]}, '${currentJSON["~path"]}' );"/>
                                        <span class="slider"></span>
                                    </label>
                                    ${newT.t(localStorage.getItem('lang'), currentJSON["ITEMS"]["ITEM"][1]["@LNGID"])}
                                </p>
                            </fieldset>
                            <div class="col-1" onclick="javascript:sendMessageWPF({'Command':'RemovingElement', 'Params': { 'elementType':'${elementType}', 'elementNumber': '${last}' }}, comm = { 'funcName': 'addElement', 'params': {'id' : '${last}', 'elementType': '' }})" class="mt-2 ml-1">
                                <i class="fa-solid fa-xmark fire"></i>
                            </div>
                        </div>
                    </div>`;
        }
        return '';
    } catch (e) { console.log('Error', e) }
}

//#endregion

// showing element function
async function showElement(id, elementType) {
    let elType = Object.keys(BUTTON_IMAGES).find(im => elementType.toUpperCase().includes(im));
    let color = Object.keys(BUTTON_COLORS).find(x => elementType.includes(x));
    let returnedJson;
    try {
        let result = await boundAsync.getJsonForElement(elementType, +id); // ret for test////////////////////////////////////////////////////////////////
        returnedJson = JSON.parse(result);
        if (!returnedJson) {
            alert(newT.t(localStorage.getItem('lang'), 'error_happened'));
            return;
        }
        if (Object.keys(returnedJson).length > 0) {
            var el = document.getElementById("selected_area");
            id = parseInt(id);
            const fieldset = document.createElement('fieldset');
            fieldset.id = `id_${id}`;
            fieldset.insertAdjacentHTML(
                'afterbegin',
                `<legend>${BUTTON_IMAGES[elType].sign || elementType.split('_').slice(1).join(' ')} ${id}</legend>
                <button onclick="javascript: callAddressModal('${elementType}', '${id}')" type="button" class="btn btn-position-right">${newT.t(localStorage.getItem('lang'), 'modif_address')}</button>`);
            drawFields(fieldset, returnedJson, color ? BUTTON_COLORS[color] : '');
            var oldFieldset = el.querySelectorAll("[id^='id_']")[0];
            if (oldFieldset) oldFieldset.replaceWith(fieldset);
            else el.appendChild(fieldset);
            collapsible();
            addVisitedBackground();
            setupSelFixer()
        }
        if (elementType.endsWith("ZONE")) {
            getZoneDevices(+id);
        }
    } catch (e) {
        console.log('Error', e);
    }
}

// loadDiv function required for TABs
function loadDiv(it, id, value, type) {
    // alert(`it -> ${it}, id -> ${id}, value -> ${value}, type -> ${type}`);
    if (!value) return;
    var element = document.getElementById(value);
    //console.log('element.innerHTML', element, ' value ', value)
    var addElement = document.getElementById(id);
    //console.log(it, "value -> element", value, "->", element, "id", id);
    if (!type) {
        switch (true) {
            case value.startsWith("0_"):
            case value.startsWith("1_"):
            case value.startsWith("2_"):
            case value.startsWith("3_"):
            case value.startsWith("4_"):
            case value.startsWith("5_"):
            case value.startsWith("6_"):
            case value.startsWith("7_"):
            case value.startsWith("8_"):
            case value.startsWith("9_"):
            case value.startsWith("10_"):
            case value.startsWith("11_"):
            case value.includes("Teletek"):
            case value.includes("System"):
            case value.includes("sounder_zonal"):
            case value.includes("common_fire"):
                addElement.innerHTML = element.innerHTML;
                var execute = addElement.querySelector('select');
                if (execute && execute.onchange.toLocaleString().includes("loadDiv")) {
                    execute.onchange();
                }
                break;
            default:
                addElement.innerHTML = "";
                break;
        }
    } else {
        createLoopTypeMenu(it, addElement, type); return;
    }
    adjustCollapsibleHeight(it, element.scrollHeight)
    addVisitedBackground();
    setupSelFixer();
}

function adjustCollapsibleHeight(selectElement, addElementHeight = 0) {
    var content = selectElement.parentNode;
    while (!content.classList.contains("collapsible-content")) {
        content = content.parentNode;
        if (content === document.body)
            break;
    }
    if (content.classList.contains("collapsible-content")) {
        content.style.maxHeight = (content.scrollHeight + addElementHeight) + "px";
    }
}

function alertScanFinished(show) {
    if (show === 'alert') {
        document.location.reload();
    }
}
//#endregion