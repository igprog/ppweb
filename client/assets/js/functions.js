﻿/*!
functions.js
(c) 2017-2021 IG PROG, www.igprog.hr
*/
angular.module('functions', [])

.factory('functions', ['$rootScope', '$window', '$translate', '$sessionStorage', '$http', function ($rootScope, $window, $translate, $sessionStorage, $http) {
    return {
        isNullOrEmpty: function (x) {
            var res = false;
            if (x === '' || x == undefined || x == null) {
                res = true;
            }
            return res;
        },
        getDateDiff: function (x) {
            var today = new Date();
            var date1 = new Date(x);
            var diffDays = Math.abs(parseInt((today - date1) / (1000 * 60 * 60 * 24)));
            return diffDays;
        },
        getTwoDateDiff: function (x, y) {
            var date1 = new Date(x);
            var date2 = new Date(y);
            var diffDays = Math.abs(parseInt((date2 - date1) / (1000 * 60 * 60 * 24)));
            return diffDays;
        },
        dateToString: function (x) {
            var day = x.getDate();
            day = day < 10 ? '0' + day : day;
            var mo = x.getMonth();
            mo = mo + 1 < 10 ? '0' + (mo + 1) : mo + 1;
            var yr = x.getFullYear();
            return yr + '-' + mo + '-' + day;
        },
        correctDate: function (date) {
            var offset = date.getTimezoneOffset() / 60;
            var diff = offset < 0 ? $sessionStorage.config.serverhostgreenwichtimediff + Math.abs(offset) : $sessionStorage.config.serverhostgreenwichtimediff - offset;
            date.setHours(date.getHours() + diff);
            return date;
        },
        post: function (service, webmethod, data) {
            return $http({
                url: '../' + service + '.asmx/' + webmethod, method: 'POST', data: data
            }).then(function (response) {
                return JSON.parse(response.data.d);
            }, function (response) {
                return response.data.d;
            });
        }
    }
}]);

;
