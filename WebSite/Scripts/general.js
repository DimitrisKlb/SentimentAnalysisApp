$(document).ready(function () {

    /******************** Sidebar ********************/
    var trigger = $(".hamburger");
    var sidebarAlwaysOpenLimit = 1200;
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

    // Close sidebar on page load if window is small
    if ($(window).width() < sidebarSmallLimit) {
        sidebarToggle();
    }

    // Open the sidebar and keep it that way if window is big 
    $(window).resize(function () {
        if ($(window).width() > sidebarAlwaysOpenLimit) {
            trigger.hide();
            if ($("#wrapper").hasClass("toggled") == false) {
                sidebarToggle();
            }
        } else {
            trigger.show();
        }
    });

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

    // Hide any Banner on ESC press
    $(document).keyup(function (e) {
        if (e.keyCode == 27) {
            $("#bannerMsg .close").click();
        }
    });

});