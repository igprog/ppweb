﻿/*!
app.js
(c) 2017-2021 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'pascalprecht.translate', 'ngMaterial', 'chart.js', 'ngStorage', 'functions', 'charts'])

.config(['$stateProvider', '$urlRouterProvider', '$translateProvider', '$translatePartialLoaderProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $translatePartialLoaderProvider, $httpProvider) {

    $stateProvider
        .state('login', {
            url: '/login', templateUrl: './assets/partials/login.html', controller: 'loginCtrl'
        })
        .state('signup', {
            url: '/signup', templateUrl: './assets/partials/signup.html', controller: 'signupCtrl'
        })
        .state('user', {
            url: '/user', templateUrl: './assets/partials/user.html', controller: 'userCtrl'
        })
        .state('users', {
            url: '/users', templateUrl: './assets/partials/users.html', controller: 'userCtrl'
        })
        .state('newuser', {
            url: '/newuser', templateUrl: './assets/partials/newuser.html', controller: 'userCtrl'
        })
        .state('order', {
            url: '/order', templateUrl: './assets/partials/order.html', controller: 'orderCtrl'
        })
        .state('dashboard', {
            url: '/dashboard', templateUrl: './assets/partials/dashboard.html', controller: 'dashboardCtrl'
        })
        .state('clientsdata', {
            url: '/clientsdata', templateUrl: './assets/partials/clientsdata.html', controller: 'clientsCtrl'
        })
        .state('calculation', {
            url: '/calculation', templateUrl: './assets/partials/calculation.html', controller: 'calculationCtrl'
        })
        .state('activities', {
            url: '/activities', templateUrl: './assets/partials/activities.html', controller: 'activitiesCtrl'
        })
        .state('diets', {
            url: '/diets', templateUrl: './assets/partials/diets.html', controller: 'dietsCtrl'
        })
        .state('meals', {
            url: '/meals', templateUrl: './assets/partials/meals.html', controller: 'mealsCtrl'
        })
        .state('menu', {
            url: '/menu', templateUrl: './assets/partials/menu.html', controller: 'menuCtrl'
        })
        .state('analysis', {
            url: '/analysis', templateUrl: './assets/partials/analysis.html', controller: 'menuCtrl'
        })
        .state('myfoods', {
            url: '/myfoods', templateUrl: './assets/partials/myfoods.html', controller: 'myFoodsCtrl'
        })
        .state('myrecipes', {
            url: '/myrecipes', templateUrl: './assets/partials/myrecipes.html', controller: 'myRecipesCtrl'
        })
        .state('prices', {
            url: '/prices', templateUrl: './assets/partials/prices.html', controller: 'pricesCtrl'
        })
        .state('scheduler', {
            url: '/scheduler', templateUrl: './assets/partials/scheduler.html', controller: 'schedulerCtrl'
        })
        .state('clientapp', {
            url: '/clientapp', templateUrl: './assets/partials/clientapp.html', controller: 'clientAppCtrl'
        })
        .state('info', {
            url: '/info', templateUrl: './assets/partials/info.html'
        })
        .state('loginas', {
            url: '/loginas', templateUrl: './assets/partials/loginas.html', controller: 'loginCtrl'
        })
        .state('mydiets', {
            url: '/mydiets', templateUrl: './assets/partials/mydiets.html', controller: 'myDietsCtrl'
        })

    $urlRouterProvider.otherwise("/");

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

.config(function ($mdDateLocaleProvider) {
    $mdDateLocaleProvider.formatDate = function (date) {
        return moment(date).format("DD.MM.YYYY");
    }
})

.run(function ($window, $rootScope) {
    $rootScope.online = navigator.onLine;

    $window.addEventListener("offline", function () {
        $rootScope.$apply(function () {
            $rootScope.online = false;
        });
    }, false);
    $window.addEventListener("online", function () {
        $rootScope.$apply(function () {
            $rootScope.online = true;
        });
    }, false);
})

.controller('AppCtrl', ['$scope', '$mdDialog', '$timeout', '$q', '$log', '$rootScope', '$localStorage', '$sessionStorage', '$window', '$http', '$translate', '$translatePartialLoader', 'functions', '$state', function ($scope, $mdDialog, $timeout, $q, $log, $rootScope, $localStorage, $sessionStorage, $window, $http, $translate, $translatePartialLoader, functions, $state) {
    $rootScope.loginUser = $sessionStorage.loginuser;
    $rootScope.user = $sessionStorage.user;
    var queryLang = null;

    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    $scope.today = new Date();
    $rootScope.unitSystem = 1;

    if ((navigator.userAgent.indexOf("MSIE") !== -1) || (!!document.documentMode === true)) {
        $rootScope.browserMsg = {
            title: 'you are currently using internet explorer. some functionality may not work properly',
            description: 'for a better experience in using the application, please use some of the modern browsers such as google chrome, mozilla firefox, microsoft edge etc.'
        };
    }

    /***** Check internet speed  *****/
    $rootScope.connection = null;
    var connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
    if (connection) {
        $rootScope.connection = connection;
    }
    /***** Check internet speed  *****/

    if (angular.isDefined($sessionStorage.user)) {
        if ($sessionStorage.user !== null) {
            if ($sessionStorage.user.licenceStatus === 'demo') {
                $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled') + '.';
                $rootScope.mainMessageBtn = $translate.instant('activate full version');
            }
        }
    }

    var getAppAlert = function () {
        functions.post('Files', 'LoadSettings', {}).then(function (d) {
            $scope.appAlert = d.appAlert;
        });
    }
    getAppAlert();

    $rootScope.initMyCalculation = function () {
        functions.post('Calculations', 'Init', { userType: 2 }).then(function (d) {
            $rootScope.myCalculation = d;
            $rootScope.myCalculation.recommendedEnergyIntake = null;
            $rootScope.myCalculation.recommendedEnergyExpenditure = null;
        });
    }

    var getConfig = function () {
        if (location.href.includes('?')) {
            var queryStr = location.href.split('?')[1];
            if (queryStr.substring(0, 4) === 'lang') {
                queryLang = queryStr.substring(5);
            }
        }
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              $sessionStorage.config = response.data;
              if (localStorage.language) {
                  $rootScope.setLanguage(localStorage.language);
              } else {
                  $rootScope.setLanguage($rootScope.config.language);
              }
              if (queryLang !== undefined) {
                  if (queryLang === 'hr' || queryLang === 'ba' || queryLang === 'sr' || queryLang === 'sr_cyrl' || queryLang === 'en') {
                      $rootScope.setLanguage(queryLang);
                  }
              }
              if ($sessionStorage.islogin == true) { $rootScope.loadData(); }
              if (angular.isUndefined($rootScope.myCalculation)) { $rootScope.initMyCalculation() };

              if (localStorage.ppwversion) {
                  if (localStorage.ppwversion !== $rootScope.config.version) {
                      $timeout(function () {
                          openNotificationPopup();
                      }, 600);
                  }
              } else {
                  $timeout(function () {
                      openNotificationPopup();
                  }, 600);
              }
          });
    };
    getConfig();

    $scope.currLanguageTitle = null
    var getLanguageTitle = function (x) {
        if ($scope.config !== undefined) {
            $scope.currLanguageTitle = $rootScope.config.languages.find(a => a.code === x).title;
        }
    }

    $rootScope.setLanguage = function (x) {
        $translate.use(x);
        $translatePartialLoader.addPart('main');
        $rootScope.config.language = x;
        if (typeof (Storage) !== "undefined") {
            localStorage.language = x;
        }
        $sessionStorage.config.language = x;
        if ($sessionStorage.usergroupid != undefined || $sessionStorage.usergroupid != null) {
            $rootScope.loadData();
        }
        getLanguageTitle(x);

        /***** Set currency *****/
        $rootScope.config.currency = !functions.isNullOrEmpty(localStorage.currency) ? localStorage.currency : $rootScope.config.languages.find(a => a.code === x).currency.code;
    };

    $rootScope.loadFoods = function () {
        $rootScope.loading = true;
        functions.post('Foods', 'Load', { userId: $sessionStorage.usergroupid, userType: $rootScope.user.userType, lang: $rootScope.config.language }).then(function (d) {
            var data = d;
            $rootScope.foods = data.foods.concat(data.myFoods);
            $rootScope.myFoods = data.myFoods;
            $rootScope.foodGroups = data.foodGroups;
            $rootScope.loading = false;
        });
    };

    $rootScope.loadPals = function () {
        functions.post('Calculations', 'LoadPal', {}).then(function (d) {
            $rootScope.pals = d;
        });
    };

    $rootScope.loadGoals = function () {
        functions.post('Goals', 'Load', {}).then(function (d) {
            $rootScope.goals = d;
        });
    };

    $rootScope.loadActivities = function () {
        $rootScope.loading = true;
        functions.post('Activities', 'Load', { lang: $rootScope.config.language }).then(function (d) {
            $rootScope.activities = d;
            angular.forEach($rootScope.activities, function (value, key) {
                $rootScope.activities[key].activity = $translate.instant($rootScope.activities[key].activity).replace('&gt;', '<').replace('&lt;', '>');
            })
            $rootScope.loading = false;
        });
    };

    $rootScope.loadDiets = function () {
        functions.post('Diets', 'Load', { lang: $rootScope.config.language, userId: $rootScope.user.userGroupId }).then(function (d) {
            $rootScope.diets = d;
            //angular.forEach($rootScope.diets, function (value, key) {
            //    $rootScope.diets[key].diet = $translate.instant($rootScope.diets[key].diet).replace('&gt;', '<').replace('&lt;', '>');;
            //    $rootScope.diets[key].dietDescription = $translate.instant($rootScope.diets[key].dietDescription).replace('&gt;', '<').replace('&lt;', '>');
            //    $rootScope.diets[key].note = $translate.instant($rootScope.diets[key].note).replace('&gt;', '<').replace('&lt;', '>');
            //})
        });
    };

    $rootScope.getActiveEvents = function () {
        $scope.activeEvents = [];
        if ($rootScope.user.userType == 0) { return false; }
        var now = new Date().getTime();
        functions.post('Scheduler', 'GetActiveEvents', { user: $rootScope.user, now: now }).then(function (d) {
            $scope.activeEvents = d;
        });
    };

    $rootScope.loadData = function () {
        if ($sessionStorage.user == null) {
            $scope.toggleTpl('login');
            $rootScope.isLogin = false;
        } else {
            $rootScope.loadFoods();
            $rootScope.loadPals();
            $rootScope.loadGoals();
            $rootScope.loadActivities();
            $rootScope.loadDiets();
            $rootScope.getActiveEvents();
        }
    }

    $scope.toggleTpl = function (x) {
        $state.go(x);
    };

    var checkUser = function () {
        if ($sessionStorage.userid == "" || $sessionStorage.userid == undefined || $sessionStorage.user == null || $sessionStorage.user.licenceStatus == 'expired') {
            $scope.toggleTpl('login');
            $rootScope.isLogin = false;
        } else {
            $scope.toggleTpl('dashboard');
            $scope.activeTab = 0;
            $rootScope.isLogin = true;
        }
    }
    checkUser();

    var validateForm = function () {
        if ($rootScope.clientData.clientId === null) {
            return false;
        }
        if ($rootScope.clientData.height <= 0) {
            functions.alert($translate.instant('height is required'));
            return false;
        }
        if ($rootScope.clientData.weight <= 0) {
            functions.alert($translate.instant('weight is required'));
            return false;
        }
        if ($rootScope.clientData.pal.value <= 0) {
            functions.alert($translate.instant('choose physical activity level'));
            return false;
        }
        return true;
    }

    $scope.toggleNewTpl = function (x) {
        if ($rootScope.clientData !== undefined) {
            if ($rootScope.clientData.length === 0 && x === 'clientsdata') {
                $state.go(x);
                $rootScope.selectedNavItem = x;
                return;
            }
            if (x !== 'clientsdata') {
                if (validateForm() == false) {
                    return;
                };
            }
            if ($rootScope.clientData.meals == null) {
                $state.go('meals');
                $rootScope.selectedNavItem = 'meals';
                functions.alert($translate.instant('choose meals'), '');
                return;
            }
            if (x == 'menu' && $rootScope.clientData.meals.length > 0 && !$rootScope.isMyMeals && $rootScope.clientData.meals[0].code == 'B') {
                if ($rootScope.clientData.meals[1].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                    $state.go('meals');
                    functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                    return;
                }
                if ($rootScope.clientData.meals[3].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                    $state.go('meals');
                    functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                    return;
                }
            }
            if (x == 'menu') {
                if ($rootScope.myMeals !== undefined) {
                    $rootScope.setMealCode();
                }
            }
            if (x !== 'clientsdata') {
                $rootScope.saveClientData($rootScope.clientData);
            }
        } else {
            if (x !== 'clientsdata') {
                functions.alert($translate.instant('choose client'), '');
                return;
            }
        }
        $state.go(x);
        $rootScope.selectedNavItem = x;
    };
    if ($sessionStorage.islogin) {
        if ($rootScope.user.licenceStatus === 'expired') {
            $state.go('login');
        } else {
            $scope.toggleNewTpl('clientsdata');
        }
    }

    $scope.logout = function () {
        $sessionStorage.loginuser = null;
        $sessionStorage.user = null;
        $rootScope.user = null;
        $sessionStorage.userid = "";
        $sessionStorage.username = "";
        $rootScope.isLogin = false;
        $rootScope.client = null;
        $rootScope.isLogin = false;
        $sessionStorage.islogin = false;
        $sessionStorage.usergroupid = null;
        $rootScope.mainMessage = null;
        $state.go('login');
    }

    $rootScope.saveClientData = function (x) {
        if (validateForm() == false) {
            return;
        };
        if ($rootScope.clientData.meals == null) {
            $state.go('meals');
            $rootScope.selectedNavItem = 'meals';
            functions.alert($translate.instant('choose meals'), '');
            return;
        }
        if ($rootScope.clientData.meals.length > 0 && !$rootScope.isMyMeals && $rootScope.clientData.meals[0].code == 'B') {
            if ($rootScope.clientData.meals[1].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                $state.go('meals');
                functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                return;
            }
            if ($rootScope.clientData.meals[3].isSelected == false && $rootScope.clientData.meals[5].isSelected == true) {
                $state.go('meals');
                functions.alert($translate.instant('the selected meal combination is not allowed in the menu') + '!', $rootScope.clientData.meals[5].title + ' ' + $translate.instant('in this combination must be turned off') + '.');
                return;
            }
        }
        saveClientData(x);
    }

    var saveClientData = function (x) {
        if (x.hip == '') { x.hip = 0; }
        if (x.waist == '') { x.waist = 0; }
        if (x.bodyFat.bodyFatPerc == '') { x.bodyFat.bodyFatPerc = 0; }
        x.userId = $rootScope.user.userId;
        x.clientId = x.clientId == null ? $rootScope.client.clientId : x.clientId;
        x.date = functions.dateToString(x.date);

        /***** Remove desc from meals *****/
        if (x.meals.length > 0) {
            x.meals.map(a => a.description = null);
        }
        /***** Remove desc from meals *****/

        functions.post('ClientsData', 'Save', { userId: $sessionStorage.usergroupid, x: x, userType: $rootScope.user.userType }).then(function (d) {
            $rootScope.clientData.date = new Date($rootScope.clientData.date);
        });
    }

    $scope.hideMsg = function () {
        $rootScope.mainMessage = null;
    }

    $scope.currMealTitle = null;
    $rootScope.getMealTitle = function (x) {
        var title = null;
        if (x.code === 'B') { title = $translate.instant('breakfast'); }
        else if (x.code === 'MS') { title = $translate.instant('morning snack'); }
        else if (x.code === 'L') { title = $translate.instant('lunch'); }
        else if (x.code === 'AS') { title = $translate.instant('afternoon snack'); }
        else if (x.code === 'D') { title = $translate.instant('dinner'); }
        else if (x.code === 'MBS') { title = $translate.instant('meal before sleep'); }
        else title = x.title;
        if ($rootScope.currentMenu !== undefined) {
            var currMealTitle_ = $rootScope.currentMenu.data.meals.find(a => a.code === $rootScope.currentMeal);
            if (currMealTitle_ !== undefined) {
                $scope.currMealTitle = currMealTitle_.title;
            }
        }
        return title;
    }


    $scope.changeUnitSystem = function (x) {
        $rootScope.unitSystem = x;
        $rootScope.convertToStandardSystem();
    }

    $scope.standard = {
        height_feet: 0,
        height_inches: 0,
        weight: 0,
        waist: 0,
        hip: 0
    }

    $scope.convertToMetricSystem = function (x) {
        $rootScope.clientData.height = (x.height_feet * 30.48 + x.height_inches * 2.54).toFixed(1);
        $rootScope.clientData.weight = (x.weight * 0.45349237).toFixed(1);
        $rootScope.clientData.waist = (x.waist * 2.54).toFixed(1);
        $rootScope.clientData.hip = (x.hip * 2.54).toFixed(1);
        /*  Cm.Text = Format(Feet * 30.48 + Inches * 2.54, "0")   'visina cm
          Kg.Text = Format(Pounds * 0.45349237, "0")   'masa kg
          Waist_cm.Text = Format(Waist_inches * 2.54, "0")  'opseg struka cm
          Hip_cm.Text = Format(Hip_inches * 2.54, "0")  'opseg bokova cm */
    }

    $rootScope.convertToStandardSystem = function () {
        var height_inches = $rootScope.clientData.height * 0.3937;
        $scope.standard.height_feet = (parseInt(height_inches / 12)).toFixed(0);
        var rest_height_feet = (height_inches / 12) - parseInt(height_inches / 12);
        var rest_height_inches = (rest_height_feet * 12);
        $scope.standard.height_inches = (rest_height_inches).toFixed(0);
        $scope.standard.weight = ($rootScope.clientData.weight / 0.45349237).toFixed(0);
        $scope.standard.waist = ($rootScope.clientData.waist / 2.54).toFixed(0);
        $scope.standard.hip = ($rootScope.clientData.hip / 2.54).toFixed(0);
    }

    $scope.populateDdl = function (from, to) {
        var list = [];
        for (var i = from; i <= to; i++) {
            list.push(i);
        }
        return list;
    }

    $scope.showUpdates = true;
    $scope.toggleUpdates = function () {
        $scope.showUpdates = !$scope.showUpdates;
    };

    $scope.dateDiff = function () {
        if (localStorage.lastvisit) {
            return functions.getDateDiff(localStorage.lastvisit)
        } else {
            return 0;
        }
    }

    var openNotificationPopup = function () {
        $mdDialog.show({
            controller: notificationPoupCtrl,
            templateUrl: 'assets/partials/popup/notification.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: {}
        })
        .then(function (response) {
            window.location.reload(true);
        }, function () {
            window.location.reload(true);
        });
    };

    var notificationPoupCtrl = function ($scope, $rootScope, $mdDialog, $localStorage) {
        $scope.config = $rootScope.config;
        $scope.showUpdates = $scope.config.showupdatedesinitvisible;
        $scope.toggleUpdates = function () {
            $scope.showUpdates = !$scope.showUpdates;
        };

        if (typeof (Storage) !== "undefined") {
            localStorage.ppwversion = $scope.config.version;
        }

        $scope.hide = function () {
            $mdDialog.hide();
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }

    var socialSharePopup = function () {
        if (typeof (Storage) !== "undefined") {
            if (!localStorage.socailshare) {
                $timeout(function () {
                    openSocialSharePopup();
                }, 600000);
            }
        }
    }
    //socialSharePopup();

    var openSocialSharePopup = function () {
        $mdDialog.show({
            controller: socialSharePoupCtrl,
            templateUrl: 'assets/partials/popup/socialshare.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: {}
        })
        .then(function (response) {
        }, function () {
        });
    };

    var socialSharePoupCtrl = function ($scope, $rootScope, $mdDialog, $localStorage) {
        localStorage.socailshare = 'ok';

        $scope.hide = function () {
            $mdDialog.hide();
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }

    /***** Ticket *****/
    $scope.sendTicket = function () {
        openSendTicketPopup();
    }

    var openSendTicketPopup = function () {
        $mdDialog.show({
            controller: openSendTicketPopupCtrl,
            templateUrl: 'assets/partials/popup/ticket.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { user: $rootScope.user }
        })
       .then(function (x) {
       }, function () {
       });
    }

    var openSendTicketPopupCtrl = function ($scope, $mdDialog, $http, d, $translate, functions) {
        $scope.alert_des = null;
        $scope.alert_email = null;
        $scope.sending = false;
        $scope.maxLength = 4000;

        var init = function () {
            functions.post('Tickets', 'Init', {}).then(function (d) {
                $scope.d = d;
                $scope.d.user = $rootScope.user;
            });
        }
        init();

        $scope.confirm = function (x, sendMail) {
            $scope.titlealert = null;
            $scope.emailalert = null;
            $scope.alert_des = null;
            $scope.alert_email = null;
            if (functions.isNullOrEmpty(x.desc)) {
                $scope.alert_des = $translate.instant('description is required');
                return false;
            }
            if (x.desc.length > $scope.maxLength) {
                $scope.alert_des = `${$translate.instant('maximum number of characters is')} ${$scope.maxLength}`;
                return false;
            }
            if (functions.isNullOrEmpty(x.user.email)) {
                $scope.alert_email = $translate.instant('email is required');
                return false;
            }
            var body = x.desc + '. E-mail: ' + x.user.email + ', User Name: ' + x.user.userName;
            $scope.sending = true;
            functions.post('Tickets', 'Save', { x: x, sendMail: sendMail, attachFile: true, lang: $rootScope.config.language }).then(function (d) {
                functions.alert($translate.instant(d.response.msg), '');
                $mdDialog.hide();
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        /********* Ticket Image *********/
        $scope.uploadFile = function () {
            var content = new FormData(document.getElementById("formUpload"));
            $http({
                url: $sessionStorage.config.backend + '/UploadTempImgHandler.ashx',
                method: 'POST',
                headers: { 'Content-Type': undefined },
                data: content,
            }).then(function (response) {
                $scope.d.fileName = response.data;
                $scope.d.filePath = `../upload/users/${$scope.d.user.userGroupId}/temp/${$scope.d.fileName}`;
            },
           function (response) {
               alert($translate.instant(response.data));
           });
        }

        $scope.removeFile = function (x) {
            if (confirm($translate.instant('remove image') + '?')) {
                removeFile(x);
            }
        }

        var removeFile = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Files.asmx/DeleteTempFile',
                method: 'POST',
                data: { x: x },
            }).then(function (response) {
                $scope.d.fileName = response.data.d;
            },
           function (response) {
               alert($translate.instant(response.data.d));
           });
        }
        /********* Ticket Image *********/
    };
    /***** Ticket *****/

    $rootScope.foodPopupCtrl = function ($scope, $mdDialog, d, $http, $translate) {
        $scope.d = d;
        $scope.foods = d.foods;
        $scope.myFoods = d.myFoods;
        $scope.foodGroups = d.foodGroups;
        $scope.quantityAlert = null;
        $scope.massAlert = null;
        $scope.loading = false;
        var initFood = null;

        var initFoodForEdit = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/InitFoodForEdit',
                method: "POST",
                data: { food: x }
            })
            .then(function (response) {
                initFood = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var isEditMode = false;
        if (d.food == null) {
            $scope.food = null;
            initFood = null;
            isEditMode = false;
        } else {
            $scope.food = d.food;
            initFoodForEdit(d.food);
            isEditMode = true;
        }

        $scope.limit = 100;

        $scope.initCurrentFoodGroup = function () {
            $scope.currentGroup = { code: 'A', title: 'all foods' };
        }
        $scope.initCurrentFoodGroup();

        var initThermalTreatment = function () {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/InitThermalTreatment',
                method: "POST",
                data: ''
            })
            .then(function (response) {
                $scope.selectedThermalTreatment = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }
        initThermalTreatment();

        $scope.showMyFoods = function (x) {
            $scope.isShowMyFood = x;
        }

        // TOOD: translate on server side
        $scope.getFoodDetails = function (x) {
            if ($scope.isShowMyFood == true) {
                getMyFoodDetails(x);
                return false;
            }
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userId, id: JSON.parse(x).id }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
                if ($scope.food.id === null) {
                    getMyFoodDetails(x);
                } else {
                    $scope.food.food = $translate.instant($scope.food.food);
                    $scope.food.unit = $translate.instant($scope.food.unit);
                    $scope.food.foodGroup.title = $translate.instant($scope.food.foodGroup.title);
                    $scope.food.meal.title = $translate.instant($scope.food.meal.title);
                    angular.forEach($scope.food.thermalTreatments, function (value, key) {
                        $scope.food.thermalTreatments[key].thermalTreatment.title = $translate.instant($scope.food.thermalTreatments[key].thermalTreatment.title);
                    })
                    initFood = angular.copy($scope.food);
                }
                $scope.loading = false;
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var getMyFoodDetails = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Get',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, id: JSON.parse(x).id }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
                $scope.food.unit = $translate.instant($scope.food.unit);
                $scope.food.foodGroup.title = $translate.instant($scope.food.foodGroup.title);
                initFood = angular.copy($scope.food);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        var currThermalTreatmentIdx = 0;
        $scope.getThermalTreatment = function (x, idx) {
            if (functions.isNullOrEmpty(idx)) {
                idx = currThermalTreatmentIdx;
            }
            angular.forEach(x, function (value, key) {
                value.isSelected = false;
            })
            $scope.selectedThermalTreatment = x[idx];
            x[idx].isSelected = true;
            currThermalTreatmentIdx = idx;
            if (isEditMode) {
                isEditMode = false;
            } else {
                includeThermalTreatment($scope.selectedThermalTreatment);
            }
        }

        var includeThermalTreatment = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/IncludeThermalTreatment',
                method: "POST",
                data: { initFood: initFood, food: $scope.food, thermalTreatment: x }
            })
            .then(function (response) {
                $scope.food = JSON.parse(response.data.d);
            },
            function (response) {
                alert(response.data.d)
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            $scope.quantityAlert = null;
            $scope.massAlert = null;
            if (functions.isNullOrEmpty(x.quantity) || x.quantity == 0) {
                $scope.quantityAlert = $translate.instant('quantity is required');
                return;
            }
            if (functions.isNullOrEmpty(x.mass) || x.mass == 0) {
                $scope.massAlert = $translate.instant('mass is required');
                return;
            }
            var data = { food: x, initFood: initFood }
            $mdDialog.hide(data);
        }

        $scope.changeQuantity = function (x, type) {
            isEditMode = false;
            var currentFood = $scope.food.food;  // << in the case where user change food title
            $scope.food = functions.changeQuantity(initFood, x, type);
            $scope.food.food = currentFood;
        }



        $scope.change = function (x, type) {
            if (type === 'quantity' && $scope.food.quantity + x > 0) {
                $scope.food.quantity = Number($scope.food.quantity) + Number(x);
                $scope.changeQuantity($scope.food, 'quantity');
            }
            if (type === 'mass' && $scope.food.mass + x > 0) {
                $scope.food.mass = Number($scope.food.mass) + Number(x);
                $scope.changeQuantity($scope.food, 'mass');
            }
        }

        $scope.showFoodSubGroups = function (x) {
            if (x.parent == 'A') {
                $scope.currentMainGroup = x.group.code;
            }
        }

        $scope.changeFoodGroup = function (x) {
            $scope.searchFood = '';
            $scope.limit = $scope.foods.length + 1;
            if (x.code === 'MYF') {
                $scope.showMyFoods(true);
            } else {
                $scope.showMyFoods(false);
            }
            $scope.currentGroup = {
                code: x.code,
                title: x.title
            };
        }

        $scope.checkIf = function (x) {
            if (x.foodGroup.code == $scope.currentGroup.code || $scope.currentGroup.code == 'A' || $scope.isShowMyFood == true) {
                return true;
            } else {
                if ($scope.currentGroup.code == $scope.currentMainGroup) {
                    if (x.foodGroup.parent == $scope.currentGroup.code) {
                        return true;
                    }
                } else {
                    return false;
                }
            }
        }

        $scope.loadMore = function () {
            $scope.limit = $scope.limit + $scope.foods.length;
        }

        $scope.openAsMyFood = function (x) {
            var data = { food: x, initFood: initFood, openAsMyFood: true }
            $mdDialog.hide(data);
        }

    };

}])

.controller('loginCtrl', ['$scope', '$http', '$localStorage', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$mdDialog', '$state', function ($scope, $http, $localStorage, $sessionStorage, $window, $rootScope, functions, $translate, $mdDialog, $state) {
    var webService = 'Users';

    $scope.login = function (u, p) {
        $scope.errorMesage = null;
        if (functions.isNullOrEmpty(u) || functions.isNullOrEmpty(p)) {
            $scope.errorLogin = true;
            $scope.errorMesage = $translate.instant('enter user name (email) and password');
            return false;
        }
        $rootScope.loading = true;
        functions.post(webService, 'Login', { userName: u, password: p }).then(function (d) {
            if (d.userId !== null) {
                loginResponse(d);
            } else {
                $rootScope.loading = false;
                $scope.errorLogin = true;
                $scope.errorMesage = $translate.instant('wrong user name or password');
                //$state.go('signup');  //<< Only for first registration
            }
        });
    }

    var loginResponse = function (d) {
        $rootScope.user = d;
        if ($rootScope.user.userId !== $rootScope.user.userGroupId && $rootScope.user.isActive === false) {
            $rootScope.loading = false;
            $scope.errorLogin = true;
            $scope.errorMesage = $translate.instant('your account is not active') + '. ' + $translate.instant('please contact your administrator');
            return false;
        }
        $rootScope.loginUser = d;
        $sessionStorage.loginuser = $rootScope.loginUser;
        $sessionStorage.userid = $rootScope.user.userId;
        $sessionStorage.usergroupid = $rootScope.user.userGroupId;
        $sessionStorage.username = $rootScope.user.userName;
        $sessionStorage.user = $rootScope.user;
        $sessionStorage.islogin = true;
        $rootScope.isLogin = true;
        $rootScope.loadData();

        if (typeof (Storage) !== "undefined") {
            localStorage.lastvisit = new Date();
        }
        if ($rootScope.user.licenceStatus === 'expired') {
            functions.alert($translate.instant('your subscription has expired'), $translate.instant('renew subscription'));
            $state.go('dashboard');
            $state.go('order');
        } else {
            $state.go('dashboard');
            if ($rootScope.user.daysToExpite <= 10 && $rootScope.user.daysToExpite > 0) {
                $rootScope.mainMessage = $translate.instant('your subscription will expire in') + ' ' + $rootScope.user.daysToExpite + ' ' + ($rootScope.user.daysToExpite == 1 ? $translate.instant('day') : $translate.instant('days')) + '.';
                $rootScope.mainMessageBtn = $translate.instant('renew subscription');
            }
            if ($rootScope.user.daysToExpite == 0) {
                $rootScope.mainMessage = $translate.instant('your subscription will expire today') + '.';
                $rootScope.mainMessageBtn = $translate.instant('renew subscription');
            }
            if ($rootScope.user.licenceStatus == 'demo') {
                $rootScope.mainMessage = $translate.instant('you are currently working in a demo version') + '. ' + $translate.instant('some functions are disabled') + '.';
                $rootScope.mainMessageBtn = $translate.instant('activate full version');
            }
            if ($rootScope.config.language == 'en') {
                $rootScope.unitSystem = 0;
            } else {
                $rootScope.unitSystem = 1;
            }
        }

        /***** activity.log *****/
        // activityLog($rootScope.user.userId, 'LOGIN', new Date().toLocaleString());
    }

    /*
    var activityLog = function (userId, activity, dateTime) {
        if ($rootScope.config.showactivitylog) {
            functions.post('Log', 'SaveActivityLog', { userId: userId, activity: activity, dateTime: dateTime }).then(function (d) {
            });
        }
    }
    */

    /***** Login As *****/
    $scope.admin = {
        username: null,
        password: null,
        isadmin: false
    };
    $scope.adminLogin = function (x) {
        functions.post('Admin', 'Login', { username: x.username, password: x.password }).then(function (d) {
            $scope.admin.isadmin = d;
        });
    }

    $scope.loginAs = function (uid) {
        if (!functions.isNullOrEmpty(uid)) {
            functions.post(webService, 'Get', { userId: uid }).then(function (d) {
                if (d.userId !== null) {
                    loginResponse(d);
                    $rootScope.config.debug = true;
                } else {
                    $rootScope.loading = false;
                    $scope.errorLogin = true;
                    $scope.errorMesage = $translate.instant('user was not found');
                }
            });
        }
    }
    /***** Admin Login *****/

    $scope.signup = function () {
        $state.go('signup');
    }

    $scope.forgotPasswordPopup = function () {
        $mdDialog.show({
            controller: $scope.forgotPasswordPopupCtrl,
            templateUrl: 'assets/partials/popup/forgotpassword.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen,
            d: ''
        })
        .then(function (response) {
        }, function () {
        });
    };

    $scope.forgotPasswordPopupCtrl = function ($scope, $mdDialog, $http, $translate) {
        $scope.confirm = function (x) {
            forgotPassword(x);
        }

        $scope.sending = false;
        var forgotPassword = function (x) {
            $scope.sending = true;
            functions.post(webService, 'ForgotPassword', { email: x, lang: $rootScope.config.language }).then(function (d) {
                $scope.sending = false;
                $mdDialog.hide();
                functions.alert(d, '');
            });
        }

        $scope.hide = function () {
            $mdDialog.hide();
        };

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    }

}])

.controller('signupCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, functions, $translate, $state) {
    var webService = 'Users';
    $scope.showAlert = false;
    $scope.passwordConfirm = '';
    $scope.emailConfirm = '';
    $scope.signupdisabled = false;
    $scope.accept = false;

    var init = function () {
        functions.post(webService, 'Init', {}).then(function (d) {
            $scope.newUser = d;
        });
    }
    init();

    $scope.signup = function () {
        $scope.signupdisabled = true;
        $scope.newUser.userName = $scope.newUser.email;
        if (functions.isNullOrEmpty($scope.newUser.firstName) || functions.isNullOrEmpty($scope.newUser.lastName) || functions.isNullOrEmpty($scope.newUser.email) || functions.isNullOrEmpty($scope.newUser.password) || functions.isNullOrEmpty($scope.passwordConfirm) || functions.isNullOrEmpty($scope.emailConfirm)) {
            functions.alert($translate.instant('all fields are required'), '');
            $scope.signupdisabled = false;
            return false;
        }
        if ($scope.newUser.email != $scope.emailConfirm) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('emails are not the same'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }
        if ($scope.accept == false) {
            $scope.signupdisabled = false;
            functions.alert($translate.instant('you must agree to the terms and conditions'), '');
            return false;
        }
        $scope.signingUp = true;
        functions.post(webService, 'Signup', { x: $scope.newUser, lang: $rootScope.config.language }).then(function (d) {
            $scope.signingUp = false;
            if (d === 'registration completed successfully') {
                $scope.alertMessage = d;
                $scope.showAlert = true;
                sendSignupMail($scope.newUser, $rootScope.config.language);
            } else {
                functions.alert($translate.instant(d), '');
            }
        });
    }

    var sendSignupMail = function (newUser, lang) {
        functions.post(webService, 'SendSignupMail', { x: newUser, lang: lang }).then(function (d) {
        });
    }

}])

.controller("schedulerCtrl", ['$scope', '$localStorage', '$http', '$rootScope', '$timeout', '$sessionStorage', '$mdDialog', 'functions', '$translate', '$window', function ($scope, $localStorage, $http, $rootScope, $timeout, $sessionStorage, $mdDialog, functions, $translate, $window) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    var webService = 'Scheduler';
    $scope.id = '#myScheduler';
    $scope.room = 0;
    $scope.uid = $rootScope.user.userId;
    $scope.loading = false;

    var showScheduler = function () {
        YUI().use('aui-scheduler', function (Y) {
            var agendaView = new Y.SchedulerAgendaView();
            var dayView = new Y.SchedulerDayView();
            var weekView = new Y.SchedulerWeekView();
            var monthView = new Y.SchedulerMonthView();
            var eventRecorder = new Y.SchedulerEventRecorder({
                on: {
                    save: function (event) {
                        addEvent(this.getTemplateData(), event);
                    },
                    edit: function (event) {
                        addEvent(this.getTemplateData(), event);
                    },
                    delete: function (event) {
                        removeEvent(this.getTemplateData(), event);
                    }
                }
            });

            $scope.id = $scope.uid == null ? 'myScheduler' : $scope.uid;

            new Y.Scheduler({
                activeView: weekView,
                boundingBox: '#' + $scope.id,
                date: new Date(),
                eventRecorder: eventRecorder,
                items: $rootScope.events,
                render: true,
                views: [dayView, weekView, monthView, agendaView],
                strings: {
                    agenda: $translate.instant('agenda'),
                    day: $translate.instant('day_'),
                    month: $translate.instant('month'),
                    table: $translate.instant('table'),
                    today: $translate.instant('today'),
                    week: $translate.instant('week'),
                    year: $translate.instant('year')
                },
            }
          );
        });
    }

    var getUsers = function () {
        functions.post('Users', 'GetUsersByUserGroup', { userGroupId: $rootScope.user.userGroupId }).then(function (d) {
            $scope.users = d;
            $scope.getSchedulerEvents($rootScope.user.userId);
        });
    };

    $scope.getSchedulerEvents = function (uid) {
        $scope.loading = true;
        functions.post(webService, 'GetSchedulerEvents', { user: $rootScope.user, room: $scope.room, uid: uid }).then(function (d) {
            $rootScope.events = d;
            $timeout(function () {
                $scope.loading = false;
                showScheduler();
            }, 200);
        });
    };
    getUsers();

    var addEvent = function (x, event) {
        $rootScope.events.push({
            room: $scope.room,
            clientId: angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null, //  null,  // << TODO
            content: event.details[0].newSchedulerEvent.changed.content,
            endDate: x.endDate,
            startDate: x.startDate,
            userId: $rootScope.user.userId
        });

        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null;
        eventObj.content = event.details[0].newSchedulerEvent.changed.content == null ? x.content : event.details[0].newSchedulerEvent.changed.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.userId = $rootScope.user.userId;

        var eventObj_old = {};
        eventObj_old.room = $scope.room;
        eventObj_old.clientId = angular.isDefined($rootScope.client) && $rootScope.client != null ? $rootScope.client.clientId : null;
        eventObj_old.content = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.content) ? x.content : event.details[0].newSchedulerEvent.lastChange.content.prevVal;
        eventObj_old.endDate = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.endDate) ? x.endDate : Date.parse(event.details[0].newSchedulerEvent.lastChange.endDate.prevVal);
        eventObj_old.startDate = angular.isUndefined(event.details[0].newSchedulerEvent.lastChange.startDate) ? x.startDate : Date.parse(event.details[0].newSchedulerEvent.lastChange.startDate.prevVal);
        eventObj_old.userId = $rootScope.user.userId;
        remove(eventObj_old);

        $timeout(function () {
            save(eventObj);
        }, 500);
    }

    var save = function (x) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if (!functions.checkPermission($rootScope.user, 'standard&premium')) return;
        functions.post(webService, 'Save', { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }).then(function (d) {
            getAppointmentsCountByUserId();
            $rootScope.getActiveEvents();
        });
    }

    var removeEvent = function (x, event) {
        var eventObj = {};
        eventObj.room = $scope.room;
        eventObj.clientId = '0';
        eventObj.content = x.content;
        eventObj.endDate = x.endDate;
        eventObj.startDate = x.startDate;
        eventObj.userId = $rootScope.user.userId;
        remove(eventObj);
    }

    var remove = function (x) {
        functions.post(webService, 'Delete', { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId, x: x }).then(function (d) {
            getAppointmentsCountByUserId();
            $rootScope.getActiveEvents();
        });
    }

    var getAppointmentsCountByUserId = function () {
        functions.post(webService, 'GetAppointmentsCountByUserId', { userGroupId: $rootScope.user.userGroupId, userId: $rootScope.user.userId }).then(function (d) {
            $rootScope.user.datasum.scheduler = d;
        });
    }

    var removeAllEvents = function () {
        functions.post(webService, 'RemoveAllEvents', { userGroupId: $rootScope.user.userGroupId }).then(function (d) {
            $scope.uid = null;
            getAppointmentsCountByUserId();
            $rootScope.getActiveEvents();
        });
    }

    $scope.removeAllEvents = function () {
        var confirm = $mdDialog.confirm()
              .title($translate.instant('remove all events') + '?')
              .ok($translate.instant('yes') + '!')
              .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            removeAllEvents();
        }, function () {
        });
    }

}])

.controller('userCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'Users';

    $scope.adminTypes = [
       {
           value: 0,
           text: 'Supervizor'
       },
       {
           value: 1,
           text: 'Admin'
       },
       {
           value: 2,
           text: 'Student'
       }
    ];

    var init = function () {
        functions.post(webService, 'Init', {}).then(function (d) {
            $scope.newUser = d;
            $scope.newUser.adminType = 1;
            load();
        });
    }

    var load = function () {
        functions.post(webService, 'GetUsersByUserGroup', { userGroupId: $sessionStorage.usergroupid }).then(function (d) {
            $scope.users = d;
        });
    };

    init();

    $scope.adminType = function (x) {
        switch (x) {
            case 0:
                return 'Supervizor';
                break;
            case 1:
                return 'Admin';
                break;
            case 2:
                return 'Student';
                break;
            default:
                return '';
        }
    }

    $scope.signup = function () {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;

        if ($scope.users.length >= $rootScope.user.maxNumberOfUsers) {
            functions.alert($translate.instant('max number of users is') + ' ' + $rootScope.user.maxNumberOfUsers, '');
            return;
        }

        if (!angular.isDefined($rootScope.user)) { $rootScope.user = $scope.newUser; }

        $scope.newUser.userName = $scope.newUser.email;
        $scope.newUser.companyName = $rootScope.user.companyName;
        $scope.newUser.address = $rootScope.user.address;
        $scope.newUser.postalCode = $rootScope.user.postalCode;
        $scope.newUser.city = $rootScope.user.city;
        $scope.newUser.country = $rootScope.user.country;
        $scope.newUser.pin = $rootScope.user.pin;
        $scope.newUser.phone = $rootScope.user.phone;
        $scope.newUser.userGroupId = $rootScope.user.userGroupId;
        $scope.newUser.expirationDate = $rootScope.user.expirationDate;
        $scope.newUser.isActive = true;
        $scope.newUser.userType = $rootScope.user.userType;

        if ($scope.newUser.password == "" || $scope.passwordConfirm == "") {
            functions.alert($translate.instant('enter password'), '');
            return false;
        }
        if ($scope.newUser.password != $scope.passwordConfirm) {
            functions.alert($translate.instant('passwords are not the same'), '');
            return false;
        }
        $scope.creatingNewUser = true;
        functions.post(webService, 'Signup', { x: $scope.newUser, lang: $rootScope.config.language }).then(function (d) {
            load();
            $scope.creatingNewUser = false;
            functions.alert($translate.instant(d));
            sendSignupMail($scope.newUser, $rootScope.config.language);
        });
    }

    var sendSignupMail = function (newUser, lang) {
        functions.post(webService, 'SendSignupMail', { x: newUser, lang: lang }).then(function (d) {
        });
    }

    $scope.update = function (user) {
        functions.post(webService, 'Update', { x: user }).then(function (d) {
            functions.alert($translate.instant(d));
        });
    }

    $scope.showUser = function (x) {
        functions.post(webService, 'Get', { userId: x }).then(function (d) {
            $rootScope.user = d;
            $state.go('user');
        });
    };

    $scope.updateUser = function (user) {
        functions.post(webService, 'Update', { x: user }).then(function (d) {
            functions.alert($translate.instant(d));
        });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
              .title($translate.instant('delete user') + '?')
              .textContent(x.firstName + ' ' + x.lastName)
              .targetEvent(x)
              .ok($translate.instant('yes') + '!')
              .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (user) {
        functions.post(webService, 'Delete', { x: user }).then(function (d) {
            load();
        });
    }

    $scope.showpass = false;
    $scope.showPassword = function () {
        $scope.showpass = $scope.showpass == true ? false : true;
    }

    /********* Logo ************/
    var isLogoExists = function () {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/IsLogoExists',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, filename: 'logo.png' },
        }).then(function (response) {
            if (response.data.d == 'TRUE') {
                $scope.showLogo = true;
            } else {
                $scope.showLogo = false;
                $scope.logo = null;
            }
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }
    isLogoExists();

    $scope.logo = '../upload/users/' + $rootScope.user.userGroupId + '/logo.png?v=' + new Date().getTime();
    $scope.upload = function () {
        if ($rootScope.user.adminType != 0) { return false; }
        var content = new FormData(document.getElementById("formUpload"));
        $http({
            url: $sessionStorage.config.backend + '/UploadLogoHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            $scope.showLogo = true;
            $scope.logo = '../upload/users/' + $rootScope.user.userGroupId + '/logo.png?v=' + new Date().getTime();
            if (response.data != 'OK') {
                functions.alert($translate.instant(response.data));
            }
            isLogoExists();
        },
       function (response) {
           functions.alert($translate.instant(response.data));
       });
    }

    $scope.removeLogo = function (x) {
        if (x.adminType != 0) { return false; }
        var confirm = $mdDialog.confirm()
                   .title($translate.instant('remove logo') + '?')
                   .targetEvent(x)
                   .ok($translate.instant('yes') + '!')
                   .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            removeLogo(x);
        }, function () {
        });
    }

    var removeLogo = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/DeleteLogo',
            method: 'POST',
            data: { userId: x.userId, filename: 'logo.png' },
        }).then(function (response) {
            $scope.showLogo = false;
            $scope.logo = null;
            if (response.data.d != 'OK') {
                functions.alert($translate.instant(response.data.d));
            }
        },
       function (response) {
           functions.alert($translate.instant(response.data.d));
       });
    }
    /********* Logo ************/

    $scope.sending = false;
    $scope.sendDeleteAccountLink = function (user) {
        $scope.sending = true;
        functions.post(webService, 'SendDeleteAccountLink', { x: user, lang: $rootScope.config.language }).then(function (d) {
            $scope.sending = false;
            functions.alert($translate.instant(d));
            $window.location.href = '../';
        });
    }

}])

//-------------- Program Prehrane Controllers---------------
.controller('dashboardCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', 'functions', '$translate', '$timeout', '$state', 'charts', function ($scope, $http, $sessionStorage, $window, $rootScope, functions, $translate, $timeout, $state, charts) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    var getUser = function () {
        if ($rootScope.user === null) { return; }
        functions.post('Users', 'Get', { userId: $rootScope.user.userId }).then(function (d) {
            $rootScope.user = d;
            $scope.expirationDate = new Date($rootScope.user.expirationDate);
        });
    }
    getUser();

    var showHelpAlert = function () {
        $timeout(function () {
            if ($rootScope.selectedNavItem === 'dashboard') {
                functions.post('Clients', 'GetClientsCount', { userId: $sessionStorage.usergroupid }).then(function (d) {
                    var count = d;
                    if (count !== 0) {
                        functions.alert($translate.instant('need help') + '?', $translate.instant('contact our technical support by email') + ': ' + $rootScope.config.email + ' ' + $translate.instant('or phone') + ': ' + $rootScope.config.phone + '.');
                        $sessionStorage.showHelpAlert = true;
                    }
                });
            }
        }, 8000);
    }

    if ($rootScope.user !== null) {
        if ($rootScope.user.licenceStatus === 'demo' && $rootScope.config.language === 'hr' && $sessionStorage.showHelpAlert === undefined) {
            showHelpAlert();
        }
    } else {
        $state.go('login');
    }

    /***** Internet speed test *****/
    $scope.loadingSpeedTest = false;
    var connectionChart = function () {
        $scope.loadingSpeedTest = true;
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            var id = 'connectionChart';
            var value = $rootScope.connection.downlink;
            var unit = 'Mbps';
            var options = {
                title: 'Mbps',
                min: 0,
                max: 10,
                greenFrom: 1.35,
                greenTo: 10,
                yellowFrom: 1,
                yellowTo: 1.35,
                redFrom: 0,
                redTo: 1,
                minorTicks: 0.1
            };
            google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
            $scope.loadingSpeedTest = false;
        }, 1500);
    }

    $scope.testInternetSpeed = function () {
        connectionChart();
    }
    /***** Internet speed test *****/

}])

.controller('clientsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', '$timeout', 'charts', '$filter', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, $timeout, charts, $filter, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;
    var webService = 'Clients.asmx';
    $scope.displayType = 0;

    $scope.toggleTpl = function (x) {
        $scope.clientDataTpl = x;
    };
    $scope.toggleTpl('inputData');

    $scope.toggleSubTpl = function (x) {
        $scope.subTpl = x;
    };

    var init = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Init',
            method: "POST",
            data: { client: x }
        })
        .then(function (response) {
            $rootScope.clientData = JSON.parse(response.data.d);
            $rootScope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
            $rootScope.clientData.date = new Date($rootScope.clientData.date);
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    var initClient = function () {
        $http({
            url: $sessionStorage.config.backend + 'Clients.asmx/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.client = JSON.parse(response.data.d);
            $rootScope.client.date = new Date(new Date().setHours(0, 0, 0, 0));
            $scope.d = $rootScope.client;
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    var getClients = function () {
        if ($sessionStorage.usergroupid === undefined) { return false; }
        $rootScope.loading = true;
        $http({
            url: $sessionStorage.config.backend + webService + '/Load',
            method: 'POST',
            data: { userId: $sessionStorage.usergroupid, user: $rootScope.user }
        })
        .then(function (response) {
            $rootScope.clients = JSON.parse(response.data.d);
            $rootScope.loading = false;
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
            $rootScope.loading = false;
        });
    };
    getClients();

    $rootScope.newClient = function () {
        $rootScope.showDetailCalculationOfEnergyExpenditure = false;
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: ""
        })
        .then(function (response) {
            $rootScope.client = JSON.parse(response.data.d);
            $rootScope.client.date = new Date(new Date().setHours(0, 0, 0, 0));
            $rootScope.client.birthDate = new Date(new Date().setHours(0, 0, 0, 0));
            $rootScope.clientData = [];
            $rootScope.calculation = [];
            $rootScope.initMyCalculation();
            $scope.d = $rootScope.client;
            $rootScope.goalWeightValue_ = null;
            $scope.openPopup();
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    $scope.openPopup = function () {
        $mdDialog.show({
            controller: $scope.popupCtrl,
            templateUrl: 'assets/partials/popup/client.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            d: $scope.d
        })
        .then(function (response) {
            $rootScope.client = response;
            $rootScope.currTpl = './assets/partials/main.html';
            $scope.toggleNewTpl('clientsdata');
            $scope.get($rootScope.client);
        }, function () {
        });
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http, $timeout) {
        $scope.d = d;
        $scope.d.date = new Date($scope.d.date);
        $scope.d.birthDate = new Date($scope.d.birthDate);
        $scope.user = $rootScope.user;

        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.firstNameRequiredMsq = null;
        $scope.firstNameRequiredMsq = null;

        $scope.save = function (x) {
            if (x.firstName == '' || x.firstName == null) {
                $scope.firstNameRequiredMsq = 'first name is required';
                return false;
            } else {
                $scope.firstNameRequiredMsq = null;
                if (functions.getDateDiff(x.birthDate) < 365) {
                    $scope.birthDateRequiredMsq = 'birth date is required';
                    return false;
                } else {
                    $scope.birthDateRequiredMsq = null;
                }
            }

            x.userId = $sessionStorage.userid;
            x.birthDate = functions.dateToString(x.birthDate);
            $http({
                url: $sessionStorage.config.backend + webService + '/Save',
                method: 'POST',
                data: { user: $rootScope.user, x: x, lang: $rootScope.config.language }
            })
           .then(function (response) {
               if (JSON.parse(response.data.d).message != null) {
                   functions.alert($translate.instant(JSON.parse(response.data.d).message), '');
                   return false;
               }
               $scope.d = JSON.parse(response.data.d).data;
           },
           function (response) {
               functions.alert($translate.instant(response.data.d), '');
           });
        }

        $scope.forward = function (d) {
            if (d.clientId !== null) {
                $mdDialog.hide(d);
            }
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                  .title($translate.instant('are you sure you want to delete') + '?')
                  .textContent($translate.instant('client') + ': ' + x.firstName + ' ' + x.lastName)
                  .targetEvent(x)
                  .ok($translate.instant('yes'))
                  .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                $http({
                    url: $sessionStorage.config.backend + webService + '/Delete',
                    method: "POST",
                    data: { userId: $sessionStorage.usergroupid, clientId: x.clientId, user: $rootScope.user }
                })
               .then(function (response) {
                   $rootScope.clients = JSON.parse(response.data.d);
                   $mdDialog.cancel();
               },
               function (response) {
                   functions.alert($translate.instant(response.data.d), '');
               });
            }, function () {
            });
        };

        /********* Profile Image *********/
        $scope.uploadImg = function () {
            var content = new FormData(document.getElementById("formUpload"));
            $http({
                url: $sessionStorage.config.backend + '/UploadProfileImgHandler.ashx',
                method: 'POST',
                headers: { 'Content-Type': undefined },
                data: content,
            }).then(function (response) {
                $scope.d.profileImg = response.data;
            },
           function (response) {
               alert($translate.instant(response.data));
           });
        }

        $scope.removeProfileImg = function (x) {
            if (confirm($translate.instant('remove image') + '?')) {
                removeProfileImg(x);
            }
        }

        var removeProfileImg = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Files.asmx/DeleteProfileImg',
                method: 'POST',
                data: { x: x },
            }).then(function (response) {
                $scope.d.profileImg = response.data.d;
            },
           function (response) {
               alert($translate.instant(response.data.d));
           });
        }
        /********* Profile Image *********/

        $scope.moreInfo = false;
        $scope.showMoreInfo = function (moreInfo) {
            $scope.moreInfo = !moreInfo;
        }

    }

    $scope.edit = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            $scope.d = JSON.parse(response.data.d);
            $scope.openPopup();
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.search = function () {
        $scope.toggleTpl('inputData');
        $scope.openSearchPopup();
    }

    $rootScope.openSearchPopup = function () {
        $mdDialog.show({
            controller: $scope.searchPopupCtrl,
            templateUrl: 'assets/partials/popup/searchclients.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true
        })
        .then(function (response) {
            $rootScope.client = response;
            $rootScope.currTpl = './assets/partials/main.html';
            $scope.toggleNewTpl('clientsdata');
            $scope.get(response);
        }, function () {
        });
    };

    $scope.searchPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.loading = false;
        var load = function () {
            $scope.loading = true;
            $http({
                url: $sessionStorage.config.backend + 'Clients.asmx/Load',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, user: $rootScope.user }
            })
            .then(function (response) {
                $scope.d = JSON.parse(response.data.d);
                $scope.loading = false;
            },
            function (response) {
                $scope.loading = false;
                alert(response.data.d);
            });
        }
        load();

        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        $scope.getDateFormat = function (x) {
            return new Date(x);
        }
        $scope.hide = function (x) {
            $mdDialog.hide(x);
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.get = function (x) {
            $scope.hide(x);
        }

        $scope.addNewClient = function () {
            $mdDialog.cancel();
            $rootScope.newClient();
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                  .title($translate.instant('are you sure you want to delete') + '?')
                  .textContent($translate.instant('client') + ': ' + x.firstName + ' ' + x.lastName)
                  .targetEvent(x)
                  .ok($translate.instant('yes'))
                  .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        };

        var remove = function (x) {
            $http({
                url: $sessionStorage.config.backend + webService + '/Delete',
                method: "POST",
                data: { userId: $sessionStorage.usergroupid, clientId: x.clientId, user: $rootScope.user }
            })
           .then(function (response) {
               $rootScope.clients = JSON.parse(response.data.d);
               $rootScope.client = [];
               $rootScope.clientData = [];
               if ($rootScope.clients.length > 0) {
                   $rootScope.openSearchPopup();
               }
           },
           function (response) {
               alert(response.data.d)
           });
        }
    }

    $scope.get = function (x) {
        $rootScope.showDetailCalculationOfEnergyExpenditure = false;
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Get',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            if (JSON.parse(response.data.d).id != null) {
                $rootScope.clientData = JSON.parse(response.data.d);
                $rootScope.clientData.date = new Date(new Date().setHours(0, 0, 0, 0));
                $scope.getPalDetails($rootScope.clientData.pal.value);
                getCalculation();
                getMyCalculation();
                if ($rootScope.clientData.dailyActivities.activities == null) {
                    $rootScope.clientData.dailyActivities.activities = [];
                }
                if ($rootScope.clientData.dailyActivities.activities.length > 0) {
                    $rootScope.showDetailCalculationOfEnergyExpenditure = true;
                }
                if ($rootScope.unitSystem == 0 && $rootScope.config.language == 'en') {
                    $rootScope.convertToStandardSystem();
                }
                $rootScope.goalWeightValue_ = null;
            } else {
                init(x);
            }
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.getClient = function (x) {
        $rootScope.client = x;
        $rootScope.currTpl = './assets/partials/main.html';
        $scope.toggleNewTpl('clientsdata');
        $scope.get(x);
    }

    $scope.getPalDetails = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetPalDetails',
            method: "POST",
            data: { palValue: x }
        })
        .then(function (response) {
            $rootScope.pal = JSON.parse(response.data.d)
            $rootScope.pal.value = x;
            $rootScope.clientData.pal = $rootScope.pal;
            $scope.toggleTpl('inputData');
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.getClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/GetClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: x.clientId }
        })
        .then(function (response) {
            $scope.toggleTpl('clientStatictic');
            $scope.clientLog = JSON.parse(response.data.d);
            angular.forEach($scope.clientLog, function (x, key) {
                x.date = new Date(x.date);
            });
            if ($rootScope.goalWeightValue_ == null) {
                getCalculation();
            } else {
                setClientLogGraphData($scope.displayType, $scope.clientLogsDays);
            }
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.removeClientLog = function (x, idx) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete record') + '?')
            .textContent($translate.instant('record date') + ': ' + $filter('date')(x.date, "dd.MM.yyyy") + ', ' + $translate.instant('mass') + ': ' + x.weight + ' kg')
            .targetEvent(x)
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            removeClientLog(x);
        }, function () {
        });
    }

    var removeClientLog = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/Delete',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientData: x }
        })
        .then(function (response) {
            $scope.getClientLog(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.updateClientLog = function (x) {
        var cd = angular.copy(x);
        cd.date = functions.dateToString(cd.date);
        $http({
            url: $sessionStorage.config.backend + 'ClientsData.asmx/UpdateClientLog',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientData: cd }
        })
        .then(function (response) {
            $scope.getClientLog(x);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    var getCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData, userType: $rootScope.user.userType }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            setClientLogGraphData($scope.displayType, $scope.clientLogsDays);
        },
        function (response) {
            if (response.data.d === undefined) {
                functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
            } else {
                functions.alert(response.data.d, '');
            }
        });
    };

    var getMyCalculation = function () {
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/GetMyCalculation',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId }
        })
        .then(function (response) {
            $rootScope.myCalculation = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    var initChartDays = function () {
        $scope.chartDays = [
           { days: 7, title: 'last 7 days' },
           { days: 14, title: 'last 14 days' },
           { days: 30, title: 'last 30 days' },
           { days: 92, title: 'last 3 months' },
           { days: 180, title: 'last 6 months' },
           { days: 365, title: 'last 12 months' },
           { days: 100000, title: 'all' }
        ]
        $scope.clientLogsDays = $scope.chartDays[2];
    }
    initChartDays();

    var displayTypeTitle = $translate.instant('weight') + ' ' + $translate.instant('kg');
    $scope.changeDisplayType = function (type, clientLogsDays) {
        $scope.displayType = type;
        setClientLogGraphData(type, clientLogsDays);
    }

    var getDisplayTypeTitle = function (type) {
        if (type == 0) {
            return $translate.instant('weight') + ' ' + $translate.instant('kg');
        } else if (type == 1) {
            return $translate.instant('waist') + ' ' + $translate.instant('cm');
        } else if (type == 2) {
            return  $translate.instant('hip') + ' ' + $translate.instant('cm');
        } else {
            return null;
        }
    }

    var getRecommendedWeight = function (h) {
        return {
            min: Math.round(((18.5 * h * h) / 10000) * 10) / 10,
            max: Math.round(((25 * h * h) / 10000) * 10) / 10
        }
    }

    $scope.changeGoalWeightValue = function (value, type, clientLogsDays) {
        $rootScope.goalWeightValue_ = parseInt(value);
        setClientLogGraphData(type, clientLogsDays);
    }

    var getGoalLog = function (deficit, key, x, firstWeight, firstDate, currDate) {
        var goal = (firstWeight + (functions.getTwoDateDiff(firstDate, currDate)) * deficit / 7000).toFixed(1);
        var value = 0;
        var goalLimit = $rootScope.goalWeightValue_ !== undefined ? parseInt($rootScope.goalWeightValue_) : 0;
        if (goalLimit == 0) {
            if (deficit == 0) {
                goalLimit = x.weight;
            } else if (deficit > 0) {
                goalLimit = (getRecommendedWeight(x.height).min + getRecommendedWeight(x.height).max) / 2;
            } else {
                goalLimit = getRecommendedWeight(x.height).max;
            }
        }
        if (key == 0) {
            value = x.weight;
        }
        if (deficit > 0) {
            if (goal <= goalLimit) {
                value = goal;
            } else {
                value = goalLimit;
            }
        } else {
            if (goal >= goalLimit) {
                value = goal;
            } else {
                value = goalLimit;
            }
        }
        return value;
    }

    var setClientLogGraphData = function (type, clientLogsDays) {
        $scope.clientLog_ = [];
        var clientLog = [];
        var goalFrom = [];
        var goalTo = [];
        var goalWeight = [];
        var labels = [];

        //TODO - goal (depending of type, reduction increase, fixed Goal)
        if (angular.isDefined($rootScope.calculation.recommendedWeight)) {
            var days = 30;
            var goal = 0;
            var deficit = ($rootScope.calculation.recommendedEnergyIntake - $rootScope.calculation.recommendedEnergyExpenditure) - $rootScope.calculation.tee;
            if (clientLogsDays !== undefined) {
                days = clientLogsDays.days;
                $scope.clientLogsDays = clientLogsDays;
            }
            angular.forEach($scope.clientLog, function (x, key) {
                if (functions.getDateDiff(x.date) <= days) {
                    $scope.clientLog_.push(x);
                    if (type == 0) {
                        clientLog.push(x.weight);
                        goalFrom.push(getRecommendedWeight(x.height).min);
                        goalTo.push(getRecommendedWeight(x.height).max);
                        /********** goal **********/
                        goal = getGoalLog(deficit, key, x, $scope.clientLog[0].weight, $scope.clientLog[0].date, x.date);
                        goalWeight.push(goal);
                        /**************************/
                    }
                    if (type == 1) { clientLog.push(x.waist); goalFrom.push(95); }
                    if (type == 2) { clientLog.push(x.hip); goalFrom.push(97); }
                    if (key % (Math.floor($scope.clientLog.length / 31) + 1) === 0) {
                        labels.push(new Date(x.date).toLocaleDateString());
                    } else {
                        labels.push("");
                    }
                }
            });
        }

        $scope.clientLogGraphData = charts.createGraph(
            [getDisplayTypeTitle($scope.displayType), $translate.instant("lower limit"), $translate.instant("upper limit"), $translate.instant("goal")],
            [
                clientLog,
                goalFrom,
                goalTo,
                goalWeight
            ],
            labels,
            ['#3399ff', '#ff3333', '#33ff33', '#ffd633'],
            {
                responsive: true, maintainAspectRatio: true, legend: { display: true },
                scales: {
                    xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }],
                    yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: false } }]
                }
            },
            [
                { label: getDisplayTypeTitle($scope.displayType), borderWidth: 1, type: 'bar', fill: true },
                { label: $translate.instant("lower limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("upper limit"), borderWidth: 2, type: 'line', fill: false },
                { label: $translate.instant("goal") + ' (2 ' + $translate.instant("kg") + '/' + $translate.instant("mo") + ')', borderWidth: 3, type: 'line', fill: false, strokeColor: "#33ff33", fillColor: "#43ff33" }
            ]
        )

    };

    $scope.setClientLogGraphData = function (type, clientLogsDays) {
        setClientLogGraphData(type, clientLogsDays);
    }

    $scope.getDateFormat = function (x) {
        return new Date(x);
    }

    $scope.change = function (x, scope) {
        switch (scope) {
            case 'height':
                return $rootScope.clientData.height = $rootScope.clientData.height * 1 + x;
                break;
            case 'weight':
                return $rootScope.clientData.weight = $rootScope.clientData.weight * 1 + x;
                break;
            case 'waist':
                return $rootScope.clientData.waist = $rootScope.clientData.waist * 1 + x;
                break;
            case 'hip':
                return $rootScope.clientData.hip = $rootScope.clientData.hip * 1 + x;
                break;
            default:
                return '';
        }
    }

    $scope.pdfLink = null;
    $scope.creatingPdf = false;
    $scope.printClientPdf = function () {
        if ($scope.creatingPdf) {
            return;
        }
        $scope.creatingPdf = true;
        functions.post('PrintPdf', 'ClientPdf', { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, lang: $rootScope.config.language, headerInfo: $rootScope.user.headerInfo }).then(function (d) {
            $scope.creatingPdf = false;
            var fileName = d;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        });

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }
    }

    $scope.showSettings = false;
    $scope.toggleSettings = function (showSettings) {
        $scope.showSettings = !showSettings;
    }

    var initClientLogSettings = function () {
        functions.post('PrintPdf', 'InitClientLogSettings', {}).then(function (d) {
            $scope.settings = d;
        });
    }
    initClientLogSettings();

    $scope.pdfLink1 = null;
    $scope.printClientLogPdf = function (settings) {
        if ($scope.creatingPdf) {
            return;
        }
        $scope.creatingPdf = true;
        var img = null;
        if (document.getElementById("clientDataChart") != null) {
            img = document.getElementById("clientDataChart").toDataURL("image/png").replace(/^data:image\/(png|jpg);base64,/, "");
        }
        functions.post('PrintPdf', 'ClientLogPdf', { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, clientLog: $scope.clientLog_, lang: $rootScope.config.language, imageData: img, headerInfo: $rootScope.user.headerInfo, settings: settings }).then(function (d) {
            $scope.creatingPdf = false;
            var fileName = d;
            $scope.pdfLink1 = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        });
    }

    $scope.hidePdfLink1 = function () {
        $scope.pdfLink1 = null;
    }

    $scope.clientLogDiff = function (type, clientLog, x, idx) {
        if (x === undefined) { return; }
        var diff = 0;
        if (clientLog.length - idx == 1) return {
            diff: diff.toFixed(1),
            icon: 'fa fa-circle text-success'
        }
        switch (type) {
            case 'weight': diff = (x.weight - clientLog[clientLog.length - idx - 2].weight).toFixed(1);
                break;
            case 'waist': diff = (x.waist - clientLog[clientLog.length - idx - 2].waist).toFixed(1);
                break;
            case 'hip': diff = (x.hip - clientLog[clientLog.length - idx - 2].hip).toFixed(1);
                break;
            default:
                diff = 0;
                break;
        }
        if (diff > 0) {
            return {
                diff: diff,
                icon: 'fa fa-arrow-up text-danger'
            }
        }
        if (diff < 0) {
            return {
                diff: diff,
                icon: 'fa fa-arrow-down text-info'
            }
        }
        if (diff == 0) {
            return {
                diff: diff,
                icon: 'fa fa-circle text-success'
            }
        }
    }

    $scope.bodyFatPopup = function (x) {
        $mdDialog.show({
            controller: $scope.bodyFatPopupCtrl,
            templateUrl: 'assets/partials/popup/bodyfat.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: { clientData: x, userType: $rootScope.user.userType }
        })
      .then(function (response) {
          $rootScope.clientData.bodyFat.bodyFatPerc = response;
      }, function () {
      });
    };

    $scope.bodyFatPopupCtrl = function ($scope, $mdDialog, d, $http) {
        var webService = 'BodyFat.asmx';
        var clientData = d.clientData;
        var userType = d.userType;
        $scope.d = null;
        $scope.method = 'JP3';
        $scope.svg = clientData.gender.value === 0 ? 'manSvg' : 'womanSvg';

        var init = function () {
            $http({
                url: $sessionStorage.config.backend + webService + '/InitCaliperMeasurements',
                method: "POST",
                data: { clientData: clientData }
            })
            .then(function (response) {
                $scope.d = JSON.parse(response.data.d);
                $scope.d.data.recordDate = functions.dateToString(clientData.date);
            });
        }
        init();

        $scope.calculate = function (x) {
            if (!functions.checkPermission(d, 'premium')) return;
            $http({
                url: $sessionStorage.config.backend + webService + '/CaliperCalculate',
                method: "POST",
                data: { data: x }
            })
            .then(function (response) {
                $scope.d.data.bodyFat = JSON.parse(response.data.d);
            });
        }

        $scope.confirm = function (x) {
            if (!functions.checkPermission(d, 'premium')) return;
            $http({
                url: $sessionStorage.config.backend + webService + '/Save',
                method: "POST",
                data: { x: x }
            })
            .then(function (response) {
                var res = JSON.parse(response.data.d);
                $mdDialog.hide(res);
            });
        }

        initColor = function () {
            $scope.pointColor = {
                CH: '#fff',
                AB: '#fff',
                TH: '#fff',
                TR: '#fff',
                SUB: '#fff',
                SU: '#fff',
                MI: '#fff',
                BI: '#fff',
            }
        }
        initColor();
        $scope.selectPoint = function (x) {
            initColor();
            var selectColor = '#33cc33';
            if (x === 'CH') {
                $scope.pointColor.CH = selectColor;
            } else if (x == 'AB') {
                $scope.pointColor.AB = selectColor;
            }
            else if (x == 'TH') {
                $scope.pointColor.TH = selectColor;
            }
            else if (x == 'TR') {
                $scope.pointColor.TR = selectColor;
            }
            else if (x == 'SUB') {
                $scope.pointColor.SUB = selectColor;
            }
            else if (x == 'SU') {
                $scope.pointColor.SU = selectColor;
            }
            else if (x == 'MI') {
                $scope.pointColor.MI = selectColor;
            }
            else if (x == 'BI') {
                $scope.pointColor.BI = selectColor;
            }

        }

        $scope.setMethod = function (x) {
            $scope.d.data = x;
            $scope.d.data.clientData = clientData;
            $scope.d.data.recordDate = functions.dateToString(clientData.date);
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    };

    $scope.detailExpenditurePopup = function (x) {
        $mdDialog.show({
            controller: $scope.detailExpenditurePopupCtrl,
            templateUrl: 'assets/partials/popup/detailexpenditure.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
            d: { dailyActivities: x, activities: $rootScope.activities }
        }).then(function (response) {
            if ($rootScope.user.licenceStatus !== 'demo') {
                $rootScope.clientData.dailyActivities = response;
            }
        }, function () {
        });
    };

    $scope.detailExpenditurePopupCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = angular.copy(d);
        $scope.licenceStatus = $rootScope.user.licenceStatus;

        $scope.totalDailyEnergyExpenditure = {
            value: 0,
            duration: 0
        }

        var init = function () {
            $http({
                url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Init',
                method: "POST",
                data: ''
            })
          .then(function (response) {
              $scope.dailyActivity = JSON.parse(response.data.d);
          },
          function (response) {
              functions.alert($translate.instant(response.data.d), '');
          });
        }
        init();

        var setTime = function (h) {
            $scope.hours = [];
            $scope.minutes = [];
            for (i = h; i < 25; i++) {
                $scope.hours.push(i);
            }
            for (i = 0; i < 60; i = i + 5) {
                $scope.minutes.push(i);
            }
        }

        var initTime = function () {
            $scope.from = {
                hour: 0,
                min: 0
            };
            $scope.to = {
                hour: 0,
                min: 0
            }
            setTime(0);
        }
        initTime();

        $scope.clearDailyActivities = function () {
            $scope.d.dailyActivities.activities = [];
            $scope.d.dailyActivities.energy = 0;
            $scope.d.dailyActivities.duration = 0;
            $scope.totalDailyEnergyExpenditure.value = 0;
            $scope.totalDailyEnergyExpenditure.duration = 0;
            $scope.save($scope.d.dailyActivities.activities);
            initTime();
        }

        var totalEnergy = function () {
            var e = 0;
            angular.forEach($scope.d.dailyActivities.activities, function (value, key) {
                e = e + value.energy;
            })
            return e;
        }

        var totalDuration = function () {
            var d = 0;
            angular.forEach($scope.d.dailyActivities.activities, function (value, key) {
                d = d + value.duration;
            })
            return d;
        }

        $scope.confirmActivity = function (x) {
            if (timeDiff($scope.from, $scope.to) == 0) {
                functions.alert($translate.instant('the start time and end of activity can not be the same'), '');
                return false;
            }
            $scope.dailyActivity.id = angular.fromJson(x).id;
            $scope.dailyActivity.activity = angular.fromJson(x).activity;
            $scope.dailyActivity.from.hour = $scope.from.hour;
            $scope.dailyActivity.from.min = $scope.from.min;
            $scope.dailyActivity.to.hour = $scope.to.hour;
            $scope.dailyActivity.to.min = $scope.to.min;
            $scope.dailyActivity.duration = timeDiff($scope.from, $scope.to);
            $scope.dailyActivity.energy = energy(timeDiff($scope.from, $scope.to), angular.fromJson(x).factorKcal);

            $scope.d.dailyActivities.activities.push(angular.copy($scope.dailyActivity));
            $scope.totalDailyEnergyExpenditure.value = totalEnergy();
            $scope.d.dailyActivities.energy = $scope.totalDailyEnergyExpenditure.value;
            $scope.totalDailyEnergyExpenditure.duration = totalDuration();
            $scope.d.dailyActivities.duration = $scope.totalDailyEnergyExpenditure.duration;

            $scope.from = angular.copy($scope.to);
            setTime($scope.from.hour);
        }

        var timeDiff = function (from, to) {
            return (to.hour * 60 + to.min) - (from.hour * 60 + from.min);
        }

        var energy = function (duration, factor) {
            return duration * factor;
        }

        $scope.save = function (x) {
            if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
            $http({
                url: $sessionStorage.config.backend + 'DetailEnergyExpenditure.asmx/Save',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, clientId: $rootScope.client.clientId, activities: x }
            })
          .then(function (response) {
              $scope.d.dailyActivities = JSON.parse(response.data.d);
              $mdDialog.hide($scope.d.dailyActivities);
          },
          function (response) {
              functions.alert($translate.instant(response.data.d), '');
          });
        }

        var getTotal = function () {
            if ($rootScope.d === undefined) {
                return false;
            }
            if ($scope.d.dailyActivities.activities == null) {
                $scope.d.dailyActivities.activities = [];
            }
            if ($scope.d.clientData.dailyActivities.activities.length > 0) {
                $scope.totalDailyEnergyExpenditure.value = totalEnergy();
                $scope.totalDailyEnergyExpenditure.duration = totalDuration();
                var lastActivity = $scope.d.dailyActivities.activities[$scope.d.dailyActivities.activities.length - 1];
                $scope.from = {
                    hour: lastActivity.to.hour,
                    min: lastActivity.to.min
                };
                $scope.to = {
                    hour: lastActivity.to.hour,
                    min: lastActivity.to.min
                }
                setTime(lastActivity.to.hour);
            }
        }
        $timeout(function () {  // TODO, ne ucita prvi put 
            getTotal();
        }, 1000);

        $scope.selectHours = function () {
            if ($scope.to.hour == 24) {
                $scope.to.min = 0;
                $scope.minutes = [];
                $scope.minutes.push(0);
            }
        }

        $scope.confirm = function (x) {
            $mdDialog.hide(x);
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    };

}])

.controller('calculationCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'charts', '$timeout', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, $timeout, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;

    var webService = 'Calculations.asmx';

    $scope.getBmiClass = function (x) {
        if (x < 18.5) { return { text: 'text-info', icon: 'fa fa-exclamation-triangle' }; }
        if (x >= 18.5 && x <= 25) { return { text: 'text-success', icon: 'fa fa-check-circle' }; }
        if (x > 25 && x < 30) { return { text: 'text-warning', icon: 'fa fa-exclamation-circle' }; }
        if (x >= 30) { return { text: 'text-danger', icon: 'fa fa-exclamation-triangle' }; }
    }

    $scope.getWaistClass = function (x) {
        if (x.value < x.increasedRisk) { return { text: 'text-success', icon: 'fa fa-check-circle' }; }
        if (x.value >= x.increasedRisk && x.value < x.highRisk) { return { text: 'text-warning', icon: 'fa fa-exclamation-circle' }; }
        if (x.value >= x.highRisk) { return { text: 'text-danger', icon: 'fa fa-exclamation-triangle' }; }
    }

    var getCharts = function () {
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            bmiChart();
            whrChart();
            waistChart();
            bfChart();
        }, 1000);
    }

    var bmiChart = function () {
        var id = 'bmiChart';
        var value = $rootScope.calculation.bmi.value.toFixed(1);
        var unit = 'BMI';
        var options = {
            title: 'BMI',
            min: 15,
            max: 34,
            greenFrom: 18.5,
            greenTo: 25,
            yellowFrom: 25,
            yellowTo: 30,
            redFrom: 30,
            redTo: 34,
            minorTicks: 5
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var whrChart = function () {
        var id = 'whrChart';
        var value = $rootScope.calculation.whr.value.toFixed(1);
        var unit = 'WHR';
        var increasedRisk = $rootScope.calculation.whr.increasedRisk;
        var highRisk = $rootScope.calculation.whr.highRisk;
        var optimal = $rootScope.calculation.whr.optimal;
        var options = {
            title: 'WHR',
            min: 0,
            max: 1.6,
            greenFrom: optimal - 0.1,
            greenTo: increasedRisk,
            yellowFrom: increasedRisk,
            yellowTo: highRisk,
            redFrom: highRisk,
            redTo: 1.6,
            minorTicks: 0.1
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var waistChart = function () {
        var id = 'waistChart';
        var value = $rootScope.calculation.waist.value.toFixed(1);
        var increasedRisk = $rootScope.calculation.waist.increasedRisk;
        var highRisk = $rootScope.calculation.waist.highRisk;
        var unit = 'cm';
        var options = {
            title: 'WHR',
            min: 0,
            max: 140,
            greenFrom: 70,
            greenTo: increasedRisk,
            yellowFrom: increasedRisk,
            yellowTo: highRisk,
            redFrom: highRisk,
            redTo: 140,
            minorTicks: 5
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var bfChart = function () {
        var id = 'bfChart';
        var value = $rootScope.calculation.bodyFat.bodyFatPerc.toFixed(1);
        var unit = '%';
        var gender = $rootScope.client.gender.value;
        var options = {
            title: '%',
            min: 0,
            max: 60,
            greenFrom: gender == 0 ? 6 : 14,
            greenTo: gender == 0 ? 18 : 25,
            yellowFrom: gender == 0 ? 18 : 25,
            yellowTo: gender == 0 ? 25 : 32,
            redFrom: gender == 0 ? 25 : 32,
            redTo: 60,
            minorTicks: 5
        };
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));
    }

    var getGoals = function () {
        $http({
            url: $sessionStorage.config.backend + 'Goals.asmx/Load',
            method: "POST",
            data: ''
        })
        .then(function (response) {
            $rootScope.goals = JSON.parse(response.data.d);
            isGoalDisabled();
        },
        function (response) {
            alert(response.data.d)
        });
    };

    if ($rootScope.goalWeightValue_ === undefined) { $rootScope.goalWeightValue_ = 0; }
    //$scope.changeGoalWeightValue = function (x) {
    //    //$rootScope.goalWeightValue_ = angular.copy(x);
    //    $rootScope.clientData.targetedMass = angular.copy(x);
    //}
    //$scope.changeGoalWeightValue = function (x) {
    //    $rootScope.goalWeightValue_ = angular.copy(x);
    //}

    $scope.getGoal = function (goal) {
        var x = goal.code;
        if (goal.isDisabled) { return false; }
        var energy = 0;
        var activity = 0;
        var goalWeightValue = 0;
        switch (x) {
            case "G1":  // redukcija tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.tee - 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    goalWeightValue = Math.round(angular.copy($rootScope.clientData.weight));
                } else {
                    goalWeightValue = Math.round(angular.copy($rootScope.calculation.recommendedWeight.max));
                }
                break;
            case "G2":  // zadrzavanje postojece tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.tee + $rootScope.appCalculation.recommendedEnergyExpenditure;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake - 300;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                goalWeightValue = Math.round(angular.copy($rootScope.clientData.weight));
                break;
            case "G3":  // povecanje tjelesne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 300 + $rootScope.appCalculation.recommendedEnergyExpenditure;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure;
                }
                if ($rootScope.appCalculation.goal.code == "G4") {
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 500;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 200;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    goalWeightValue = Math.round(angular.copy($rootScope.calculation.recommendedWeight.max));
                } else {
                    goalWeightValue = $rootScope.clientData.weight < $rootScope.calculation.recommendedWeight.min ? Math.round(angular.copy($rootScope.calculation.recommendedWeight.min)) : Math.round(angular.copy(parseInt($rootScope.clientData.weight) + 10));  //TODO
                }
                break;
            case "G4":  // povecanje misicne mase
                if ($rootScope.appCalculation.goal.code == "G1") {
                    energy = $rootScope.appCalculation.tee + $rootScope.appCalculation.recommendedEnergyExpenditure;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 200;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    energy = $rootScope.appCalculation.tee + 500;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 200;
                }
                if ($rootScope.appCalculation.goal.code == "G3") {   //TODO
                    energy = $rootScope.appCalculation.recommendedEnergyIntake + 400;
                    activity = $rootScope.appCalculation.recommendedEnergyExpenditure + 100;
                }
                if ($rootScope.appCalculation.goal.code == "G2") {
                    goalWeightValue = $rootScope.clientData.weight < $rootScope.calculation.recommendedWeight.min ? Math.round(angular.copy($rootScope.calculation.recommendedWeight.min)) : Math.round(angular.copy(parseInt($rootScope.clientData.weight) + 10));  //TODO
                } else {
                    goalWeightValue = Math.round(angular.copy($rootScope.clientData.weight));
                }
                break;
            default:
                energy = 0;
                activity = 0;
                break;
        }
        // $scope.changeGoalWeightValue(goalWeightValue);

        angular.forEach($rootScope.goals, function (value, key) {
            if (value.code == x) {
                $rootScope.clientData.goal.code = value.code;
                $rootScope.clientData.goal.title = value.title;
                $rootScope.calculation.goal.code = x;
            }
        })

        $rootScope.calculation.recommendedEnergyIntake = Math.round(energy);
        $rootScope.calculation.recommendedEnergyExpenditure = Math.round(activity);

        // $rootScope.calculation.targetedMass = $rootScope.clientData.targetedMass > 0 ? $rootScope.clientData.targetedMass : Math.round(goalWeightValue);
        if ($rootScope.clientData.targetedMass == 0) {
            $rootScope.clientData.targetedMass = Math.round(goalWeightValue);
        }
    }


    var isGoalDisabled = function () {
        if ($rootScope.calculation.bmi.value < 18.5) {
            $rootScope.goals[0].isDisabled = true;
        }
        if ($rootScope.calculation.bmi.value > 25) {
            $rootScope.goals[2].isDisabled = true;
        }
    }

    $scope.creatingPdf = false;
    $scope.printPdf = function () {
        $scope.creatingPdf = true;
        functions.post('PrintPdf', 'CalculationPdf', { userId: $sessionStorage.usergroupid, client: $rootScope.client, clientData: $rootScope.clientData, calculation: $rootScope.calculation, myCalculation: $rootScope.myCalculation, lang: $rootScope.config.language, headerInfo: $rootScope.user.headerInfo }).then(function (d) {
            var fileName = d;
            $scope.creatingPdf = false;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
        });
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }

    $scope.clearMyCalculation = function () {
        $rootScope.initMyCalculation();
    }

    $scope.saveMyCalculation = function (x) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        var myCalculation = angular.copy($rootScope.calculation);
        myCalculation.recommendedEnergyIntake = x.recommendedEnergyIntake;
        myCalculation.recommendedEnergyExpenditure = x.recommendedEnergyExpenditure;
        $http({
            url: $sessionStorage.config.backend + 'Calculations.asmx/SaveMyCalculation',
            method: "POST",
            data: { userId: $sessionStorage.usergroupid, clientId: $rootScope.client.clientId, myCalculation: myCalculation }
        })
        .then(function (response) {
            functions.alert($translate.instant(response.data.d), '');
        },
        function (response) {
            functions.alert($translate.instant(response.data.d), '');
        });
    }

    var getCalculation = function () {
        if ($rootScope.clientData === undefined) { return false; }
        if ($rootScope.clientData.bmrEquation === 'KMA' && $rootScope.clientData.bodyFat.bodyFatPerc == 0) {
            $rootScope.clientData.bmrEquation = 'MSJ';  /*** MifflinStJeor ***/
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/GetCalculation',
            method: "POST",
            data: { client: $rootScope.clientData, userType: $rootScope.user.userType }
        })
        .then(function (response) {
            $rootScope.calculation = JSON.parse(response.data.d);
            $rootScope.appCalculation = JSON.parse(response.data.d);

            if ($rootScope.clientData.goal.code == undefined || $rootScope.clientData.goal.code == null || $rootScope.clientData.goal.code == 0) {
                $rootScope.clientData.goal.code = $rootScope.calculation.goal.code;
            }

            getCharts();
            getGoals();
            $scope.getGoal($rootScope.clientData.goal);
        },
        function (response) {
            if (response.data.d === undefined) {
                functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
            } else {
                functions.alert(response.data.d, '');
            }
        });
    };

    /*******BMR Equations*******/
    if ($rootScope.clientData !== undefined) {
        if ($rootScope.clientData.bodyFat.bodyFatPerc !== 0) {
            $rootScope.clientData.bmrEquation = 'KMA'; /*** KatchMcArdle ***/
        }
    }
    $scope.bmrIsDisabled = false;
    $scope.setBmrEquation = function (x) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if (!functions.checkPermission($rootScope.user, 'standard&premium')) return;
        if (!functions.checkPermission($rootScope.user, 'premium')) return;
        if (x === 'KMA' && $rootScope.clientData.bodyFat.bodyFatPerc == 0) {
            functions.alert($translate.instant('body fat is required'), '');
            $state.go('clientsdata');
            return false;
        }
        getCalculation();
    }
    $scope.checkBmrEquation = function (x) {
        $scope.bmrIsDisabled = x.isDisabled;
        if (x.isDisabled) {
            return false;
        }
    }
    /*******BMR Equations*******/

    getCalculation();

}])

.controller('activitiesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;

    var webService = 'Activities.asmx';
    $scope.orderdirection = '-';
    $scope.orderby = function (x) {
        var direction = $scope.orderdirection == '+' ? '-' : '+';
        $scope.order = direction + x;
        $scope.orderdirection = direction;
    }
    $scope.orderby('activity');
    $scope.energy = 0;

    if ($rootScope.activities == undefined) { $rootScope.loadActivities(); };

    if (angular.isDefined($rootScope.appCalculation) && angular.isDefined($rootScope.myCalculation)) {
        if (!functions.isNullOrEmpty($rootScope.myCalculation.recommendedEnergyExpenditure)) {
            $rootScope.calculation.recommendedEnergyExpenditure = $rootScope.myCalculation.recommendedEnergyExpenditure;
        }
    } else {
        $state.go('calculation');
        $rootScope.selectedNavItem = 'calculation';
    }
    var getEnergyLeft = function () {
        var energy = 0;
        if ($rootScope.clientData !== undefined) {
            if ($rootScope.clientData.activities.length > 0) {
                angular.forEach($rootScope.clientData.activities, function (value, key) {
                    energy = energy + value.energy;
                })
            }
        }
        $scope.energy = energy.toFixed();
        return $rootScope.calculation.recommendedEnergyExpenditure - energy;
    }
    getEnergyLeft();

    $scope.openPopup = function (x) {
        energyLeft = getEnergyLeft();
        if (energyLeft > 10) {  // todo
            $mdDialog.show({
                controller: $scope.popupCtrl,
                templateUrl: 'assets/partials/popup/activity.html',
                parent: angular.element(document.body),
                targetEvent: '',
                clickOutsideToClose: true,
                fullscreen: $scope.customFullscreen, // Only for -xs, -sm breakpoints.
                d: { activity: x, energy: energyLeft }
            })
          .then(function (response) {
              energyLeft = response;
              getEnergyLeft();
          }, function () {
          });
        } else {
            functions.alert($translate.instant('the selected additional energy expenditure is the same as recommended'), '');
        }
    };

    $scope.popupCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.d = d.activity;
        var energy = d.energy;

        $scope.duration = Math.round((energy / ($scope.d.factorKcal * $rootScope.clientData.weight)) * 60);
        // d = (e / (f * w)) * 60
        // d / 60 = e / (f*w)
        // (d * (f*w)) / 60 = e

        // e = d * (f * w) / 60

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            energy = ($scope.duration * ($scope.d.factorKcal * $rootScope.clientData.weight)) / 60;
            $rootScope.clientData.activities.push({
                'id': x.id,
                'activity': x.activity,
                'duration': $scope.duration,
                'energy': energy
            });
            $mdDialog.hide(energy);
        }
    };

    $scope.removeActivity = function (x, idx) {
        $rootScope.clientData.activities.splice(idx, 1);
        getEnergyLeft();
    }

}])

.controller('dietsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;
    var webService = 'Diets';

    if ($rootScope.diets === undefined) { $rootScope.loadDiets(); };

    var get = function (x) {
        functions.post(webService, 'Get', { id: x, lang: $rootScope.config.language }).then(function (d) {
            $rootScope.clientData.diet = d;
        });
    };

    var init = function () {
        var age = $rootScope.clientData.age;
        var diet = '';
        var goal = $rootScope.clientData.goal.code;

        if (age < 14) {
            switch (goal) {
                case 'G1':
                    diet = 'd4';
                    break;
                case 'G2':
                    diet = 'd1';
                    break;
                case 'G3':
                    diet = 'd1';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd1';
                    break;
            }
        }
        if (age >= 14 && age < 18) {
            switch (goal) {
                case 'G1':
                    diet = 'd5';
                    break;
                case 'G2':
                    diet = 'd2';
                    break;
                case 'G3':
                    diet = 'd2';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd2';
                    break;
            }
        }
        if (age >= 18) {
            switch (goal) {
                case 'G1':
                    diet = 'd6';
                    break;
                case 'G2':
                    diet = 'd3';
                    break;
                case 'G3':
                    diet = 'd3';
                    break;
                case 'G4':
                    diet = 'd7';
                    break;
                default:
                    diet = 'd3';
                    break;
            }
        }
        get(diet);
    }
    if ($rootScope.clientData === undefined) {
        $state.go('clientsdata');
    } else if ($rootScope.clientData.diet.id === null) {
        init();
    }

    $scope.select = function (x) {
        $rootScope.clientData.diet = x;
    };

}])

.controller('mealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;

    if ($rootScope.clientData === undefined) {
        $state.go('clientsdata');
        $rootScope.selectedNavItem = 'clientsdata';
        return false;
    }

    $scope.toggleMealsTpl = function (x) {
        $rootScope.mealsTpl = x;
        $rootScope.mealsAreChanged = true;
        if (x === 'myMeals' && $rootScope.myMeals !== undefined) {
            $rootScope.clientData.meals = $rootScope.myMeals.data.meals;
        }
    }

    var defineMealsType = function () {
        if ($rootScope.currentMenu !== undefined) {
            if ($rootScope.currentMenu.id !== null) {
                if ($rootScope.currentMenu.data === null) {
                    $rootScope.mealsTpl = 'standardMeals';
                    return;
                }
                if ($rootScope.currentMenu.data.meals === null) {
                    $rootScope.mealsTpl = 'standardMeals';
                    return;
                }
                if ($rootScope.currentMenu.data.meals.length > 0) {
                    if ($rootScope.currentMenu.data.meals[0].code == 'B') {
                        $rootScope.mealsTpl = 'standardMeals';
                    } else {
                        $rootScope.mealsTpl = 'myMeals';
                    }
                    return;
                } else {
                    $rootScope.mealsTpl = 'standardMeals';
                }
            }
        }
        if ($rootScope.clientData.myMeals !== undefined && $rootScope.clientData.myMeals != null) {
            if ($rootScope.clientData.myMeals.data != null) {
                if ($rootScope.clientData.myMeals.data.meals.length >= 2) {
                    $rootScope.mealsTpl = 'myMeals';
                } else {
                    $rootScope.mealsTpl = 'standardMeals';
                }
            } else {
                $rootScope.mealsTpl = 'standardMeals';
            }
        } else {
            $rootScope.mealsTpl = 'standardMeals';
        }
    }
    defineMealsType();

}])

.controller('standardMealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'Meals';
    $rootScope.isMyMeals = false;

    var load = function () {
        functions.post(webService, 'Load', {}).then(function (d) {
            $rootScope.clientData.meals = d;
            // TODO translate meals on server side
            angular.forEach($rootScope.clientData.meals, function (value, key) {
                $rootScope.clientData.meals[key].title = $translate.instant($rootScope.clientData.meals[key].title);
            })
        });
    };
    if ($rootScope.clientData.meals.length === 0) {
        load();
    } else if ($rootScope.clientData.meals[0].code !== 'B') {
        load();
    }

}])

.controller('myMealsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'MyMeals';
    var init = function () {
        functions.post(webService, 'Init', { user: $rootScope.user }).then(function (d) {
            $rootScope.myMeals = d;
            $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
            $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            $rootScope.isMyMeals = true;
        });
    }

    var getClientMeals = function () {
        if ($rootScope.currentMenu !== undefined) {
            if ($rootScope.currentMenu.id != null) {
                return false;
            }
        }
        if ($rootScope.clientData.myMeals !== undefined && $rootScope.clientData.myMeals != null) {
            if ($rootScope.clientData.myMeals.data != null) {
                if ($rootScope.clientData.myMeals.data.meals.length > 0) {
                    $rootScope.myMeals = angular.copy($rootScope.clientData.myMeals);
                }
            } else {
                init();
            }
        } else {
            init();
        }
    }

    var initMyMeals = function () {
        if (!angular.isDefined($rootScope.myMeals)) {
            functions.post(webService, 'Init', { user: $rootScope.user }).then(function (d) {
                $rootScope.myMeals = d;
                getClientMeals();
                $rootScope.isMyMeals = true;
            });
        } else {
            getClientMeals();
        }

    }
    initMyMeals();

    $scope.new = function () {
        if ($rootScope.user.userType < 2) { return }
        init();
    }

    $scope.getTemplate = function () {
        if ($rootScope.user.userType < 2) { return }
        functions.post(webService, 'Template', { user: $rootScope.user, lang: $rootScope.config.language }).then(function (d) {
            $rootScope.myMeals = d;
            if ($rootScope.user.userType > 2) {
                $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
                $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            }
            $rootScope.isMyMeals = true;
            $rootScope.mealsAreChanged = true;
        });
    }

    var setMealCode = function () {
        if ($rootScope.myMeals !== undefined) {
            if ($rootScope.myMeals.data != null) {
                if ($rootScope.myMeals.data.meals.length > 0) {
                    angular.forEach($rootScope.myMeals.data.meals, function (value, key) {
                        value.code = 'MM' + key;
                        $rootScope.myMeals.data.energyPerc[key].meal.code = value.code;
                    })
                    $rootScope.isMyMeals = true;
                    $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
                }
            }
        }
    }

    $scope.add = function () {
        if ($rootScope.myMeals === undefined) {
            init();
        } else {
            addNewRow();
        }
    }

    $rootScope.setMealCode = function () {
        if ($rootScope.isMyMeals) {
            setMealCode();
        }
    }

    var addNewRow = function () {
        if ($rootScope.user.userType < 2) { return false; }
        if ($rootScope.myMeals.data.meals.length >= 9) {
            functions.alert($translate.instant('you have reached the maximum number of meals'), '');
            return false;
        }
        $rootScope.myMeals.data.meals.push({
            code: "",
            title: "",
            description: "",
            isSelected: true,
            isDisabled: false
        });
        $rootScope.myMeals.data.energyPerc.push({
            meal: {
                code: "",
                energyMinPercentage: 0,
                energyMaxPercentage: 0,
                energyMin: 0,
                energyMax: 0
            }
        });
        setMealCode();
    }

    $scope.removeMeal = function (idx) {
        if ($rootScope.user.userType < 2) { return false; }
        $rootScope.myMeals.data.meals.splice(idx, 1);
        $rootScope.myMeals.data.energyPerc.splice(idx, 1);
        setMealCode();
    }

    $scope.save = function () {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if ($rootScope.user.userType < 2) return;

        if ($rootScope.myMeals.data.meals.length < 3) {
            functions.alert($translate.instant('choose at least 3 meals'), '');
            return;
        }
        if (functions.isNullOrEmpty($rootScope.myMeals.title)) {
            functions.alert($translate.instant('title is required'), '');
            return;
        }

        functions.post(webService, 'Save', { userId: $sessionStorage.usergroupid, x: $rootScope.myMeals }).then(function (d) {
            if (!d.isSuccess) {
                functions.alert($translate.instant(d.msg), '');
                return;
            }
            $rootScope.myMeals = d.data;
            $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
            $rootScope.isMyMeals = true;
        });
    }

    $scope.remove = function (id) {
        if ($rootScope.user.userType < 2) {
            return false;
        }
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete meals') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(id);
        }, function () {
        });
    };

    remove = function (id) {
        functions.post(webService, 'Delete', { userId: $sessionStorage.usergroupid, id: id }).then(function (d) {
            $scope.mealsList = d;
            init();
        });
    }

    $scope.search = function () {
        openMyMealsPopup();
    }

    var openMyMealsPopup = function () {
        if ($rootScope.user.userType < 2) {
            return;
        }
        $mdDialog.show({
            controller: getMyMealsPopupCtrl,
            templateUrl: 'assets/partials/popup/mymeals.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (response) {
            $rootScope.myMeals = response;
            $rootScope.clientData.myMeals = angular.copy($rootScope.myMeals);
            $rootScope.clientData.meals = $rootScope.clientData.myMeals.data.meals;
            $rootScope.isMyMeals = true;
            $rootScope.mealsAreChanged = true;
        }, function () {
        });
    };

    var getMyMealsPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            functions.post(webService, 'Load', { userId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.d = d;
                $scope.loading = false;
            });
        }
        load();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post(webService, 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $scope.meals = d;
                $mdDialog.hide($scope.meals);
            });
        }

        $scope.confirm = function (x) {
            get(x);
        }

    };

}])

.controller('menuCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'charts', '$timeout', 'functions', '$translate', '$state', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, charts, $timeout, functions, $translate, $state) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }
    $rootScope.selectedNavItem = $state.current.name;

    if ($rootScope.calculation === undefined) {
        $state.go('clientsdata');
        $rootScope.selectedNavItem = 'clientsdata';
        return;
    }

    if ($rootScope.calculation.recommendedEnergyIntake === undefined) {
        $state.go('calculation');
        $rootScope.selectedNavItem = 'calculation';
        return;
    }

    if ($rootScope.clientData === undefined) {
        $state.go('meals');
        $rootScope.selectedNavItem = 'meals';
        return;
    }
    var webService = 'Foods.asmx';
    $scope.addFoodBtnIcon = 'fa fa-plus';
    $scope.addFoodBtn = false;

    if ($rootScope.foods == undefined) { $rootScope.loadFoods(); };

    function initPrintSettings() {
        functions.post('PrintPdf', 'InitMenuSettings', {}).then(function (d) {
            $scope.printSettings = d;
        });
    };
    initPrintSettings();

    $rootScope.selectedFoods = $rootScope.selectedFoods == undefined ? [] : $rootScope.selectedFoods;

    if ($rootScope.clientData.meals.length < 3) {
        $state.go('meals');
        $rootScope.selectedNavItem = 'meals';
        functions.alert($translate.instant('choose at least 3 meals'), '');
    }

    var getRecommendations = function (clientData) {
        if ($rootScope.calculation === undefined) return;
        var energyPerc = null;
        if (clientData.myMeals !== undefined && clientData.myMeals != null) {
            if (clientData.myMeals.data != null) {
                if (clientData.myMeals.data.meals.length >= 2 && $rootScope.isMyMeals == true) {
                    clientData.meals = clientData.myMeals.data.meals;
                    energyPerc = clientData.myMeals.data.energyPerc;
                }
            }
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/GetRecommendations',
            method: "POST",
            data: { client: clientData, recommendedEnergyIntake: $rootScope.calculation.recommendedEnergyIntake, myRecommendedEnergyIntake: $rootScope.myCalculation.recommendedEnergyIntake, myMealsEnergyPerc: energyPerc }
        })
       .then(function (response) {
           $rootScope.recommendations = JSON.parse(response.data.d);
           $timeout(function () {
               displayCharts();
           }, 500);
       },
       function (response) {
           if (response.data.d === undefined) {
               functions.alert($translate.instant('you have to refresh the page. press Ctrl+F5') + '.', '');
           } else {
               functions.alert(response.data.d, '');
           }
       });
    };
    getRecommendations($rootScope.clientData);

    var init = function () {
        $http({
            url: $sessionStorage.config.backend + webService + '/Init',
            method: "POST",
            data: { lang: $rootScope.config.language }
        })
        .then(function (response) {
            $scope.food = response.data.d.food;
            initMenuDetails();
        },
        function (response) {
            alert(response.data.d)
        });
    };

    var initMenuDetails = function () {
        $http({
            url: $sessionStorage.config.backend + 'Menues.asmx/Init',
            method: "POST",
            data: {}
        })
        .then(function (response) {
            $rootScope.currentMenu = JSON.parse(response.data.d);
            if ($rootScope.client === null) {
                $state.go('dashboard');
                $rootScope.selectedNavItem = 'dashboard';
                functions.alert($translate.instant('choose client'), '');
                return;
            }
            $rootScope.currentMenu.client = $rootScope.client;
            $rootScope.currentMenu.client.clientData = $rootScope.clientData;  //TODO sredit

            $rootScope.currentMenu.data.meals = $rootScope.clientData.meals;
            if ($rootScope.mealsTpl === 'myMeals') {
                $rootScope.currentMenu.data.meals = $rootScope.myMeals.data.meals;
                angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                    if (value.description !== '') {
                        $rootScope.currentMenu.data.meals[key].description = value.description + '~';
                    }
                })
            }

            angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                $rootScope.currentMenu.splitMealDesc.push({
                    code: value.code,
                    title: value.title,
                    dishDesc: [{ title: null, desc: null, id: null }],
                    isSelected: value.isSelected,
                    isDisabled: value.isDisabled
                });
            })

            $rootScope.currentMeal = 'B';
            if ($rootScope.currentMenu !== undefined) {
                if ($rootScope.currentMenu.data !== null) {
                    if ($rootScope.currentMenu.data.meals.length > 0) {
                        $rootScope.currentMeal = $rootScope.currentMenu.data.meals[0].code;
                    }
                }
            }

            getTotals($rootScope.currentMenu);
            getRecommendations($rootScope.currentMenu.client.clientData);
        },
        function (response) {
            alert(response.data.d)
        });
    };

    $scope.addNewDishTitle = function (idx) {
        $rootScope.currentMenu.splitMealDesc[idx].dishDesc.push({ title: null, desc: null, id: null });
    }

    $scope.removeNewDishTitle = function (idx, idx1) {
        $rootScope.currentMenu.splitMealDesc[idx].dishDesc.splice(idx1, 1);
    }

    $scope.removeDish = function (currentMenu, idx, idx1, id, mealCode) {
        if (currentMenu.splitMealDesc[idx].dishDesc.length > 1) {
            $scope.removeNewDishTitle(idx, idx1);
        } else if (currentMenu.splitMealDesc[idx].dishDesc.length === 1 && idx1 === 0) {
            currentMenu.splitMealDesc[idx].dishDesc[0] = { title: null, desc: null, id: null };
        }
        $rootScope.currentMenu.data.selectedFoods = currentMenu.data.selectedFoods.filter(a => a.id.split(';')[1] !== id);
        $rootScope.currentMenu.data.selectedInitFoods = currentMenu.data.selectedInitFoods.filter(a => a.id.split(';')[1] !== id);
        getTotals($rootScope.currentMenu);
    }

    $scope.toggleMeals = function (x) {
        $rootScope.currentMeal = x;
        getCurrMealTotal(x);
    };

    if ($rootScope.mealsAreChanged) {
        $rootScope.mealsAreChanged = false;
        init();
    } else {
        if ($rootScope.currentMenu === undefined) {
            init();
        } else {
            if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
                init();
            } else {
                var oldMeals = $rootScope.currentMenu.data.meals;
                $rootScope.currentMenu.data.meals = angular.copy($rootScope.clientData.meals);
                angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                    if (key >= $rootScope.currentMenu.data.meals.length || key >= oldMeals.length) { return false; }
                    if (oldMeals[key].code == value.code && $rootScope.currentMenu.data.selectedFoods.length > 0) {
                        value.description = oldMeals[key].description;
                    }
                })
                $rootScope.currentMenu.client = $rootScope.client;
                $rootScope.currentMenu.client.clientData = $rootScope.clientData;  //TODO sredit
            }
        }
    }

    $scope.toggleAnalytics = function (x) {
        $scope.loading = true;
        $timeout(function () {
            $scope.loading = false;
            $scope.analyticsTpl = x;
            if ($rootScope.menuTpl == 'dailyMenuTpl') {
                getTotals($rootScope.currentMenu);
            } else {
                getWeeklyMenuTotals($rootScope.weeklyMenu.menuList)
            }
        }, 700);
    };
    $scope.toggleAnalytics('chartsTpl');

    $scope.changeQuantity = function (x, type, idx) {
        $scope.selectedThermalTreatment = null; //TODO
        angular.forEach($rootScope.currentMenu.data.selectedFoods[idx].thermalTreatments, function (value, key) {
            if (value.isSelected == true) {
                $scope.selectedThermalTreatment = value;
            }
        })
        $rootScope.currentMenu.data.selectedFoods[idx] = functions.changeQuantity($rootScope.currentMenu.data.selectedInitFoods[idx], x, type);
        $timeout(function () {
            if ($rootScope.currentMenu.data.selectedFoods[idx].quantity > 0.0001 && !isNaN($rootScope.currentMenu.data.selectedFoods[idx].quantity) && $rootScope.currentMenu.data.selectedFoods[idx].mass > 0.0001 && !isNaN($rootScope.currentMenu.data.selectedFoods[idx].mass)) {
                getTotals($rootScope.currentMenu);
            }
        }, 200);
    }

    $scope.change = function (x, type, idx) {
        if (type === 'quantity' && $rootScope.currentMenu.data.selectedFoods[idx].quantity + x > 0) {
            $rootScope.currentMenu.data.selectedFoods[idx].quantity = $rootScope.currentMenu.data.selectedFoods[idx].quantity * 1 + x;
            $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'quantity', idx);
        }
        if (type === 'mass' && $rootScope.currentMenu.data.selectedFoods[idx].mass + x > 0) {
            $rootScope.currentMenu.data.selectedFoods[idx].mass = $rootScope.currentMenu.data.selectedFoods[idx].mass * 1 + x;
            $scope.changeQuantity($rootScope.currentMenu.data.selectedFoods[idx], 'mass', idx);
        }
    }

    $scope.openFoodPopup = function (x, idx) {
        $scope.addFoodBtn = true;
        $scope.addFoodBtnIcon = 'fa fa-spinner fa-spin';
        if ($rootScope.user.licenceStatus === 'demo' && $rootScope.currentMenu.data.selectedFoods.length > 9) {
            if (!functions.checkPermissionAlert('in demo version maximum number of choosen foods is 10', 'activate full version')) return;
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
            return false;
        }
        $mdDialog.show({
            controller: $rootScope.foodPopupCtrl,
            templateUrl: 'assets/partials/popup/food.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { foods: $rootScope.foods, myFoods: $rootScope.myFoods, foodGroups: $rootScope.foodGroups, food: x, idx: idx, config: $rootScope.config }
        })
        .then(function (x) {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
            if (x.openAsMyFood !== undefined) {
                if (x.openAsMyFood == true) {
                    $state.go('myfoods');
                    $rootScope.selectedNavItem = 'myfoods';
                    $rootScope.myFood_ = x.initFood;
                } else {
                    $scope.addFoodToMeal(x.food, x.initFood, idx);
                }
            } else {
                $scope.addFoodToMeal(x.food, x.initFood, idx);
            }
        }, function () {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
        });
    };

    $scope.addFoodToMeal = function (x, initFood, idx) {
        if (x.food != undefined || x.food != null) {
            x.meal.code = $rootScope.currentMeal;

            angular.forEach($rootScope.clientData.meals, function (value, key) {
                if (value.code == x.meal.code) {
                    x.meal.title = $translate.instant(value.title);
                }
            })

            if (idx == undefined) {
                $rootScope.currentMenu.data.selectedFoods.push(x);
                $rootScope.currentMenu.data.selectedInitFoods.push(initFood);
            } else {
                $rootScope.currentMenu.data.selectedFoods[idx] = x;
                $rootScope.currentMenu.data.selectedInitFoods[idx] = initFood;
            }

            initFood.meal.code = $rootScope.currentMeal;

            $scope.food = [];
            $scope.choosenFood = "";
            $scope.thermalTreatment = "";
            getTotals($rootScope.currentMenu);
        }
    }

    $scope.new = function () {
        angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
            if (value.description !== '') {
                $rootScope.currentMenu.data.meals[key].description = '';
            }
        })
        init();
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
             .title($translate.instant('remove menu') + '?')
             .textContent(x.title)
             .targetEvent(x)
             .ok($translate.instant('yes'))
             .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    }

    var remove = function (x) {
        functions.post('Menues', 'Delete', { user: $rootScope.user, menu: x }).then(function (d) {
            if (d.msg !== null) {
                functions.alert($translate.instant(d.msg), '');
            } else {
                init();
            }
        });
    }


    $scope.removeFood = function (x, idx) {
        $rootScope.currentMenu.data.selectedFoods.splice(idx, 1);
        $rootScope.currentMenu.data.selectedInitFoods.splice(idx, 1);
        getTotals($rootScope.currentMenu);
    }

    $scope.printPreview = function () {
        $mdDialog.show({
            controller: $scope.printPreviewCtrl,
            templateUrl: 'assets/partials/popup/printmenu.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen,
            d: { currentMenu: $rootScope.currentMenu, clientData: $rootScope.clientData, client: $rootScope.client, totals: $rootScope.totals, settings: $scope.printSettings, config: $rootScope.config, user: $rootScope.user, loginUser: $rootScope.loginUser }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printPreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.currentMenu = d.currentMenu;
        $scope.clientData = d.clientData;
        $scope.client = d.client;
        $scope.totals = d.totals;
        $scope.settings = d.settings;
        $scope.settings.headerInfo = d.user.headerInfo;
        $scope.settings.date = new Date(new Date($scope.currentMenu.date)).toLocaleDateString();
        $scope.settings.author = d.user.firstName + ' ' + d.user.lastName;
        $scope.config = d.config;
        $scope.loginUser = d.loginUser;
        $scope.pdfLink == null;
        $scope.creatingPdf = false;

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.changeNumberOfConsumers = function (x) {
            if (x < 1 || functions.isNullOrEmpty(x)) { return false }
            $scope.settings.consumers = x;
            functions.post('Foods', 'ChangeNumberOfConsumers', { foods: $scope.currentMenu.data.selectedFoods, number: x }).then(function (d) {
                $scope.foods = d;
            });
        }
        if (angular.isDefined($scope.currentMenu)) { $scope.changeNumberOfConsumers($scope.settings.consumers); }

        $scope.copyToClipboard = function (id) {
            return functions.copyToClipboard(id);
        }

        $scope.getMealTitle = function (x) {
            return $rootScope.getMealTitle(x);
        }

        $scope.getServDescription = function (x) {
            var des = "";
            if (x.cerealsServ > 0) { des = servDes(des, x.cerealsServ, "cereals_"); }
            if (x.vegetablesServ > 0) { des = servDes(des, x.vegetablesServ, "vegetables_"); }
            if (x.fruitServ > 0) { des = servDes(des, x.fruitServ, "fruit_"); }
            if (x.meatServ > 0) { des = servDes(des, x.meatServ, "meat_"); }
            if (x.milkServ > 0) { des = servDes(des, x.milkServ, "milk_"); }
            if (x.fatsServ > 0) { des = servDes(des, x.fatsServ, "fats_"); }
            return des;
        }

        function servDes(des, serv, title) {
            return (functions.isNullOrEmpty(des) ? '' : (des + ', ')) + serv + ' serv. ' + $translate.instant(title);
        }

        $scope.getCurrMealDesc = function (x, splitMealDesc) {
            return splitMealDesc.find(a => a.code === x.code).dishDesc;
        }

        $scope.printMenuPdf = function (consumers, date, author, settings) {
            if (angular.isDefined($scope.currentMenu)) {
                $scope.creatingPdf = true;
                functions.post('PrintPdf', 'MenuPdf', { userId: $sessionStorage.usergroupid, currentMenu: $scope.currentMenu, totals: $rootScope.totals, lang: $rootScope.config.language, settings: settings }).then(function (d) {
                    var fileName = d;
                    $scope.creatingPdf = false;
                    $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
                });
            }
        }

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }

        $scope.setAuthor = function (x) {
            $scope.author = x;
        }

        $scope.setDate = function (x) {
            $scope.date = x;
        }

        $scope.collapse = function () {
            if (window.innerWidth < 1200) {
                return 'collapse';
            } else {
                return null;
            }
        }

        /**** More recipes in one meal *****/
        var currMealTitle = null;
        $scope.getCurrDishTitle = function (id, x, splitMealDesc) {
            currMealDesc = $scope.getCurrMealDesc(x, splitMealDesc);
            var t = null;
            if (currMealDesc.length > 1) {
                angular.forEach(currMealDesc, function (value, key) {
                    if (id.split(';').length === 2) {
                        if (value.id === id.split(';')[1]) {
                            t = value.title;
                        }
                    }
                });
                var title = currMealTitle !== t ? t : null;
                currMealTitle = angular.copy(t);
                return title;
            } else {
                return null;
            }
        }
        /**** More recipes in one meal *****/

    };

    $scope.get = function () {
        if ($rootScope.user.licenceStatus === 'demo') {
            if (!functions.checkPermissionAlert('this function is not available in demo version', 'activate full version')) return;
        } else {
            getMenuPopup();
        }
    }

    var getMenuPopup = function () {
        $mdDialog.show({
            controller: getMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/getmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            data: { config: $rootScope.config, clientData: $rootScope.clientData }
        })
        .then(function (x) {
            $rootScope.currentMenu.id = x.id;
            $rootScope.currentMenu.title = x.title;
            $rootScope.currentMenu.note = x.note;
            $rootScope.currentMenu.userId = x.userId;
            $rootScope.currentMenu.data = x.data;
            $rootScope.currentMenu.client.clientData = $rootScope.clientData;
            $rootScope.currentMenu.client.clientData.meals = x.data.meals;
            $rootScope.currentMenu.client.clientData.myMeals = x.client.clientData.myMeals;
            $rootScope.currentMenu.splitMealDesc = x.splitMealDesc;
            $rootScope.isMyMeals = false;
            if ($rootScope.currentMenu.client.clientData.myMeals != null) {
                if ($rootScope.currentMenu.client.clientData.myMeals.data != null) {
                    if ($rootScope.currentMenu.client.clientData.myMeals.data.meals.length >= 2) {
                        $rootScope.isMyMeals = true;
                    }
                }
            }

            getRecommendations(angular.copy($rootScope.currentMenu.client.clientData));
            getTotals($rootScope.currentMenu);
            $rootScope.currentMeal = x.data.meals[0].code;
        }, function () {
        });
    };

    var getMenuPopupCtrl = function ($scope, $mdDialog, $http, data, $translate, $translatePartialLoader, $timeout) {
        $scope.config = data.config;
        $scope.clientData = data.clientData;
        $scope.loadType = 0;
        $scope.type = 0;
        $scope.appMenues = false;
        $scope.toTranslate = false;
        $scope.toLanguage = '';
        $scope.searchValue = null;
        $scope.clientId = null;
        var limit = $rootScope.config.showmenuslimit;
        $scope.limit = limit;
        var offset = 0;
        $scope.d = [];
        var isLoadMore = false;
        $scope.user = $rootScope.user;

        $scope.toggle = function (type) {
            if (type == 'myMenus') {
                $scope.d = [];
                limit = $rootScope.config.showmenuslimit;
                offset = 0;
                load(null, null, null);
            } else if (type == 'appMenus') {
                loadAppMenues();
            }
        }

        $scope.loadMore = function (search, clientId, userId) {
            isLoadMore = true;
            offset += $rootScope.config.showmenuslimit;
            load(search, clientId, userId);
        }

        var load = function (search, clientId, userId) {
            $scope.loading = true;
            $scope.appMenues = false;
            if (!isLoadMore) {
                $scope.d = [];
            }
            functions.post('Menues', 'Load', { userGroupId: $rootScope.user.userGroupId, limit: limit, offset: offset, search: search, clientId: clientId, userId: userId }).then(function (d) {
                angular.forEach(d, function (x, key) {
                    x.date = new Date(x.date);
                });
                if ($scope.d !== null && isLoadMore) {
                    $scope.d = $scope.d.concat(d);
                } else {
                    $scope.d = d;
                }
                $scope.loading = false;
            });
        }
        load(null, null, null);

        $scope.currUserId = null;
        $scope.load = function (search, type, clientId, userId) {
            $scope.currUserId = userId;
            $scope.d = [];
            offset = 0;
            if (type == 0) {
                $scope.clientId = null;
                load(search, null, userId);
            } else {
                $scope.clientId = clientId;
                load(search, clientId, userId);
            }
        }

        var getUsersByUserGroup = function () {
            functions.post('Users', 'GetUsersByUserGroup', { userGroupId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.users = d;
                $scope.users.unshift({ userId: null, firstName: $translate.instant('all users'), lastName: null });
            });
        };
        getUsersByUserGroup();

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                 .title($translate.instant('remove menu') + '?')
                 .textContent(x.title)
                 .targetEvent(x)
                 .ok($translate.instant('yes'))
                 .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        }

        var remove = function (x) {
            functions.post('Menues', 'Delete', { user: $rootScope.user, menu: x }).then(function (d) {
                if (d.msg !== null) {
                    functions.alert($translate.instant(d.msg), '');
                }
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post('Menues', 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $mdDialog.hide(d);
            });
        }

        var loadAppMenues = function () {
            $scope.appMenues = true;
            functions.post('Menues', 'LoadAppMenues', { lang: $rootScope.config.language }).then(function (d) {
                $scope.d = d;
            });
        }

        var getAppMenu = function (x) {
            functions.post('Menues', 'GetAppMenu', { id: x.id, lang: $rootScope.config.language, toTranslate: $scope.toTranslate }).then(function (d) {
                if ($scope.toTranslate) {
                    translateFoods(d);
                }
                $mdDialog.hide(d);
            });
        }

        $scope.confirm = function (x) {
            $scope.appMenues === true ? getAppMenu(x) : get(x);
        }

        $scope.setToTranslate = function (x) {
            $scope.toTranslate = x;
        }

        $scope.setToLanguage = function (x) {
            $scope.toLanguage = x;
        }

        var translateFoods = function (menu) {
            $rootScope.setLanguage($scope.toLanguage);
            $timeout(function () {
                angular.forEach(menu.data.selectedFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                angular.forEach(menu.data.selectedInitFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                $mdDialog.hide(menu);
                $rootScope.setLanguage('hr');
            }, 500);
        }

    };

    $scope.save = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length === 0) {
            return false;
        }
        if ($rootScope.user.licenceStatus === 'demo') {
            if(!functions.checkPermissionAlert('the saving function is disabled in demo version', 'activate full version')) return;
            return false;
        } else {
            openSaveMenuPopup();
        }
    }

    //TODO remove client from params
    var openSaveMenuPopup = function () {
        $rootScope.client.clientData = $rootScope.clientData;
        $mdDialog.show({
            controller: openSaveMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/savemenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: $rootScope.currentMenu, client: $rootScope.client, totals: $rootScope.totals, config: $sessionStorage.config, user: $rootScope.user }
        })
       .then(function (x) {
           $rootScope.currentMenu = x;
       }, function () {
       });
    }

    var openSaveMenuPopupCtrl = function ($scope, $mdDialog, $http, d, $translate) {
        $scope.d = angular.copy(d);
        var save = function (currentMenu) {
            if (functions.isNullOrEmpty(currentMenu.title)) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu title'), '');
                openSaveMenuPopup();
                return;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            var myMeals = null;
            if (currentMenu.data.meals.length > 2) {
                if (currentMenu.data.meals[0].code !== 'B') {
                    myMeals = $scope.d.client.clientData.myMeals;
                }
            }
            currentMenu.date = functions.dateToString(currentMenu.date);
            functions.post('Menues', 'Save', { userId: $rootScope.user.userGroupId, x: currentMenu, user: $scope.d.user, myMeals: myMeals }).then(function (d) {
                if (d.isSuccess) {
                    $scope.d.currentMenu = d.data;
                    $mdDialog.hide($scope.d.currentMenu);
                } else {
                    functions.alert($translate.instant(d.msg), d.msg1 !== null ? $translate.instant(d.msg1) : null);
                }
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x, saveasnew) {
            x.client = d.client;
            x.userId = d.client.userId;
            x.id = saveasnew == true ? null : x.id;
            if (d.totals === undefined) {
                functions.alert('d.totals == undefined', $translate.instant('report a problem'));
                return;
            }
            x.energy = d.totals.energy;
            x.date = new Date(new Date().setHours(0, 0, 0, 0));
            save(x);
        }

        var saveAppMenu = function (currentMenu) {
            if (functions.isNullOrEmpty(currentMenu.title)) {
                document.getElementById("txtMenuTitle").focus();
                functions.alert($translate.instant('enter menu title'), '');
                openSaveMenuPopup();
                return false;
            }
            currentMenu.diet = d.client.clientData.diet.diet;
            functions.post('Menues', 'SaveAppMenu', { x: currentMenu, lang: $rootScope.config.language }).then(function (d) {
                functions.alert('ok', '');
            });
        }

        $scope.saveAppMenu = function (x) {
            x.energy = d.totals.energy;
            saveAppMenu(x);
        }
    };

    var getCurrMealTotal = function (meal) {
        $scope.currMealTotal = $rootScope.totals.mealsTotal.find(a => a.code === meal);
    }

    var getTotals = function (x) {
        if (x === undefined) { return false; }
        functions.post('Foods', 'GetTotals', { selectedFoods: x.data.selectedFoods, meals: x.data.meals }).then(function (d) {
            $rootScope.totals = d;
            $rootScope.totals.price.currency = $rootScope.config.currency;
            displayCharts();
            getCurrMealTotal($rootScope.currentMeal);
        });
    }



    var displayCharts = function () {
        if ($rootScope.recommendations === undefined || $rootScope.totals === undefined) { return false; }
        $scope.mealsTotals = [];
        $scope.mealsMin = [];
        $scope.mealsMax = [];
        $scope.mealsTitles = [];

        //TODO: nutrients meal chart
        $scope.nutriMealTotals = {
            carbohydrates: [],
            proteins: [],
            fats: []
        }

        if ($rootScope.currentMenu === undefined) { return; }
        angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
            if (value.isSelected == true && angular.isDefined($rootScope.totals)) {
                if ($rootScope.totals === null) return;
                if (angular.isDefined($rootScope.totals.mealsTotal)) {
                    if (key < $rootScope.recommendations.mealsRecommendationEnergy.length) {
                        $scope.mealsTotals.push($rootScope.totals.mealsTotal.length > 0 && $rootScope.totals.mealsTotal.length <= $rootScope.recommendations.mealsRecommendationEnergy.length ? $rootScope.totals.mealsTotal[key].energy.val.toFixed(1) : 0);

                        $scope.nutriMealTotals.carbohydrates.push($rootScope.totals.mealsTotal.length > 0 ? $rootScope.totals.mealsTotal[key].carbohydrates.perc.toFixed(1) : 0);
                        $scope.nutriMealTotals.proteins.push($rootScope.totals.mealsTotal.length > 0 ? $rootScope.totals.mealsTotal[key].proteins.perc.toFixed(1) : 0);
                        $scope.nutriMealTotals.fats.push($rootScope.totals.mealsTotal.length > 0 ? $rootScope.totals.mealsTotal[key].fats.perc.toFixed(1) : 0);

                        if ($rootScope.recommendations !== undefined) {
                            if (angular.isDefined($rootScope.recommendations.mealsRecommendationEnergy)) {
                                $scope.mealsMin.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMin);
                                $scope.mealsMax.push($rootScope.recommendations.mealsRecommendationEnergy[key].meal.energyMax);
                            }
                        }
                        $scope.mealsTitles.push($translate.instant($rootScope.getMealTitle(value)));
                    }
                }
            }
        })

        totalEnergyChart();
        otherFoodChart();
        carbohydratesChart();
        proteinsChart();
        saturatedFatsChart();
        trifluoroaceticAcidChart();
        cholesterolChart();
        fatsChart();

        var t = $rootScope.totals;
        var r = $rootScope.recommendations
        $rootScope.servGraphData = charts.createGraph(
                [$translate.instant('cereals'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],
                [
                    [t.servings.cerealsServ, t.servings.vegetablesServ, t.servings.fruitServ, t.servings.meatServ, t.servings.milkServ, t.servings.fatsServ],
                    [r.servings.cerealsServ, r.servings.vegetablesServ, r.servings.fruitServ, r.servings.meatServ, r.servings.milkServ, r.servings.fatsServ]
                ],
                [$translate.instant('cereals'), $translate.instant('vegetables'), $translate.instant('fruit'), $translate.instant('meat'), $translate.instant('milk'), $translate.instant('fats')],
                ['#45b7cd', '#33cc33', '#33cc33'],
                {
                    responsive: true, maintainAspectRatio: true, legend: { display: true },
                    scales: {
                        xAxes: [{ display: true, scaleLabel: { display: false }, ticks: { beginAtZero: true } }],
                        yAxes: [{ display: true, scaleLabel: { display: false }, ticks: { beginAtZero: true } }]
                    }
                },
                [
                     {
                         label: $translate.instant('choosen'),
                         borderWidth: 1,
                         type: 'bar',
                         fill: true
                     },
                     {
                         label: $translate.instant('recommended'),
                         borderWidth: 3,
                         hoverBackgroundColor: "rgba(255,99,132,0.4)",
                         hoverBorderColor: "rgba(255,99,132,1)",
                         type: 'line',
                         fill: true
                     }
                ]
        );

        $rootScope.pieGraphData = charts.createGraph(
                [$translate.instant('carbohydrates'), $translate.instant('proteins'), $translate.instant('fats')],
                [t.carbohydratesPercentage, t.proteinsPercentage, t.fatsPercentage],
                [$translate.instant('carbohydrates') + ' (%)', $translate.instant('proteins') + ' (%)', $translate.instant('fats') + ' (%)'],
                ['#f2f20f', '#28c1e0', '#ed722f'],
                {
                    responsive: true, maintainAspectRatio: false, legend: { display: true, position: 'right', "labels": { "fontSize": 14 } },
                    scales: {
                        xAxes: [{ display: false, scaleLabel: { display: false }, ticks: { beginAtZero: true } }],
                        yAxes: [{ display: false, scaleLabel: { display: false }, ticks: { beginAtZero: true } }]
                    }
                },
                []
        );

        var mealsGraphData = function (displayLegend) {
            return charts.createGraph(
              $scope.mealsTitles,
              [$scope.mealsTotals, $scope.mealsMin, $scope.mealsMax],
              $scope.mealsTitles,
              ['#45b7cd', '#ff6384', '#33cc33'],
              {
                  responsive: true, maintainAspectRatio: true, legend: { display: displayLegend },
                  scales: {
                      xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true } }],
                      yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true, stepSize: 200 } }]
                  }
              },
              [
                   {
                       label: $translate.instant('choosen') + ' (' + $translate.instant('kcal') + ')',
                       borderWidth: 1,
                       type: 'bar',
                       fill: true
                   },
                   {
                       label: $translate.instant('recommended') + ' ' + $translate.instant('from') + ' (' + $translate.instant('kcal') + ')',
                       borderWidth: 3,
                       hoverBackgroundColor: "rgba(255,99,132,0.4)",
                       hoverBorderColor: "rgba(255,99,132,1)",
                       type: 'line',
                       fill: false
                   },
                    {
                        label: $translate.instant('recommended') + ' ' + $translate.instant('to') + ' (' + $translate.instant('kcal') + ')',
                        borderWidth: 3,
                        hoverBackgroundColor: "rgba(255,99,132,0.4)",
                        hoverBorderColor: "rgba(255,99,132,1)",
                        type: 'line',
                        fill: false
                    }
              ]
            );
        }
        $rootScope.mealsGraphData_menu = mealsGraphData(false);
        $rootScope.mealsGraphData_analysis = mealsGraphData(true);

        var mealsNutriGraphData = function (displayLegend) {
            return charts.createGraph(
              ['carbs', 'proteins', 'fats'],
              [$scope.nutriMealTotals.carbohydrates, $scope.nutriMealTotals.proteins, $scope.nutriMealTotals.fats],
              $scope.mealsTitles,
              ['#f2f20f', '#28c1e0', '#ed722f'],
              {
                  responsive: true, maintainAspectRatio: true, legend: { display: displayLegend },
                  scales: {
                      xAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true } }],
                      yAxes: [{ display: true, scaleLabel: { display: true }, ticks: { beginAtZero: true, stepSize: 100 } }]
                  }
              },
              [
                   {
                       label: $translate.instant('carbohydrates') + ' (' + $translate.instant('%') + ')',
                       borderWidth: 1,
                       type: 'bar',
                       fill: true
                   },
                   {
                       label: $translate.instant('proteins') + ' (' + $translate.instant('%') + ')',
                       borderWidth: 1,
                       type: 'bar',
                       fill: true
                   },
                    {
                        label: $translate.instant('fats') + ' (' + $translate.instant('%') + ')',
                        borderWidth: 1,
                        type: 'bar',
                        fill: true
                    }
              ]
            );

        }
        $rootScope.mealsNutriGraphData_menu = mealsNutriGraphData(false);
        $rootScope.mealsNutriGraphData_analysis = mealsNutriGraphData(true);

        $scope.parametersGraphDataOther = charts.stackedChart(
            [$translate.instant('choosen')],
            [
                [t.starch, t.totalSugar, t.glucose, t.fructose, t.saccharose, t.maltose, t.lactose]
            ],
            [
                $translate.instant('starch') + ' (' + $translate.instant('g') + ')',
                $translate.instant('total sugar') + ' (' + $translate.instant('g') + ')',
                $translate.instant('glucose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('fructose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('saccharose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('maltose') + ' (' + $translate.instant('g') + ')',
                $translate.instant('lactose') + ' (' + $translate.instant('g') + ')'
            ],
            ['#33cc33'],
            '');

        //TODO
        $scope.parametersGraphData = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('recommended dietary allowance') + ' (' + $translate.instant('rda').toUpperCase() + ')'],
            [
                [
                    t.fibers,
                    t.monounsaturatedFats,
                    t.polyunsaturatedFats,
                    t.calcium,
                    t.magnesium,
                    t.phosphorus,
                    t.iron,
                    t.copper,
                    t.zinc,
                    t.manganese,
                    t.selenium,
                    t.iodine,
                    t.retinol,
                    t.vitaminD,
                    t.vitaminE,
                    t.vitaminB1,
                    t.vitaminB2,
                    t.vitaminB3,
                    t.vitaminB6,
                    t.vitaminB12,
                    t.folate,
                    t.pantothenicAcid,
                    t.biotin,
                    t.vitaminC,
                    t.vitaminK
                ],
                [
                    r.fibers.rda,
                    r.monounsaturatedFats.rda,
                    r.polyunsaturatedFats.rda,
                    r.calcium.rda,
                    r.magnesium.rda,
                    r.phosphorus.rda,
                    r.iron.rda,
                    r.copper.rda,
                    r.zinc.rda,
                    r.manganese.rda,
                    r.selenium.rda,
                    r.iodine.rda,
                    r.retinol.rda,
                    r.vitaminD.rda,
                    r.vitaminE.rda,
                    r.vitaminB1.rda,
                    r.vitaminB2.rda,
                    r.vitaminB3.rda,
                    r.vitaminB6.rda,
                    r.vitaminB12.rda,
                    r.folate.rda,
                    r.pantothenicAcid.rda,
                    r.biotin.rda,
                    r.vitaminC.rda,
                    r.vitaminK.rda
                ]
            ],
            [
                $translate.instant('fibers') + ' (' + $translate.instant('g') + ')',
                $translate.instant('monounsaturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('polyunsaturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('calcium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('magnesium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('phosphorus') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('iron') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('copper') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('zinc') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('manganese') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('selenium') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('iodine') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('retinol') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin D') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin E') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B1') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B2') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B3') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B6') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin B12') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('folate') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('pantothenic acid') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('biotin') + ' (' + $translate.instant('ug') + ')',
                $translate.instant('vitamin C') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('vitamin K') + ' (' + $translate.instant('ug') + ')',
            ],
            ['#45b7cd', '#33cc33'],
            $translate.instant('parameters'));

        $scope.parametersGraphDataUI = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('upper intake level') + ' (' + $translate.instant('ul').toUpperCase() + ')'],
            [
                [t.saturatedFats, t.trifluoroaceticAcid, t.cholesterol],
                [r.saturatedFats.ui, r.trifluoroaceticAcid.ui, r.cholesterol.ui]
            ],
            [
                $translate.instant('saturated fats') + ' (' + $translate.instant('g') + ')',
                $translate.instant('trifluoroacetic acid') + ' (' + $translate.instant('g') + ')',
                $translate.instant('cholesterol') + ' (' + $translate.instant('mg') + ')'
            ],
            ['#f44242', '#33cc33'],
            '');

        //TODO
        $scope.parametersGraphDataMDA = charts.stackedChart(
            [$translate.instant('choosen'), $translate.instant('upper intake level') + ' (' + $translate.instant('ul').toUpperCase() + ')', $translate.instant('minimum dietary allowance') + ' (' + $translate.instant('mda').toUpperCase() + ')'],
            [
                [t.sodium, t.potassium, t.chlorine],
                [r.sodium.ui],
                [r.sodium.mda, r.potassium.mda, r.chlorine.mda]
            ],
            [
                $translate.instant('sodium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('potassium') + ' (' + $translate.instant('mg') + ')',
                $translate.instant('chlorine') + ' (' + $translate.instant('mg') + ')'
            ],
            ['#45b7cd', '#33cc33'],
            '');

    }

    var totalEnergyChart = function () {
        if ($rootScope.totals === null) return;
        var recommended = parseInt($rootScope.recommendations.energy);
        var id = 'energyChart';
        var value = $rootScope.totals.energy.toFixed(0);
        var unit = 'kcal';

        var options = {
            title: 'energy',
            min: 0,
            max: recommended + (recommended * 0.06),
            greenFrom: recommended - (recommended * 0.02),
            greenTo: recommended + (recommended * 0.02),
            yellowFrom: recommended + (recommended * 0.02),
            yellowTo: recommended + (recommended * 0.04),
            redFrom: recommended + (recommended * 0.04),
            redTo: recommended + (recommended * 0.06),
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isEnergyOk = function () {
            if (value < recommended - (recommended * 0.02)) {
                return 'fa fa-minus';
            }
            if (value >= recommended - (recommended * 0.02) && value <= recommended + (recommended * 0.04)) {
                return 'fa fa-check';
            }
            if (value > recommended + (recommended * 0.04)) {
                return 'fa fa-exclamation';
            }
        }
    }

    var otherFoodChart = function () {
        var suggested = $rootScope.recommendations.servings.otherFoodsEnergy;
        var id = 'otherFoodChart';
        var value = $rootScope.totals.servings.otherFoodsEnergy.toFixed(0);
        var unit = 'kcal';

        var options = {
            title: 'energy',
            min: 0,
            max: suggested + (suggested * 0.5),
            greenFrom: 0,
            greenTo: suggested - (suggested * 0.02),
            yellowFrom: suggested - (suggested * 0.02),
            yellowTo: suggested,
            redFrom: suggested,
            redTo: suggested + (suggested * 0.5),
            minorTicks: 5
        };
        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isOtherFoodOk = function () {
            return value < suggested ? 'label label-success fa fa-check' : 'label label-danger fa fa-exclamation';
        }
    }

    var carbohydratesChart = function () {
        var recommended = {
            from: $rootScope.recommendations.carbohydratesPercentageMin,
            to: $rootScope.recommendations.carbohydratesPercentageMax
        }
        var id = 'carbohydratesChart';
        var value = $rootScope.totals.carbohydratesPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'carbohidrates',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isCarbohydratesOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var proteinsChart = function () {
        var recommended = {
            from: $rootScope.recommendations.proteinsPercentageMin,
            to: $rootScope.recommendations.proteinsPercentageMax
        }
        var id = 'proteinsChart';
        var value = $rootScope.totals.proteinsPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'proteins',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isProteinsOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var fatsChart = function () {
        var recommended = {
            from: $rootScope.recommendations.fatsPercentageMin,
            to: $rootScope.recommendations.fatsPercentageMax
        }
        var id = 'fatsChart';
        var value = $rootScope.totals.fatsPercentage.toFixed(0);
        var unit = '%';

        var options = {
            title: 'fats',
            min: 0,
            max: 100,
            greenFrom: recommended.from,
            greenTo: recommended.to - (recommended.to * 0.02),
            yellowFrom: recommended.to - (recommended.to * 0.02),
            yellowTo: recommended.to,
            redFrom: recommended.to,
            redTo: 100,
            minorTicks: 5
        };

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, unit, options));

        $scope.isFatsOk = function () {
            if (value < recommended.from) {
                return 'fa fa-minus';
            }
            if (value >= recommended.from && value <= recommended.to) {
                return 'fa fa-check';
            }
            if (value > recommended.to) {
                return 'fa fa-plus';
            }
        }
    }

    var saturatedFatsChart = function () {
        var id = 'saturatedFatsChart';
        var value = $rootScope.totals.saturatedFats;
        unit = 'g';
        var options = {
            title: 'saturated fats',
            min: 0,
            max: Math.round($rootScope.recommendations.saturatedFats.ui + $rootScope.recommendations.saturatedFats.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.saturatedFats.ui - $rootScope.recommendations.saturatedFats.ui * 0.2,
            yellowFrom: $rootScope.recommendations.saturatedFats.ui - $rootScope.recommendations.saturatedFats.ui * 0.2,
            yellowTo: $rootScope.recommendations.saturatedFats.ui,
            redFrom: $rootScope.recommendations.saturatedFats.ui,
            redTo: $rootScope.recommendations.saturatedFats.ui + $rootScope.recommendations.saturatedFats.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }

    var trifluoroaceticAcidChart = function () {
        var id = 'trifluoroaceticAcidChart';
        var value = $rootScope.totals.trifluoroaceticAcid;
        unit = 'g';
        var options = {
            title: 'trifluoroacetic acid',
            min: 0,
            max: Math.round($rootScope.recommendations.trifluoroaceticAcid.ui + $rootScope.recommendations.trifluoroaceticAcid.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.trifluoroaceticAcid.ui - $rootScope.recommendations.trifluoroaceticAcid.ui * 0.2,
            yellowFrom: $rootScope.recommendations.trifluoroaceticAcid.ui - $rootScope.recommendations.trifluoroaceticAcid.ui * 0.2,
            yellowTo: $rootScope.recommendations.trifluoroaceticAcid.ui,
            redFrom: $rootScope.recommendations.trifluoroaceticAcid.ui,
            redTo: $rootScope.recommendations.trifluoroaceticAcid.ui + $rootScope.recommendations.trifluoroaceticAcid.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }

    var cholesterolChart = function () {
        var id = 'cholesterolChart';
        var value = $rootScope.totals.cholesterol;
        unit = 'mg';
        var options = {
            title: 'cholesterol',
            min: 0,
            max: Math.round($rootScope.recommendations.cholesterol.ui + $rootScope.recommendations.cholesterol.ui * 0.4),
            greenFrom: 0,
            greenTo: $rootScope.recommendations.cholesterol.ui - $rootScope.recommendations.cholesterol.ui * 0.2,
            yellowFrom: $rootScope.recommendations.cholesterol.ui - $rootScope.recommendations.cholesterol.ui * 0.2,
            yellowTo: $rootScope.recommendations.cholesterol.ui,
            redFrom: $rootScope.recommendations.cholesterol.ui,
            redTo: $rootScope.recommendations.cholesterol.ui + $rootScope.recommendations.cholesterol.ui * 0.4,
            minorTicks: 5
        };
        charts.guageChart(id, value, unit, options);
    }

    var mealEnergyChart = function (idx) {
        var recommended = $rootScope.recommendations.energy;
        var id = 'mealEnergyChart_' + idx;
        var value = angular.isDefined($rootScope.totals) ? $rootScope.totals.mealsTotal[idx].energy.val : 0;

        $scope.testmealenergy = value;

        var title = 'Energija';
        var min = 0;
        var max = recommended + (recommended * 0.2);
        var greenFrom = recommended - (recommended * 0.02);
        var greenTo = recommended + (recommended * 0.02);
        var yellowFrom = recommended + (recommended * 0.02);
        var yellowTo = recommended + (recommended * 0.04);
        var redFrom = recommended + (recommended * 0.04);
        var redTo = max;
        var minorTicks = 5;

        google.charts.load('current', { 'packages': ['gauge'] });
        google.charts.setOnLoadCallback(charts.guageChart(id, value, title, min, max, greenFrom, greenTo, yellowFrom, yellowTo, redFrom, redTo, minorTicks));
    }

    $scope.moveUp = function (idx) {
        var _idx = idx;
        if (idx > 0) {
            angular.forEach($rootScope.currentMenu.data.selectedFoods, function (value, key) {
                if (value.meal.code == $rootScope.currentMeal) {
                    if (key < idx) {
                        _idx = key + 1;
                    }
                }
            });
            var tmp = $rootScope.currentMenu.data.selectedFoods[_idx - 1];
            $rootScope.currentMenu.data.selectedFoods[_idx - 1] = $rootScope.currentMenu.data.selectedFoods[idx];
            $rootScope.currentMenu.data.selectedFoods[idx] = tmp;
            var tmp_init = $rootScope.currentMenu.data.selectedInitFoods[_idx - 1];
            $rootScope.currentMenu.data.selectedInitFoods[_idx - 1] = $rootScope.currentMenu.data.selectedInitFoods[idx];
            $rootScope.currentMenu.data.selectedInitFoods[idx] = tmp_init;
        }
    }

    $scope.moveDown = function (idx) {
        var _idx = idx;
        if (idx < $rootScope.currentMenu.data.selectedFoods.length - 1) {
            var keepGoing = true;
            angular.forEach($rootScope.currentMenu.data.selectedFoods, function (value, key) {
                if (value.meal.code == $rootScope.currentMeal && keepGoing == true) {
                    if (key > idx && key > 0) {
                        _idx = key - 1;
                        keepGoing = false;
                    }
                }
            });
            var tmp = $rootScope.currentMenu.data.selectedFoods[_idx + 1];
            $rootScope.currentMenu.data.selectedFoods[_idx + 1] = $rootScope.currentMenu.data.selectedFoods[idx];
            $rootScope.currentMenu.data.selectedFoods[idx] = tmp;
            var tmp_init = $rootScope.currentMenu.data.selectedInitFoods[_idx + 1];
            $rootScope.currentMenu.data.selectedInitFoods[_idx + 1] = $rootScope.currentMenu.data.selectedInitFoods[idx];
            $rootScope.currentMenu.data.selectedInitFoods[idx] = tmp_init;
        }
    }

    $scope.filterMeal = function (x) {
        return x.meal.code === $rootScope.currentMeal;
    }

    $scope.parameterStyle = function (total, r) {
        if (!angular.isDefined(total) || !angular.isDefined(r)) { return; }
        if (r.mda !== null && total < r.mda) {
            return 'background-color:#9bc1ff; color:white';
        }
        if (r.ui != null && total > r.ui) {
            return 'background-color:#f94040; color:white';
        }
    }

    $scope.mealParameterStyle = function (total, r) {
        if (!angular.isDefined(total.val) || !angular.isDefined(r)) { return; }
        if (r.ui !== null && total.val > r.ui) {
            return 'background-color:#f94040; color:white';
        }
    }

    $scope.openRecipePopup = function () {
        openRecipePopup();
    }

    var openRecipePopup = function () {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        $mdDialog.show({
            controller: getRecipePopupCtrl,
            templateUrl: 'assets/partials/popup/getrecipe.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            clientData: $rootScope.clientData,
            config: $rootScope.config
        })
        .then(function (recipe) {
            angular.forEach(recipe.data.selectedFoods, function (value, key) {
                var idx = $rootScope.currentMenu.data.selectedFoods.length;
                value.id = value.id + ';' + recipe.id;
                $scope.addFoodToMeal(value, recipe.data.selectedInitFoods[key], idx);
            });
            var dishDescList = angular.copy($rootScope.currentMenu.splitMealDesc.find(a => a.code === $rootScope.currentMeal).dishDesc);
            var loop = true;
            angular.forEach(dishDescList, function (value, key) {
                if (loop) {
                    if (value.title === null && value.desc === null) {
                        $rootScope.currentMenu.splitMealDesc.find(a => a.code === $rootScope.currentMeal).dishDesc[key] = recipe.dishDesc;
                        loop = false;
                    } else {
                        $rootScope.currentMenu.splitMealDesc.find(a => a.code === $rootScope.currentMeal).dishDesc.push(recipe.dishDesc);
                        loop = false;
                    }
                }
            });

            getTotals($rootScope.currentMenu);
        }, function () {
        });
    };

    var getRecipePopupCtrl = function ($scope, $mdDialog, $http, clientData, config, $translate, $translatePartialLoader, $timeout, $state) {
        $scope.clientData = clientData;
        $scope.config = config;
        $scope.user = $rootScope.user;
        $scope.loadType = 0;
        $scope.type = 0;
        $scope.appRecipes = false;
        $scope.toTranslate = false;
        $scope.toLanguage = '';
        $scope.showDescription = true;
        $scope.limit = 20;
        $scope.searchValue = null;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            $scope.loading = true;
            $scope.appRecipes = false;
            functions.post('Recipes', 'Load', { userId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.appRecipes = false;
                $scope.d = d;
                $scope.loading = false;
            });
        }
        load();

        var init = function () {
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            functions.post('Recipes', 'Init', {}).then(function (d) {
                $scope.recipe = d;
                $scope.recipe.mealGroups.unshift({ code: null, title: 'all' });
                $scope.mealGroup = d.mealGroup;
            });
        }
        init();

        $scope.load = function () {
            load();
        }

        $scope.search = function (query, mealGroup) {
            if (mealGroup === null) {
                $scope.mealGroup = {
                    code: null,
                    title: $translate.instant('all')
                }
            }
            functions.post('Recipes', 'Search', { userId: $rootScope.user.userGroupId, query: query, mealGroup: mealGroup }).then(function (d) {
                $scope.d = d;
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.toggleMyRecipeTpl = function () {
            $mdDialog.cancel();
            $state.go('myrecipes');
            $rootScope.selectedNavItem = 'myrecipes';
        }

        $scope.get = function (x) {
            get(x);
        }

        var get = function (x, showDescription) {
            functions.post('Recipes', 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $scope.recipe = d;
                if (showDescription == true) {
                    angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                        if (value.code == $rootScope.currentMeal) {
                            value.description = value.description == '' ? $scope.recipe.title + '.\n' + $scope.recipe.description : value.description + '\n' + $scope.recipe.title + '.\n' + $scope.recipe.description;
                        }
                    });
                }
                $mdDialog.hide($scope.recipe);
            });
        }

        $scope.loadAppRecipes = function () {
            $scope.appRecipes = true;
            functions.post('Recipes', 'LoadAppRecipes', { lang: $rootScope.config.language }).then(function (d) {
                $scope.d = d;
            });
        }

        var getAppRecipe = function (x, showDescription) {
            functions.post('Recipes', 'GetAppRecipe', { id: x.id, lang: $rootScope.config.language, toTranslate: $scope.toTranslate }).then(function (d) {
                $scope.recipe = d;
                if (showDescription == true) {
                    angular.forEach($rootScope.currentMenu.data.meals, function (value, key) {
                        if (value.code == $rootScope.currentMeal) {
                            value.description = value.description == '' ? $scope.recipe.description : value.description + '\n' + $scope.recipe.description;
                        }
                    });
                }
                $mdDialog.hide($scope.recipe);

                //**********TODO - translate recipes*****************
                //var menu = JSON.parse(response.data.d);
                //if ($scope.toTranslate == true) {
                //    translateFoods(menu);
                //}
                //$mdDialog.hide(menu);
                //****************************************************
            });
        }

        $scope.confirm = function (x, showDescription) {
            $scope.appRecipes == true ? getAppRecipe(x, showDescription) : get(x, showDescription);
        }

        $scope.setToTranslate = function (x) {
            $scope.toTranslate = x;
        }

        $scope.setToLanguage = function (x) {
            $scope.toLanguage = x;
        }

        var translateFoods = function (menu) {
            $rootScope.setLanguage($scope.toLanguage);
            $timeout(function () {
                angular.forEach(menu.data.selectedFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                angular.forEach(menu.data.selectedInitFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                $mdDialog.hide(menu);
                $rootScope.setLanguage('hr');
            }, 500);
        }

    };

    var getWeeklyMenuTotals = function (x) {
        functions.post('WeeklyMenus', 'GetWeeklyMenusTotals', { userId: $sessionStorage.usergroupid, menuList: x }).then(function (d) {
            $rootScope.totals = d;
            $rootScope.totals.price.currency = $rootScope.config.currency;
            displayCharts();
        });
    }
    $scope.getWeeklyMenuTotals = function (x) {
        return getWeeklyMenuTotals(x);
    }

    $scope.toggleMenu = function (x) {
        $scope.loading = true;
        $timeout(function () {
            $scope.loading = false;
            $rootScope.menuTpl = x;
            if (x == 'dailyMenuTpl') {
                getTotals($rootScope.currentMenu);
            }
        }, 700);
    }
    if ($rootScope.menuTpl !== 'weeklyMenuTpl') {
        $rootScope.menuTpl = 'dailyMenuTpl';
    }

    var printDailyMenu = function () {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            functions.alert($translate.instant('menu is empty') + '!', '');
            return false;
        }
        $scope.creatingPdf1 = true;
        var img = imageData();

        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            functions.post('PrintPdf', 'MenuDetailsPdf', { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, calculation: $rootScope.calculation, totals: $rootScope.totals, recommendations: $rootScope.recommendations, lang: $rootScope.config.language, imageData: img, headerInfo: $rootScope.user.headerInfo }).then(function (d) {
                var fileName = d;
                $scope.creatingPdf1 = false;
                $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
            });
        }
    }

    var printWeeklyMenu = function () {
        $scope.creatingPdf1 = true;
        if (angular.isDefined($rootScope.currentMenu)) {
            var currentMenu = angular.copy($rootScope.currentMenu);
            currentMenu.title = $rootScope.weeklyMenu.title;
            currentMenu.note = $rootScope.weeklyMenu.note;
            currentMenu.diet = $rootScope.weeklyMenu.diet.diet;
            var img = imageData();
            functions.post('PrintPdf', 'MenuDetailsPdf', { userId: $sessionStorage.usergroupid, currentMenu: currentMenu, calculation: $rootScope.calculation, totals: $rootScope.totals, recommendations: $rootScope.recommendations, lang: $rootScope.config.language, imageData: img, headerInfo: $rootScope.user.headerInfo }).then(function (d) {
                var fileName = d;
                $scope.creatingPdf1 = false;
                $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
            });
        }
    }

    var imageData = function () {
        var img = [];
        if ($scope.showCharts) {
            img = getImageData(img, "mealsChart");
            img = getImageData(img, "mealsNutriChart");
            img = getImageData(img, "servChart");
            img = getImageData(img, "pieChart");
            img = getImageData(img, "parametersGraphDataOther");
            img = getImageData(img, "parametersGraphDataUI");
            img = getImageData(img, "parametersGraphDataMDA");
            img = getImageData(img, "parametersGraphData");
        }
        return img;
    }
    var getImageData = function (img, id) {
        if (document.getElementById(id) != null) {
            img.push(document.getElementById(id).toDataURL("image/png").replace(/^data:image\/(png|jpg);base64,/, ""));
        }
        return img;
    }

    $scope.showCharts = true;
    $scope.pdfLink == null;
    $scope.creatingPdf1 = false;
    $scope.printMenuDetailsPdf = function () {
        if ($rootScope.menuTpl === 'dailyMenuTpl') {
            printDailyMenu();
        } else {
            printWeeklyMenu();
        }
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }
    //----------------------------------------

    $scope.saveRecipeFromMenu = function (data, currentMeal) {
        if (data.selectedFoods.length == 0) {
            return false;
        }
        $state.go('myrecipes');
        $rootScope.selectedNavItem = 'myrecipes';
        $rootScope.recipeData = data;
        $rootScope.currentMealForRecipe = currentMeal;
    }

    $scope.getMealTotal = function (x) {
        var total = null;
        angular.forEach($rootScope.totals.mealsTotal, function (value, key) {
            if (value.code == x) {
                total = value;
            }
        })
        return total;
    }

    $scope.getMealRecommendation = function (x) {
        var recommendations = null;
        angular.forEach($rootScope.recommendations.mealsRecommendationEnergy, function (value, key) {
            if (value.meal.code == x) {
                recommendations = value.meal;
            }
        })
        return recommendations;
    }

    $scope.checkTotal = function (total, min, max) {
        var icon = 'pull-right fa fa-';
        if (total > max) {
            return icon + 'chevron-circle-right text-danger';
        } else if (total < min) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkEnergy = function (total, r) {
        var icon = 'pull-right fa fa-';
        if ((total / r) - 1 > 0.05) {
            return icon + 'chevron-circle-right text-danger';
        } else if ((total / r) - 1 < -0.05) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkServ = function (total, r) {
        var icon = 'pull-right fa fa-';
        if ((total - r) > 1) {
            return icon + 'chevron-circle-right text-danger';
        } else if ((total - r) < -1) {
            return icon + 'chevron-circle-left text-info';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.checkOtherFoods = function (total, r) {
        var icon = 'pull-right fa fa-';
        if (total > r) {
            return icon + 'chevron-circle-right text-danger';
        } else {
            return icon + 'check-circle text-success';
        }
    }

    $scope.shoppingList = [];
    $scope.getShoppingList = function (x) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if (!functions.checkPermission($rootScope.user, 'premium')) return;
        openShoppingListPopup(x);
    }

    var openShoppingListPopup = function (x) {
        if ($rootScope.currentMenu.data.selectedFoods.length == 0) {
            return false;
        }
        $mdDialog.show({
            controller: shoppingListPdfCtrl,
            templateUrl: 'assets/partials/popup/shoppinglist.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { currentMenu: x, settings: $scope.printSettings }
        })
        .then(function (r) {
            alert(r);
        }, function () {
        });
    };

    var shoppingListPdfCtrl = function ($scope, $rootScope, $mdDialog, $http, d, $translate, $translatePartialLoader) {
        $scope.currentMenu = d.currentMenu;
        $scope.settings = d.settings;
        $scope.consumers = 1;
        $scope.pdfLink == null;
        $scope.creatingPdf = false;
        $scope.d = null;

        var createShoppingList = function (x, c) {
            functions.post('ShoppingList', 'Create', { x: x, consumers: c, lang: $rootScope.config.language }).then(function (d) {
                $scope.d = d;
                if ($scope.d.total) {
                    if ($scope.d.total.price > 0) {
                        $scope.settings.showPrice = true;
                    }
                }
            });
        }
        createShoppingList($scope.currentMenu.data.selectedFoods, $scope.consumers);

        $scope.changeNumberOfConsumers = function (x) {
            if (x < 1 || functions.isNullOrEmpty(x)) { return false }
            createShoppingList($scope.currentMenu.data.selectedFoods, x);
        }

        $scope.copyToClipboard = function (id) {
            return functions.copyToClipboard(id);
        }

        $scope.printShoppingListPdf = function (sl, s) {
            if ($scope.d === null) return;
            $scope.creatingPdf = true;
            s.headerInfo = $rootScope.user.headerInfo;
            if (angular.isDefined($scope.currentMenu)) {
                functions.post('PrintPdf', 'ShoppingList', { userId: $sessionStorage.usergroupid, shoppingList: sl, title: $scope.currentMenu.title, note: $scope.currentMenu.note, lang: $rootScope.config.language, settings: s }).then(function (d) {
                    var fileName = d;
                    $scope.creatingPdf = false;
                    $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
                });
            }
        }

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    };

    $scope.openPrintPdfPopup = function () {
        openPrintPdfPopup();
    }

    $scope.getFoodGroupClass = function (x) {
        if (!$rootScope.config.showfoodgroupscolors) {
            return '';
        }
        switch (x) {
            case 'C': return 'bg-cereals'; break;
            case 'V': return 'bg-vegetables'; break;
            case 'F': return 'bg-fruit'; break;
            case 'M': case 'EUM': case 'NFM': case 'MFM': case 'FFM': return 'bg-meat'; break;
            case 'MI': case 'LFMI': case 'SMI': case 'FFMI': return 'bg-milk'; break;
            case 'FA': case 'SF': case 'UF': case 'MUF': return 'bg-fat'; break;
            case 'OF': return 'bg-otherfoods'; break;
            case 'MF': return 'bg-mixedfoods'; break;
            case 'PM': return 'bg-preparedmeals'; break;
            default:
                return '';
        }
    }

    $scope.openColorGroupsInfoPopup = function () {
        $mdDialog.show({
            controller: colorGroupsInfoPopupCtrl,
            templateUrl: 'assets/partials/popup/colorfoodgroups.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true
        })
        .then(function (recipe) {
        }, function () {
        });
    };
    var colorGroupsInfoPopupCtrl = function ($scope, $mdDialog) {
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    };

    $scope.mealDescHelp = function (x) {
        $scope.showMenuDescHelp = x;
    }
    $scope.mealDescHelp(false);

    $scope.chartResp = function (h, h_resp) {
        return window.innerWidth < 767 ? h_resp : h;
    }

    $scope.clearMeal = function (currentMenu, meal) {
        $rootScope.currentMenu.data.selectedFoods = currentMenu.data.selectedFoods.filter(a => a.meal.code !== meal);
        $rootScope.currentMenu.data.selectedInitFoods = currentMenu.data.selectedInitFoods.filter(a => a.meal.code !== meal);
        $rootScope.currentMenu.data.meals.filter(a => a.code === meal)[0].description = null;
        $rootScope.currentMenu.splitMealDesc.filter(a => a.code === meal)[0].dishDesc = [{ title: null, desc: null, id: null }];
        getTotals($rootScope.currentMenu);
    }

    $scope.isVisibleClearMealBtn = function (selectedFoods, meal) {
        return selectedFoods.length > 1 ? selectedFoods.filter(a => a.meal.code === meal).length > 1 : false;
    }

    $scope.showMealMicronutrients = false;
    $scope.toggleMealMicronutrients = function (show) {
        if (!functions.checkPermission($rootScope.user, 'premium')) return;
        $scope.showMealMicronutrients = !show;
    }

}])

.controller('myFoodsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$timeout', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $timeout) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'MyFoods';
    $scope.unit = null;

    var init = function () {
        functions.post('Foods', 'Init', { lang: $rootScope.config.language }).then(function (d) {
            var res = d;
            if ($rootScope.myFood_ !== undefined) {
                if ($rootScope.myFood_ != null) {
                    $scope.myFood = $rootScope.myFood_;
                    $rootScope.myFood_ = null;
                } else {
                    $scope.myFood = res.food;
                }
            } else {
                $scope.myFood = res.food;
            }
            $scope.units = res.units;
            $scope.mainFoodGroups = res.foodGroups;
            $('.selectpicker').selectpicker({
                style: 'btn-default',
                size: 4
            });
        });
    };
    init();

    $scope.new = function () {
        init();
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete food') + '?')
            .textContent(x.food)
            .targetEvent(x)
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        functions.post(webService, 'Delete', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
            $rootScope.loadFoods();
            init();
        });
    }

    var loadMyFoods = function () {
        functions.post(webService, 'Load', { userId: $sessionStorage.usergroupid }).then(function (d) {
            var data = d;
            $rootScope.myFoods = data.foods;
        });
    }

    $scope.save = function (x) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if (functions.isNullOrEmpty(x.food)) {
            functions.alert($translate.instant('food title is required'), '');
            return false;
        }
        x.unit = $scope.unit;
        if (functions.isNullOrEmpty(x.unit)) {
            functions.alert($translate.instant('unit is required'), '');
            return false;
        }
        if (checkIsOtherFood(x) == true) {
            x.servings.cerealsServ = 0;
            x.servings.vegetablesServ = 0;
            x.servings.fruitServ = 0;
            x.servings.meatServ = 0;
            x.servings.milkServ = 0;
            x.servings.fatsServ = 0;
            x.servings.otherFoodsServ = 1;
            x.servings.otherFoodsEnergy = x.energy;
            x.foodGroup.code = 'OF';
        };
        functions.post(webService, 'Save', { userId: $rootScope.user.userGroupId, x: x }).then(function (d) {
            if (d === undefined) {
                functions.alert($translate.instant('error').toUpperCase() + '!', $translate.instant('If the value of a particular parameter is unknown, leave zero') + '.');
                return;
            }
            if (d.isSuccess) {
                $rootScope.loadFoods();
            } else {
                functions.alert($translate.instant(d.msg), d.msg1 !== null ? $translate.instant(d.msg1) : null);
            }
        });
    };

    var checkIsOtherFood = function (x) {
        if (x.servings.cerealsServ > 0 ||
            x.servings.vegetablesServ > 0 ||
            x.servings.fruitServ > 0 ||
            x.servings.meatServ > 0 ||
            x.servings.milkServ > 0 ||
            x.servings.fatsServ > 0) {
            x.servings.otherFoodsServ = 0;
            x.servings.otherFoodsEnergy = 0;
            return false;
        } else {
            return true;
        }
    }

    $scope.search = function () {
        openMyFoodsPopup();
    }

    var openMyFoodsPopup = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $mdDialog.show({
            controller: getMyFoodsPopupCtrl,
            templateUrl: 'assets/partials/popup/myfoods.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (d) {
            $scope.myFood = d;
        }, function () {
        });
    };

    var getMyFoodsPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.loading = true;
            functions.post('MyFoods', 'Load', { userId: $sessionStorage.usergroupid }).then(function (d) {
                var data = d;
                $scope.d = data.foods;
                $scope.loading = false;
            });
        };
        load();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post('MyFoods', 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                var myFood = d;
                $mdDialog.hide(myFood);
            });
        }

        $scope.confirm = function (x) {
            get(x);
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant('delete food') + '?')
                .textContent(x.food)
                .targetEvent(x)
                .ok($translate.instant('yes'))
                .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
                openMyFoodsPopup();
            }, function () {
                openMyFoodsPopup();
            });
        };

        var remove = function (x) {
            functions.post('MyFoods', 'Delete', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $rootScope.loadFoods();
                load();
            });
        }
    };

    //***** USDA *****
    $scope.openUsdaPopup = function () {
        $mdDialog.show({
            controller: usdaPopupCtrl,
            templateUrl: 'assets/partials/popup/usda.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (res) {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Init',
                method: "POST",
                data: { lang: $rootScope.config.language }
            })
            .then(function (response) {
                var r = JSON.parse(response.data.d);
                $scope.myFood = r.food;
                $scope.units = r.units;
                $scope.mainFoodGroups = r.foodGroups;

                /***** USDA *****/
                var d = res.nutrients;
                var portion = res.portion;
                $scope.myFood.food = d.description;
                $scope.myFood.mass = portion === undefined || portion == 100 ? 100 : portion.gramWeight;
                $scope.myFood.unit = portion === undefined || portion == 100 ? '' : portion.portionDescription == 'Quantity not specified' ? '' : portion.portionDescription;
                $scope.myFood.proteins = nutriAmount(d.foodNutrients, 1003);
                $scope.myFood.fats = nutriAmount(d.foodNutrients, 1004);
                $scope.myFood.carbohydrates = nutriAmount(d.foodNutrients, 1005);
                $scope.myFood.energy = nutriAmount(d.foodNutrients, 1008);
                $scope.myFood.totalSugar = nutriAmount(d.foodNutrients, 2000);
                $scope.myFood.fibers = nutriAmount(d.foodNutrients, 1079);
                $scope.myFood.calcium = nutriAmount(d.foodNutrients, 1087);
                $scope.myFood.iron = nutriAmount(d.foodNutrients, 1089);
                $scope.myFood.magnesium = nutriAmount(d.foodNutrients, 1090);
                $scope.myFood.phosphorus = nutriAmount(d.foodNutrients, 1091);
                $scope.myFood.potassium = nutriAmount(d.foodNutrients, 1092);
                $scope.myFood.sodium = nutriAmount(d.foodNutrients, 1093);
                $scope.myFood.zinc = nutriAmount(d.foodNutrients, 1095);
                $scope.myFood.copper = nutriAmount(d.foodNutrients, 1098);
                $scope.myFood.selenium = nutriAmount(d.foodNutrients, 1103);
                $scope.myFood.retinol = nutriAmount(d.foodNutrients, 1105);
                $scope.myFood.carotene = nutriAmount(d.foodNutrients, 1107);
                $scope.myFood.vitaminE = nutriAmount(d.foodNutrients, 1109);
                $scope.myFood.vitaminD = nutriAmount(d.foodNutrients, 1114);
                $scope.myFood.vitaminC = nutriAmount(d.foodNutrients, 1162);
                $scope.myFood.vitaminB6 = nutriAmount(d.foodNutrients, 1175);
                $scope.myFood.folate = nutriAmount(d.foodNutrients, 1177);
                $scope.myFood.vitaminB12 = nutriAmount(d.foodNutrients, 1178);
                $scope.myFood.vitaminK = nutriAmount(d.foodNutrients, 1185);
                $scope.myFood.cholesterol = nutriAmount(d.foodNutrients, 1253);
                $scope.myFood.saturatedFats = nutriAmount(d.foodNutrients, 1258);
                $scope.myFood.monounsaturatedFats = nutriAmount(d.foodNutrients, 1292);
                $scope.myFood.polyunsaturatedFats = nutriAmount(d.foodNutrients, 1293);
                /***** USDA *****/

            },
            function (response) {
                alert(response.data.d)
            });
        }, function () {
        });
    };

    var nutriAmount = function (foodNutrients, id) {
        var x = foodNutrients.find(a => a.nutrient.id === id);
        return x !== undefined ? x.amount : 0;
    }

    var usdaPopupCtrl = function ($scope, $mdDialog, $http) {
        var webService = 'Usda.asmx';
        $scope.loading = false;
        $scope.loading1 = false;
        $scope.page = 1;
        $scope.foods = null;
        $scope.d = null;
        $scope.d_ = null;
        $scope.fdcId = null;
        $scope.searchValue = null;
        $scope.pages = [];
        $scope.gramWeight = 100;

        $scope.initPages = function (n) {
            $scope.pages = [];
            for (var i = 1; i <= n; i++) {
                $scope.pages.push(i);
            }
        }
        $scope.initPages(5);

        var checkUserType = function () {
            if (!functions.checkPermission($rootScope.user, 'premium')) return;
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.search = function (searchValue, page) {
            $scope.loading = true;
            var param = null;
            if (searchValue !== null) {
                checkUserType();
                param = 'generalSearchInput=' + searchValue + '&pageNumber=' + page;
            } else {
                if (page > 1) {
                    checkUserType();
                }
                param = 'pageNumber=' + page;
            }
            $http({
                url: $sessionStorage.config.backend + webService + '/Search',
                method: "POST",
                data: { param }
            })
          .then(function (response) {
              if (response.data.d == 'The operation has timed out') {
                  functions.alert(response.data.d);
                  $scope.loading = false;
                  return;
              }
              $scope.foods = JSON.parse(response.data.d);
              $scope.page = page;
              if (page === 1) {
                  $scope.initPages($scope.foods.totalPages > 5 ? 5 : $scope.foods.totalPages);
              }
              $scope.loading = false;
          },
          function (response) {
              $scope.loading = false;
              alert(response.data.d);
          });
        }
        $scope.search(null, 1);

        $scope.get = function (x) {
            if (x === null) { return false; }
            $scope.loading1 = true;
            $http({
                url: $sessionStorage.config.backend + webService + '/Get',
                method: "POST",
                data: { id: x }
            })
          .then(function (response) {
              if (response.data.d == 'The operation has timed out') {
                  functions.alert(response.data.d);
                  $scope.loading1 = false;
                  return;
              }
              $scope.d = JSON.parse(response.data.d);
              $scope.d_ = JSON.parse(response.data.d);
              $scope.gramWeight = 100;
              $scope.loading1 = false;
          },
          function (response) {
              $scope.loading1 = false;
              alert(response.data.d);
          });
        }

        $scope.confirm = function (x, portion) {
            checkUserType();
            var res = {
                nutrients: x,
                portion: portion
            }
            $mdDialog.hide(res);
        }

        $scope.nextPage = function (searchValue, x) {
            if ($scope.page > 4 && x === 1) {
                $scope.pages.push($scope.page + 1);
                $scope.pages.shift();
            }
            if ($scope.page > 5 && x === -1) {
                $scope.pages.pop();
                $scope.pages.unshift($scope.pages[0] - 1);
            }

            $scope.page = $scope.page + x;
            $scope.search(searchValue, $scope.page);
        }

        $scope.changeFoodPortions = function (gramWeight) {
            if (gramWeight === undefined) { gramWeight = 100 }
            angular.forEach($scope.d_.foodNutrients, function (value, key) {
                if (value.amount !== undefined) {
                    $scope.d.foodNutrients[key].amount = (value.amount * gramWeight / 100).toFixed(2);
                }
            })
            $scope.gramWeight = gramWeight;
        }

        $scope.show = function (x) {
            return Number.isInteger(x) || x == 'Quantity not specified' ? false : true;
        }

    };

}])

.controller('myRecipesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'Recipes.asmx';
    $scope.addFoodBtnIcon = 'fa fa-plus';
    $scope.addFoodBtn = false;

    var init = function () {
        functions.post('Recipes', 'Init', {}).then(function (d) {
            $scope.d = d;
            $scope.currentRecipe = null;
            $rootScope.totals = null;
            recipeFromMenu();
            load();
        });
    };

    var load = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $rootScope.loading = true;
        functions.post('Recipes', 'Load', { userId: $sessionStorage.usergroupid }).then(function (d) {
            $scope.recipes = d;
            $rootScope.loading = false;
        });
    };

    var recipeFromMenu = function () {
        if (angular.isDefined($rootScope.recipeData)) {
            if ($rootScope.recipeData != null) {
                if (angular.isDefined($rootScope.recipeData.selectedFoods)) {
                    angular.forEach($rootScope.recipeData.selectedFoods, function (value, key) {
                        if (value.meal.code == $rootScope.currentMealForRecipe) {
                            $scope.d.data.selectedFoods.push(value);
                            $scope.d.data.selectedInitFoods.push($rootScope.recipeData.selectedFoods[key]);
                        }
                    })
                    var splitMealDesc = $rootScope.currentMenu.splitMealDesc.filter(a => a.code === $rootScope.currentMealForRecipe)
                    if (splitMealDesc.length > 0) {
                        $scope.d.title = splitMealDesc[0].dishDesc[0].title;
                        $scope.d.description = splitMealDesc[0].dishDesc[0].desc;
                    }
                }
            }
        }
    }

    init();

    $scope.add = function (x) {
        $scope.d.push(x);
    }

    $scope.openFoodPopup = function (food, idx) {
        $scope.addFoodBtn = true;
        $scope.addFoodBtnIcon = 'fa fa-spinner fa-spin';
        $mdDialog.show({
            controller: $rootScope.foodPopupCtrl,
            templateUrl: 'assets/partials/popup/food.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { foods: $rootScope.foods, myFoods: $rootScope.myFoods, foodGroups: $rootScope.foodGroups, food: food, idx: idx, config: $rootScope.config }
        })
        .then(function (x) {
            $scope.food = x;
            if (idx == null) {
                $scope.d.data.selectedFoods.push(x.food);
                $scope.d.data.selectedInitFoods.push(x.initFood);
            } else {
                $scope.d.data.selectedFoods[idx] = x.food;
                $scope.d.data.selectedInitFoods[idx] = x.initFood;
            }
            getTotals($scope.d);
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
        }, function () {
            $scope.addFoodBtnIcon = 'fa fa-plus';
            $scope.addFoodBtn = false;
        });
    }

    $scope.new = function () {
        $rootScope.recipeData = null;
        init();
    }

    $scope.get = function (id) {
        if (id == null) {
            return false;
        }
        functions.post('Recipes', 'Get', { userId: $sessionStorage.usergroupid, id: id }).then(function (d) {
            $scope.d = d;
            getTotals($scope.d);
        });
    }

    $scope.save = function (recipe) {
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        if (recipe.title === '' || recipe.title === null) {
            functions.alert($translate.instant('enter recipe title'), '');
            return false;
        }
        if (recipe.data.selectedFoods.length == 0) {
            functions.alert($translate.instant('choose food'), '');
            return false;
        }
        functions.post('Recipes', 'Save', { userId: $sessionStorage.usergroupid, x: recipe }).then(function (d) {
            if (d.isSuccess) {
                $scope.d = d.data;
                load();
            } else {
                functions.alert($translate.instant(d.msg), d.msg1 !== null ? $translate.instant(d.msg1) : null);
            }
        });
    }

    $scope.removeFood = function (idx) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete food') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            $scope.d.data.selectedFoods.splice(idx, 1);
            $scope.d.data.selectedInitFoods.splice(idx, 1);
            getTotals($scope.d);
        }, function () {
        });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete recipe') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        $http({
            url: $sessionStorage.config.backend + webService + '/Delete',
            method: "POST",
            data: { userId: $rootScope.user.userGroupId, id: x.id }
        })
        .then(function (response) {
            init();
        },
        function (response) {
            alert(response.data.d);
        });
    }

    $scope.search = function () {
        openMyRecipesPopup();
    }

    var openMyRecipesPopup = function () {
        if ($rootScope.user.licenceStatus == 'demo') { return false; }
        $mdDialog.show({
            controller: getMyRecipesPopupCtrl,
            templateUrl: 'assets/partials/popup/myrecipes.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (resp) {
            $scope.d = resp;
            getTotals($scope.d);
        }, function () {
        });
    };

    var getMyRecipesPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;
        $scope.userId = $rootScope.user.userId;
        $scope.userGroupId = $rootScope.user.userGroupId;
        $scope.sharingRecipes = false;
        $scope.searchValue = null;
        $scope.loading = false;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.sharingRecipes = false;
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            $scope.loading = true;
            functions.post('Recipes', 'Load', { userId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.d = d;
                $scope.loading = false;
            });
        }
        load();

        $scope.load = function () {
            return load();
        }

        var init = function () {
            if ($rootScope.user.licenceStatus == 'demo') {
                return false;
            }
            functions.post('Recipes', 'Init', {}).then(function (d) {
                $scope.init = d;
                $scope.init.mealGroups.unshift({ code: null, title: 'all' });
            });
        }
        init();

        $scope.search = function (query, mealGroup, sharingRecipes) {
            if (mealGroup === null) {
                $scope.mealGroup = {
                    code: null,
                    title: $translate.instant('all')
                }
            }
            $scope.sharingRecipes = sharingRecipes;
            var service = sharingRecipes ? 'SharingRecipes' : 'Recipes';
            functions.post(service, 'Search', { userId: $rootScope.user.userGroupId, query: query, mealGroup: mealGroup }).then(function (d) {
                $scope.d = d;
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (userGroupId, x) {
            functions.post('Recipes', 'Get', { userId: userGroupId, id: x.id }).then(function (d) {
                $mdDialog.hide(d);
            });
        }

        $scope.confirm = function (x) {
            get($rootScope.user.userGroupId, x);
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant('delete recipe') + '?')
                .textContent(x.title)
                .targetEvent(x)
                .ok($translate.instant('yes'))
                .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
                openMyRecipesPopup();
            }, function () {
                openMyRecipesPopup();
            });
        };

        var remove = function (x) {
            functions.post('Recipes', 'Delete', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                init();
            });
        }

        /***** Shared Recipes *****/
        $scope.languages = $rootScope.config.languages;
        $scope.lang = null;
        var loadSharingRecipes = function (userId, status, showUserRecipes, lang) {
            $scope.lang = lang;
            $scope.loading = true;
            functions.post('SharingRecipes', 'Load', { userId: userId, status: status, showUserRecipes: showUserRecipes, lang: lang }).then(function (d) {
                $scope.d = d;
                $scope.sharingRecipes = true;
                $scope.loading = false;
            });
        }

        $scope.showUserRecipes = false;
        $rootScope.showPointer = false;
        $scope.showUserRecipes = false;
        $scope.loadSharingRecipes = function (userId, status, showUserRecipes, lang) {
            $scope.showUserRecipes = showUserRecipes;
            if (status === null && !showUserRecipes) {
                status = 1;
            }
            functions.post('SharingRecipes', 'CheckIfSharingUser', { userId: userId }).then(function (d) {
                if (d) {
                    loadSharingRecipes(userId, status, showUserRecipes, lang);
                } else {
                    $rootScope.showPointer = true;
                    functions.alert($translate.instant('share_alert'), '');
                }
            });
        };

        $scope.confirmShared = function (userId, x) {
            functions.post('SharingRecipes', 'Get', { userId: userId, id: x.sharingData.recipeId }).then(function (d) {
                var resp = d;
                if (d.sharingData.recipeOwner.userId !== userId) {
                    resp.id = null;
                }
                functions.post('SharingRecipes', 'UpdateViews', { id: x.sharingData.recipeId }).then(function (d) {
                    $mdDialog.hide(resp);
                });
            });
        }
        /***** Shared Recipes *****/

    };

    $scope.saveRecipeAsMyFood = function (recipe) {
        if (recipe.data.selectedFoods.length == 0) {
            functions.alert($translate.instant('choose food'), '');
            return false;
        }
        saveRecipeAsMyFoodPopup(recipe);
    }

    var saveRecipeAsMyFoodPopup = function (recipe) {
        $mdDialog.show({
            controller: saveRecipeAsMyFoodPopupCtrl,
            templateUrl: 'assets/partials/popup/saverecipeasmyfood.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            data: recipe
        })
        .then(function (recipe) {
            $scope.d = recipe;
        }, function () {
        });
    }

    var saveRecipeAsMyFoodPopupCtrl = function ($scope, $mdDialog, $http, data, functions, $rootScope) {
        $scope.d = {
            recipe: data,
            units: [],
            unit: null,
            titleAlert: false,
            unitAlert: false
        }

        var init = function () {
            $http({
                url: $sessionStorage.config.backend + 'Foods.asmx/Init',
                method: "POST",
                data: { lang: $rootScope.config.language }
            })
            .then(function (response) {
                var res = JSON.parse(response.data.d);
                $scope.d.units = res.units;
            },
            function (response) {
                alert(response.data.d)
            });
        };
        init();

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
            if (functions.isNullOrEmpty(x.recipe.title)) {
                $scope.d.titleAlert = true;
            } else if (functions.isNullOrEmpty(x.unit)) {
                $scope.d.unitAlert = true;
            } else {
                save(x);
            }
        }

        var save = function (x) {
            $http({
                url: $sessionStorage.config.backend + 'Recipes.asmx/SaveAsFood',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId, recipe: x.recipe, unit: x.unit }
            })
            .then(function (response) {
                $rootScope.loadFoods();
                $mdDialog.hide(x.recipe);
                functions.alert($translate.instant(response.data.d), '');
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }

        var loadMyFoods = function () {
            $http({
                url: $sessionStorage.config.backend + 'MyFoods.asmx/Load',
                method: "POST",
                data: { userId: $rootScope.user.userGroupId }
            })
            .then(function (response) {
                var data = JSON.parse(response.data.d);
                $rootScope.myFoods = data.foods;
            },
            function (response) {
                functions.alert($translate.instant(response.data.d), '');
            });
        }
    }

    $scope.printRecipePreview = function (x) {
        if (!functions.checkPermission($rootScope.user, 'premium')) return;
        $mdDialog.show({
            controller: $scope.printRecipePreviewCtrl,
            templateUrl: 'assets/partials/popup/printrecipe.html',
            parent: angular.element(document.body),
            targetEvent: '',
            clickOutsideToClose: true,
            fullscreen: $scope.customFullscreen,
            d: { recipe: x, totals: $scope.totals, settings: $scope.printSettings, config: $rootScope.config, user: $rootScope.user }
        })
        .then(function () {
        }, function () {
        });
    };

    $scope.printRecipePreviewCtrl = function ($scope, $mdDialog, d, $http) {
        $scope.recipe = d.recipe;
        $scope.totals = d.totals;
        $scope.settings = d.settings;
        $scope.config = d.config;
        $scope.author = d.user.firstName + ' ' + d.user.lastName;
        $scope.date = new Date(new Date()).toLocaleDateString();
        $scope.userId = d.user.userGroupId;

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        function initPrintSettings() {
            functions.post('PrintPdf', 'InitRecipeSettings', {}).then(function (d) {
                $scope.settings = d;
            });
        };

        $scope.changeNumberOfConsumers = function (x) {
            if (x < 1 || functions.isNullOrEmpty(x)) { return false }
            functions.post('Foods', 'ChangeNumberOfConsumers', { foods: $scope.recipe.data.selectedFoods, number: x }).then(function (d) {
                $scope.foods = d;
            });
        }
        if (angular.isDefined($scope.recipe)) {
            $scope.changeNumberOfConsumers(1);
            initPrintSettings();
        }

        $scope.copyToClipboard = function (id) {
            return functions.copyToClipboard(id);
        }

        $scope.getServDescription = function (x) {
            var des = "";
            if (x.cerealsServ > 0) { des = servDes(des, x.cerealsServ, "cereals_"); }
            if (x.vegetablesServ > 0) { des = servDes(des, x.vegetablesServ, "vegetables_"); }
            if (x.fruitServ > 0) { des = servDes(des, x.fruitServ, "fruit_"); }
            if (x.meatServ > 0) { des = servDes(des, x.meatServ, "meat_"); }
            if (x.milkServ > 0) { des = servDes(des, x.milkServ, "milk_"); }
            if (x.fatsServ > 0) { des = servDes(des, x.fatsServ, "fats_"); }
            return des;
        }

        function servDes(des, serv, title) {
            return (functions.isNullOrEmpty(des) ? '' : (des + ', ')) + serv + ' serv. ' + $translate.instant(title);
        }


        $scope.pdfLink == null;
        $scope.creatingPdf = false;

        $scope.printRecipePdf = function (settings) {
            if (angular.isDefined($scope.recipe)) {
                $scope.creatingPdf = true;
                settings.headerInfo = d.user.headerInfo;
                functions.post('PrintPdf', 'RecipePdf', { userId: $sessionStorage.usergroupid, recipe: $scope.recipe, totals: $scope.totals, lang: $rootScope.config.language, settings: settings }).then(function (d) {
                    var fileName = d;
                    $scope.creatingPdf = false;
                    $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
                });
            }
        }

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }

        $scope.setAuthor = function (x) {
            $scope.author = x;
        }

        $scope.setDate = function (x) {
            $scope.date = x;
        }

    };

    var getTotals = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Foods.asmx/GetTotals',
            method: "POST",
            data: { selectedFoods: x.data.selectedFoods, meals: null }
        })
       .then(function (response) {
           $scope.totals = JSON.parse(response.data.d);
           $scope.totals.price.currency = $rootScope.config.currency;
       },
       function (response) {
           alert(response.data.d)
       });
    }

    /********* Recipe Image *********/
    $scope.uploadImg = function () {
        var content = new FormData(document.getElementById("formUpload"));
        $http({
            url: $sessionStorage.config.backend + '/UploadRecipeImgHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            $scope.d.recipeImg = response.data;
            $scope.d.recipeImgPath = `../upload/users/${$rootScope.user.userGroupId}/recipes/${$scope.d.id}/recipeimg/${$scope.d.recipeImg}`;
        },
       function (response) {
           alert($translate.instant(response.data));
       });
    }

    $scope.removeRecipeImg = function (x) {
        if (confirm($translate.instant('remove image') + '?')) {
            removeRecipeImg(x);
        }
    }

    var removeRecipeImg = function (x) {
        $http({
            url: $sessionStorage.config.backend + 'Files.asmx/DeleteRecipeImg',
            method: 'POST',
            data: { x: x, userId: $rootScope.user.userGroupId },
        }).then(function (response) {
            $scope.d.recipeImg = response.data.d;
        },
       function (response) {
           alert($translate.instant(response.data.d));
       });
    }
    /********* Recipe Image *********/

    /********* Shared Recipes *********/
    $scope.saveSharedRecipe = function (x) {
        if (x.id === null) {
            if (x.title !== null) {
                functions.alert($translate.instant('save recipe'), '');
                $scope.d.isShared = false;
                return;
            } else {
                functions.alert($translate.instant('choose recipe'), '');
                $scope.d.isShared = false;
                return;
            }
            $scope.d.isShared = false;
        } else {
            if (x.isShared) {
                x.sharingData.recipeOwner.userId = $rootScope.user.userId;
                x.sharingData.recipeOwner.userGroupId = $rootScope.user.userGroupId;
                x.sharingData.lang = $rootScope.config.language;
                functions.post('SharingRecipes', 'Save', { x: x }).then(function (d) {
                    functions.alert($translate.instant(d), '');
                });
            } else {
                functions.post('SharingRecipes', 'Delete', { id: x.id }).then(function (d) {
                });
            }
        }
    };
    /********* Shared Recipes *********/


}])

.controller('pricesCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$translate', 'functions', '$mdDialog', '$localStorage', function ($scope, $http, $sessionStorage, $window, $rootScope, $translate, functions, $mdDialog, $localStorage) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'Prices';
    $scope.foodListType = 0;
    $scope.searchQuery = null;
    $scope.getFoodList = function (x) {
        $scope.foodList = x == 0 ? $rootScope.foods : $rootScope.myFoods;
    }
    $scope.getFoodList($scope.foodListType);

    var init = function () {
        functions.post(webService, 'Init', {}).then(function (d) {
            $scope.price = d;
            $scope.price.netPrice.currency = $rootScope.config.currency;
            $scope.load(null);
        });
    };
    init();

    $scope.load = function (query) {
        $scope.searchQuery = query;
        functions.post(webService, 'Load', { query: query, userId: $rootScope.user.userGroupId }).then(function (d) {
            $scope.prices = d;
        });
    }

    $scope.selectFood = function (x) {
        var obj = JSON.parse(x);
        $scope.price.food.id = obj.id;
        $scope.price.food.title = obj.food;
        $scope.calculateUnitPrice(x);
    }

    $scope.calculateUnitPrice = function (x) {
        if (angular.isObject(x)) {
            x.unitPrice.value = x.netPrice.value * (1 / x.mass.value) * 1000;
        }
    }

    $scope.save = function (x) {
        if (x.food.title === '' || x.netPrice.currency === '') {
            return false;
        }
        if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
        localStorage.currency = x.netPrice.currency;
        $rootScope.config.currency = x.netPrice.currency;
        $scope.price.netPrice.currency = localStorage.currency;
        functions.post(webService, 'Save', { userId: $rootScope.user.userGroupId, x: x }).then(function (d) {
            $scope.load($scope.searchQuery);
            if (d !== null) {
                functions.alert($translate.instant(d), '');
            }
        });
    }

    $scope.remove = function (id) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete input') + '?')
            .textContent()
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(id);
        }, function () {
        });
    };

    var remove = function (id) {
        functions.post(webService, 'Delete', { userId: $rootScope.user.userGroupId, id: id }).then(function (d) {
            $scope.load($scope.searchQuery);
        });
    }

}])

.controller('orderCtrl', ['$scope', '$http', '$window', '$rootScope', '$translate', 'functions', function ($scope, $http, $window, $rootScope, $translate, functions) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    $scope.discount = null;
    var getDiscount = function () {
        functions.post('Prices', 'GetDiscount', {}).then(function (d) {
            $scope.discount = d;
        });
    }
    getDiscount();

    $scope.application = $rootScope.config.title; // $translate.instant('nutrition program web');
    $scope.version = 'STANDARD';
    $scope.userType = 1;
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = $translate.instant('send order');
    $scope.showUserDetails = $rootScope.user.userName != '' ? false : true;
    $scope.showErrorAlert = false;
    $scope.showPaymentDetails = false;
    $scope.alertMsg = null;

    $scope.login = function (u, p) {
        functions.post('Users', 'Login', { userName: u, password: p }).then(function (d) {
            var user = d;
            if (user.userId !== null) {
                $scope.user.firstName = user.firstName;
                $scope.user.lastName = user.lastName;
                $scope.user.companyName = user.companyName;
                $scope.user.address = user.address;
                $scope.user.postalCode = user.postalCode;
                $scope.user.city = user.city;
                $scope.user.country = user.country;
                $scope.user.pin = user.pin;
                $scope.user.email = user.email;
                $scope.user.userType = user.userType;
                $scope.user.userGroupId = user.userGroupId;
                $scope.user.discountCoeff = user.discountCoeff;
                $scope.showUserDetails = true;
                $scope.showErrorAlert = false;
                $scope.calculatePrice();
            } else {
                $scope.showErrorAlert = true;
                $scope.errorMesage = $translate.instant('wrong user name or password');
            }
        });
    }

    var init = function () {
        functions.post('Orders', 'Init', {}).then(function (d) {
            $scope.user = d;
            $scope.user.userName = $rootScope.user.userName;
            $scope.user.password = $rootScope.user.password;
            $scope.user.application = $scope.application;
            $scope.user.version = $scope.version;
            $scope.user.licence = 1;
            $scope.user.licenceNumber = 1;
            $scope.user.userType = $scope.userType;
            $scope.login($scope.user.userName, $scope.user.password);
        });
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              init();
          });
    };

    $scope.changeUserType = function (x) {
        $scope.userType = x;
    }

    var maxUsers = function () {
        $scope.maxUsers = [];
        for (var i = 5; i < 101; i++) {
            $scope.maxUsers.push(i);
        }
    }
    maxUsers();

    $scope.premiumUsers = 5;

    $scope.setPremiumUsers = function (x) {
        $scope.premiumUsers = x;
        $scope.calculatePrice();
    }

    $scope.calculatePrice = function () {
        var unitprice = 0;
        var totalprice = 0;

        $scope.user.version = $scope.version;
        if ($scope.user.userType == 0) { unitprice = $rootScope.config.packages[0].price; $scope.user.version = $rootScope.config.packages[0].title; }
        if ($scope.user.userType == 1) { unitprice = $rootScope.config.packages[1].price; $scope.user.version = $rootScope.config.packages[1].title; }
        if ($scope.user.userType == 2) { unitprice = $rootScope.config.packages[2].price; $scope.user.version = $rootScope.config.packages[2].title; }

        if ($scope.user.licence > 1) {
            unitprice = unitprice * $scope.user.licence - ((unitprice * $scope.user.licence) * ($scope.user.licence / 10))
        }

        $scope.user.licenceNumber = 1;
        totalprice = $scope.user.licenceNumber > 1 ? unitprice * $scope.user.licenceNumber - (unitprice * $scope.user.licenceNumber * 0.1) : unitprice;
        var additionalUsers = $scope.premiumUsers > 5 && $scope.user.userType == 2 ? ($scope.premiumUsers - 5) * 500 : 0;  // 500kn/additional user;
        $scope.user.price = totalprice + additionalUsers;
        $scope.user.priceEur = (totalprice + additionalUsers) / $rootScope.config.eur;

        if ($scope.user.discountCoeff > 0) {
            $scope.discountCoeff = $scope.user.discountCoeff;
            if ($scope.discount.perc === 0) {
                $scope.discountCoeff = $scope.discount.oldUserDiscountPerc.packages.find(a => a.package === $scope.user.version.toLowerCase()).discountPerc / 100;
            }
            if ($scope.discountCoeff > 0) {
                $scope.user.priceWithDiscount = $scope.user.price - (Math.round($scope.user.price * $scope.discountCoeff) * 100 / 100);
                $scope.user.priceWithDiscountEur = $scope.user.priceEur - (Math.round($scope.user.priceEur * $scope.discountCoeff) * 100 / 100);
            }
        } else {
            $scope.discountCoeff = 0;
        }

        //if ($scope.user.discountCoeff > 0) {
        //    $scope.user.priceWithDiscount = $scope.user.price - (Math.round($scope.user.price * $scope.user.discountCoeff) * 100 / 100);
        //    $scope.user.priceWithDiscountEur = $scope.user.priceEur - (Math.round($scope.user.priceEur * $scope.user.discountCoeff) * 100 / 100);
        //}

    }

    if ($rootScope.config == undefined) {
        getConfig();
    } else {
        init();
    }

    $scope.order = function (application, version) {
        init();
        window.location.hash = 'orderform';
        $scope.user.application = application;
        $scope.user.version = version;
        $scope.calculatePrice();
    }

    $scope.setApplication = function (x) {
        $scope.user.application = x;
        $scope.calculatePrice();
    }

    $scope.sendOrder = function (user) {
        $scope.showErrorAlert = false;
        if (user.email == '' || user.firstName == '' || user.lastName == '' || user.address == '' || user.postalCode == '' || user.city == '' || user.country == '') {
            $scope.showErrorAlert = true;
            $scope.errorMesage = $translate.instant('all fields are required');
            return false;
        }
        if ($scope.userType == 1) {
            if (user.companyName == '' || user.pin == '') {
                $scope.showErrorAlert = true;
                $scope.errorMesage = $translate.instant('all fields are required');
                return false;
            }
        }
        if (!(user.country.toLowerCase().startsWith('hr') || user.country.toLowerCase().startsWith('cro'))) {
            user.isForeign = true;
        }
        user.maxNumberOfUsers = $scope.premiumUsers;

        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = $translate.instant('sending') + '...  ' + $translate.instant('please wait');
        $scope.isSendButtonDisabled = true;
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/SendOrder',
            method: 'POST',
            data: { x: user, lang: $rootScope.config.language }
        })
       .then(function (response) {
           if (!response.data.d.isSuccess) {
               $scope.showAlert = false;
               $scope.showPaymentDetails = false;
               $scope.sendicon = 'fa fa-angle-double-right';
               $scope.sendicontitle = $translate.instant('send');
               $scope.isSendButtonDisabled = false;
               functions.alert($translate.instant('order is not sent') + '. ' + $translate.instant('please try again') + '.', $translate.instant('error') + ': ' + response.data.d.msg);
           } else {
               $scope.alertMsg = response.data.d.msg;
               $scope.showAlert = true;
               $scope.showPaymentDetails = true;
           }
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.sendicon = 'fa fa-paper-plane-o';
           $scope.isSendButtonDisabled = false;
           $scope.sendicontitle = $translate.instant('send');
           alert(response.data.d);
       });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }

}])

.controller('weeklyMenuCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    var webService = 'WeeklyMenus';
    $scope.consumers = 1;
    $scope.date = new Date(new Date($rootScope.currentMenu.date)).toLocaleDateString();
    $scope.author = $rootScope.user.firstName + ' ' + $rootScope.user.lastName;
    $scope.printType = 1;

    $scope.getDay = function (x) {
        switch (x) {
            case 0: return $translate.instant('monday');
            case 1: return $translate.instant('tuesday');
            case 2: return $translate.instant('wednesday');
            case 3: return $translate.instant('thursday');
            case 4: return $translate.instant('friday');
            case 5: return $translate.instant('saturday');
            case 6: return $translate.instant('sunday');
            default: return '';
        }
    }

    $scope.initPrintSettings = function (printType) {
        $scope.printType = printType;
        functions.post('PrintPdf', printType === 0 ? 'InitWeeklyMenuSettings' : 'InitMenuSettings', {}).then(function (d) {
            $scope.printSettings = d;
            $scope.printSettings.weeklyMenuType = printType;
        });
    }
    $scope.initPrintSettings($scope.printType);

    var emptyMenuList = true;
    var isEmptyList = function (x) {
        emptyMenuList = true;
        angular.forEach(x, function (value, key) {
            if (!functions.isNullOrEmpty(value)) {
                emptyMenuList = false;
                return false;
            }
        });
    }
    $scope.isEmptyList = function () {
        isEmptyList($scope.weeklyMenu.menuList);
        return emptyMenuList;
    }

    var init = function () {
        $scope.loading = true;
        functions.post(webService, 'Init', { user: $rootScope.user, client: $rootScope.client, lang: $rootScope.config.language }).then(function (d) {
            $rootScope.weeklyMenu = d;
            $scope.loading = false;
        });
    }
    if (!angular.isDefined($rootScope.weeklyMenu)) { init(); }

    $scope.new = function () {
        init();
    }

    $scope.printWindow = function () {
        window.print();
    };

    $scope.pdfLink = null;
    $scope.creatingPdf = false;

    $scope.openPdf = function () {
        if ($scope.pdfLink != null) {
            window.open($scope.pdfLink, window.innerWidth <= 800 && window.innerHeight <= 600 ? '_self' : '_blank');
        }
    }

    $scope.get = function (idx) {
        if ($rootScope.user.licenceStatus === 'demo') {
            if(!functions.checkPermissionAlert('this function is not available in demo version', 'activate full version')) return;
        } else {
            getMenuPopup(idx);
        }
    }

    var getMenuPopup = function (idx) {
        $mdDialog.show({
            controller: getMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/getmenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            data: { config: $rootScope.config, clientData: $rootScope.clientData }
        })
        .then(function (x) {
            $scope.weeklyMenu.menuList[idx] = x.id;
            $scope.weeklyMenu.menuDes[idx].title = x.title;
            $scope.weeklyMenu.menuDes[idx].diet = $translate.instant(x.diet);
            $scope.weeklyMenu.menuDes[idx].energy = x.energy;
        }, function () {
        });
    };

    var getMenuPopupCtrl = function ($scope, $mdDialog, $http, data, $translate, $translatePartialLoader, $timeout) {
        $scope.config = data.config;
        $scope.clientData = data.clientData;
        $scope.loadType = 0;
        $scope.type = 0;
        $scope.appMenues = false;
        $scope.toTranslate = false;
        $scope.toLanguage = '';
        $scope.searchValue = null;
        $scope.hideNav = true;
        var limit = $rootScope.config.showmenuslimit;
        $scope.limit = limit;
        var offset = 0;
        $scope.d = [];
        var isLoadMore = false;
        $scope.user = $rootScope.user;

        $scope.toggle = function (type) {
            if (type == 'myMenus') {
                $scope.d = [];
                limit = $rootScope.config.showmenuslimit;
                offset = 0;
                load(null, null);
            } else if (type == 'appMenus') {
                loadAppMenues();
            }
        }

        $scope.loadMore = function (search, clientId, userId) {
            isLoadMore = true;
            offset += $rootScope.config.showmenuslimit;
            load(search, clientId, userId);
        }

        $scope.d = [];
        var load = function (search, clientId, userId) {
            $scope.loading = true;
            $scope.appMenues = false;
            if (!isLoadMore) {
                $scope.d = [];
            }
            functions.post('Menues', 'Load', { userGroupId: $rootScope.user.userGroupId, limit: limit, offset: offset, search: search, clientId: clientId, userId: userId }).then(function (d) {
                angular.forEach(d, function (x, key) {
                    x.date = new Date(x.date);
                });
                if ($scope.d !== null && isLoadMore) {
                    $scope.d = $scope.d.concat(d);
                } else {
                    $scope.d = d;
                }
                $scope.loading = false;
            });
        }
        load(null, null, null);

        $scope.currUserId = null;
        $scope.load = function (search, type, clientId, userId) {
            $scope.currUserId = userId;
            $scope.d = [];
            offset = 0;
            if (type == 0) {
                $scope.clientId = null;
                load(search, null, userId);
            } else {
                $scope.clientId = clientId;
                load(search, clientId, userId);
            }
        }

        var getUsersByUserGroup = function () {
            functions.post('Users', 'GetUsersByUserGroup', { userGroupId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.users = d;
                $scope.users.unshift({ userId: null, firstName: $translate.instant('all users'), lastName: null });
            });
        };
        getUsersByUserGroup();

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                 .title($translate.instant('remove menu') + '?')
                 .textContent(x.title)
                 .targetEvent(x)
                 .ok($translate.instant('yes'))
                 .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        }

        var remove = function (x) {
            functions.post('Menues', 'Delete', { user: $rootScope.user, menu: x }).then(function (d) {
                if (d.msg !== null) {
                    functions.alert($translate.instant(d.msg), '');
                }
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post('Menues', 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $mdDialog.hide(d);
            });
        }

        var loadAppMenues = function () {
            $scope.appMenues = true;
            functions.post('Menues', 'LoadAppMenues', { lang: $rootScope.config.language }).then(function (d) {
                $scope.d = d;
            });
        }

        var getAppMenu = function (x) {
            functions.post('Menues', 'GetAppMenu', { id: x.id, lang: $rootScope.config.language, toTranslate: $scope.toTranslate }).then(function (d) {
                if ($scope.toTranslate) {
                    translateFoods(d);
                }
                $mdDialog.hide(d);
            });
        }

        $scope.confirm = function (x) {
            $scope.appMenues == true ? getAppMenu(x) : get(x);
        }

        $scope.setToTranslate = function (x) {
            $scope.toTranslate = x;
        }

        $scope.setToLanguage = function (x) {
            $scope.toLanguage = x;
        }

        var translateFoods = function (menu) {
            $rootScope.setLanguage($scope.toLanguage);
            $timeout(function () {
                angular.forEach(menu.data.selectedFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                angular.forEach(menu.data.selectedInitFoods, function (value, key) {
                    value.food = $translate.instant(value.food);
                    value.unit = $translate.instant(value.unit);
                })
                $mdDialog.hide(menu);
                $rootScope.setLanguage('hr');
            }, 500);
        }

    };

    $scope.creatingPdf = false;
    $scope.pageSizes = ['A4', 'A3', 'A2', 'A1'];

    $scope.printWeeklyMenu = function (printSettings) {
        if (emptyMenuList) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        $scope.pdfLink = null;
        $scope.creatingPdf = true;
        functions.post('PrintPdf', 'WeeklyMenuPdf', { userId: $sessionStorage.usergroupid, weeklyMenu: $rootScope.weeklyMenu, lang: $rootScope.config.language, settings: printSettings }).then(function (d) {
            var fileName = d;
            $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
            $scope.creatingPdf = false;
        });
    }

    $scope.hidePdfLink = function () {
        $scope.pdfLink = null;
    }

    //********* search ************
    $scope.search = function () {
        openSearchMenuPopup();
    }

    var openSearchMenuPopup = function () {
        $mdDialog.show({
            controller: openSearchMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/searchweeklymenus.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { clientData: $rootScope.clientData }
        })
       .then(function (response) {
           $rootScope.weeklyMenu = response;
           $rootScope.weeklyMenu.client = $rootScope.client;
           $rootScope.weeklyMenu.diet = $rootScope.clientData.diet;
       }, function () {
       });
    }

    var openSearchMenuPopupCtrl = function ($scope, $mdDialog, $http, $translate, functions, d) {
        var webService = 'WeeklyMenus';
        $scope.clientData = d.clientData;
        $scope.type = 0;
        $scope.searchValue = null;
        $scope.clientId = null;
        var limit = $rootScope.config.showmenuslimit;
        $scope.limit = limit;
        var offset = 0;
        $scope.d = [];
        var isLoadMore = false;
        $scope.user = $rootScope.user;

        $scope.loadMore = function (search, clientId, userId) {
            isLoadMore = true;
            offset += $rootScope.config.showmenuslimit;
            load(search, clientId, userId);
        }

        var load = function (search, clientId, userId) {
            $scope.loading = true;
            $scope.appMenues = false;
            if (!isLoadMore) {
                $scope.d = [];
            }
            functions.post(webService, 'Load', { userGroupId: $rootScope.user.userGroupId, limit: limit, offset: offset, search: search, clientId: clientId, userId: userId, lang: $rootScope.config.language }).then(function (d) {
                angular.forEach(d, function (x, key) {
                    x.date = new Date(x.date);
                });
                if ($scope.d !== null && isLoadMore) {
                    $scope.d = $scope.d.concat(d);
                } else {
                    $scope.d = d;
                }
                $scope.loading = false;
            });
        }
        load(null, null, null);

        $scope.currUserId = null;
        $scope.load = function (search, type, clientId, userId) {
            $scope.currUserId = userId;
            $scope.d = [];
            offset = 0;
            if (type == 0) {
                $scope.clientId = null;
                load(search, null, userId);
            } else {
                $scope.clientId = clientId;
                load(search, clientId, userId);
            }
        }

        var getUsersByUserGroup = function () {
            functions.post('Users', 'GetUsersByUserGroup', { userGroupId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.users = d;
                $scope.users.unshift({ userId: null, firstName: $translate.instant('all users'), lastName: null });
            });
        };
        getUsersByUserGroup();

        $scope.remove = function (x) {
            if (emptyMenuList) return false;
            var confirm = $mdDialog.confirm()
                 .title($translate.instant('remove menu') + '?')
                 .textContent(x.title)
                 .targetEvent(x)
                 .ok($translate.instant('yes'))
                 .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
            }, function () {
            });
        }

        var remove = function (x) {
            functions.post(webService, 'Delete', { user: $rootScope.user, menu: x, lang: $rootScope.config.language }).then(function (d) {
                if (d.msg !== null) {
                    functions.alert($translate.instant(d.msg), '');
                } else {
                    $scope.d = [];
                }
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post(webService, 'Get', { userId: $rootScope.user.userGroupId, id: x.id, lang: $rootScope.config.language }).then(function (d) {
                menu = d;
                $mdDialog.hide(menu);
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x) {
            get(x);
        }

    };

    //********* save ************
    $scope.save = function () {
        if (emptyMenuList) {
            functions.alert($translate.instant('select menus'), '');
            return false;
        }
        openSaveMenuPopup();
    }

    var openSaveMenuPopup = function () {
        $mdDialog.show({
            controller: openSaveMenuPopupCtrl,
            templateUrl: 'assets/partials/popup/saveweeklymenu.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { weeklyMenu: $rootScope.weeklyMenu }
        })
       .then(function (response) {
           $rootScope.weeklyMenu = response;
       }, function () {
       });
    }

    var openSaveMenuPopupCtrl = function ($scope, $mdDialog, d, $http, $translate, functions) {
        var webService = 'WeeklyMenus';
        $scope.d = angular.copy(d.weeklyMenu);
        $scope.user = $rootScope.user;

        var save = function (x) {
            if (!functions.demoAlert($rootScope.user.licenceStatus)) return;
            if (functions.isNullOrEmpty(x.title)) {
                functions.alert($translate.instant('enter menu title'), '');
                return false;
            }

            functions.post(webService, 'Save', { user: $rootScope.user, x: x }).then(function (d) {
                if (d.isSuccess) {
                    $scope.d = d.data;
                    $mdDialog.hide($scope.d);
                } else {
                    functions.alert($translate.instant(d.msg), d.msg1 !== null ? $translate.instant(d.msg1) : null);
                }
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.confirm = function (x, saveasnew) {
            x.client = d.weeklyMenu.client;
            x.userId = d.weeklyMenu.client.userId;
            x.id = saveasnew == true ? null : x.id;
            x.date = new Date(new Date().setHours(0, 0, 0, 0));
            save(x);
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        var get = function (x) {
            functions.post(webService, 'Get', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                $scope.d = d;
                $mdDialog.hide($scope.d);
            });
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

    };

    //********* remove ************

    $scope.remove = function (x) {
        if (emptyMenuList) { return false; }
        var confirm = $mdDialog.confirm()
             .title($translate.instant('remove menu') + '?')
             .textContent(x.title)
             .targetEvent(x)
             .ok($translate.instant('yes'))
             .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    }

    var remove = function (x) {
        functions.post(webService, 'Delete', { user: $rootScope.user, menu: x, lang: $rootScope.config.language }).then(function (d) {
            if (d.msg !== null) {
                functions.alert($translate.instant(d.msg), '');
            } else {
                init();
            }
        });
    }

    $scope.openShoppingListPopup = function (x) {
        if (x.length === 0) {
            return false;
        }
        $mdDialog.show({
            controller: shoppingListPdfCtrl,
            templateUrl: 'assets/partials/popup/shoppinglist.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
            d: { weeklyMenu: x, settings: $scope.printSettings }
        })
        .then(function (r) {
            alert(r);
        }, function () {
        });
    };

    var shoppingListPdfCtrl = function ($scope, $rootScope, $mdDialog, $http, d, $translate, $translatePartialLoader) {
        $scope.currentMenu = d.weeklyMenu;
        var menuSettings = d.settings;
        $scope.pdfLink == null;
        $scope.creatingPdf = false;
        $scope.d = null;

        var initSettings = function () {
            functions.post('PrintPdf', 'InitShoppingListSettings', {}).then(function (d) {
                $scope.settings = d;
                $scope.settings.consumers = menuSettings.consumers;
                createShoppingList($scope.currentMenu.menuList, $scope.settings.consumers);
            });
        }

        var createShoppingList = function (x, consumers) {
            functions.post('ShoppingList', 'CreateWeeklyShoppingList', { userId: $sessionStorage.usergroupid, menuList: x, consumers: consumers, lang: $rootScope.config.language }).then(function (d) {
                $scope.d = d;
                if ($scope.d.total) {
                    if ($scope.d.total.price > 0) {
                        $scope.settings.showPrice = true;
                    }
                }
            });
        }
        initSettings();

        $scope.changeNumberOfConsumers = function (x) {
            if (x < 1 || functions.isNullOrEmpty(x)) { return false }
            createShoppingList($scope.currentMenu.menuList, x);
        }

        $scope.copyToClipboard = function (id) {
            return functions.copyToClipboard(id);
        }

        $scope.printShoppingListPdf = function (sl, s) {
            if (sl === null) return;
            $scope.creatingPdf = true;
            s.headerInfo = $rootScope.user.headerInfo;
            functions.post('PrintPdf', 'ShoppingList', { userId: $sessionStorage.usergroupid, shoppingList: sl, title: $scope.currentMenu.title, note: $scope.currentMenu.note, lang: $rootScope.config.language, settings: s }).then(function (d) {
                var fileName = d;
                $scope.creatingPdf = false;
                $scope.pdfLink = $sessionStorage.config.backend + 'upload/users/' + $rootScope.user.userGroupId + '/pdf/' + fileName + '.pdf';
            });
        }

        $scope.hidePdfLink = function () {
            $scope.pdfLink = null;
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };
    };

    $scope.hidePdfSLLink = function () {
        $scope.pdfSLLink = null;
    }

    $scope.setAuthor = function (x) {
        $scope.author = x;
    }

    $scope.setDate = function (x) {
        $scope.date = x;
    }

}])

.controller('clientAppCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', '$timeout', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate, $timeout) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'ClientApp.asmx';
    $scope.show = false;
    $scope.showTitle = $translate.instant('show access data');

    $scope.toggle = function (client, clientApp) {
        $scope.show = !$scope.show;
        if ($scope.show == true) {
            if (clientApp.id == null) {
                $scope.getActivationCode(client, clientApp);
            }
            $scope.showTitle = $translate.instant('hide access data');
        } else {
            $scope.showTitle = $translate.instant('show access data');
        }
    };

    $scope.get = function (x) {
        $scope.client = x;
        $scope.show = false;
        $scope.showTitle = $translate.instant('show access data');
        $http({
            url: $sessionStorage.config.backend + webService + '/Get',
            method: "POST",
            data: { clientId: x.clientId }
        })
        .then(function (response) {
            $scope.clientApp = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.client = null;
    $scope.getActivationCode = function (client, clientApp) {
        if (clientApp.id == null) {
            clientApp.clientId = client.clientId;
            clientApp.userId = $rootScope.user.userGroupId;
            clientApp.lang = $rootScope.config.language;
        }
        $http({
            url: $sessionStorage.config.backend + webService + '/GetActivationCode',
            method: "POST",
            data: { x: clientApp }
        })
        .then(function (response) {
            $scope.clientApp = JSON.parse(response.data.d);
        },
        function (response) {
            alert(response.data.d)
        });
    }

    $scope.clientAppUrl = function (x) {
        if (x !== undefined) {
            return $rootScope.config.clientapppageurl + '?uid=' + $rootScope.user.userGroupId + '&cid=' + x.clientId + '&lang=' + $rootScope.config.language;
        } else {
            return;
        }
    }

    $scope.sendingMail = false;
    $scope.sendAppLinkToClientEmail = function (client) {
        if ($scope.sendingMail == true) { return false; }
        if (functions.isNullOrEmpty(client.email)) {
            functions.alert($translate.instant('email is required'), '');
            return false;
        }
        $scope.sendingMail = true;
        var link = $scope.clientAppUrl(client);
        var messageSubject = $translate.instant('nutrition program web') + '. ' + $translate.instant('application access data');
        var messageBody = '<p>' + $translate.instant('dear') + ',' + '</p>' +
            $translate.instant('we send you the access data to track your body weight and download menus') + '.' +
            '<br />' +
            '<br />' +
            $translate.instant('web application') + ': ' + '<strong><a href="' + $rootScope.config.clientapppageurl + '">' + $rootScope.config.clientapppageurl + '</a></strong>' +
            '<br />' +
            $translate.instant('or') + ' ' + $translate.instant('android application') + ': ' + '<strong>' + '<a href="' + $rootScope.config.clientapp_apk + '">' + $rootScope.config.clientapp_apk + '</a></strong>' +
            '<br />' +
            '<iframe src="https://www.appsgeyser.com/social_widget/social_widget.php?width=100&height=100&apkName=Program Prehrane Klijent_8297899&simpleVersion=yes" width="180" height="220" vspace="0" hspace="0" frameborder="no" scrolling="no" seamless="" allowtransparency="true"></iframe>' +
            '<br />' +
            $translate.instant('activation code') + ': ' + '<strong>' + $scope.clientApp.code + '</strong>' +
            '<br />' +
            '<hr />' +
             $translate.instant('or') + ' ' + $translate.instant('web application') + ' (' + $translate.instant('without activation code') + '): ' + '<strong><a href="' + link + '">' + link + '</a></strong>' +
            '<br />' +
            '<br />' +
            '<i>* ' + $translate.instant('this is an automatically generated email – please do not reply to it') + '</i>' +
            '<br />' +
            '<p>' + $translate.instant('best regards') + '</p>' +
            '<a href="' + $rootScope.config.webpageurl + '">' + $rootScope.config.webpage + '</a>'
        $http({
            url: $sessionStorage.config.backend + 'Mail.asmx/SendMessage',
            method: "POST",
            data: { sendTo: client.email, messageSubject: messageSubject, messageBody: messageBody, lang: $rootScope.config.language, send_cc: false }
        })
        .then(function (response) {
            $scope.sendingMail = false;
            $scope.client = null;
            $scope.show = false;
            functions.alert(response.data.d, '');
        },
        function (response) {
            $scope.sendingMail = false;
            functions.alert($translate.instant(response.data.d), '');
        });
    }

}])

.controller('myDietsCtrl', ['$scope', '$http', '$sessionStorage', '$window', '$rootScope', '$mdDialog', 'functions', '$translate', function ($scope, $http, $sessionStorage, $window, $rootScope, $mdDialog, functions, $translate) {
    if ($rootScope.user === undefined) {
        $window.location.href = '/app/#/login';
    }

    var webService = 'MyDiets';

    var init = function () {
        functions.post(webService, 'Init', {}).then(function (d) {
            $scope.d = d;
        });
    };

    init();

    $scope.add = function (x) {
        $scope.myDiets.push(x);
    }

    $scope.new = function () {
        init();
    }

    $scope.save = function (d) {
        if (!functions.checkPermission($rootScope.user, 'premium')) return;
        if (d.diet === '' || d.diet === null) {
            functions.alert($translate.instant('title is required'), '');
            return;
        }
        functions.post(webService, 'Save', { userId: $sessionStorage.usergroupid, x: d }).then(function (d) {
            if (d.isSuccess) {
                $scope.d = d.data;
                $rootScope.loadDiets();
            } else {
                functions.alert($translate.instant(d.msg), d.msg1 !== null ? $translate.instant(d.msg1) : null);
            }
        });
    }

    $scope.remove = function (x) {
        var confirm = $mdDialog.confirm()
            .title($translate.instant('delete recipe') + '?')
            .textContent(x.diet)
            .targetEvent()
            .ok($translate.instant('yes'))
            .cancel($translate.instant('no'));
        $mdDialog.show(confirm).then(function () {
            remove(x);
        }, function () {
        });
    };

    var remove = function (x) {
        functions.post(webService, 'Delete', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
            init();
            $rootScope.loadDiets();
        });
    }

    $scope.search = function () {
        openMyDietsPopup();
    }

    var openMyDietsPopup = function () {
        if ($rootScope.user.licenceStatus === 'demo') { return; }
        $mdDialog.show({
            controller: getMyDietsPopupCtrl,
            templateUrl: 'assets/partials/popup/mydiets.html',
            parent: angular.element(document.body),
            clickOutsideToClose: true,
        })
        .then(function (resp) {
            $scope.d = resp;
        }, function () {
        });
    };

    var getMyDietsPopupCtrl = function ($scope, $mdDialog, $http) {
        $scope.limit = 20;
        $scope.userId = $rootScope.user.userId;
        $scope.userGroupId = $rootScope.user.userGroupId;
        $scope.searchQuery = null;
        $scope.loading = false;

        $scope.loadMore = function () {
            $scope.limit += 20;
        }

        var load = function () {
            $scope.sharingRecipes = false;
            if ($rootScope.user.licenceStatus === 'demo') {
                return;
            }
            $scope.loading = true;
            functions.post(webService, 'Load', { userId: $rootScope.user.userGroupId }).then(function (d) {
                $scope.d = d;
                $scope.loading = false;
            });
        }
        load();

        $scope.load = function () {
            return load();
        }

        $scope.cancel = function () {
            $mdDialog.cancel();
        };

        $scope.search = function (searchQuery) {
            $scope.loading = true;
            functions.post(webService, 'Search', { userId: $rootScope.user.userGroupId, query: searchQuery }).then(function (d) {
                $scope.d = d;
                $scope.loading = false;
            });
        }

        var get = function (userGroupId, id) {
            functions.post(webService, 'Get', { userId: userGroupId, id: id }).then(function (d) {
                $mdDialog.hide(d);
            });
        }

        $scope.confirm = function (id) {
            get($rootScope.user.userGroupId, id);
        }

        $scope.remove = function (x) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant('delete recipe') + '?')
                .textContent(x.diet)
                .targetEvent(x)
                .ok($translate.instant('yes'))
                .cancel($translate.instant('no'));
            $mdDialog.show(confirm).then(function () {
                remove(x);
                openMyRecipesPopup();
            }, function () {
                openMyRecipesPopup();
            });
        };

        var remove = function (x) {
            functions.post(webService, 'Delete', { userId: $rootScope.user.userGroupId, id: x.id }).then(function (d) {
                load();
                $rootScope.loadDiets();
            });
        }

    };

}])
//-------------end Program Prehrane Controllers--------------------

.directive('allowOnlyNumbers', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            elm.on('keydown', function (event) {
                var $input = $(this);
                var value = $input.val();
                //value = value.replace(/[^0-9]/g, '')
                value = value.replace(',', '.');
                $input.val(value);
                if (event.which == 64 || event.which == 16) {
                    // to allow numbers  
                    return false;
                } else if (event.which >= 48 && event.which <= 57) {
                    // to allow numbers  
                    return true;
                } else if (event.which >= 96 && event.which <= 105) {
                    // to allow numpad number  
                    return true;
                } else if ([8, 13, 27, 37, 38, 39, 40].indexOf(event.which) > -1) {
                    // to allow backspace, enter, escape, arrows  
                    return true;
                } else if (event.which == 110 || event.which == 188 || event.which == 190) {
                    // to allow ',' and '.'
                    return true;
                } else if (event.which == 46) {
                    // to allow delete
                    return true;
                } else {
                    event.preventDefault();
                    // to stop others  
                    return false;
                }
            });
        }
    }
})

.directive('customPopover', function () {
    return {
        restrict: 'A',
        template: '<span><i class="fa fa-info-circle text-info" style="cursor:pointer"></i> {{label}}</span>',
        link: function (scope, el, attrs) {
            scope.label = attrs.popoverLabel;
            $(el).popover({
                trigger: 'click',
                html: true,
                content: attrs.popoverHtml,
                placement: attrs.popoverPlacement
            });
        }
    };
})

.directive('jsonDirective', function () {
    return {
        restrict: 'E',
        scope: {
            data: '=',
            desc: '='
        },
        templateUrl: './assets/partials/directive/json.html',
        controller: 'jsonCtrl'
    };
})

.controller('jsonCtrl', ['$scope', '$rootScope', function ($scope, $rootScope) {
    $scope.isShow = false;
    $scope.debug = $rootScope.config.debug;
    $scope.show = function () {
        $scope.isShow = !$scope.isShow;
    }
}])

.directive('loadingDirective', function () {
    return {
        restrict: 'E',
        scope: {
            value: '=',
            showdesc: '='
        },
        templateUrl: './assets/partials/directive/loading.html'
    };
})

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

.directive('patternValidator', [
    function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, elem, attrs, ctrl) {
                ctrl.$parsers.unshift(function (viewValue) {
                    var patt = new RegExp(attrs.patternValidator);
                    var isValid = patt.test(viewValue);
                    ctrl.$setValidity('passwordPattern', isValid);
                    return viewValue;
                });
            }
        };
    }
]);

;



