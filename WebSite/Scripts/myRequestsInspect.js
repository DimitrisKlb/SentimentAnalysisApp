$(document).ready(function () {
    $("#sidebar-wrapper #myRequestsNavLink").addClass("active");

    /****************************** Google Charts ******************************/

    try {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(initCharts);
    } catch (e) {
        $(".chart-error").show();
    }


    // Returns the 3 scores from a trigger row
    function getValuesFromRow(theExecRow) {
        var pos = theExecRow.find(".posScore").text();
        var neg = theExecRow.find(".negScore").text();
        return [pos, neg, 100 - pos - neg];
    }
    // Returns the date from a trigger row, as [yyyy, mm, dd]
    function getDateFromRow(theExecRow) {
        var date = theExecRow.find(".theDate").text();
        return [
            Number(date.slice(0, 4)),
            Number(date.slice(5, 7)),
            Number(date.slice(8, 10))
        ];
    }

    function updateDataDonut(data, theExecRow) {
        var newValues = getValuesFromRow(theExecRow);
        data.setCell(0, 1, newValues[0]);
        data.setCell(1, 1, newValues[1]);
        data.setCell(2, 1, newValues[2]);
        return data;
    }

    var theLabels = ["Positive", "Negative", "Neutral"];
    var theColors = ["rgb(51, 153, 51)", "rgb(128, 0, 0)", "rgb(230, 230, 0)"];
    var dataGraph, dataDonut;

    // Initialize and draw the 2 charts
    function initCharts() {
        var donutErrorTrigger = $("#chart-basic-container .chart-error");
        var graphErrorTrigger = $("#chart-graph-container .chart-error");

        try {
            dataDonut = initialiseDonutData();
            drawDonut(dataDonut);
        } catch (err) {
            donutErrorTrigger.show();
        }

        try {
            dataGraph = initialiseGraphData();
            drawGraph(dataGraph, 500);
        } catch (err) {
            graphErrorTrigger.show();
        }

        // Redraw charts on window resize
        $(window).resize(function () {
            try {
                drawDonut(dataDonut);
            } catch (err) {
                donutErrorTrigger.show();
            }

            try {
                drawGraph(dataGraph, 100);
            } catch (err) {
                graphErrorTrigger.show();
            }
        });

        // Table rows act as trigger to update the donut chart
        $(".update-chart-basic").click(function () {
            try {
                $(".update-chart-basic").removeClass("selectedExec");
                $(this).addClass("selectedExec");
                var newData = updateDataDonut(dataDonut, $(this));
                drawDonut(newData);
            } catch (err) {
                donutErrorTrigger.show();
            }
        });

    }

    /**************** Donut Chart ***************/

    function initialiseDonutData() {
        // Get the row corresponding to the latest Execution
        var latestExecRow = $(".update-chart-basic:first-of-type");
        latestExecRow.addClass("selectedExec");

        // Initial Donut Data
        var initialValues = getValuesFromRow(latestExecRow);
        var data = google.visualization.arrayToDataTable([
          ["Sentiment Type", "Percentage"],
          [theLabels[0], 50],
          [theLabels[1], 50],
          [theLabels[2], 10]
        ]);
        return updateDataDonut(data, latestExecRow);
    }

    function drawDonut(data) {
        var options = {
            colors: theColors,
            fontName: "Roboto",
            legend: {
                position: "bottom"
            },
            animation: {
                startup: true,
                duration: 500
            },
            pieHole: 0.4,
            pieSliceTextStyle: {
                fontSize: 15
            },
            tooltip: {
                ignoreBounds: true,
                text: "percentage",
                trigger: "selection"
            }
        };

        var chart = new google.visualization.PieChart(document.getElementById("chart-basic"));
        chart.draw(data, options);
    }

    /**************** Line Chart ***************/

    function initialiseGraphData() {
        var data = new google.visualization.DataTable();
        data.addColumn('date', 'Year');
        data.addColumn('number', 'Positive');
        data.addColumn('number', 'Negative');

        $(".update-chart-basic").each(function () {
            var values = getValuesFromRow($(this));
            var dateData = getDateFromRow($(this));
            var date = new Date(dateData[0], dateData[1], dateData[2]);
            data.addRow([date, values[0] / 100, values[1] / 100]);
        });

        return data;
    }

    function drawGraph(data, animDuration) {

        var options = {
            title: "Progression over Time",
            titleTextStyle: {
                fontSize: 18,
                bold: false
            },
            legend: { position: "bottom" },
            animation: {
                startup: true,
                duration: animDuration
            },
            axisTitlesPosition: "out",
            colors: [theColors[0], theColors[1]],
            curveType: "none",
            fontName: "Roboto",
            hAxis: {
                gridlines: {
                    count: 10
                },
                maxTextLines: 8,
                title: "Date",
                titleTextStyle: {
                    fontSize: 16
                }
            },
            pointsVisible: true,
            pointSize: 12,
            tooltip: {
                ignoreBounds: true,
                trigger: "selection"
            },
            vAxis: {
                format: "percent",
                gridlines: {
                    count: 6
                },
                title: "Sentiment Percentage (%)",
                titleTextStyle: {
                    fontSize: 16
                }
            }
        };
        var chart = new google.visualization.LineChart(document.getElementById("chart-graph"));
        chart.draw(data, options);
    }

});