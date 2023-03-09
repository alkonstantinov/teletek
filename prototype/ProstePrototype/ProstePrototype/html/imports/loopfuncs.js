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
    document.getElementById(`${loopType}_modal`).querySelector('#list-tab').innerHTML = "";
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
        let deviceList = document.getElementById(`${loopType}_modal`).querySelector('#list-tab');
        let deviceListName = id.includes('NONE') ? "DEVICES" : id.split('_').slice(1).join(' ');
        let { minDevices, maxDevices } = getConfig('', deviceListName); // set minDevices and maxDevices depending on Sensors or Modules
        if (attachedDevicesList.filter(x => x >= minDevices && x <= maxDevices).length === maxDevicesAllowed) return; // in case of full list of Sensors or Modules
        deviceList.insertAdjacentHTML('beforeend', `<div class="col-12 pt-2" style="text-align: center;text-decoration: underline;">${deviceListName}</div>`)

        let noneElement = listJson.find(e => e.includes("NONE")); // case Sensors or Modules
        if (!noneElement) noneElement = id; // "IRIS_TTENONE" - case TTE_NONE

        listJson.filter(k => !k.includes('NONE')).forEach(deviceName => {
            let key = getKey(deviceName);
            deviceList.insertAdjacentHTML('beforeend', `
                <button type="button"
                        class="list-group-item col"
                        id="${deviceName}"
                        onclick="javascript: openDeviceAddressModal(this.id, '${loopNumber}', '${loopType}', '${noneElement}' )"
                        data-toggle="tab"
                        role="tab"
                        aria-selected="false">
                    <div class="btnStyle fire">
                        <img src="${DEVICES_CONSTS[key].im}"
                                alt=""
                                width="50"
                                height="50"
                                class="m15" />
                        <div class="someS">
                            <h5>${DEVICES_CONSTS[key].sign}</h5>
                        </div>
                    </div>
                </button>
            `)
        });
    });
}

// function fired when choosing the device from the modal and creating the button
const openDeviceAddressModal = (deviceName, loopNumber, loopType, noneElement, current = -1) => {
    $(`#${loopType}_modal`).modal('hide');

    let { key, minDevices, maxDevices } = getConfig(deviceName);

    let addrModal = document.getElementById("addressModal");
    document.getElementById("addressModalLabel").innerText = new T().t(localStorage.getItem('lang'), "device_address")
    addrModal.querySelector(".btn.btn-secondary").innerText = new T().t(localStorage.getItem('lang'), "MenuClose");
    let innerSelectText = `<div class="form-item roww mt-1">
                                    <label for="select_address">${new T().t(localStorage.getItem('lang'), "set_address")} ${loopType}</label>
                                    <div class="select">
                                        <select id="select_address" name="select_address">`;

    for (var i = minDevices; i <= maxDevices; i++) {
        let currentId = i === +current ? "selected" : "";
        let disabled = attachedDevicesList.includes(i) ? "disabled style='background: #aaa;'" : ""
        innerSelectText += `<option value="${i}" ${disabled} ${currentId}>${i}</option>`;
    }

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
    //alert(`loopType:'${loopType}', deviceName:'${deviceName}', loopNumber:'${loopNumber}', address:'${address}', noneElement:'${noneElement}', key:${key}`);
    const newDeviceInner = `<div class="col-12" id='${loopType}_${address}'>
                                <div class="row">
                                    <div class="col-10 pr-1">
                                        <a href="javascript:showDevice('${loopType}', '${deviceName}', '${loopNumber}', '${address}', '${noneElement}')" onclick="javascript:addActive('#selected_area')" >
                                            <div class="btnStyle fire" id='${address}_${deviceName}'>
                                                <img src="${DEVICES_CONSTS[key].im}" alt="" width="25" height="25" class="m15" />
                                                <div class="someS">
                                                    <div class="h5">${address + '. ' + DEVICES_CONSTS[key].sign}</div>
                                                </div>
                                            </div>
                                        </a>
                                    </div>
                                    <div class="col-1 p-0 m-0" onclick="javascript:removeDevice('${loopType}', '${loopNumber}', '${deviceName}', '${address}')">
                                        <i class="fa-solid fa-xmark fire"></i>
                                    </div>
                                </div>
                            </div>`;

    var el = document.getElementById(`new_${DEVICES_CONSTS[key].type}_${loopType}`);
    if (el) { // inserting
        el.insertAdjacentHTML('beforeend', newDeviceInner)
    }
    // reordering
    [].map.call(el.children, Object).sort(function (a, b) {
        return +a.id.match(/\d+$/) - +b.id.match(/\d+$/);
    }).forEach(function (elem) {
        el.appendChild(elem);
    });

    if (attachedDevicesList.length === maxDevicesAllowed && DEVICES_CONSTS[key].type === "device") {
        let button = document.getElementById(`btn_${loopType}`);
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
    boundAsync.getLoopDevices(mainKey, +loopNumber).then(res => {
        if (res) {
            let resJSONList = JSON.parse(res);
            let deviceData = resJSONList.find(dd => dd["~address"] === `${address}` && dd["~device"] === deviceName)["Groups"]["~noname"]["fields"];
            let elem = document.getElementById(`selected_${loopType.includes("TTE") ? "device" : "sensor"}_${loopType}`);
            showOrderedDevices(loopType, deviceName, address, elem, deviceData, loopNumber, noneElement)
            collapsible();
            addVisitedBackground();
        }
    }).catch(err => alert(err));
}

function showOrderedDevices(loopType, deviceName, address, elem, deviceData, loopNumber, noneElement) {
    let { key } = getConfig(deviceName);
    let fieldsetDevice = createFieldset(`selected_${loopType}_${deviceName}_${address}`, `${address}. ${DEVICES_CONSTS[key].sign}`);
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
            case (key.includes("NAME_I")):
                if (!fieldsetChannels) fieldsetChannels = createFieldset(`channels_${loopType}_${deviceName}_${address}`, new T().t(localStorage.getItem('lang'), "channels"));
                inner = transformGroupElement(deviceData[key]);
                if (inner) fieldsetChannels.insertAdjacentHTML('beforeend', `<div class="col-xs-12 col-lg-6">${inner}</div>`);
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
            it.dataset.content = r;
        }
    });
    $(it).popover({ trigger: "hover" });
}

const removeDevice = (loopType, loopNumber, deviceName, address) => {
    let { key } = getConfig(deviceName);

    sendMessageWPF({
        'Command': 'RemovingLoopElement',
        'Params': { 'NO_LOOP': mainKey, 'loopType': loopType, 'loopNumber': loopNumber, 'deviceName': deviceName, 'deviceAddress': address }
    });

    let parent = document.getElementById(`new_${DEVICES_CONSTS[key].type}_${loopType}`);
    let childNode = document.getElementById(`${loopType}_${address}`);
    parent.removeChild(childNode);
    let shownDevice = document.getElementById(`selected_${loopType}_${deviceName}_${address}`);
    if (shownDevice) {
        shownDevice.parentNode.removeChild(shownDevice);
    }

    attachedDevicesList = attachedDevicesList.filter(x => x !== +address);

    // update deviceNmbr button
    updateDeviceNmbr();

    if (attachedDevicesList.length < maxDevicesAllowed && document.getElementById(`btn_${loopType}`).style.display === "none") {
        document.getElementById(`btn_${loopType}`).style.display = "inline-flex";
    }
}

const fillLoopElements = (loopNumber, loopType) => {
    boundAsync.getLoopDevices(mainKey, +loopNumber).then(res => {
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
        let loopsList = JSON.parse(r);
        // creating the founded loops
        Object.keys(loopsList).forEach(key => {
            addLoop(loopsList[key], "old");
        });
    }).catch(err => alert(err));
    $("#deviceList").modal('hide')
}

// initial loop function for adding a loop; - launch when pressin button ADD LOOP or + LOOP
const loopFunc = () => {
    if (lst.length - 1 === elements) {
        alert(new T().t(localStorage.getItem('lang'), "max_number_of_loops_created_already"));
        $("#deviceList").modal('hide'); //.hide();
        return;
    } else {
        sendMessageWPF({
            'Command': 'AddingLoop',
            'Params': { 'elementType': mainKey, 'elementNumber': lst.length },
            'Callback': 'loopCallback' // -> the back-end call the function loopCallBack with default 'CallBackParams': [mainKey, lst.length, 'CHANGE']
        });
        $("#deviceList").modal('show');// data - toggle="modal" data - target="#deviceList"
    }
}

// function to fill in the modal with TELETEK_LOOP and SYSTEM SENSOR LOOP
function loopCallback(key = mainKey, len = lst.length, command = 'CHANGE') {
    boundAsync.getJsonNodeForElement(key, len, command).then(res => {
        var changeJson = JSON.parse(res);
        if (!Object.keys(changeJson).length) { // guard that the res is full
            var i = 0;
            do {
                loopFunc(); i++;
            } while (i === 3); // up to 3 trials
            if (i === 3) {
                alert("Error 3 occurred. Please, connect your software providers!"); return;
            }
        }
        var listTab = document.getElementById('deviceList').querySelector("#list-tab"); // $("#deviceList").find("#list-tab")[0];
        Object.keys(changeJson).forEach(k => {
            let tabListButton = `<button type="button"
                                class="list-group-item col-6"
                                id="list-${k}" data-toggle="tab"
                                onclick="javascript: addLoop('${k}');"
                                role="tab"
                                aria-controls="list-${k}"
                                aria-selected="false">
                            <div class="btnStyle fire">
                                <i class="fa-solid fa-arrows-spin fa-3x fire p15" aria-hidden="true">
                                    <br /><span class="someS">
                                        <span class="h5">${k.includes("TTE") ? "Teletek" : "System Sensor"} Loop</span>
                                    </span>
                                </i>
                            </div>
                        </button>`
            listTab.insertAdjacentHTML('beforeend', tabListButton);
        });
    }).catch(err => alert(err));
}

//actual adding loop elements function
function addLoop(loopType, newFlag = "new") {
    if (!loopType) return;

    var last = parseInt(loopType.charAt(loopType.length - 1)); // get the index of the loop
    if (lst.includes(last)) {
        alert("Error with index");
        return;
    }

    let color = Object.keys(BUTTON_COLORS).find(c => loopType.toUpperCase().includes(c));
    let elType = Object.keys(BUTTON_IMAGES).find(im => loopType.toUpperCase().includes(im));

    const newLoop = `<div class="col-12" id=${loopType}>
                <div class="row">
                    <div class="col-11 pr-1">
                        <a href="javascript:showLoop('${last}', '${loopType}')" onclick="javascript:addActive()">
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
                    <div class="col-1 p-0 m-0" onclick="javascript: exchangingLoop('${loopType}')">
                        <i class="fa-solid fa-right-left fire"></i>
                    </div>
                </div>
            </div>`;

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
    if (last === 1 && !document.getElementById("rvmBtn")) {
        let btnGroup = document.getElementById("btnGroup");
        btnGroup.classList += ' multi-button';
        let len = btnGroup.firstElementChild.childNodes.length;
        btnGroup.firstElementChild.childNodes[len - 1].textContent = "";
        btnGroup.insertAdjacentHTML('beforeend', `<button class="btn-border-black">${new T().t(localStorage.getItem('lang'), "loop")}</button><button onclick="javasript: removeLoop()" class="btn-round btn-border-black" id="rvmBtn"><i class="fa-solid fa-minus"></i></button>`);
    }

    // reordering
    var main = document.getElementById('new');

    [].map.call(main.children, Object).sort(function (a, b) {
        return +a.id.match(/\d+/) - +b.id.match(/\d+/);
    }).forEach(function (elem) {
        main.appendChild(elem);
    });

    lst.push(last);
    $('#deviceList').modal('hide');
}

function showLoop(loopNumber, loopType) {
    let el = document.getElementById("selected_area");
    attachedDevicesList = [];
    deviceNmbr = attachedDevicesList.length;
    let targetTTE = `<div class="row fullHeight">
                    <div class="col-3 bl fire scroll">
                        <div id="new_device_${loopType}" class="row">
                            <button class="btn-small btn-border-black" onclick="javacript: calculateLoopDevices(${loopNumber})" id="calculateDevices"
                                data-bs-toggle="modal" data-bs-target="#showDevicesListModal" >
                                    ${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}
                            </button>
                        </div>
                    </div>
                    <div class="col-9" style="z-index: 1;height: fit-content;">
                        <div id="selected_device_${loopType}" style="background: white; margin-right: -1rem">

                        </div>
                    </div>
                </div>

                <div class="modal fade" tabindex="-1" role="dialog" id="${loopType}_modal" aria-hidden="true">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="row list-group fire justify-content-center" id="list-tab" role="tablist">
                                                        
                            </div>
                        </div>
                    </div>
                </div>

                <div style="bottom: 10px; position: absolute;" class="buttons-row mt-5">
                    <button style="display: inline-flex; margin: -3px;" type="button" onclick="javascript: loopElementFunc(${loopNumber}, '${loopType}'); return false;" 
                        data-bs-toggle="modal" data-bs-target="#${loopType}_modal" id="btn_${loopType}" class="btn-round btn-border-black">
                            <i class="fa-solid fa-plus 5x"></i> ${new T().t(localStorage.getItem('lang'), "add_new")} ${new T().t(localStorage.getItem('lang'), "device_in_loop")} ${loopNumber}
                    </button>
                </div>`;

    var targetSSL = `<div class="row fullHeight">
                            <div class="col-3 bl fire scroll">
                                <button class="btn-small btn-border-black" onclick="javacript: calculateLoopDevices()" id="calculateDevices"
                                    data-bs-toggle="modal" data-bs-target="#showDevicesListModal">
                                        ${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}
                                </button>
                                <p>${new T().t(localStorage.getItem('lang'), "sensors")}</p>
                                <div id="new_sensor_${loopType}" class="row">

                                </div>
                                <p>${new T().t(localStorage.getItem('lang'), "modules")}</p>
                                <div id="new_module_${loopType}" class="row">

                                </div>
                            </div>
                            <div class="col-9" style="z-index: 1;height: fit-content;">
                                <div id="selected_sensor_${loopType}" style="background: white; margin-right: -1rem">

                                </div>
                            </div>
                        </div>

                        <div class="modal fade" tabindex="-1" role="dialog" id="${loopType}_modal" aria-hidden="true">
                            <div class="modal-dialog" role="document">
                                <div class="modal-content">
                                    <div class="row list-group fire" id="list-tab" role="tablist">
                                               
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div style="bottom: 10px; position: absolute;" class="buttons-row mt-5">
                            <button style="display: inline-flex; margin: -5px;" type="button" onclick="javascript: loopElementFunc(${loopNumber}, '${loopType}'); return false;"
                                data-bs-toggle="modal" data-bs-target="#${loopType}_modal" id="btn_${loopType}" class="btn-round btn-border-black">
                                    <i class="fa-solid fa-plus 5x"></i> ${new T().t(localStorage.getItem('lang'), "add_new")} ${new T().t(localStorage.getItem('lang'), "sensormodule_in_loop")} ${loopNumber}
                            </button>
                        </div>`;

    if (loopType.includes("TTE")) {
        el.innerHTML = targetTTE;
    } else {
        el.innerHTML = targetSSL;
    }

    var script = document.getElementById("script_modal");
    if (!script) {
        script = document.createElement('script');
        script.id = "script_modal";
        document.body.appendChild(script);
    }
    script.innerHTML = `$("#${loopType}_modal").on('hidden.bs.modal', function () {
            $("#${loopType}_modal").find("#list-tab").empty();
        });`;

    fillLoopElements(loopNumber, loopType);
}

function exchangingLoop(loopType) {
    alert(`exchanging ${loopType}, 'mainKey': ${mainKey}, 'lst.length': ${lst.length}`)
}

async function removeLoop() {
    // check if there are some connected elements
    let returnString = await boundAsync.checkLoopConnection(mainKey, lst.at(-1));
    // show confirmation dialog
    if (returnString.length > 0) {
        let deviceListJSON = JSON.parse(returnString);
        $('#showConfirmationModal .modal-title').html(`${new T().t(localStorage.getItem('lang'), "are_you_sure_removing")} ${new T().t(localStorage.getItem('lang'), "loop")} ${lst.at(-1)}?`);

        let modalContent = $('#showConfirmationModal .modal-body')[0];

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

        $("#bigClose, #xClose").on("click", function () {
            $("#showConfirmationModal").modal("hide");
        });
        $("#yesBtn").off('click').on("click", removeLoopAfterConfirm);

        $("#showConfirmationModal").modal("show");
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

    if (loopNumber === 1) {
        let btnGroup = document.getElementById("btnGroup");
        btnGroup.classList.remove('multi-button');
        let firstEl = btnGroup.firstElementChild;
        firstEl.insertAdjacentHTML('beforeend', ` ${new T().t(localStorage.getItem('lang'), "add_new")} ${new T().t(localStorage.getItem('lang'), "loop")}`);
        btnGroup.replaceChildren(firstEl);
    }
    $("#showConfirmationModal").modal("hide");
}

function calculateLoopDevices(loopNumber) {
    let modal = $(document.getElementById("showDevicesListModal"));

    let modalTitle = modal.find('.modal-title')[0];
    modalTitle.innerHTML = `${new T().t(localStorage.getItem('lang'), 'number_of_devices_per_loop')}:`;
    let modalContent = modal.find('.modal-body')[0];

    const deviceMap = new Map();
    boundAsync.getLoopDevices(mainKey, +loopNumber).then(res => {
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
    return Object.keys(DEVICES_CONSTS).find(k => deviceName.includes(k));
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
    if (DEVICES_CONSTS[key].type === "module") {
        minDevices += 100;
        maxDevices += 100;
    }

    return { key, minDevices, maxDevices };
}

function updateDeviceNmbr() {
    deviceNmbr = attachedDevicesList.length;
    let showBtn = document.getElementById("calculateDevices");
    showBtn.innerHTML = `${new T().t(localStorage.getItem('lang'), "number_of_devices")}: ${deviceNmbr}`;
}

// creating a fieldset
function createFieldset(id, legendName) {
    let fieldset = document.createElement("fieldset");
    fieldset.id = id;
    fieldset.className = "row align-items-center";
    fieldset.insertAdjacentHTML("afterbegin", `<legend>${legendName}</legend>`);
    return fieldset;
}
//#endregion UTILS