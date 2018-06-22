﻿/*!
charts.js
(c) 2018 IG PROG, www.igprog.hr
*/
angular.module('charts', [])

.factory('charts', [function () {
    return {
        'createGraph': function (s, d, l, c, dso, legend) {
            return {
                series: s,
                data: d,
                labels: l,
                colors: c,
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    legend: {
                        display: legend
                    },
                    scales: {
                        xAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                            },
                            ticks: {
                                beginAtZero: true,
                                stepSize: 10
                            }
                        }],
                        yAxes: [{
                            display: true,
                            scaleLabel: {
                                display: true,
                            },
                            ticks: {
                                beginAtZero: true,
                                stepSize: 10
                            }
                        }]
                    }
                },
                datasetOverride: dso
            }
        },
        'guageChart': function (id, value, unit, options) {
            if (google.visualization === undefined || document.getElementById(id) == null) { return false; }
            var data = google.visualization.arrayToDataTable([
                  ['Label', 'Value'],
                  [unit, 80]
            ]);
            data.setValue(0, 1, value);
            var chart = new google.visualization.Gauge(document.getElementById(id));
            chart.draw(data, options);
        }
    }
}]);

;
