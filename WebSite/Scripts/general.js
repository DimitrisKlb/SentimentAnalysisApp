$(document).ready(function () {
    
    /******************** Sidebar ********************/
    var trigger = $(".hamburger");
    var sidebarSmallLimit = 500;
    
    // Sidebar hide-show     
    trigger.click(function () {
        sidebarToggle();
    });

    function sidebarToggle() {
        $("#wrapper").toggleClass("toggled");
        trigger.toggleClass("is-open");
        trigger.toggleClass("is-closed");
    }


    if ($(window).width() < sidebarSmallLimit) {
        sidebarToggle();
    } 

    // Sidebar Dropdown
    var closeDropdown = false;
    $(".dropdown.keep-open").on("show.bs.dropdown", function () {
        $(this).find(".dropdown-menu").first().stop(true, true).slideDown();
        closeDropdown = false;
    });

    $(".dropdown.keep-open .dropdown-toggle").on("click", function () {
        closeDropdown = true;
    });

    $(".dropdown.keep-open").on("hide.bs.dropdown", function () {
        if (closeDropdown === false) {
            return false;
        }
        $(this).find('.dropdown-menu').first().stop(true, true).slideUp();
    });



});