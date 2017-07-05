$(document).ready(function () {
    $("#sidebar-wrapper #myRequestsNavLink").addClass("active");

    $(".clickable-row").click(function () {
        window.location = $(this).data("href");
    });

    /******************** Datatables ********************/

    // Transform the sortableTables to Datatables
    var nonSortableCols = [];
    $(".sortableTable th").each(function (index) {
        if ($(this).hasClass("noSort")) {
            nonSortableCols.push(index);
        }
    });
    
    $(".sortableTable").dataTable({
        "paging": false,
        "bPaginate": false,
        "bAutoWidth": false,
        "bInfo": false,
        "aoColumnDefs": [
            { "bSortable": false, "aTargets": nonSortableCols }
        ],
        "oLanguage": {
            "sSearch": "<i class='fa fa-search'></i>"
        }
    });

});