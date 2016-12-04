$(document).ready(function () {
    $(".viewTextResults").click(function () { 
        $("#textResults").load("/Home/ViewMinedTexts/", { searchRequestID: $(this).find(".sRequestID").text() });
    });
});