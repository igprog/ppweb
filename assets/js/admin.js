﻿/*!
admin.js
(c) 2018-2021 IG PROG, www.igprog.hr
*/
angular.module('app', [])

.config(['$httpProvider', function ($httpProvider) {
    //*******************disable catche**********************
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************
}])

.controller('adminCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {

    var getConfig = () => {
        $http.get('./config/config.json')
          .then((response) => {
              $rootScope.config = response.data;
          });
    };
    getConfig();

    $scope.islogin = false;
    $scope.year = new Date().getFullYear();

    $scope.toggleTpl = (x) => {
        $rootScope.tpl = x;
    };
    $scope.toggleTpl('login');

    init = () => {
        $scope.user = {
            username: null,
            password: null
        }
    }
    init();

    $scope.login = (u) => {
        functions.post('Admin', 'Login', { username: u.username, password: u.password }).then((d) => {
            $scope.islogin = d;
            $scope.islogin === true ? $scope.toggleTpl('programPrehraneWeb') : alert('error login');
        });
    }

    $scope.logout = () => {
        $scope.islogin = false;
        $scope.toggleTpl('login');
        init();
    }

}])

.controller('applicationCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {

    var load = function () {
        functions.post('Instal', 'Load', {}).then(function (d) {
            $scope.d = d;
        });
    }
    load();

}])

.controller('webAppCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {
    $scope.showDetails = false;
    $scope.showActive = false;
    $scope.loading = false;
    $scope.limit = 10;
    $scope.page = 1;
    $scope.searchQuery = '';
    $scope.isDesc = true;
    $scope.activeTab = 'users';
    $scope.adminCode = null;
    $scope.licenceStatus = null;

    function setYears() {
        $scope.years = [];
        for (var i = 2017; i <= $scope.year; i++) {
            $scope.years.push(i);
        }
        $scope.year = new Date().getFullYear();
    }
    setYears();

    var total = function (year) {
        $scope.loading = true;
        functions.post('Users', 'Total', { year: year }).then(function (d) {
            $scope.t = d;
            $scope.loading = false;
        });
    }

    $scope.total = function (year) {
        $scope.activeTab = 'total';
        google.charts.load('current', { packages: ['corechart'] });
        total(year);
    }

    $scope.getTotalActivatedUsersByCity = function () {
        $scope.loading = true;
        functions.post('Users', 'GetTotalActivatedUsersByCity', {}).then(function (d) {
            $scope.activatedUsersByCity = d;
            $scope.loading = false;
        });
    }

    var load = function (limit) {
        $scope.loading = true;
        functions.post('Users', 'Load', { limit: limit, page: $scope.page, isDesc: $scope.isDesc }).then(function (d) {
            $scope.d = d;
            $scope.loading = false;
        });
    }

    $scope.search = function (searchQuery, showActive, licenceStatus, limit, isDesc) {
        $scope.loading = true;
        $scope.activeTab = 'users';
        $scope.page = 1;
        $scope.isDesc = isDesc;
        functions.post('Users', 'Search', { query: searchQuery, limit: limit, page: $scope.page, activeUsers: showActive, licenceStatus: licenceStatus, isDesc: isDesc }).then(function (d) {
            $scope.d = d;
            $scope.loading = false;
        });
    }
    $scope.search(null, false, null, $scope.limit, $scope.isDesc);

    $scope.update = function (user) {
        functions.post('Users', 'Update', { x: user }).then(function (d) {
            load($scope.limit);
            alert(d);
        });
    }

    $scope.currUser = null;
    $scope.info = function (x) {
        $scope.currUser = x;
        functions.post('Users', 'GetUserSum', { userGroupId: x.userGroupId, userId: x.userId, userType: x.userType, adminType: x.adminType }).then(function (d) {
            $scope.userTotal = d;
        });
    }

    $scope.remove = function (user) {
        if (confirm("Briši " + user.firstName + " " + user.lastName + "?")) {
            remove(user);
        }
    }

    var remove = function (user) {
        if (user.userId !== user.userGroupId) {
            alert('Samo glavni nositelj računa može biti obrisan.');
            return;
        }
        if (user.licenceStatus === 'active') {
            alert('Aktivan korisnik ne može biti izvrisan. Ako želiš izbrisati aktivnog korisnika prvo ga deaktiviraj.');
            return;
        }
        functions.post('Users', 'DeleteAllUserGroup', { x: user }).then(function (d) {
            load($scope.limit);
            alert(d.msg);
        });
    }

    $scope.idxStart = 0;
    $scope.idxEnd = 10;
    $scope.setPage = function (x) {
        $scope.idxStart = 0 + x;
        $scope.idxEnd = 10 + x;
    }

    $scope.showAllPages = function () {
        $scope.idxStart = 0;
        $scope.idxEnd = $scope.d.length;
    }

    $scope.nextPage = function (limit) {
        $scope.page = $scope.page + 1;
        load(limit);
    }

    $scope.prevPage = function (limit) {
        if ($scope.page > 1) {
            $scope.page = $scope.page - 1;
            load(limit);
        }
    }

    $scope.updateInfo = function (x) {
        functions.post('Users', 'UpdateUserInfoFromOrdersTbl', { email: x }).then(function (d) {
            alert(d);
        });
    }

    $scope.countMyFoodsWithSameIdAsAppFoods = function (x) {
        functions.post('MyFoods', 'CountMyFoodsWithSameIdAsAppFoods', {userId: x.userGroupId}).then(function(d) {
            alert(d);
        });
    }

    $scope.fixMyFoodsId = function (x) {
        functions.post('MyFoods', 'FixMyFoodsId', {userId: x.userGroupId}).then(function(d) {
            alert(d);
        });
    }

    $scope.createSubusers = function (x, prefix) {
        functions.post('Users', 'CreateSubusers', { x: x, prefix: prefix }).then(function (d) {
            alert(d);
        });
    }

    $scope.addYear = function (idx) {
        functions.post('Admin', 'AddYear', {}).then(function (d) {
            $scope.d[idx].expirationDate = d;
        });
    }

    /***** Shared Recipes *****/
    $scope.loadSharingRecipes = function () {
        $scope.activeTab = 'sharingRecipes';
        $scope.loading = true;
        functions.post('SharingRecipes', 'Load', { userId: null, status: null, showUserRecipes: true, lang: null }).then(function (d) {
            $scope.sharingRecipes = d;
            $scope.loading = false;
        });
    }

    $scope.getSharingRecipe = function (x, idx) {
        $scope.loading = true;
        functions.post('SharingRecipes', 'Get', {userId: null, id: x.id }).then(function (d) {
            $scope.sharingRecipes[idx] = d;
            $scope.loading = false;
        });
    }

    $scope.saveSharingRecipe = function (x) {
        x.sharingData.adminSave = true;
        functions.post('SharingRecipes', 'Save', { x: x }).then(function (d) {
            $scope.loadSharingRecipes();
        });
    }

    $scope.removeRecipe = function (x) {
        var r = confirm("Briši " + x.title + "?");
        if (r) {
            functions.post('SharingRecipes', 'Delete', { id: x.id }).then(function (d) {
                $scope.loadSharingRecipes();
                alert(d);
            });
        }
    }
    /***** Shared Recipes *****/

    /***** Error Log *****/
    $scope.loadErrorLog = function () {
        $scope.activeTab = 'errorLog';
        $scope.loading = true;
        functions.post('Log', 'Load', { fileName: 'error.log' }).then(function (d) {
            $scope.errorLog = d;
            $scope.loading = false;
        });
    }
    /***** Error Log *****/

    /***** Activity Log *****/
    var loadSettings = function () {
        functions.post('Files', 'LoadSettings', {}).then(function (d) {
            $scope.settings = d;
        });
    }
    loadSettings();

    $scope.activeTabActivity = 'lastActivity';
    $scope.loadActivityLog = function () {
        $scope.activeTab = 'activityLog';
        $scope.loading = true;
        functions.post('Log', 'Load', { fileName: 'activity.log' }).then(function (d) {
            $scope.activityLog = d;
            loadLoginLog(false);
            $scope.loading = false;
        });
    }

    var loadLoginLog = function (isAll) {
        $scope.loading = true;
        functions.post('Log', 'LoadLoginLog', { isAll: isAll }).then(function (d) {
            $scope.loginLog = d;
            $scope.loading = false;
        });
    }

    $scope.loadLoginLog = function (isAll) {
        $scope.activeTabActivity = isAll ? 'allActivities' : 'lastActivity';
        loadLoginLog(isAll);
    }
    /***** Activity Log *****/

    $scope.saveLog = function (filName, x) {
        functions.post('Log', 'Save', { fileName: filName, content: x }).then(function (d) {
        });
    }


    /***** Tickets *****/
    $scope.ticketType = 0;
    $scope.ticketStatus = null;
    $scope.currIdx = null;
    $scope.showTicketDetails = function (idx) {
        $scope.currIdx = idx;
    }

    $scope.loadTickets = function (type, status) {
        type = JSON.parse(type);
        status = JSON.parse(status);
        $scope.ticketType = type;
        $scope.ticketStatus = status;
        $scope.activeTab = 'tickets';
        $scope.loading = true;

        functions.post('Tickets', 'Load', {type: type, status: status }).then(function (d) {
            $scope.tickets = d;
            $scope.currIdx = null;
            $scope.loading = false;
        });
    }

    $scope.saveTicket = function (x, sendMail) {
        functions.post('Tickets', 'Save', {x, sendMail: sendMail, attachFile: false, lang: 'hr'}).then(function (d) {
            $scope.loadTickets($scope.ticketType, $scope.ticketStatus);
        });
    }

    $scope.addTicket = function () {
        functions.post('Tickets', 'Init', {}).then(function (d) {
            $scope.tickets.unshift(d);
        });
    }

    $scope.removeTicket = function (x) {
        if (confirm("Briši ticket: " + x.title + " (" + x.desc + ")?")) {
            functions.post('Tickets', 'Delete', { id: x.id }).then(function (d) {
                $scope.loadTickets($scope.ticketType, $scope.ticketStatus);
            });
        }
    }

    $scope.getStatusIcon = function (status) {
        switch (status) {
            case 0: return 'fa fa-exclamation-triangle text-danger';
                break;
            case 1: return 'fa fa-eye text-primary';
                break;
            case 2: return 'fa fa-check text-success';
                break;
            case 3: return 'fa fa-times-circle text-warning ';
                break;
            default: return null;
        }
    }
    /***** Tickets *****/

    $scope.checkAdminCode = (x) => {
        functions.post('Admin', 'CheckAdminCode', { code: x }).then((d) => {
            $rootScope.config.debug = d;
        });
    }

}])

.controller('ordersCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {
    $scope.searchOrders = null;
    function setYears() {
        $scope.years = [];
        for (var i = 2017; i <= $scope.year; i++) {
            $scope.years.push(i);
        }
        $scope.year = new Date().getFullYear();
    }
    setYears();

    var load = function (year, search) {
        functions.post('Orders', 'Load', {year: year, search: search}).then(function(d) {
            $scope.orders = d;
        });
    }
    load($scope.year, null);

    $scope.load = function (year, search) {
        return load(year, search);
    }

    $scope.createInvoice = function (order, tpl) {
        functions.post('Invoice', 'InitPP', {order: order}).then(function(d) {
            $rootScope.i = d;
            $rootScope.tpl = tpl;
        });
    }

    $scope.remove = function (x) {
        if (confirm("Briši narudžbu br. " + x.orderNumber + " (" + x.firstName + " " + x.lastName + " - " + x.companyName + ")?")) {
            functions.post('Orders', 'Delete', { id: x.id }).then(function (d) {
                load($scope.year, null);
                alert(d);
            });
        }
    }

}])

.controller('invoiceCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.searchInvoices = null;

    $scope.getTotal = function (x) {
        var total = 0;
        angular.forEach(x, function (value, key) {
            total += value.unitPrice * value.qty;
        })
        $scope.total = total;
        return total;
    }

    var getLocalDateAndTime = function () {
        var date = new Date();
        var month = parseInt(date.getMonth()) + 1;
        var min = parseInt(date.getMinutes()) < 10 ? '0' + date.getMinutes() : date.getMinutes();
        var res = date.getDate() + '.' + month + '.' + date.getFullYear() + ', ' + date.getHours() + ':' + min;
        return res;
    }

    var initForm = function () {
        $scope.isInvoice = false;
        $scope.pdfTempLink = null;
        $scope.pdfLink = null;
        $scope.loading = false;
        $scope.loading_1 = false;
        $scope.loading_2 = false;
        $scope.invoices = [];
        $scope.showInvoices = false;
        $scope.total = angular.isDefined($rootScope.i) ? $scope.getTotal($rootScope.i.items) : 0;
        $scope.year = new Date().getFullYear();
    }
    initForm();

    $scope.setNote = function (i) {
        if (i.isElectronicBill) {
            i.showSignature = false;
            i.note = 'Račun je ispostavljen elektroničkim putem i pravovaljan je bez potpisa i pečata.';
        } else {
            i.showSignature = true;
            i.note = null;
        }
    }

    $scope.init = function () {
        $scope.showInvoices = false;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $rootScope.i = JSON.parse(response.data.d);
         $rootScope.i.dateAndTime = getLocalDateAndTime();
         initForm();
     },
     function (response) {
         alert(response.data.d);
     });
    }
    if(angular.isUndefined($rootScope.i)) { $scope.init(); }

    $scope.load = function (year, search) {
        $scope.showInvoices = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Load',
            method: 'POST',
            data: { year: year, search: search }
        })
     .then(function (response) {
         $scope.invoices = JSON.parse(response.data.d);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    $scope.get = function (x) {
        $scope.showInvoices = false;
        $rootScope.i = x;
        $scope.getTotal($rootScope.i.items);
    }

    $scope.copy = function (x) {
        $scope.showInvoices = false;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $rootScope.i = JSON.parse(response.data.d);
         initForm();
         $rootScope.i.firstName = x.firstName;
         $rootScope.i.lastName = x.lastName;
         $rootScope.i.companyName = x.companyName;
         $rootScope.i.address = x.address;
         $rootScope.i.postalCode = x.postalCode;
         $rootScope.i.city = x.city;
         $rootScope.i.country = x.country;
         $rootScope.i.pin = x.pin;
         $rootScope.i.note = x.note;
         $rootScope.i.items = x.items;
         $scope.getTotal($rootScope.i.items);
     },
     function (response) {
         alert(response.data.d);
     });
    }

    $scope.add = function () {
        $rootScope.i.items.push({
            title: '',
            qty: 1,
            unitPrice: 0
        })
        $scope.getTotal($rootScope.i.items);
    }

    $scope.remove = function (idx) {
        $rootScope.i.items.splice(idx, 1);
        $scope.getTotal($rootScope.i.items);
    }

    $scope.createPdf = function (i) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        if (i.number == '' || i.number == null) {
            alert('enter order number');
            return false;
        }
        $scope.loading = true;
        $scope.pdfTempLink = null;
        $scope.pdfLink = null;
        $scope.tempFileName = null;
        $http({
            url: $rootScope.config.backend + 'PrintPdf.asmx/InvoicePdf',
            method: 'POST',
            data: { invoice: i }
        })
     .then(function (response) {
         $scope.loading = false;
         $scope.tempFileName = response.data.d;
         $scope.pdfTempLink = $rootScope.config.backend + 'upload/invoice/temp/' + $scope.tempFileName + '.pdf';
     },
     function (response) {
         $scope.loading = false;
         alert(response.data.d);
     });
    }

    $scope.save = function (i) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        $scope.loading_1 = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/Save',
            method: 'POST',
            data: { x: i, pdf: $scope.tempFileName }
        })
     .then(function (response) {
         $scope.loading_1 = false;
         $rootScope.i = JSON.parse(response.data.d);
         $scope.fileName = $rootScope.i.year + '/' + $rootScope.i.fileName; //  response.data.d;
         $scope.pdfLink = $rootScope.config.backend + 'upload/invoice/' + $scope.fileName + '.pdf';
     },
     function (response) {
         $scope.loading_1 = false;
         alert(response.data.d);
     });
    }

    $scope.saveDb = function (i) {
        if ($rootScope.i.firstName == null && $rootScope.i.lastName == null && $rootScope.i.companyName == null) {
            alert('Upiši ime ili naziv');
            return false;
        }
        $scope.loading_2 = true;
        $http({
            url: $rootScope.config.backend + 'Invoice.asmx/SaveDb',
            method: 'POST',
            data: { x: i }
        })
     .then(function (response) {
         $scope.loading_2 = false;
         $rootScope.i = JSON.parse(response.data.d);
        // alert(response.data.d);
     },
     function (response) {
         $scope.loading_2 = false;
         alert(response.data.d);
     });
    }

    $scope.setPaidAmount = function (x) {
        $scope.getTotal(x.items);
        if (x.isPaid == true) {
            $scope.i.paidAmount = $scope.total;
        } else {
            $scope.i.paidAmount = 0;
            $scope.i.paidDate = '';
        }
    }

    $scope.removeInvoice = function (x, year, search) {
        if (confirm("Briši račun br." + x.number + ", Iznos: " + x.total + "?")) {
            $http({
                url: $rootScope.config.backend + 'Invoice.asmx/Delete',
                method: 'POST',
                data: { x: x }
            })
            .then(function (response) {
                $scope.load(year, search);
                alert(response.data.d);
            },
            function (response) {
                alert(response.data.d);
            });
        }
    }

}])

.controller('settingsCtrl', ['$scope', '$http', '$rootScope', 'functions', function ($scope, $http, $rootScope, functions) {

    var load = function () {
        functions.post('Files', 'LoadSettings', {}).then(function (d) {
            $scope.settings = d;
        });
    }
    load();

    $scope.save = function (settings) {
        if (settings.discount.perc === undefined || settings.discount.perc === null || settings.discount.perc < 0 || settings.discount.perc > 99) {
            alert('Popust mora biti brojčana vrijednost od 0 do 99.');
            return;
        }
        functions.post('Files', 'SaveSettings', { settings: settings }).then(function (d) {
            $scope.settings = d;
        });
    }

}])

.factory('functions', ['$http', function ($http) {
    return {
        post: function (service, webmethod, data) {
            return $http({
                url: '../' + service + '.asmx/' + webmethod, method: 'POST', data: data
            }).then(function (response) {
                return JSON.parse(response.data.d);
            },function (response) {
                return response.data.d;
            });
        }
    }
}]);


;
