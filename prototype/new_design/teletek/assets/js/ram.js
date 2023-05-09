document.addEventListener('DOMContentLoaded', function () {

    const sidebar = document.querySelector("#ram_sidebar");
    const sidebarWrapper = sidebar.firstElementChild;
    const bar1 = document.querySelector("#ram_bar_1");
    const bar2 = document.querySelector("#ram_bar_2");
    const bar3 = document.querySelector("#ram_bar_3");
    const main = document.querySelector('#ram_main');
    const hamburgerBtn = document.querySelector("#ram_hamburger_btn");
    const resizable = document.querySelectorAll(".ram_resizable");
    const resizers = document.querySelectorAll('.ram_resizer');
    const togglesBtn = document.querySelectorAll('.ram_toggle_btn');
    const mainTransition = 300;

    window.addEventListener('load', () =>{
        main.style.marginLeft = sidebarWrapper.offsetWidth + 'px';
    });
    window.addEventListener('resize', () =>{
        main.style.marginLeft = sidebarWrapper.offsetWidth + 'px';
    });

    /* Hamburger Button */
    hamburgerBtn.addEventListener('click', () => {
        sidebar.classList.toggle("open");
        hamburgerBtn.firstElementChild.classList.toggle("open");
        setTimeout(() => { main.style.marginLeft = sidebarWrapper.offsetWidth + 'px' }, mainTransition);
    });

});