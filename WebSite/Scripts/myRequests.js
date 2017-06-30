$(document).ready(function () {
    $("#sidebar-wrapper #myRequestsNavLink").addClass("active");

    $(".viewExecutions").click(function () {
        $("#executions").load("/MyRequests/ViewExecutions/", { searchRequestID: $(this).find(".sRequestID").text() });
    });
});