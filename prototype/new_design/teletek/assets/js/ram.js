document.addEventListener('DOMContentLoaded', function () {

    const sidebar = document.querySelector("#ram_sidebar");

    const panel1 = document.querySelector("#ram_panel_1");
    const panel2 = document.querySelector("#ram_panel_2");
    const panelAdd = document.querySelector("#ram_panel_add");
    const nav = document.querySelector("#ram_nav");
    const main = document.querySelector('#ram_main_wrapper');
    const toggleBtns = document.querySelectorAll(".ram_toggle_btn");
    const resizers = document.querySelectorAll(".ram_resizer");
    const cards1 = panel1.querySelectorAll(".ram_card");
    const cards2 = panel2.querySelectorAll(".ram_card");
    const AddCards = panelAdd.querySelectorAll(".ram_card");

    const accordionItem = document.querySelectorAll("#ram_sidebar .ram_list_group_item");
    const showPanelAddBtns = document.querySelectorAll(".ram_show_panel_add");
    const hidePanelAddBtns = document.querySelectorAll(".ram_hide_panel_add");

    const panelOpenWidth = 180;
    const panelClosedWidth = 80;
    const panelAddOpenWidth = 360;
    const panelAddClosedWidth = 260;


    /* Calculate Panels height */
    function calcHeight(e) {
        let el = e[0].target;

        if (el.classList.contains("ram_panel")) {
            let ph = (nav.offsetHeight + el.children[1].offsetHeight + 15);
            el.children[0].style.height = `calc(100vh - ${ph}px)`;
        } else if (el.id == "ram_sidebar") {
            let sh = (el.children[0].offsetHeight + el.children[2].offsetHeight);
            el.children[1].style.height = `calc(100vh - ${sh}px)`;
            el.children[1].style.marginTop = `${el.children[0].offsetHeight}px`;
        }
    }

    const observerPanel = new MutationObserver(calcHeight);
    const observerOptions = {
        attributes: true,
        attributeFilter: ["style"],
    };
    observerPanel.observe(panel1, observerOptions);
    observerPanel.observe(panel2, observerOptions);
    observerPanel.observe(panelAdd, observerOptions);
    observerPanel.observe(sidebar, observerOptions);


    /* Active elements behavior */
    function activeElements(arr, t = true) {
        arr.forEach(function (el) {
            el.addEventListener('click', function () {
                if (t) {
                    arr.forEach((elInactive) => {
                        elInactive.classList.remove("active");
                    });
                }
                el.classList.toggle("active");
            });

        });
    }

    activeElements(cards1);
    activeElements(cards2);
    activeElements(AddCards, false);
    activeElements(accordionItem);

    /* Show or hide panel_add */
    function showPanelAdd() {
        panelAdd.style.marginLeft = "0px";
        /* TO-DO Add resource */
    }

    function hidePanelAdd() {
        panelAdd.style.marginLeft = `-${panelAdd.offsetWidth}px`;
        /* TO-DO Remove resource */
    }

    hidePanelAddBtns.forEach(btn => {
        btn.addEventListener('click', hidePanelAdd);
    });

    showPanelAddBtns.forEach(btn => {
        btn.addEventListener('click', showPanelAdd);
    });
    accordionItem.forEach(btn => {
        btn.addEventListener('click', showPanelAdd);
    });


    /* Open and close Sidebar and Panels*/
    toggleBtns.forEach((btn) => {
        btn.addEventListener('click', () => {
            let resizedEl = btn.parentElement.parentElement;

            let resizedW = resizedEl.offsetWidth;
            let icon = resizedEl.querySelector(".ram_icon.toggle");

            if (resizedEl.id == "ram_sidebar") {
                btn.firstElementChild.classList.toggle("open");
                resizedEl.classList.toggle("closed");
            } else {
                let cardsContainer = resizedEl.querySelector(".ram_cards");

                if (resizedEl.id !== "ram_panel_add") {
                    if (resizedEl.offsetWidth >= panelOpenWidth) {

                        resizedEl.style.width = `${panelClosedWidth}px`;
                        cardsContainer.style.gridTemplateColumns = "repeat(1, 1fr)";
                        resizedEl.classList.add("closed");
                        icon.classList.add("open");
                    } else {

                        resizedEl.style.width = `${panelOpenWidth}px`;
                        cardsContainer.style.gridTemplateColumns = "repeat(1, 1fr)";
                        resizedEl.classList.remove("closed");
                        icon.classList.remove("open");
                    }

                } else {
                    if (resizedEl.offsetWidth >= panelAddOpenWidth) {

                        resizedEl.style.width = `${panelAddClosedWidth}px`;
                        cardsContainer.style.gridTemplateColumns = "repeat(4, 1fr)";
                        resizedEl.classList.add("closed");
                        icon.classList.add("open");
                    } else {

                        resizedEl.style.width = `${panelAddOpenWidth}px`;
                        cardsContainer.style.gridTemplateColumns = "repeat(2, 1fr)";
                        resizedEl.classList.remove("closed");
                        icon.classList.remove("open");
                    }
                }
            }

        });
    });


    /* Rezize panels */
    var dragging = false;
    var initiator = '';
    let resizedEl = '';
    let icon = '';

    let x = 0;
    let resizedElW = 0;
    let cardsContainer = null;

    /* Calculate Devices column on Panel resize */
    function calcCardsColumn(e) {
        let el = e[0].target;
        let cW = el.offsetWidth;
        cardsContainer = el.querySelectorAll(".ram_cards")[0];

        if (cW >= 1260) {
            cardsContainer.style.gridTemplateColumns = "repeat(8, 1fr)";
        } else if (cW >= 1110) {
            cardsContainer.style.gridTemplateColumns = "repeat(7, 1fr)";
        } else if (cW >= 960) {
            cardsContainer.style.gridTemplateColumns = "repeat(6, 1fr)";
        } else if (cW >= 810) {
            cardsContainer.style.gridTemplateColumns = "repeat(5, 1fr)";
        } else if (cW >= 660) {
            cardsContainer.style.gridTemplateColumns = "repeat(4, 1fr)";
        } else if (cW >= 510) {
            cardsContainer.style.gridTemplateColumns = "repeat(3, 1fr)";
        } else if (cW >= 360) {
            cardsContainer.style.gridTemplateColumns = "repeat(2, 1fr)";
        } else {
            if (el.id == "ram_panel_add") {
                cardsContainer.style.gridTemplateColumns = "repeat(4, 1fr)";
            } else {
                cardsContainer.style.gridTemplateColumns = "repeat(1, 1fr)";
            }
        }
    }

    const observerCards = new MutationObserver(calcCardsColumn);

    resizers.forEach(el => {
        el.addEventListener('mousedown', function (e) {
            e.preventDefault();
            dragging = true;
            x = e.clientX;
            initiator = el;
            resizedEl = initiator.parentElement;
            resizedElW = resizedEl.getBoundingClientRect().width;
            icon = resizedEl.querySelector(".ram_icon.toggle");
            resizedEl.classList.remove("ram_animate");
            main.classList.remove("ram_animate");

            observerCards.observe(resizedEl, observerOptions);

            document.addEventListener('mousemove', onMouseMove);
        });
    });

    document.addEventListener('mouseup', function (e) {
        e.preventDefault();
        if (dragging) {
            document.removeEventListener('mousemove', onMouseMove);
            document.body.style.removeProperty('cursor');
            document.body.style.removeProperty('user-select');
            resizedEl.classList.add("ram_animate");
            main.classList.add("ram_animate");

            observerCards.disconnect();

            dragging = false;
        }
    });

    function onMouseMove(e) {
        e.preventDefault();
        const dx = (e.clientX - x);
        const newWidth = (resizedElW + dx);

        document.body.style.cursor = "col-resize";
        document.body.style.userSelect = "none";

        if (resizedEl.id !== "ram_panel_add") {
            if (resizedEl.offsetWidth >= panelOpenWidth) {
                resizedEl.classList.remove("closed");
                icon.classList.remove("open");
            } else {
                resizedEl.classList.add("closed");
                icon.classList.add("open");
            }

            if (newWidth >= 80) {
                resizedEl.style.width = `${newWidth}px`;
            } else {
                resizedEl.style.width = `${panelClosedWidth}px`;
            }
        } else {
            if (resizedEl.offsetWidth >= panelAddOpenWidth) {
                resizedEl.classList.remove("closed");
                icon.classList.remove("open");
            } else {
                resizedEl.classList.add("closed");
                icon.classList.add("open");
            }

            if (newWidth >= panelAddClosedWidth) {
                resizedEl.style.width = `${newWidth}px`;
                oldWidth = newWidth;
            } else {
                resizedEl.style.width = `${panelAddClosedWidth}px`;
            }
        }
    }

});