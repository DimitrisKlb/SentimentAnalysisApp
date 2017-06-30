$(document).ready(function () {

    /********** Setup Navbar Functionality **********/
    var theNavbar = "#sidebar-wrapper";
    var homeNavLinks = $(theNavbar + " .homeNavLink a");
    var safeOffset = 5; //Pixels added to scroll position

    //Navbar auto-detection of active section, according to scroll (bootstrap scrollspy)

    //Remove url apart from hash, so that scrollspy works correctly
    homeNavLinks.each(function () {
        $(this).attr("href", this.hash);
    });
    $("body").scrollspy({
        target: theNavbar,
        offset: safeOffset
    });

    //Correct the scroll position on page load-reload
    //Also allows prevention of annoying behavior-bug of
    //Mozilla 's <window.location.hash = hash;>
    function correctScroll() {
        var theHash = window.location.hash;
        var target = $(theHash).offset().top + safeOffset;
        window.scrollTo(0, target);
    }

    $(window).on("load", function(){
        setTimeout(correctScroll, 50);
    });

    //Scrolling to the correct section
    homeNavLinks.click(function (event) {
        event.preventDefault();
        var hash = this.hash;
        var target = $(hash).offset().top + safeOffset;
        $("html, body").stop().animate(
            { scrollTop: target }, 1000, function () {
                window.location.hash = hash;
                correctScroll();
			}
		);
    });

});