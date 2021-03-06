﻿/*!
functions.js
(c) 2017-2021 IG PROG, www.igprog.hr
*/
angular.module('functions', [])

.factory('functions', ['$mdDialog', '$rootScope', '$window', '$translate', '$sessionStorage', '$state', '$http', function ($mdDialog, $rootScope, $window, $translate, $sessionStorage, $state, $http) {
    return {
        alert: function (title, content) {
            var confirm = $mdDialog.confirm()
            .title(title)
            .textContent(content)
            .targetEvent('')
            .ok($translate.instant('ok'))
            .cancel('');
            $mdDialog.show(confirm).then(function () {
            }, function () {
            });
        },
        checkPermissionAlert: function (alert, textContent) {
            var confirm = $mdDialog.confirm()
                .title($translate.instant(alert))
                .textContent($translate.instant(textContent))
                .ok($translate.instant('yes'))
                .cancel($translate.instant('not now'));
            $mdDialog.show(confirm).then(function () {
                $state.go('order');
            }, function () {
            });
        },
        checkPermission: function (user, license) {
            var note = null;
            var userType = 2;
            if (license === 'premium') {
                note = 'this function is available only in premium package';
                userType = 2;
            } else if (license === 'standard&premium') {
                note = 'this function is available only in standard and premium package';
                userType = 1;
            }
            if (user.userType < userType || user.licenceStatus === 'demo') {
                this.checkPermissionAlert(note, 'send order');
                return false;
            } else {
                return true;
            }
        },
        demoAlert: function (licenceStatus) {
            if (licenceStatus === 'demo') {
                this.checkPermissionAlert('this function is not available in demo version', 'activate full version');
                return false;
            } else {
                return true;
            }
        },
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
        copyToClipboard: function (id) {
            var el = document.getElementById(id);
            var range = document.createRange();
            range.selectNodeContents(el);
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
            document.execCommand('copy');
        },
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
        changeQuantity: function (initFood, x, type) {
            var food = angular.copy(initFood);
            var k = 1;
            if (!isNaN(x.quantity) && !isNaN(x.mass)) {
                switch (type) {
                    case "quantity":
                        k = Number(x.quantity) / Number(initFood.quantity);
                        food.quantity = x.quantity;
                        food.mass = this.smartRound(initFood.mass * k);
                        break;
                    case "mass":
                        k = Number(x.mass) / Number(initFood.mass);
                        food.mass = x.mass;
                        food.quantity = this.smartRound(initFood.quantity * k);
                        break;
                    default:
                        break;
                }
                return this.changeFQ(initFood, food, k);
            } else {
                return x;
            }
        },
        smartRound: function (value) {
            var i = 1;
            if (value >= 1) { i = 1; }
            if (value < 1 && value >= 0.1) { i = 2; }
            if (value < 0.1 && value >= 0.01) { i = 3; }
            if (value < 0.01 && value >= 0.001) { i = 4; }
            if (value < 0.001 && value >= 0.0001) { i = 5; }
            if (value < 0.0001) { i = 10; }
            return Number(value.toFixed(i));
        },
        changeFQ: function (initFood, x, k) {
            var smartRound = this.smartRound;
            x.unit = this.getUnit(x.quantity, initFood.unit);
            x.energy = smartRound(initFood.energy * k);
            x.carbohydrates = smartRound(initFood.carbohydrates * k);
            x.proteins = smartRound(initFood.proteins * k);
            x.fats = smartRound(initFood.fats * k);
            x.servings.cerealsServ = smartRound(initFood.servings.cerealsServ * k);
            x.servings.vegetablesServ = smartRound(initFood.servings.vegetablesServ * k);
            x.servings.fruitServ = smartRound(initFood.servings.fruitServ * k);
            x.servings.meatServ = smartRound(initFood.servings.meatServ * k);
            x.servings.milkServ = smartRound(initFood.servings.milkServ * k);
            x.servings.fatsServ = smartRound(initFood.servings.fatsServ * k);
            x.servings.otherFoodsServ = smartRound(initFood.servings.otherFoodsServ * 1);
            x.servings.otherFoodsEnergy = smartRound(initFood.servings.otherFoodsEnergy * k);
            x.starch = smartRound(initFood.starch * k);
            x.totalSugar = smartRound(initFood.totalSugar * k);
            x.glucose = smartRound(initFood.glucose * k);
            x.fructose = smartRound(initFood.fructose * k);
            x.saccharose = smartRound(initFood.saccharose * k);
            x.maltose = smartRound(initFood.maltose * k);
            x.lactose = smartRound(initFood.lactose * k);
            x.fibers = smartRound(initFood.fibers * k);
            x.saturatedFats = smartRound(initFood.saturatedFats * k);
            x.monounsaturatedFats = smartRound(initFood.monounsaturatedFats * k);
            x.polyunsaturatedFats = smartRound(initFood.polyunsaturatedFats * k);
            x.trifluoroaceticAcid = smartRound(initFood.trifluoroaceticAcid * k);
            x.cholesterol = smartRound(initFood.cholesterol * k);
            x.sodium = smartRound(initFood.sodium * k);
            x.potassium = smartRound(initFood.potassium * k);
            x.calcium = smartRound(initFood.calcium * k);
            x.magnesium = smartRound(initFood.magnesium * k);
            x.phosphorus = smartRound(initFood.phosphorus * k);
            x.iron = smartRound(initFood.iron * k);
            x.copper = smartRound(initFood.copper * k);
            x.zinc = smartRound(initFood.zinc * k);
            x.chlorine = smartRound(initFood.chlorine * k);
            x.manganese = smartRound(initFood.manganese * k);
            x.selenium = smartRound(initFood.selenium * k);
            x.iodine = smartRound(initFood.iodine * k);
            x.retinol = smartRound(initFood.retinol * k);
            x.carotene = smartRound(initFood.carotene * k);
            x.vitaminD = smartRound(initFood.vitaminD * k);
            x.vitaminE = smartRound(initFood.vitaminE * k);
            x.vitaminB1 = smartRound(initFood.vitaminB1 * k);
            x.vitaminB2 = smartRound(initFood.vitaminB2 * k);
            x.vitaminB3 = smartRound(initFood.vitaminB3 * k);
            x.vitaminB6 = smartRound(initFood.vitaminB6 * k);
            x.vitaminB12 = smartRound(initFood.vitaminB12 * k);
            x.folate = smartRound(initFood.folate * k);
            x.pantothenicAcid = smartRound(initFood.pantothenicAcid * k);
            x.biotin = smartRound(initFood.biotin * k);
            x.vitaminC = smartRound(initFood.vitaminC * k);
            x.vitaminK = smartRound(initFood.vitaminK * k);
            x.price.value = (initFood.price.value * k).toFixed(2);
            x.price.currency = initFood.price.currency;
            return x;
        },
        smartUnit: function (qty, unit, unit1, unit2) {
            if ((qty > 1 && qty < 5) || (qty > 0.1 && qty < 0.5)) { unit = unit1; }
            if (qty >= 5 || (qty >= 0.5 && qty < 1)) { unit = unit2; }
            return unit;
        },
        getUnit: function (qty, unit) {
            unit = this.initUnit(unit);
            var smartUnit = this.smartUnit;
            switch (unit) {
                // region hr
                case "jušna žlica":
                    unit = smartUnit(qty, unit, "jušne žlice", "jušnih žlica");
                    break;
                case "šalica":
                    unit = smartUnit(qty, unit, "šalice", "šalica");
                    break;
                case "plod":
                    unit = smartUnit(qty, unit, "ploda", "plodova");
                    break;
                case "čajna žličica":
                    unit = smartUnit(qty, unit, "čajne žličice", "čajnih žličica");
                    break;
                case "porcija":
                    unit = smartUnit(qty, unit, "porcije", "porcija");
                    break;
                case "limenka":
                    unit = smartUnit(qty, unit, "limenke", "limenki");
                    break;
                case "kriška":
                    unit = smartUnit(qty, unit, "kriške", "kriški");
                    break;
                case "boca":
                    unit = smartUnit(qty, unit, "boce", "boca");
                    break;
                case "čaša":
                    unit = smartUnit(qty, unit, "čaše", "čaša");
                    break;
                case "polovica":
                    unit = smartUnit(qty, unit, "polovice", "polovica");
                    break;
                case "mali komad":
                    unit = smartUnit(qty, unit, "mala komada", "malih komada");
                    break;
                case "listić":
                    unit = smartUnit(qty, unit, "listića", "listića");
                    break;
                case "zrno":
                    unit = smartUnit(qty, unit, "zrna", "zrna");
                    break;
                case "veliki plod":
                    unit = smartUnit(qty, unit, "velika ploda", "velikih plodova");
                    break;
                case "srednji plod":
                    unit = smartUnit(qty, unit, "srednja ploda", "srednjih plodova");
                    break;
                case "veliki komad":
                    unit = smartUnit(qty, unit, "velika komada", "velikih komada");
                    break;
                case "komad":
                    unit = smartUnit(qty, unit, "komada", "komada");
                    break;
                case "list":
                    unit = smartUnit(qty, unit, "lista", "listova");
                    break;
                case "filet":
                    unit = smartUnit(qty, unit, "fileta", "fileta");
                    break;
                case "čašica":
                    unit = smartUnit(qty, unit, "čašice", "čašica");
                    break;
                case "štruca":
                    unit = smartUnit(qty, unit, "štruce", "štruca");
                    break;
                case "pakiranje":
                    unit = smartUnit(qty, unit, "pakiranja", "pakiranja");
                    break;
                    // endregion hr

                    // region sr
                case "šoljica":
                    unit = smartUnit(qty, unit, "šoljice", "šoljica");
                    break;
                case "šolja":
                    unit = smartUnit(qty, unit, "šolje", "šolja");
                    break;
                case "parče":
                    unit = smartUnit(qty, unit, "parčeta", "parčeta");
                    break;
                case "čajna kašika":
                    unit = smartUnit(qty, unit, "čajne kašike", "čajnih kašika");
                    break;
                case "supena kašika":
                    unit = smartUnit(qty, unit, "supene kašike", "supenih kašika");
                    break;
                case "malo parče":
                    unit = smartUnit(qty, unit, "mala parčeta", "malih parčeta");
                    break;
                case "veliko parče":
                    unit = smartUnit(qty, unit, "velika parčeta", "velikih parčeta");
                    break;
                case "kašičica":
                    unit = smartUnit(qty, unit, "kašičice", "kašičica");
                    break;
                case "flaša":
                    unit = smartUnit(qty, unit, "flaše", "flaša");
                    break;
                case "vekna":
                    unit = smartUnit(qty, unit, "vekne", "vekni");
                    break;
                case "pakovanje":
                    unit = smartUnit(qty, unit, "pakovanja", "pakovanja");
                    break;
                    // endregion sr

                    // region sr_cyrl
                case "шољица":
                    unit = smartUnit(qty, unit, "шољице", "шољица");
                    break;
                case "шоља":
                    unit = smartUnit(qty, unit, "шоље", "шоља");
                    break;
                case "парче":
                    unit = smartUnit(qty, unit, "парчета", "парчета");
                    break;
                case "чајна кашика":
                    unit = smartUnit(qty, unit, "чајне кашике", "чајних кашика");
                    break;
                case "супена кашика":
                    unit = smartUnit(qty, unit, "супене кашике", "супених кашика");
                    break;
                case "мало парче":
                    unit = smartUnit(qty, unit, "мала парчета", "малих парчета");
                    break;
                case "велико парче":
                    unit = smartUnit(qty, unit, "велика парчета", "великих парчета");
                    break;
                case "кашичица":
                    unit = smartUnit(qty, unit, "кашичице", "кашичица");
                    break;
                case "флаша":
                    unit = smartUnit(qty, unit, "флаше", "флаша");
                    break;
                case "векна":
                    unit = smartUnit(qty, unit, "векне", "векни");
                    break;
                case "паковање":
                    unit = smartUnit(qty, unit, "паковања", "паковања");
                    break;
                    // endregion sr_cyrl

                    // region en
                case "cup":
                    if (qty > 1) { unit = "cups"; }
                    break;
                case "piece":
                    if (qty > 1) { unit = "pieces"; }
                    break;
                case "small piece":
                    if (qty > 1) { unit = "small pieces"; }
                    break;
                case "medium fruit":
                    if (qty > 1) { unit = "medium fruits"; }
                    break;
                case "tablespoon":
                    if (qty > 1) { unit = "tablespoons"; }
                    break;
                case "grain":
                    if (qty > 1) { unit = "grains"; }
                    break;
                case "great fruit":
                    if (qty > 1) { unit = "great fruits"; }
                    break;
                case "slice":
                    if (qty > 1) { unit = "slices"; }
                    break;
                case "glass":
                    if (qty > 1) { unit = "glasses"; }
                    break;
                case "bottle":
                    if (qty > 1) { unit = "bottles"; }
                    break;
                case "half":
                    if (qty > 1) { unit = "half"; }
                    break;
                case "big piece":
                    if (qty > 1) { unit = "big pieces"; }
                    break;
                case "loaf":
                    if (qty > 1) { unit = "loaves"; }
                    break;
                case "shot glass":
                    if (qty > 1) { unit = "shot glasses"; }
                    break;
                case "fillet":
                    if (qty > 1) { unit = "fillets"; }
                    break;
                default:
                    break;
            }
            return unit;
        },
        initUnit: function (unit) {
            // region hr
            if (unit == "jušne žlice" || unit == "jušnih žlica") { unit = "jušna žlica"; }
            if (unit == "šalice") { unit = "šalica"; }
            if (unit == "ploda" || unit == "plodova") { unit = "plod"; }
            if (unit == "čajne žličice" || unit == "čajnih žličica") { unit = "čajna žličica"; }
            if (unit == "porcije") { unit = "porcija"; }
            if (unit == "limenke") { unit = "limenka"; }
            if (unit == "kriške" || unit == "kriški") { unit = "kriška"; }
            if (unit == "boce") { unit = "boca"; }
            if (unit == "čaše") { unit = "čaša"; }
            if (unit == "polovice") { unit = "polovica"; }
            if (unit == "mala komada" || unit == "malih komada") { unit = "mali komad"; }
            if (unit == "listića") { unit = "listić"; }
            if (unit == "zrna") { unit = "zrno"; }
            if (unit == "velika ploda" || unit == "velikih plodova") { unit = "veliki plod"; }
            if (unit == "velika komada" || unit == "velikih komada") { unit = "veliki komad"; }
            if (unit == "komada") { unit = "komad"; }
            if (unit == "lista" || unit == "listova") { unit = "list"; }
            if (unit == "fileta") { unit = "filet"; }
            if (unit == "čašice") { unit = "čašica"; }
            if (unit == "srednja ploda" || unit == "srednjih plodova") { unit = "srednji plod"; }
            if (unit == "štruce") { unit = "štruca"; }
            if (unit == "pakiranja") { unit = "pakiranje"; }
            // endregion hr

            // region sr
            if (unit == "šoljice") { unit = "šoljica"; }
            if (unit == "šolje") { unit = "šolja"; }
            if (unit == "parčeta") { unit = "parče"; }
            if (unit == "čajne kašike" || unit == "čajnih kašika") { unit = "čajna kašika"; }
            if (unit == "supene kašike" || unit == "supenih kašika") { unit = "supena kašika"; }
            if (unit == "mala parčeta" || unit == "malih parčeta") { unit = "malo parče"; }
            if (unit == "velika parčeta" || unit == "velikih parčeta") { unit = "veliko parče"; }
            if (unit == "kašičice") { unit = "kašičica"; }
            if (unit == "flaše") { unit = "flaša"; }
            if (unit == "vekne" || unit == "vekni") { unit = "vekna"; }
            if (unit == "pakovanja") { unit = "pakovanje"; }
            // endregion sr

            // region sr_cyrl
            if (unit == "шољице") { unit = "шољица"; }
            if (unit == "шоље") { unit = "шоља"; }
            if (unit == "парчета") { unit = "парче"; }
            if (unit == "чајне кашике" || unit == "чајних кашика") { unit = "чајна кашика"; }
            if (unit == "супене кашике" || unit == "супених кашика") { unit = "супена кашика"; }
            if (unit == "мала парчета" || unit == "малих парчета") { unit = "мало парче"; }
            if (unit == "велика парчета" || unit == "великих парчета") { unit = "велико парче"; }
            if (unit == "кашичице") { unit = "кашичица"; }
            if (unit == "флаше") { unit = "флаша"; }
            if (unit == "векне" || unit == "векни") { unit = "векна"; }
            if (unit == "паковања") { unit = "паковање"; }
            // endregion sr_cyrl

            // region en
            if (unit == "cups") { unit = "cup"; }
            if (unit == "pieces") { unit = "piece"; }
            if (unit == "medium fruits") { unit = "medium fruit"; }
            if (unit == "tablespoons") { unit = "tablespoon"; }
            if (unit == "grains") { unit = "grain"; }
            if (unit == "great fruits") { unit = "great fruit"; }
            if (unit == "slices") { unit = "slice"; }
            if (unit == "glasses") { unit = "glass"; }
            if (unit == "bottles") { unit = "bottle"; }
            if (unit == "half") { unit = "half"; }
            if (unit == "big pieces") { unit = "big piece"; }
            if (unit == "loaves") { unit = "loaf"; }
            if (unit == "shot glasses") { unit = "shot glass"; }
            if (unit == "fillets") { unit = "fillet"; }
            // endregion
            return unit;
        }

    }
}]);

;
