﻿/*!
functions.js
(c) 2017-2019 IG PROG, www.igprog.hr
*/
angular.module('functions', [])

.factory('functions', ['$rootScope', '$window', '$translate', '$sessionStorage', function ($rootScope, $window, $translate, $sessionStorage) {
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
        correctDate: function (date) {
            var offset = date.getTimezoneOffset() / 60;  // Offset from Greenwich
            var diff = offset < 0 ? $sessionStorage.config.serverhosttimediff + Math.abs(offset) : $sessionStorage.config.serverhosttimediff - offset;
            date.setHours(date.getHours() + diff);
            return date;
        }
    }
}]);

;
