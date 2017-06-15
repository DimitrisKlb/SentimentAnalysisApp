$(document).ready(function () {
    $(".viewExecutions").click(function () {
        $("#executions").load("/MyRequests/ViewExecutions/", { searchRequestID: $(this).find(".sRequestID").text() });
    });
});