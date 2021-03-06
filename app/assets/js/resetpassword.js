﻿/*!
resetpassword.js
(c) 2021 IG PROG, www.igprog.hr
*/
angular.module('app', ['pascalprecht.translate'])

.config(['$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($translateProvider, $translatePartialLoaderProvider, $httpProvider) {
    $translateProvider.useLoader('$translatePartialLoader', {
        urlTemplate: './assets/json/translations/{lang}/{part}.json'
    });
    $translateProvider.preferredLanguage('en');
    $translatePartialLoaderProvider.addPart('main');
    $translateProvider.useSanitizeValueStrategy('escape');

    //*******************disable catche**********************
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************

}])

.factory('F', ['$http', function ($http) {
    return {
        post: function (service, webmethod, data) {
            return $http({
                url: '../' + service + '.asmx/' + webmethod, method: 'POST', data: data
            }).then(function (response) {
                return JSON.parse(response.data.d);
            }, function (response) {
                alert(response.data);
                return response.data.d;
            });
        },
        isNullOrEmpty: function (x) {
            var res = false;
            if (x === '' || x == undefined || x == null) {
                res = true;
            }
            return res;
        },
    }
}])

.controller('appCtrl', ['$scope', '$http', '$translate', '$translatePartialLoader', 'F', function ($scope, $http, $translate, $translatePartialLoader, F) {
    var webService = 'Users';
    var config = null;
    var lang = null;
    var uid = null;
    var queryString = null;
    $scope.user = null;
    $scope.resp = null;
    $scope.d = {
        userName: null,
        password: null,
        passwordConfirm: null,
        alertMsg: null
    }

    queryString = location.search.split('&');
    if (queryString.length >= 1) {
        if (queryString[0].substring(1, 4) === 'uid') {
            uid = queryString[0].substring(5);
            $http.get('./config/config.json').then(function (response) {
                config = response.data;
                F.post(webService, 'Get', { userId: uid }).then(function (d) {
                    $scope.user = d;
                });
            });
        }
        if (queryString.length === 2) {
            if (queryString[1].substring(0, 4) === 'lang') {
                lang = queryString[1].substring(5);
                $translate.use(lang);
                $translatePartialLoader.addPart('main');
            }
        }
    }

    $scope.save = function (x) {
        $scope.d.alertMsg = null;
        if (F.isNullOrEmpty(x.userName) || F.isNullOrEmpty(x.password) || F.isNullOrEmpty(x.passwordConfirm)) {
            $scope.d.alertMsg = $translate.instant('all fields are required');
            return;
        }
        if (x.userName !== $scope.user.userName) {
            $scope.d.alertMsg = $translate.instant('wrong user name');
            return;
        }
        if (x.password !== x.passwordConfirm) {
            $scope.d.alertMsg = $translate.instant('passwords are not the same');
            return;
        }
        F.post(webService, 'ResetPassword', { uid: uid, newPasword: x.password }).then(function (d) {
            $scope.d.alertMsg = $translate.instant(d);
            $scope.resp = d;
        });
    }


}])

.directive('passwordStrength', [
    function () {
        return {
            require: 'ngModel',
            restrict: 'E',
            scope: {
                password: '=ngModel'
            },
            link: function (scope, elem, attrs, ctrl) {
                scope.$watch('password', function (newVal) {
                    scope.strength = isSatisfied(newVal && newVal.length >= 8) +
                      isSatisfied(newVal && /[A-z]/.test(newVal)) +
                      isSatisfied(newVal && /(?=.*\W)/.test(newVal)) +
                      isSatisfied(newVal && /\d/.test(newVal));
                    function isSatisfied(criteria) {
                        return criteria ? 1 : 0;
                    }
                }, true);
            },
            template: '<div class="progress">' +
              '<div ng-if="strength >= 1 && strength < 2" class="progress-bar bg-danger" style="width:25%">{{"low" | translate}}</div>' +
              '<div ng-if="strength >= 2 && strength < 3" class="progress-bar bg-warning" style="width:50%">{{"medium" | translate}}</div>' +
              '<div ng-if="strength >= 3 && strength < 4" class="progress-bar bg-warning" style="width:75%">{{"good" | translate}}</div>' +
              '<div ng-if="strength >= 4" class="progress-bar bg-success" style="width:100%">{{"high" | translate}}</div>' +
              '</div>'
        }
    }
])

;



