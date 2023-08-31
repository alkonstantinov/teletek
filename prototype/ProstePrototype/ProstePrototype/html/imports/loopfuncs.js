//#region VARIABLES
let elements;
let minElements;
let minDevicesAllowed = 1;
let lst = [0];
let mainKey;
let deviceNmbr = 0;
let attachedDevicesList = [];
let maxDevicesAllowed = 100;
//#endregion VARIABLES

//#region LOOP ELEMENT
// function called when click on + ADD DEVICE IN LOOP {loopNumber}
const loopElementFunc = (loopNumber, loopType) => {
    document.getElementById(`ram_panel_add`).querySelector('#list-tab').innerHTML = "";
    boundAsync.getJsonNode(loopType, 'CONTAINS').then(res => {
        var changeJson = JSON.parse(res)["ELEMENT"];

        if (!Array.isArray(changeJson)) {
            /* case TTE_NONE: {
              "ELEMENT": {
                "@ID": "IRIS_TTENONE",
                "@MIN": "1",
                "@MAX": "250",
                "@LNGID": "47ae198f-7ad0-449b-abe5-5fa889652643",
                "~path": "ELEMENTS.IRIS_TTELOOP1.CONTAINS.ELEMENT"
              }, ... }
            */ 
            const { id, min, max } = {
                id: changeJson["@ID"],
                min: +(changeJson["@MIN"]),
                max: +(changeJson["@MAX"])
            };
            if (min !== max) {
                maxDevicesAllowed = max;
            } else {
                maxDevicesAllowed = 100;
            }
            if (id) {
                getArrayToModal(id, loopNumber, loopType, 'CHANGE');
            }
        } else { // case Sensors or Modules
            Object.keys(changeJson).forEach(idx => {
                getArrayToModal(changeJson[idx]["@ID"], loopNumber, loopType, 'CONTAINS');
            });
        }
    });
}

// function that get the TTE_NONE or (Sensors or Modules)
const getArrayToModal = (id, loopNumber, loopType, operation) => {
    boundAsync.getJsonNode(id, operation).then(res => {
        /* case TTE_NONE
        {
          "IRIS_MIO40": {
            "@LNGID": "eda046ae-ca62-48f8-8413-8ebce415093d",
            "~path": "ELEMENTS.IRIS_TTENONE.CHANGE.IRIS_MIO40"
          },
          "IRIS_MIO04": {
            "@LNGID": "70c750d4-c4b4-4a70-98ca-d404a8dc42f6",
            "~path": "ELEMENTS.IRIS_TTENONE.CHANGE.IRIS_MIO04"
          },
          "IRIS_MIO22": {
            "@LNGID": "6f7135d8-1f02-4817-9324-fcd727a0b330",
            "~path": "ELEMENTS.IRIS_TTENONE.CHANGE.IRIS_MIO22"
          }, ... }

          case Sensors ( similar to Modules where it starts with "IRIS_MNONE")
          {
          "IRIS_SNONE": {
            "@MIN": "0",
            "@MAX": "99",
            "@LNGID": "1862d1e0-b313-450e-aae2-787d7e241f8a",
            "~path": "ELEMENTS.IRIS_SENSORS.CONTAINS.ELEMENT[0]"
          },
          "IRIS_S1251E": {
            "@MIN": "0",
            "@MAX": "99",
            "@LNGID": "4eb06886-cf2c-49c6-8bee-1bf6e9fa98ec",
            "~path": "ELEMENTS.IRIS_SENSORS.CONTAINS.ELEMENT[1]"
          }, ... }
        */
        var listJson = Object.keys(JSON.parse(res));
        let deviceList = document.getElementById(`ram_panel_add`).querySelector('#list-tab');
        let deviceListName = id.includes('NONE') ? "DEVICES" : id.split('_').slice(1).join(' ');
        let { minDevices, maxDevices } = getConfig('', deviceListName); // set minDevices and maxDevices depending on Sensors or Modules
        if (attachedDevicesList.filter(x => x >= minDevices && x <= maxDevices).length === maxDevicesAllowed) return; // in case of full list of Sensors or Modules
        deviceList.insertAdjacentHTML('beforeend', `<div class="text_grid_middle">${deviceListName}</div>`)

        let noneElement = listJson.find(e => e.includes("NONE")); // case Sensors or Modules
        if (!noneElement) noneElement = id; // "IRIS_TTENONE" - case TTE_NONE

        listJson.filter(k => !k.includes('NONE')).forEach(deviceName => {
            let key = getKey(deviceName);
            deviceList.insertAdjacentHTML('beforeend', `
            <div class="ram_card fire" id="${deviceName}" onclick="javascript: openDeviceAddressModal(this.id, '${loopNumber}', '${loopType}', '${noneElement}' )">
                <div class="ram_card_img">
                    <img src="${DEVICES_CONSTS[key].im}" alt="${DEVICES_CONSTS[key].sign}">
                </div>
                <div class="ram_card_body">
                    <h5 class="ram_card_title">${DEVICES_CONSTS[key].sign}</h5>
                </div>
            </div>
            `)
        });
    });
}

// function fired when choosing the device from the modal and creating the button
const openDeviceAddressModal = (deviceName, loopNumber, loopType, noneElement, current = -1) => {
    hidePanelAdd(); // $(`#${loopType}_modal`).modal('hide');

    let { key, minDevices, maxDevices } = getConfig(deviceName);

    let addrModal = document.getElementById("addressModal");
    document.getElementById("addressModalLabel").innerText = new T().t(localStorage.getItem('lang'), "device_address")
    addrModal.querySelector(".btn.btn-secondary").innerText = new T().t(localStorage.getItem('lang'), "MenuClose");
    let innerSelectText = `<div class="form-floating mb-3">
                               <label for="select_address" class="contents">${newT.t(localStorage.getItem('lang'), "set_address")} ${loopType}</label>                              
                               <select id="select_address" name="select_address" class="form-select ram_floating_select">`;

    for (var i = minDevices; i <= maxDevices; i++) {
        let currentId = i === +current ? "selected" : "";
        let disabled = attachedDevicesList.includes(i) ? "disabled style='background: #aaa;'" : ""
        innerSelectText += `<option value="${i}" ${disabled} ${currentId}>${i}</option>`;
    }
    innerSelectText += `</select>
                    <div>`;

    addrModal.querySelector(".modal-body").innerHTML = innerSelectText;

    addrModal.querySelector(".btn.btn-primary.addr").addEventListener('click', function handler() {
        if (!attachedDevicesList.includes(+addrModal.querySelector("select").value)) {
            if (current === -1) { // guard if new
                sendMessageWPF({
                    'Command': 'AddingLoopElement',
                    'Params': {
                        'NO_LOOP': mainKey,
                        'loopType': loopType,
                        'loopNumber': loopNumber,
                        'noneElement': noneElement,
                        'deviceName': deviceName,
                        'deviceAddress': addrModal.querySelector("select").value
                    }
                });
                // setting the element at the given address
                visualizeLoopElement(deviceName, +addrModal.querySelector("select").value, loopType, +loopNumber, key, noneElement);
            } else {
                // else it is not a new item, so modify the old one
                modifyLoopDeviceCurrentAddress(+current, loopType, deviceName, +addrModal.querySelector("select").value, +loopNumber, key, noneElement );
            }
        }
        $(addrModal).modal('hide');
        // removing the event listener function
        this.removeEventListener('click', handler);
    });
    $(addrModal).modal('show');    
}

// adding the new device button to the device list
function visualizeLoopElement(deviceName, address, loopType, loopNumber, key, noneElement) {    
    attachedDevicesList.push(+address);

    // update deviceNmbr button
    updateDeviceNmbr();

    let el = document.getElementById("new_devices");

    const sensorsSign = (!loopType.includes("TTELOOP") && !el.querySelector(`#${loopType}_0`)) ? `<p id="${loopType}_0">${new T().t(localStorage.getItem('lang'), 'sensors')}</p>` : "";
    const modulesSign = (!loopType.includes("TTELOOP") && !el.querySelector(`#${loopType}_100`)) ? `<p id="${loopType}_100">${new T().t(localStorage.getItem('lang'), 'modules')}</p>` : "";

    //alert(`loopType:'${loopType}', deviceName:'${deviceName}', loopNumber:'${loopNumber}', address:'${address}', noneElement:'${noneElement}', key:${key}`);
    const newDeviceInner = `<div class="ram_card fire" id='${loopType}_${address}' onclick="javascript:showDevice('${loopType}', '${deviceName}', '${loopNumber}', '${address}', '${noneElement}'); addActive('ram_panel_2')">
                                <div class="ram_card_img">
                                    <img src="${DEVICES_CONSTS[key].im}" alt="${address + '. ' + DEVICES_CONSTS[key].sign}">
                                </div>
                                <div class="ram_card_body">
                                    <h5 class="ram_card_title">${address + '. ' + DEVICES_CONSTS[key].sign}</h5>
                                </div>
                                <div class="ram_add_btn"
                                    onclick="javascript: event.stopPropagation(); removeDevice('${loopType}', '${loopNumber}', '${deviceName}', '${address}')">
                                    <i class="ram_icon add_device rot45"></i>
                                </div>
                            </div>`;

    
    if (el) { // inserting
        el.insertAdjacentHTML('beforeend', newDeviceInner);
    }
    // reordering
    [].map.call(el.children, Object).sort(function (a, b) {
        return +a.id.match(/\d+$/) - +b.id.match(/\d+$/);
    }).forEach(function (elem) {
        el.appendChild(elem);
    });

    if (attachedDevicesList.length === maxDevicesAllowed && DEVICES_CONSTS[key].type === "device") {
        let button = document.getElementById(`btn_devices`);
        button.style.display = "none";
    }
}

const modifyLoopDeviceCurrentAddress = (oldAddress, loopType, deviceName, newAddress, loopNumber, key, noneElement) => { // deviceName, address, loopType, loopNumber, key, noneElement
    boundAsync.modifyDeviceLoopAddress(`${oldAddress}`, loopType, `${newAddress}`).then(r => {
        if (r) {
            attachedDevicesList = attachedDevicesList.filter(el => el !== +oldAddress);
            let oldEl = document.getElementById(`${loopType}_${oldAddress}`);
            oldEl.parentNode.removeChild(oldEl);
            visualizeLoopElement(deviceName, newAddress, loopType, loopNumber, key, noneElement);
            showDevice(loopType, deviceName, loopNumber, newAddress, noneElement);
        }
    }).catch(e => console.log(e));
} 

const showDevice = (loopType, deviceName, loopNumber, address, noneElement) => {
    boundAsync.getLoopDevices(mainKey, +loopNumber, deviceName.split("_").slice(1).join(" ")).then(res => {
        if (res) {
            let resJSONList = JSON.parse(res);
            let deviceData = resJSONList.find(dd => dd["~address"] === `${address}` && dd["~device"] === deviceName)["Groups"]["~noname"]["fields"];
            let elem = document.getElementById(`selected_area`);
            showOrderedDevices(loopType, deviceName, address, elem, deviceData, loopNumber, noneElement)
            //collapsible();
            addVisitedBackground();
        }
    }).catch(err => alert(err));
}

function showOrderedDevices(loopType, deviceName, address, elem, deviceData, loopNumber, noneElement) {
    let { key } = getConfig(deviceName);
    let fieldsetDevice = createFieldset(`selected_${loopType}_${deviceName}_${address}`, `${address}. ${DEVICES_CONSTS[key].sign}`, 'first');
    fieldsetDevice.insertAdjacentHTML(
        "beforeend",
        `<button onclick="javascript: openDeviceAddressModal('${deviceName}', '${loopNumber}', '${loopType}', '${noneElement}', '${address}')" type="button" class="btn btn-position-right">
            ${new T().t(localStorage.getItem('lang'), 'modif_address')}
        </button>`
    );
    let fieldsetParameters;
    let fieldsetChannels;
    let fieldsetAlarmLevel;
    let fieldsetVerifyTime;
    let row = "";
    //alert(Object.keys(deviceData))
    for (let key of Object.keys(deviceData)) { //Object.keys(deviceData).forEach(key => {
        let inner;
        switch (true) {
            case (key.includes("STATE")):
            case (key === "SERIAL_ID"):
            case (key === "NAME"):
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetDevice.insertAdjacentHTML('beforeend', `<div class="${key.includes("STATE") ? "col-12" : "col-xs-12 col-lg-6"}">${inner}</div>`);
                break;
            case (key === "ZONE"):
            case (key === "LED"):
            case (key.includes("SOUND")):
            case (key.includes("DUST")):
                if (!fieldsetParameters) fieldsetParameters = createFieldset(`params_${loopType}_${deviceName}_${address}`, new T().t(localStorage.getItem('lang'), "parameters"));
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetParameters.insertAdjacentHTML('beforeend', `<div class="${deviceData[key]["@TYPE"] === "AND" ? 'col-12' : 'col-xs-12 col-lg-6'} pr-1">${inner}</div>`);
                break;
            case (key.includes("TYPECHANNEL")):
                //deviceData[key]["ITEMS"]["ITEM"][ParseInt(deviceData[key]["value"])]["@NAME"] : deviceData[key]["ITEMS"]["ITEM"].find(i => i.hasOwnProperty("@DEFAULT"))["@NAME"]
                deviceData[key]["openModalFirst"] = `openChannelModal(this.id, '${deviceData[key]["~path"]}', '${deviceData[key]["@TEXT"]}', '${deviceData[key]["value"] ? deviceData[key]["value"] : deviceData[key]["ITEMS"]["ITEM"].find(i => i.hasOwnProperty("@DEFAULT"))["@DEFAULT"]}', '${deviceData[key]["ITEMS"]["ITEM"].map(x => x["@NAME"])}', this.value)`;
                deviceData[key]["addChannelHelp"] = true;
                row = " row";
            case (key.includes("NAME_I")):
                if (!fieldsetChannels) fieldsetChannels = createFieldset(`channels_${loopType}_${deviceName}_${address}`, new T().t(localStorage.getItem('lang'), "channels"));
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetChannels.insertAdjacentHTML('beforeend', `<div class="col-xs-12 col-lg-6${key.includes("TYPECHANNEL") ? row : ""}">${inner}</div>`);
                break;
            case (key.includes("ALARMLEVEL")):
                if (!fieldsetAlarmLevel) fieldsetAlarmLevel = createFieldset(`channels_${loopType}_${deviceName}_${address}`, new T().t(localStorage.getItem('lang'), "levels"));
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetAlarmLevel.insertAdjacentHTML('beforeend', `<div class="col-12">${inner}</div>`);
                break;
            case (key.includes("VERIFYTIME")):
                if (!fieldsetVerifyTime) fieldsetVerifyTime = createFieldset(`channels_${loopType}_${deviceName}_${address}`, new T().t(localStorage.getItem('lang'), "verification"));
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetVerifyTime.insertAdjacentHTML('beforeend', `<div class="col-12">${inner}</div>`);
                break;
            default: break;
        }
    } //);
    if (elem && elem.innerHTML !== "") { elem.replaceChildren() }
    elem.insertAdjacentElement('afterbegin', fieldsetDevice);
    if (fieldsetParameters) elem.insertAdjacentElement('beforeend', fieldsetParameters);
    if (fieldsetChannels) elem.insertAdjacentElement('beforeend', fieldsetChannels);
    if (fieldsetAlarmLevel) elem.insertAdjacentElement('beforeend', fieldsetAlarmLevel);
    if (fieldsetVerifyTime) elem.insertAdjacentElement('beforeend', fieldsetVerifyTime);
}

function openChannelModal(id, path, channelName, oldValue, allNames, newValue) {
    let values = JSON.parse(JSON.stringify(allNames)).split(',');
    $('#showConfirmationModal .modal-title').html(`${new T().t(localStorage.getItem('lang'), 'are_you_sure_changing_channel')}?`);
    $('#showConfirmationModal .modal-body').html(`<p>${new T().t(localStorage.getItem('lang'), "please_confirm_the_change_of")} {${channelName}}: "${values[+oldValue]}" -> "${values[+newValue]}"</p>`);

    $("#bigClose, #xClose").on("click", function () {
        document.getElementById(id).value = oldValue;
        $("#showConfirmationModal").modal("hide");
    });
    $("#yesBtn").on("click", function () {
        sendMessageWPF(
            { 'Command': 'changedValue', 'Params': { 'path': path, 'newValue': newValue } }
        );
        $("#showConfirmationModal").modal("hide");
    });

    $("#showConfirmationModal").modal("show");
}

function showChannelInfo(it, path) {
    boundAsync.channelUses(path).then(r => {
        if (r) {
            const popover = bootstrap.Popover.getOrCreateInstance(it);

            return popover.setContent({
                '.popover-header': 'used_in',
                '.popover-body': r,
            });
        }
    });
}

const removeDevice = (loopType, loopNumber, deviceName, address) => {
    let key = getKey(deviceName);

    sendMessageWPF({
        'Command': 'RemovingLoopElement',
        'Params': { 'NO_LOOP': mainKey, 'loopType': loopType, 'loopNumber': loopNumber, 'deviceName': deviceName, 'deviceAddress': address }
    });

    let parent = document.getElementById(`new_devices`);
    let childNode = document.getElementById(`${loopType}_${address}`);
    parent.removeChild(childNode);
    let shownDevice = document.getElementById(`selected_${loopType}_${deviceName}_${address}`);
    if (shownDevice) {
        shownDevice.parentNode.removeChild(shownDevice);
    }

    attachedDevicesList = attachedDevicesList.filter(x => x !== +address);

    // update deviceNmbr button
    updateDeviceNmbr();

    if (attachedDevicesList.length < maxDevicesAllowed && document.getElementById('_btn_devices').style.display === "none") {
        document.getElementById('_btn_devices').style.display = "inline-flex";
    }
}

const fillLoopElements = (loopNumber, loopType) => {
    boundAsync.getLoopDevices(mainKey, +loopNumber, "").then(res => {
        if (res) {
            attachedDevicesList = [];
            let resJSONList = JSON.parse(res);
            resJSONList.forEach(deviceData => {
                let address = deviceData["~address"];
                let deviceName = deviceData["~device"];
                let { key } = getConfig(deviceName);
                visualizeLoopElement(deviceName, +address, loopType, +loopNumber, key)
            })
        }
    }).catch(err => alert(err));
}
//#endregion LOOP ELEMENT

//#region LOOP
// checking for created loops
function getLoops() {
    boundAsync.getLoops(mainKey).then(r => {
        if (!r) return;
        let loopsJson = JSON.parse(r);
        // creating the founded loops
        if (Object.keys(loopsJson).includes("ELEMENT")) {
            loopsJson = loopsJson["ELEMENT"];
        }
        let loopList = Object.keys(loopsJson);
        loopList.forEach(key => {
            addLoop(loopsJson[key], "old");
        });
    }).catch(err => alert(err));
    $("#deviceList").modal('hide')
}

// initial loop function for adding a loop; - launch when pressin button ADD LOOP or + LOOP
const loopFunc = () => {
    if (lst.length - 1 === elements) {
        alert(new T().t(localStorage.getItem('lang'), "max_number_of_loops_created_already"));
        hidePanelAdd(); // $("#deviceList").modal('hide'); //.hide();
        return;
    } else {
        const params = {
            'Command': 'AddingLoop',
            'Params': { 'elementType': mainKey, 'elementNumber': lst.length },
            'Callback': 'loopCallback' // -> the back-end call the function loopCallBack with default 'CallBackParams': [mainKey, lst.length, 'CHANGE'] for IRIS
        };
        sendMessageWPF(params);
        showPanelAdd({ 'elementType': mainKey, 'elementNumber': lst.length });// $("#deviceList").modal('show');// data - toggle="modal" data - target="#deviceList"
    }
}

/**
 * loopKeyContainsCheck returns true if given key of a loop (IRIS_TTELOOP, IRIS_NO_LOOP, etc)
 * has its 'CONTAINS' property available else false
 * @param {string} loopKey - the name of the key
 */
async function loopKeyContainsCheck(loopKey) {
    try {
        const res = await boundAsync.getJsonNode(loopKey, 'CONTAINS');
        if (res) {
            return true;
        }
        return false;
    } catch (e) {
        return false;
    }
}

/**
 * exchanges the key if equal to "Element" for the real Loopkey of its
 * @param {string} key
 * @param {json} json
 * @returns the same key or the exchanged key
 */
function exchangeElementForLoopkey(key, json) {
    if (key === "ELEMENT") {
        key = json[key]["@ID"];
    }
    return key;
}

/**
 * function to fill in the modal with TELETEK_LOOP and SYSTEM SENSOR LOOP
 * @param {string} key default the the mainKey (NO_LOOP for  Iris, SIMPO_TTELOOP ofr Simpo etc)
 * @param {number} len default to the lst.len
 * @param {string} command default to 'CHANGE'
 */
async function loopCallback(key = mainKey, len = lst.length, command = 'CHANGE') {
    try {
        const res = await boundAsync.getJsonNodeForElement(key, len, command)
        const changeJson = JSON.parse(res);

        if (!Object.keys(changeJson).length) { // guard that the res is full
            var i = 0;
            do {
                loopFunc(); i++;
            } while (i === 3); // up to 3 trials
            if (i === 3) {
                alert("Error 3 occurred. Please, connect your software providers!"); return;
            }
        }
        const listTab = document.getElementById('ram_panel_add').querySelector("#list-tab"); // old way: $("#deviceList").find("#list-tab")[0];
        listTab.innerHTML = ''; // clear any previous data there
        let optionsList = Object.keys(changeJson).filter(k => k !== "~path");

        // check if optionsList contains items that have 'CONTAINS' key and save then into finalOptionsLists
        let finalOptionsLists = [];
        for (let option of optionsList) { // use for ... of as the async await will be used
            option = exchangeElementForLoopkey(option, changeJson);

            let loopKeyVerificationResult = await loopKeyContainsCheck(option);
            if (loopKeyVerificationResult) {
                finalOptionsLists.push(option);
            } else {
                try {
                    const r = await boundAsync.getJsonNode(option, 'CHANGE');
                    if (r) {
                        const json = JSON.parse(r);
                        const keysArr = Object.keys(json).filter(rkey => rkey !== "~path");
                        for (let i = 0; i < keysArr.length; i++) {
                            let tempKey = exchangeElementForLoopkey(keysArr[i], json);
                            
                            let anotherloopKeyVerification = await loopKeyContainsCheck(tempKey);
                            if (anotherloopKeyVerification) {
                                finalOptionsLists.push(tempKey);
                                break;
                            }
                        }
                    }
                } catch (error) {
                    alert("Err: " + error);
                }
            }
        }

        // use finalOptionsLists to create the buttons
        finalOptionsLists.forEach(k => {
            let tabListButton = `<div class="ram_card fire" 
                      id="list-${k}"
                      onclick="javascript: addLoop('${k}'); hidePanelAdd();">
                    <div class="ram_card_img">
                        <img src="${BUTTON_IMAGES[k.includes("TTE") ? "TTELOOP" : "LOOP"].im}" alt="${k.includes("TTE") ? "Teletek" : "System Sensor"} Loop">
                    </div>
                    <div class="ram_card_body">
                        <h5 class="ram_card_title">${k.includes("TTE") ? "Teletek" : "System Sensor"} Loop</h5>
                    </div>
                </div>`;
            // <i class="ram_icon loop_devices"></i>

            listTab.insertAdjacentHTML('beforeend', tabListButton);
        });
    }
    catch (err) {
        alert("Err: " + err);
    }
}

/**
 * actual adding Loop function to the interface
 * @param {string} loopType
 * @param {string} newFlag default to "new". Can be "old"
 * @returns
 */
function addLoop(loopType, newFlag = "new") {
    if (!loopType) return;

    var last = parseInt(loopType.charAt(loopType.length - 1)); // get the index of the loop
    if (lst.includes(last)) {
        alert("Error with index");
        return;
    }

    let color = Object.keys(BUTTON_COLORS).find(c => loopType.toUpperCase().includes(c));
    let elType = Object.keys(BUTTON_IMAGES).find(im => loopType.toUpperCase().includes(im));
    let fieldName = `${BUTTON_IMAGES[elType].sign} ${last}`;

    let img = BUTTON_IMAGES[elType].im.split('.').pop().length === 3
        ? `<img src="${BUTTON_IMAGES[elType].im}" alt="${BUTTON_IMAGES[elType].sign}">`
        : `<i class="ram_icon ${BUTTON_IMAGES[elType].im}"></i>`;
    const newLoop = `<div id=${loopType} class="ram_card ${BUTTON_COLORS[color]}" onclick="javascript:showLoop('${last}', '${loopType}'); addActive('ram_panel_1'); hidePanelAdd(); document.getElementById('selected_area').innerHTML = '';">
                        <div class="ram_card_img">
                            ${img}
                        </div>
                        <div class="ram_card_body">
                            <h5 class="ram_card_title">${fieldName}</h5>
                        </div>
                    </div>`;
                        // <div class="ram_add_btn" onclick="javascript: exchangingLoop('${loopType}')"><i class="ram_icon edit"></i></div>

    var element = document.getElementById("new");
    var new_inner = `
                        ${element.innerHTML}
                        ${newLoop}
                    `;
    element.innerHTML = new_inner;

    // sending data about the choosen loopType if newly created
    if (newFlag === "new") {
        boundAsync.setLoopType(mainKey, last, loopType).then().catch(err => console.log(err));
    }
    // adding remove button
    if (
        ((last === 1 && !mainKey.toUpperCase().includes("SIMPO")) ||
        (last === 2 && mainKey.toUpperCase().includes("SIMPO"))) &&
        !document.getElementById("rmvBtn")
        ) {
        let btnGroup = document.getElementById("buttons");
        btnGroup.insertAdjacentHTML(
            'beforeend', 
            `<button type="button" id="rmvBtn" class="btn ram_btn btn_white fire" onclick="javascript: removeLoop();" title="${new T().t(localStorage.getItem('lang'), "remove_last_loop")}">
                <i class="ram_icon minus"></i>
            </button>`
        );
    }

    // reordering
    var main = document.getElementById('new');

    [].map.call(main.children, Object).sort(function (a, b) {
        return +a.id.match(/\d+$/) - +b.id.match(/\d+$/);
    }).forEach(function (elem) {
        main.appendChild(elem);
    });

    lst.push(last);
    hidePanelAdd(); // $('#deviceList').modal('hide');
}

function showLoop(loopNumber, loopType) {
    const old_resizer2 = document.getElementById('ram_panel_2');
    const resizer2 = document.createElement('div');
    resizer2.id = 'ram_panel_2';

    attachedDevicesList = [];
    deviceNmbr = attachedDevicesList.length;
    //resizer2.innerHTML = '';
    resizer2.classList = 'ram_panel ram_resizable ram_animate';
    let btnTitle = `${new T().t(localStorage.getItem('lang'), "add_new")} ${loopType.includes("TTE") ? new T().t(localStorage.getItem('lang'), "device_in_loop") : new T().t(localStorage.getItem('lang'), "sensormodule_in_loop")} ${loopNumber}`;

    boundAsync.addingSegmentsToElement(loopType, `${loopType.includes("TTE") ? "Teletek Loop" : "System Sensors Loop"} ${loopNumber}`);
    resizer2.insertAdjacentHTML('beforeend',
        `<div class="ram_settings middle">
            <button class="btn ram_btn btn_white fire" onclick="javacript: calculateLoopDevices(${loopNumber})" id="calculateDevices" data-bs-toggle="modal" data-bs-target="#showDevicesListModal" title="${new T().t(localStorage.getItem('lang'), "number_of_devices")}">
                <i class="ram_icon loop_devices"></i>
                <div class="ram_btn_title">
                    ${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}
                </div>
            </button>
        </div>
        <div class="ram_panel_content">
            <div id="new_devices" class="ram_cards">

            </div>
        </div>
        <div class="ram_fixed_bottom">
            <div class="ram_settings" id="buttons_devices">
                <button class="btn ram_btn btn_white fire" onclick="javascript:loopElementFunc(${loopNumber}, '${loopType}'); showPanelAdd();" id="_btn_devices" title="${btnTitle}">
                    <i class="ram_icon add_device"></i>
                    <div class="ram_btn_title">${btnTitle}</div>
                </button>
            </div>
            <button class="ram_btn ram_toggle_btn open" onclick="javascript: sidebar_toggle(this, 2);">
                <i class="ram_icon toggle"></i>
            </button>
        </div>
        <div class="ram_resizer"></div>`
    );

    /*let el = document.getElementById("selected_area");*/

    old_resizer2.replaceWith(resizer2);
    resizingPanels(); // update the resizers capability

    fillLoopElements(loopNumber, loopType);
}

function exchangingLoop(loopType) {
    alert(`exchanging ${loopType}, 'mainKey': ${mainKey}, 'lst.length': ${lst.length}`)
}

async function removeLoop() {
    // check if there are some connected elements
    let returnString = await boundAsync.checkLoopConnection(mainKey, lst.at(-1));
    
    // show confirmation dialog
    $('#showConfirmationModal .modal-title').html(`${new T().t(localStorage.getItem('lang'), "are_you_sure_removing")} ${new T().t(localStorage.getItem('lang'), "loop")} ${lst.at(-1)}?`);

    let modalContent = $('#showConfirmationModal .modal-body')[0];

    $("#bigClose, #xClose").on("click", function () {
        $("#showConfirmationModal").modal("hide");
    });
    $("#yesBtn").off('click').on("click", removeLoopAfterConfirm);
    
    if (returnString.length > 2) {
        let deviceListJSON = JSON.parse(returnString);

        const deviceMap = new Map(Object.entries(deviceListJSON));

        let innerModalContent = `<div class="table-responsive"><table class="table table-striped"><thead><tr>
                        <th scope="col">${new T().t(localStorage.getItem('lang'), "device_type")}</th>
                        <th scope="col">${new T().t(localStorage.getItem('lang'), "used_in")}</th>
                        </tr></thead><tbody>`;
        deviceMap.forEach((value, key, map) => {
            innerModalContent += `<tr>
                        <td >${key}</td>
                        <td>${value}</td>
                        </tr>`;
        })
        innerModalContent += `</tbody></table></div>`;
        modalContent.innerHTML = innerModalContent;

        $("#showConfirmationModal").modal("show");
    } else {
        let devicesListStr = await boundAsync.getLoopDevices(mainKey, lst.at(-1), "");
        let deviceListJSON;
        if (devicesListStr) { 
            deviceListJSON = JSON.parse(devicesListStr);
        }
        if (deviceListJSON && deviceListJSON.length > 0) {
            const deviceMap = new Map();

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
                            <th scope="col">${new T().t(localStorage.getItem('lang'), "used_in")}</th>
                            </tr></thead><tbody>`;
            deviceMap.forEach((value, key, map) => {
                innerModalContent += `<tr>
                            <td >${key}</td>
                            <td>${value}</td>
                            <td>${new T().t(localStorage.getItem('lang'), "not_used")}</td>
                            </tr>`;
            })
            innerModalContent += `</tbody></table></div>`;
            modalContent.innerHTML = innerModalContent;

            $("#showConfirmationModal").modal("show");
        } else {
            removeLoopAfterConfirm();
        }
    } 
}

function removeLoopAfterConfirm () {
    // if confirmed remove from the backend
    let loopNumber = lst.pop();
    sendMessageWPF({ 'Command': 'RemovingLoop', 'Params': { 'elementType': `'${mainKey}'`, 'elementNumber': `'${loopNumber}'` } });
    // remove it from the front
    let parent = document.getElementById("new");
    let last = parent.lastChild;
    if (document.querySelectorAll(`[id*="${last.id}"]`).length > 1) {
        document.getElementById("selected_area").innerHTML = '';
    }
    parent.removeChild(last);
    
    // clear all the appended elements in front if visible
    const ram_panel_2 = document.getElementById("ram_panel_2");
    if (ram_panel_2 && ram_panel_2.innerHTML !== "") {
        const calculateDevicesBtn = ram_panel_2.querySelector("#calculateDevices");
        const calculateDevicesFn = calculateDevicesBtn && calculateDevicesBtn.onclick;
        if (calculateDevicesFn) {
            const params = calculateDevicesFn.toString().match(/calculateLoopDevices\((.*?)\)/)[1];
            if (loopNumber === +params) {
                ram_panel_2.innerHTML = "";
                ram_panel_2.classList = "";
            }
        }
    }
    
    if (
        loopNumber === 1 ||
        (loopNumber === 2 && mainKey.toUpperCase().includes("SIMPO"))
    ) {
        let rmvBtn = document.getElementById("rmvBtn");
        rmvBtn.remove();
    }
    $("#showConfirmationModal").modal("hide");
}

function calculateLoopDevices(loopNumber) {
    let modal = $(document.getElementById("showDevicesListModal"));

    let modalTitle = modal.find('.modal-title')[0];
    modalTitle.innerHTML = `${new T().t(localStorage.getItem('lang'), 'number_of_devices_per_loop')}:`;
    let modalContent = modal.find('.modal-body')[0];

    const deviceMap = new Map();
    boundAsync.getLoopDevices(mainKey, +loopNumber, "").then(res => {
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
//#endregion LOOP


//#region UTILS
function getKey(deviceName) {
    return Object.keys(DEVICES_CONSTS).find(k => deviceName.includes(k) && DEVICES_CONSTS[k].type !== 'panel');
}

function getConfig(deviceName, device = '') {
    let maxDevices = maxDevicesAllowed;
    let minDevices = minDevicesAllowed;

    if (device) {
        if (device.toLowerCase() === "modules") {
            minDevices += 100;
            maxDevices += 100;
        }
        return { minDevices, maxDevices }
    }

    let key = getKey(deviceName);
    // module check
    // alert('minDevices ' + minDevices + ', maxDevices ' + maxDevices + ' DEVICES_CONSTS[key].type ' + DEVICES_CONSTS[key].type)
    if (DEVICES_CONSTS[key].type === "module") {
        minDevices += 100;
        maxDevices += 100;
    }

    return { key, minDevices, maxDevices };
}

function updateDeviceNmbr() {
    deviceNmbr = attachedDevicesList.length;
    let showBtn = document.getElementById("calculateDevices").lastElementChild;
    showBtn.innerHTML = `${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}`;
}

// creating a fieldset
function createFieldset(id, legendName, first='') {
    let fieldset = document.createElement("div");
    fieldset.id = id;
    fieldset.className = `${!first && "ram_attribute_holder "}row align-items-center`;
    if (!first) {
        fieldset.insertAdjacentHTML("afterbegin", `<legend class="ram_attribute_holder_title">${legendName}</legend>`);
    } else {
        fieldset.insertAdjacentHTML("afterbegin", `<p class="ram_attribute_holder_title">${legendName}</p>`);
    }
        
    return fieldset;
}
//#endregion UTILS