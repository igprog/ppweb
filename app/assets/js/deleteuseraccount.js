/*!
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

.controller('appCtrl', ['$scope', '$http', 'F', '$translate', '$translatePartialLoader', function ($scope, $http, F, $translate, $translatePartialLoader) {
    var webService = 'Users';
    var config = null;
    var lang = null;
    $scope.uid = null;
    var queryString = null;
    $scope.user = null;
    $scope.errorMesage = false;
    $scope.d = {
        userName: null,
        password: null
    }
    $scope.response = {
        isSuccess: false,
        msg: null
    };

    queryString = location.search.split('&');
    if (queryString.length >= 1) {
        if (queryString[0].substring(1, 4) === 'uid') {
            $scope.uid = queryString[0].substring(5);
            $http.get('./config/config.json').then(function (response) {
                config = response.data;
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

    $scope.login = function (d) {
        $scope.showErrorAlert = false;
        if (F.isNullOrEmpty(d.userName) || F.isNullOrEmpty(d.password)) {
            $scope.showErrorAlert = true;
            $scope.errorMesage = $translate.instant('all fields are required');
            return;
        }
        F.post(webService, 'Login', { userName: d.userName, password: d.password }).then(function (d) {
            var user = d;
            if (user.userId !== null) {
                if (user.userId !== $scope.uid) {
                    $scope.showErrorAlert = true;
                    $scope.errorMesage = $translate.instant('wrong user');
                } else if (user.userId !== user.userGroupId) {
                    F.alert($translate.instant('you do not have permission to delete this user account'), '');
                } else {
                    $scope.showErrorAlert = false;
                    $scope.user = user;
                }
            } else {
                $scope.showErrorAlert = true;
                $scope.errorMesage = $translate.instant('wrong user name or password');
            }
        });
    }

    var remove = function (user) {
        if (user.userId !== user.userGroupId) {
            F.alert($translate.instant('you do not have permission to delete this account'), '');
            return;
        } 
        F.post(webService, 'DeleteAllUserGroup', { x: user }).then(function (d) {
            $scope.response = d;
        });
    }

    $scope.confirm = function (user) {
        if (F.isNullOrEmpty(user.userId)) {
            F.alert('choose user', '');
            return;
        }
        if (confirm($translate.instant('delete') + " " + user.firstName + " " + user.lastName + "?")) {
            remove(user);
        }
    }

}])

;



