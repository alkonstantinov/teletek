let type = '';

function defineInputType(option) {
    type = option;
    btn = document.getElementById("nextButton");
    btn.disabled = false;
}

function undisable(value) {
    btn = document.getElementById("send_button");
    if (value) {
        btn.disabled = false;
    } else {
        btn.disabled = true;
    }
}

function checkHexRegex(event) {
    let val = event.target.value;
    let regEx = /^([0-9A-Fa-f]{1,2})$/;
    let isHex = regEx.test(val);
    if (!isHex) {
        document.getElementById(event.target.id).value = val.slice(0, -1);
    }
}

function clearInputType() {
    type = '';
    document.getElementsByName('labelName')[0].value = '';
    document.getElementsByName('colWidth')[0].value = '';
    document.getElementsByName('placeHolderText')[0].value = '';
    document.getElementsByName('maxTextLength')[0].value = '';
    document.getElementsByName('inputNameOn')[0].value = '';
    document.getElementsByName('inputNameOff')[0].value = '';
    document.getElementsByName('selectList')[0].value = '';
    document.getElementsByName('max')[0].value = '';
    document.getElementsByName('min')[0].value = '';
    document.getElementsByName('bytesData')[0].value = '0';
    document.getElementsByName('lengthData')[0].value = '1';
    $('input[name="readOnly"]').prop("checked", false);
    document.getElementById('colWidth').style.display = 'flex';
    document.getElementById('placeHolderText').style.display = 'none';
    document.getElementById('maxTextLength').style.display = 'none';
    document.getElementById('inputNameOn').style.display = 'none';
    document.getElementById('inputNameOff').style.display = 'none';
    document.getElementById('selectList').style.display = 'none';
    document.getElementById('max').style.display = 'none';
    document.getElementById('min').style.display = 'none';
    $('.custom-control-input').prop('checked', false);
}

function defineNewChildToInsert(type) {
    let input_name = document.getElementsByName('labelName')[0].value;
    if (!type || !input_name) return;
    let input_id = input_name.toLowerCase().replaceAll(' ', '_')
    let placeHolderText = document.getElementsByName('placeHolderText')[0].value || '';
    let bytesData = document.getElementsByName('bytesData')[0].value || '0';
    let lengthData = document.getElementsByName('lengthData')[0].value || '1';
    let readOnly = document.getElementsByName('readOnly')[0].checked;

    ////////////////////////////////// MODIFY THE JSON HERE OR inside the switch before the return statement

    ////////////////////////////////// END OF JSON MODIFICATION PART 

    switch (type) {
        case 'passInput':
        case 'textInput':
            let maxTextLength = document.getElementsByName('maxTextLength')[0].value || 'undefined';
            return `
                    <div class="form-item roww flex">
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <input type="${type === 'passInput' ? 'password' : 'text'}" id="${input_id}" name="${input_id}" maxlength="${maxTextLength}" placeholder="${placeHolderText}" onblur="javascript: alert(this.value)" bytes="${bytesData}" length="${lengthData}" ${readOnly ? "disabled" : ''}/>
                    </div>
                `;

        case 'checkboxInput':
            return `
                    <div class="form-item roww">
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <input type="checkbox" id="${input_id}" class="ml10" bytes="${bytesData}" length="${lengthData}" ${readOnly ? "disabled" : ''}/>
                    </div>
                `;

        case 'sliderInput':
            let input_name_on = document.getElementsByName('inputNameOn')[0].value || 'ON';
            let input_name_off = document.getElementsByName('inputNameOff')[0].value || 'OFF';
            return `<div class="form-item roww fire">
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        ${input_name && `<label for="${input_id}">${input_name}</label>`}
                        <p class="fire bordered">
                            ${input_name_off}
                            <label class="switch">
                                <input type="checkbox" id="${input_id}" name="${input_id}" bytes="${bytesData}" length="${lengthData}" ${readOnly ? "disabled" : ''}/>
                                <span class="slider"></span>
                            </label>
                            ${input_name_on}
                        </p>
                    </div>`;

        case 'selectInput':
            let selectList = document.getElementsByName('selectList')[0].value;
            selectList = selectList.split(',');
            let str = `<div class="form-item roww mt-1">
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <div class="select">
                            <select id="${input_id}"  ${readOnly ? "disabled" : ''}
                                    name="${input_id}" bytes="${bytesData}" length="${lengthData}">
                                <option value="" disabled selected>${placeHolderText || "Select your option"}</option>`;
            selectList.map(o => str += `<option value="${o.trim()}">${o.trim().charAt(0).toUpperCase() + o.trim().slice(1)}</option>`);
            str += `</select>
                        </div>
                    </div>`
            return str;

        case 'numberInput':
            let max = document.getElementsByName('max')[0].value;
            let min = document.getElementsByName('min')[0].value;
            return `
                    <div class="form-item roww">
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <input class="ml10"
                               type="number"
                               id="${input_id}"
                               name="${input_id}"
                               data-maxlength="${`${max}`.length}"
                               oninput="javascript: myFunction(this.id)"
                               onblur="javascript: myFunction2(this.id)""
                               min="${min}" max="${max}" ${readOnly ? "disabled" : ''}
                               bytes="${bytesData}" length="${lengthData}"/>
                    </div>
                `;
        case 'tabInput':
            let tabs = document.getElementsByName('selectList')[0].value;
            tabList = tabs.split(',').map(o => o.trim());
            let tabStr = `
                    <div class="form-item roww mt-1" tabInput>
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <div class="select">
                            <select id="${input_id}" ${readOnly ? "disabled" : ''}
                                    name="${input_id}" bytes="${bytesData}" length="${lengthData}">
                                <option value="" disabled selected>${placeHolderText || "Select your option"}</option>`;
            tabList.map(o => tabStr += `<option value="${o}">${o.charAt(0).toUpperCase() + o.trim().slice(1)}</option>`);
            tabStr += `</select>
                        </div>
                    </div>
                    <nav>
                      <div class="nav nav-tabs" id="nav-tab-${input_id}" role="tablist">`;
            tabList.map(o => tabStr += `<a class="nav-item nav-link" id="nav-${o}-tab" data-toggle="tab" href="#nav-${o}" role="tab" aria-controls="nav-${o}" aria-selected="false">${o.charAt(0).toUpperCase() + o.slice(1)}</a >`);
            tabStr += `</div>
                    </nav>
                    <div class="tab-content" id="nav-tabContent">`;
            tabList.map(o => tabStr += `<div class="tab-pane fade" id="nav-${o}" role="tabpanel" aria-labelledby="nav-${o}-tab">
                                <div class="col">
                                    <button type="button" data-toggle="modal" data-target="#initModal" data-whatever="nav-${o}" name="nav-${o}" class="none" onclick="javascript: clearInputType()"><i class="fa-solid fa-square-plus fa-2x"></i></button>
                                </div>
                        </div>`);
            tabStr += "</div>";
            return tabStr;
        case 'macInput':
            return `<div class="form-item roww" macInput>
                        <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
                        <label for="${input_id}">${input_name}</label>
                        <div class="row m0" id="${input_id}">
                            <input class="col-1 mr-1"
                                   type="text"
                                   id="${input_id}_ETHADDR0"
                                   name="${input_id}_ETHADDR0"
                                   placeholder="00" ${readOnly ? "disabled" : ''}
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                            <div>:</div>
                            <input class="col-1 mr-1"
                                   type="text"
                                   id="${input_id}_ETHADDR1"
                                   name="${input_id}_ETHADDR1"
                                   placeholder="00" ${readOnly ? "disabled" : ''}
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                            <div>:</div>
                            <input type="text"
                                   class="col-1 mr-1"
                                   id="${input_id}_ETHADDR2"
                                   name="${input_id}_ETHADDR2"
                                   placeholder="00" ${readOnly ? "disabled" : ''}
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                            <div>:</div>
                            <input type="text"
                                   class="col-1 mr-1"
                                   id="${input_id}_ETHADDR3"
                                   name="${input_id}_ETHADDR3"
                                   placeholder="00" disabled="${readOnly}"
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                            <div>:</div>
                            <input type="text"
                                   class="col-1 mr-1"
                                   id="${input_id}_ETHADDR4"
                                   name="${input_id}_ETHADDR4"
                                   placeholder="00" ${readOnly ? "disabled" : ''}
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                            <div>:</div>
                            <input type="text"
                                   class="col-1 mr-1"
                                   id="${input_id}_ETHADDR5"
                                   name="${input_id}_ETHADDR5"
                                   placeholder="00" ${readOnly ? "disabled" : ''}
                                   oninput="javascript: checkHexRegex(event)" maxlength="2"
                                   />
                        </div>
                    </div>`;
        default:
            return `<button class="fire collapsible ml-1 collapsible_${input_id}">${input_name}</button>
        <div class="collapsible-content col-12">
            <button type="button" id="${input_id}_btn" class="none-inherit" onclick="javascript: removeItem(this.id)"><i class="fa-solid fa-square-minus fa-2x"></i></button>
            <div class="row align-items-center" id="${input_id}">
                <div class="col">
                    <button type="button" data-toggle="modal" data-target="#initModal" data-whatever="${input_id}" name="${input_id}" class="none" onclick="javascript: clearInputType()"><i class="fa-solid fa-square-plus fa-2x"></i></button>
                </div>
            </div>
        </div>`;
    }
}

function chooseInputTypeModal(event) {
    let button = $(event.relatedTarget) // Button that triggered the modal
    let recipient = button.attr('data-whatever') // Extract info from data-* attributes
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    let modal = $(this)
    let title = modal.find('.modal-title')
    title.text('Please Choose which type of Input you would like to add')
    title[0].style.color = 'red';
    // set data-whatever to the given recipient
    let btn = modal.find('#nextButton');
    btn.attr("data-whatever", recipient);
}

function setInputData(event) {
    let button = $(event.relatedTarget) // Button that triggered the modal
    let recipient = button.attr('data-whatever') // Extract info from data-* attributes
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    let modal = $(this)
    let title = modal.find('.modal-title')
    title.text('Please fill in all required details')
    title[0].style.color = 'red';
    let maxTextLength = modal.find('.modal-body #maxTextLength')[0]
    let on = modal.find('.modal-body #inputNameOn')[0]
    let off = modal.find('.modal-body #inputNameOff')[0]
    let select = modal.find('.modal-body #selectList')[0]
    let min = modal.find('.modal-body #min')[0]
    let max = modal.find('.modal-body #max')[0]
    switch (type) {
        case 'textInput':
            maxTextLength.style.display = 'flex';
            placeHolderText.style.display = 'flex';
            break;
        case 'sliderInput':
            on.style.display = 'flex';
            off.style.display = 'flex';
            break;
        case 'tabInput':
            modal.find('.modal-body #colWidth')[0].style.display = 'none';
        case 'selectInput':
            select.style.display = 'block';
            placeHolderText.style.display = 'flex';
            break;
        case 'numberInput':
            min.style.display = 'flex';
            max.style.display = 'flex';
            break;
        case 'passInput':
            maxTextLength.style.display = 'flex';
            break;
        case 'none':
            modal.find('.modal-body #colWidth')[0].style.display = 'none';
            break;
        default: break;
    }

    let btn = modal.find('.modal-footer #send_button')
    btn[0].setAttribute("onclick", `javascript: addItem('${recipient}')`);
}

function addItem(elementName) {
    // create the element to insert
    let parent = !!elementName ? document.getElementById(elementName) : document.getElementById('divMain');
    let newChildToInsert = document.createElement('div');
    newChildToInsert.innerHTML = defineNewChildToInsert(type);
    if (type !== 'none') {
        if (!parent) return;
        let colWidth = document.getElementsByName("colWidth")[0].value;
        let newChildClass = 'col-xs-12 col-md-' + colWidth;
        console.log('class before', newChildClass)
        if (!colWidth) {
            console.log('no colWidth')
            newChildClass = 'col'
        }
        newChildToInsert.classList.value = newChildClass;
        console.log('class', newChildClass)
        if (newChildToInsert.firstElementChild.hasAttribute('macInput') || newChildToInsert.firstElementChild.hasAttribute('tabInput')) {
            newChildToInsert.classList.value = 'col-12';
        }
        console.log('parent.lastElementChild.previousElementSibling.classList', parent.lastElementChild.previousElementSibling.classList.value, 'parent.childNodes.length', parent.childElementCount)
        if (parent.childElementCount > 1 && parent.lastElementChild.previousElementSibling.classList.value === 'col') {
            let insertClass = parent.firstElementChild.classList.value;
            console.log('insertClass', insertClass);
            if (parent.childElementCount === 2 || insertClass.includes('collapsible')) {
                parent.lastElementChild.previousElementSibling.classList = "col-xs-12 col-md-6 col-4"
            } else {
                parent.lastElementChild.previousElementSibling.classList.value = insertClass;
            }
        }

        parent.insertBefore(newChildToInsert, parent.lastElementChild);
    } else {     // when we make a collapsible button
        parent.insertBefore(newChildToInsert.firstChild, parent.lastElementChild);
        parent.insertBefore(newChildToInsert.lastChild, parent.lastElementChild);
        let input_name = document.getElementsByName('labelName')[0].value;
        let input_id = input_name.toLowerCase().replaceAll(' ', '_')
        if (document.getElementsByClassName(`collapsible_${input_id}`)) {
            collapsible(`collapsible_${input_id}`);
        }
    }
}

function removeItem(id_btn) {
    let parentEl = document.getElementById(id_btn).parentNode;
    if (parentEl.classList.contains('collapsible-content')) {
        parentEl.previousElementSibling.remove();
        parentEl.remove();
    } else {
        parentEl.parentNode.remove();
    }
}