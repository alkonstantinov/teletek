var type = '';

function defineInputType(option) {
    type = option;
    btn = document.getElementById("nextButton");
    btn.disabled = false;
}

function clearInputType() {
    type = '';
    document.getElementsByName('labelName')[0].value = '';
    document.getElementsByName('inputNameOn')[0].value = '';
    document.getElementsByName('inputNameOff')[0].value = '';
    document.getElementsByName('selectList')[0].value = '';
    document.getElementsByName('max')[0].value = '';
    document.getElementsByName('min')[0].value = '';
    $('.custom-control-input').prop('checked', false);
}

function defineNewChildToInsert(type) {
    var input_name = document.getElementsByName('labelName')[0].value;
    if (!type || !input_name) return;

    var input_name_on = document.getElementsByName('inputNameOn')[0].value;
    var input_name_off = document.getElementsByName('inputNameOff')[0].value;
    var selectList = document.getElementsByName('selectList')[0].value;
    var max = document.getElementsByName('max')[0].value;
    var min = document.getElementsByName('min')[0].value;
    var input_id = input_name.toLowerCase().replaceAll(' ', '_')

    ////////////////////////////////// MODIFY THE JSON HERE OR inside the switch before the return statement

    ////////////////////////////////// END OF JSON MODIFICATION PART 

    switch (type) {
        case 'textInput':
            return `
                    <div class="form-item roww flex">
                        <label for="${input_id}">${input_name}</label>
                        <input type="text" id="${input_id}" name="${input_id}" onblur="javascript: alert(this.value)" />
                    </div>
                `;

        case 'checkbox':
            return `
                    <div class="form-item row roww pb10">
                        <label for="${input_id}">${input_name}</label>
                        <input type="checkbox" id="${input_id}" class="ml10" />
                    </div>
                `;

        case 'slider':
            return `<div class="form-item roww fire">
                        ${input_name && `<label for="${input_id}">${input_name}</label>`}
                        <p class="fire bordered">
                            ${input_name_off}
                            <label class="switch">
                                <input type="checkbox" id="${input_id}" name="${input_id}" />
                                <span class="slider"></span>
                            </label>
                            ${input_name_on}
                        </p>
                    </div>`;

        case 'select':
            selectList = selectList.split(',');
            console.log('selectList', selectList);
            return `
                    <div class="form-item roww mt-1">
                        <label for="${input_id}">${input_name}</label>
                        <div class="select">
                            <select id="${input_id}"
                                    name="${input_id}">
                                ${selectList.map(o => `<option value="${o.trim()}">${o.trim().charAt(0).toUpperCase() + o.trim().slice(1)}</option>`)}
                            </select>
                        </div>
                    </div>
                `;

        case 'number':
            return `
                    <div class="form-item roww">
                        <label for="${input_id}">${input_name}</label>
                        <input class="ml10"
                               type="number"
                               id="${input_id}"
                               name="${input_id}"
                               data-maxlength="${`${max}`.length}"
                               oninput="javascript: myFunction(this.id)"
                               onblur="javascript: myFunction2(this.id)""
                               min="${min}"
                               max="${max}" />
                    </div>
                `;

        default:
            return `<button class="fire collapsible ml-1 collapsible_${input_id}">${input_name}</button>
        <div class="collapsible-content col-12">
            <div class="row align-items-center" id="${input_id}">
                <div class="col">
                    <button type="button" data-toggle="modal" data-target="#initModal" data-whatever="${input_id}" name="${input_id}" class="none" onclick="javascript: clearInputType()"><i class="fa-solid fa-square-plus fa-2x"></i></button>
                </div>
            </div>
        </div>`;
    }
}

function chooseInputTypeModal(event) {
    var button = $(event.relatedTarget) // Button that triggered the modal
    var recipient = button.attr('data-whatever') // Extract info from data-* attributes
    console.log('recipient', recipient)
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    var modal = $(this)
    var title = modal.find('.modal-title')
    title.text('Please Choose which type of Input you would like to add')
    title[0].style.color = 'red';
    //var checked = modal.find('.modal-body [name = "customRadio"]:checked')
    // set data-whatever to the given recipient
    var btn = modal.find('#nextButton');
    btn.attr("data-whatever", recipient);
    //btn[0].setAttribute("data-whatever", recipient);
    console.log('btn[0]', btn[0])
}

function setInputData(event) {
    var button = $(event.relatedTarget) // Button that triggered the modal
    console.log('second button', button[0], 'button.data(whatever)', button)
    var recipient = button.attr('data-whatever') // Extract info from data-* attributes
    console.log('second recipient', recipient)
    // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
    var modal = $(this)
    var title = modal.find('.modal-title')
    title.text('Please fill in all required details')
    title[0].style.color = 'red';
    //var name = modal.find('.modal-body #labelName')[0]
    var on = modal.find('.modal-body #inputNameOn')[0]
    var off = modal.find('.modal-body #inputNameOff')[0]
    var select = modal.find('.modal-body #selectList')[0]
    var min = modal.find('.modal-body #min')[0]
    var max = modal.find('.modal-body #max')[0]
    switch (type) {
        case 'slider':
            on.style.display = 'flex'
            off.style.display = 'flex'
            break;
        case 'select':
            select.style.display = 'block'
            break;
        case 'number':
            min.style.display = 'flex'
            max.style.display = 'flex'
            break;
        default: break;
    }

    var btn = modal.find('.modal-footer #send_button')
    btn[0].setAttribute("onclick", `javascript: addItem('${recipient}')`);
}

function addItem(elementName) {
    // create the element to insert
    var parent = !!elementName ? document.getElementById(elementName) : document.getElementById('divMain');
        var newChildToInsert = document.createElement('div');
        newChildToInsert.innerHTML = defineNewChildToInsert(type);
    if (type !== 'none') {
        if (!parent) return;
        newChildToInsert.classList.value = 'col';
        var insertClass = parent.firstElementChild.classList.value;
        if (parent.lastElementChild.previousElementSibling === null) {
            console.log(parent.lastElementChild.classList)
        } else if (parent.lastElementChild.previousElementSibling !== null && parent.lastElementChild.previousElementSibling.previousElementSibling === null) {
            parent.lastElementChild.previousElementSibling.classList = "col-4"
        } else if ( 
            parent.lastElementChild.previousElementSibling !== null &&
            parent.lastElementChild.previousElementSibling.previousElementSibling !== null &&
            !parent.lastElementChild.previousElementSibling.previousElementSibling.classList.contains('collapsible')
        ) { 
            parent.lastElementChild.previousElementSibling.classList.value = insertClass;
        }
        parent.insertBefore(newChildToInsert, parent.lastElementChild);
    } else {     // when we make a button
        parent.insertBefore(newChildToInsert.firstChild, parent.lastElementChild);
        parent.insertBefore(newChildToInsert.lastChild, parent.lastElementChild);
        var input_name = document.getElementsByName('labelName')[0].value;
        var input_id = input_name.toLowerCase().replaceAll(' ', '_')
        console.log('addItem before')
        if (document.getElementsByClassName(`collapsible_${input_id}`)) {
            console.log('addItem inside')
            collapsible(`collapsible_${input_id}`);
        }
    }
    console.log('newChildToInsert', newChildToInsert);
    //console.log(parent)
    
    
    resetModal();
}

function resetModal() {
    document.getElementById('inputNameOn').style.display = 'none';
    document.getElementById('inputNameOff').style.display = 'none';
    document.getElementById('selectList').style.display = 'none';
    document.getElementById('max').style.display = 'none';
    document.getElementById('min').style.display = 'none';
}