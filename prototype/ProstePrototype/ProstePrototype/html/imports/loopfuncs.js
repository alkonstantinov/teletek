let deviceNmbr = 0;

const loopFunc = () => {
    if (lst.length - 1 === elements) {
        alert("Max number of loops created already");
        $("#deviceList").modal('hide'); //.hide();
        return;
    } else {        
        sendMessageWPF({
            'Command': 'AddingElement',
            'Params': { 'elementType': mainKey, 'elementNumber': lst.length },
            'Callback': 'loopCallback',
            'CallBackParams': [mainKey, lst.length, 'CHANGE']
        });    // 'CallBackParams' mandatory with 'Callback'
        $("#deviceList").modal('show');// data - toggle="modal" data - target="#deviceList"
    }
}

function loopCallback(key, len, command) {    
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

const getArrayToModal = (id, loopType, operation) => {
    boundAsync.getJsonNode(id, operation).then(res => {
        var listJson = JSON.parse(res);
        alert('loopType' + loopType);
        let deviceList = document.getElementById(`${loopType}_modal`).querySelector('#list-tab');
        let deviceListName = id.includes('NONE') ? "Devices" : id.split('_').slice(1).join(' ');
        deviceList.insertAdjacentHTML('beforeend', `<div class="col-12 pt-2" style="text-align: center;text-decoration: underline;">${deviceListName}</div>`)
        Object.keys(listJson).forEach(deviceName => {
            deviceList.insertAdjacentHTML('beforeend', `
                <button type="button"
                        class="list-group-item col"
                        id="${deviceName}"
                        onclick="javascript: addLoopElement(this.id, '${loopType}')"
                        data-toggle="tab"
                        role="tab"
                        aria-selected="false">
                    <div class="btnStyle fire">
                        <img src="../../Images/17.1251E.png"
                                alt=""
                                width="50"
                                height="50"
                                class="m15" />
                        <div class="someS">
                            <h5>${deviceName.split('_').slice(1).join(' ')}</h5>
                        </div>
                    </div>
                </button>
            `)
        });
    });
}

let lst2 = [];
let devices2 = 99;
const loopElementFunc = (loopNumber, loopType) => {
    boundAsync.getJsonNode(loopType, 'CONTAINS').then(res => {
        var changeJson = JSON.parse(res)["ELEMENT"];

        if (!Array.isArray(changeJson)) {
            const { id, min, max } = {
                id: changeJson["@ID"],
                min: +(changeJson["@MIN"]),
                max: +(changeJson["@MAX"])
            };
            if (min !== max) {
                devices2 = max;
            }
            if (id) {
                getArrayToModal(id, loopType, 'CHANGE');
            }
        } else {
            Object.keys(changeJson).forEach(idx => {
                getArrayToModal(changeJson[idx]["@ID"], loopType, 'CONTAINS');
            });
        }
    });

    //if (lst.length - 1 === elements) {
    //    alert("Max number of loops created already");
    //    $("#deviceList").modal('hide'); //.hide();
    //    return;
    //} else {
    //    sendMessageWPF({
    //        'Command': 'AddingLoopElement', 'Params': { 'elementType': mainKey, 'elementNumber': loopNumber }, 'Callback': "loopCallback"
    //        , "CallBackParams": [mainKey, lst.length, 'CHANGE']
    //    });

    //    $("#deviceList").modal('show');// data - toggle="modal" data - target="#deviceList"
    //}
}

const addLoopElement = (loopNumber, loopType) => {
    alert(loopType + ' ' + loopNumber)
}


//adding loop elements function
function addLoop(loopType) {
    if (!loopType) return;

    var last = parseInt(loopType.charAt(loopType.length - 1)); // get the index of the loop
    if (lst.includes(last)) {
        alert("Error with index");
        return;
    }
    //sendMessageWPF({ 'Command': 'AddingElement', 'Params': { 'loopType': `'${loopType}'` } });
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

    // adding remove button
    if (last === 1 && !document.getElementById("rvmBtn")) {
        let btnGroup = document.getElementById("btnGroup");
        btnGroup.classList += ' multi-button';
        let len = btnGroup.firstElementChild.childNodes.length;
        btnGroup.firstElementChild.childNodes[len - 1].textContent = "";
        btnGroup.insertAdjacentHTML('beforeend', `<button class="btn-border-black">Loop</button><button onclick="javasript: removeLoop()" class="btn-round btn-border-black" id="rvmBtn"><i class="fa-solid fa-minus"></i></button>`);
    }

    // reordering
    var main = document.getElementById('new');

    [].map.call(main.children, Object).sort(function (a, b) {
        return +a.id.match(/\d+/) - +b.id.match(/\d+/);
    }).forEach(function (elem) {
        main.appendChild(elem);
    });

    lst.push(last);
    $('#deviceList').modal('toggle');
}

function showLoop(loopNumber, loopType) {
    let el = document.getElementById("selected_area");
    let targetTTE =`<div class="row fullHeight">
                    <div class="col-3 bl fire scroll">
                        <div id="new_device_${loopType}" class="row">
                            <button class="btn-small btn-border-black" onclick="javacript: calculateLoopDevices()" id="calculateDevices"
                                data-toggle="modal" data-target="#showDevicesListModal" >
                                    Number of devices: ${deviceNmbr}
                            </button>
                        </div>
                    </div>
                    <div class="col-9" style="z-index: 1;height: fit-content;">
                        <div id="selected_device_${loopType}" style="background: white;">

                        </div>
                    </div>
                </div>

                <div class="modal fade bd-example" tabindex="-1" role="dialog" id="${loopType}_modal" aria-hidden="true">
                    <div class="modal-dialog" role="document">
                        <div class="modal-content">
                            <div class="row list-group fire justify-content-center" id="list-tab" role="tablist">
                                                        
                            </div>
                        </div>
                    </div>
                </div>

                <div style="bottom: 10px; position: absolute;" class="buttons-row mt-5">
                    <button style="display: inline-flex; margin: -5px;" type="button" onclick="javascript: loopElementFunc(${loopNumber}, '${loopType}'); return false;" 
                        data-toggle="modal" data-target="#${loopType}_modal" id="btn_${loopType}" class="btn-round btn-border-black">
                            <i class="fa-solid fa-plus 5x"></i> Add New Device in Loop ${loopNumber}
                    </button>
                </div>`;

    var targetSSL = `<div class="row fullHeight">
                            <div class="col-3 bl fire scroll">
                                <button class="btn-small btn-border-black" onclick="javacript: calculateLoopDevices()" id="calculateDevices"
                                    data-toggle="modal" data-target="#showDevicesListModal">
                                        Number of devices: ${deviceNmbr}
                                </button>
                                <p>Sensors</p>
                                <div id="new_sensor_${loopType}" class="row">

                                </div>
                                <p>Modules</p>
                                <div id="new_module_${loopType}" class="row">

                                </div>
                            </div>
                            <div class="col-9" style="z-index: 1;height: fit-content;">
                                <div id="selected_sensor_${loopType}" style="background: white;">

                                </div>
                            </div>
                        </div>

                        <div class="modal fade bd-example" tabindex="-1" role="dialog" id="${loopType}_modal" aria-hidden="true">
                            <div class="modal-dialog" role="document">
                                <div class="modal-content">
                                    <div class="row list-group fire" id="list-tab" role="tablist">
                                               
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div style="bottom: 10px; position: absolute;" class="buttons-row mt-5">
                            <button style="display: inline-flex; margin: -5px;" type="button" onclick="javascript: loopElementFunc(${loopNumber}, '${loopType}'); return false;"
                                data-toggle="modal" data-target="#${loopType}_modal" id="btn_${loopType}" class="btn-round btn-border-black">
                                    <i class="fa-solid fa-plus 5x"></i> Add New Sensor/Module in Loop ${loopNumber}
                            </button>
                        </div>`;

    if (loopType.includes("TTE")) {
        el.innerHTML = targetTTE;
    } else {
        el.innerHTML = targetSSL;
    }
}

function exchangingLoop(loopType) {
    alert(`exchanging ${loopType}, 'mainKey': ${mainKey}, 'lst.length': ${lst.length}`)
}

function removeLoop() {
    // check if there are some connected elements // TODO

    // show confirmation dialog // TODO

    // if confirmed remove from the backend
    let loopNumber = lst.pop();
    // sendMessageWPF({ 'Command': 'RemovingLoopElement', 'Params': { 'elementType': `'${mainKey}'`, 'elementNumber': `'${loopNumber}'` } }); // TODO uncomment
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
        firstEl.insertAdjacentHTML('beforeend', " Add New Loop");
        btnGroup.replaceChildren(firstEl);
    }
}

function calculateLoopDevices() {
    let modal = $(document.getElementById("showDevicesListModal"));
    let modalContent = modal.find('.modal-body')[0];
    modalContent.innerHTML = `<p>"Setting the new modal list"</p>`;
}