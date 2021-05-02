/*!
app.js
(c) 2018-2021 IG PROG, www.igprog.hr
*/
angular.module('app', ['ui.router', 'ngMaterial', 'charts'])

.config(['$stateProvider', '$urlRouterProvider', '$httpProvider', '$locationProvider', function ($stateProvider, $urlRouterProvider, $httpProvider, $locationProvider) {

    $stateProvider
        .state('home', {
            url: '/', templateUrl: './assets/pages/home.html', controller: 'appCtrl', data: {
                pageTitle: '',
                pageDescription: ''
            }
        })
        .state('about', {
            url: '/o-programu', templateUrl: './assets/pages/about.html', controller: 'appCtrl',
            data: {
                pageTitle: ' | O Aplikaciji',
                pageDescription: 'Aplikacija Program Prehrane Web je alata koji nutricionistima pomaže u izradi jelovnika i praćenju klijenata.'
            }
        })
        .state('price', {
            url: '/cijene', templateUrl: './assets/pages/price.html', controller: 'appCtrl',
            data: {
                pageTitle: ' | Cijene',
                pageDescription: 'Cijene paketa Start, Premium, Standard.'
            }
        })
        .state('registration', {
            url: '/registracija', templateUrl: './assets/pages/registration.html', controller: 'signupCtrl',
            data: {
                pageTitle: ' | Registracija',
                pageDescription: 'Registracija korisničkog računa.'
            }
        })
        .state('order', {
            url: '/narudzba', templateUrl: './assets/pages/order.html', controller: 'orderCtrl',
            data: {
                pageTitle: ' | Narudzba',
                pageDescription: 'Narudžba licence za korištenje web aplikacije Program Prehrane Web.'
            }
        })
        .state('contact', {
            url: '/kontakt', templateUrl: './assets/pages/contact.html', controller: 'appCtrl'
            , data: {
                pageTitle: '| Kontakt',
                pageDescription: 'Tehnička podrška i informacije o web aplikaciji Program Prehrne Web.'
            }
        })
        .state('uputa', {
            url: '/uputa', templateUrl: './assets/pages/uputa.html', controller: 'appCtrl',
            data: {
                pageTitle: ' | Uputa',
                pageDescription: 'Uputa za korištenje web aplikacije Program Prehrane Web.'
            }
        })
        .state('obavijesti', {
            url: '/obavijesti', templateUrl: './assets/pages/obavijesti.html',
            data: {
                pageTitle: ' | Obavijesti',
                pageDescription: 'Obavijesti o nadogradnjama aplikacije Program Prehrnae Web.'
            }
        })
        .state('cesto-postavljana-pitanja', {
            url: '/cesto-postavljana-pitanja', templateUrl: './assets/pages/cesto-postavljana-pitanja.html', controller: 'loginCtrl',
            data: {
                pageTitle: ' | Cesto postavljana pitanja',
                pageDescription: 'Ćesto postavljana pitanja korisnika aplikacije Program Prehrane Web.'
            }
        })
        .state('bmi', {
            url: '/bmi', templateUrl: './assets/pages/bmi.html', controller: 'bmiCtrl'
            , data: {
                pageTitle: ' | BMI',
                pageDescription: 'BMI Kalkulator. Kalkulator indeksa tjelesne mase.'
            }
        })
        .state('aplikacija-za-klijente', {
            url: '/aplikacija-za-klijente', templateUrl: './assets/pages/aplikacija-za-klijente.html',
            data: {
                pageTitle: ' | Aplikacija za klijente',
                pageDescription: 'Program Prehrane Klijent. Modul za klijente.'
            }
        })
        .state('izrada-uravnotezenog-jelovnika', {
            url: '/izrada-uravnotezenog-jelovnika', templateUrl: './assets/pages/izrada-uravnotezenog-jelovnika.html',
            data: {
                pageTitle: ' | Izrada uravnotezenog jelovnika',
                pageDescription: 'Osnova pravilne prehrane odnosno redukcijske dijete je uravnotežen odnos potrošnje i unosa energije te adekvatna zastupljenost namirnica iz svih skupina.'
            }
        })
        .state('plan-prehrane', {
            url: '/plan-prehrane', templateUrl: './assets/pages/plan-prehrane.html',
            data: {
                pageTitle: ' | Plan prehrane',
                pageDescription: 'Osnova pravilne prehrane odnosno redukcijske dijete je uravnotežen odnos potrošnje i unosa energije te adekvatna zastupljenost namirnica iz svih skupina.'
            }
        })
        .state('tablica-namirnica', {
            url: '/tablica-namirnica', templateUrl: './assets/pages/tablica-namirnica.html', controller: 'foodCtrl',
            data: {
                pageTitle: ' | Tablica namirnica',
                pageDescription: 'Popis namirnica iz Web aplikacij.'
            }
        })
        .state('program-prehrane-za-skole', {
            url: '/program-prehrane-za-skole', templateUrl: './assets/pages/program-prehrane-za-skole.html',
            data: {
                pageTitle: ' | Program Prehrane za Škole',
                pageDescription: 'Primjena web aplikacije za izradu jelovnika u Obrazovnim ustanovama.'
            }
        })
        .state('program-prehrane-5', {
            url: '/program-prehrane-5', templateUrl: './assets/pages/program-prehrane-5.html', controller: 'appCtrl',
            data: {
                pageTitle: '| Verzija 5.0',
                pageDescription: ' | Računalni program za izradu jelovnika Program Prehrane 5.0.'
            }
        })
        .state('povijest', {
            url: '/povijest', templateUrl: './assets/pages/povijest.html',
            data: {
                pageTitle: ' | Povijest',
                pageDescription: 'Program Prehrane je nastao je iz potrebe za izradom alata koji će nutricionistima i zdravstvenim djelatnicima ali i svim pojedincima koji vode računa o zdravoj i uravnoteženoj prehrani omogućiti brzu i jednostavnu izradu plana prehrane i programa tjelesne aktivnosti.'
            }
        })
        .state('placanje-paypal', {
            url: '/placanje-paypal', templateUrl: './assets/pages/paypal.html', controller: 'appCtrl',
            data: {
                pageTitle: ' | Plaćanje - PayPal',
                pageDescription: 'Plaćanje putem Pay-Pal-a.'
            }
        })
        .state('sr', {
            url: '/sr', templateUrl: './assets/pages/sr/home.html', controller: 'appCtrl',
            data: {
                pageTitle: ' | Plan Ishrane',
                pageDescription: 'Verzija na Srpskom jeziku'
            }
        })

    $urlRouterProvider.otherwise("/");

    if (window.history && window.history.pushState) {
        $locationProvider.html5Mode({
            enabled: true,
            requireBase: false
        });
    }

    //--------------disable catche---------------------
    if (!$httpProvider.defaults.headers.get) {
        $httpProvider.defaults.headers.get = {};
    }
    $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
    $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
    $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //-------------------------------------------------
}])

.run(['$rootScope', '$state',
    function ($rootScope, $state) {
        $rootScope.$state = $state;
}])

.controller('appCtrl', ['$scope', '$http', '$rootScope', '$anchorScroll', function ($scope, $http, $rootScope, $anchorScroll) {

    $scope.discount = null;
    var getDiscount = function () {
        $http({
            url: $rootScope.config.backend + 'Prices.asmx/GetDiscount',
            method: 'POST',
            data: {}
        })
         .then(function (response) {
             $scope.discount = JSON.parse(response.data.d);
         },
         function (response) {
             alert(JSON.parse(response.data.d));
         });
    }

    var reloadPage = () => {
        if (typeof (Storage) !== 'undefined') {
            if (localStorage.version) {
                if (localStorage.version !== $scope.config.version) {
                    localStorage.version = $scope.config.version;
                    window.location.reload(true);
                }
            } else {
                localStorage.version = $scope.config.version;
            }
        }
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              reloadPage();
              getDiscount();
          });
    };
    getConfig();

    $scope.pp5DownloadEnableCode = null;
    $scope.showDownloadLink = false;
    $scope.checkPp5DownloadEnableCode = function (code) {
        $http({
            url: $rootScope.config.backend + 'Admin.asmx/Check5DownloadEnableCode',
            method: 'POST',
            data: { fileName: 'pp5DownloadEnableCode.txt', code: code }
        })
     .then(function (response) {
         $scope.showDownloadLink = JSON.parse(response.data.d);
         if (!$scope.showDownloadLink) {
             alert('Pogrešan kod!');
         }
     },
     function (response) {
         alert(JSON.parse(response.data.d));
     });
    }

    $scope.pp5lang = 'hr';
    $scope.setDownloadLink = function (x) {
        if (x == 'hr') { $scope.pp5downloadlink = './download/' + x + '/ProgramPrehrane5.exe'; }
        if (x == 'rs') { $scope.pp5downloadlink = './download/' + x + '/ProgramPrehrane5S.exe'; }
    }
    $scope.setDownloadLink($scope.pp5lang);

    var d = new Date();
    $scope.year = d.getFullYear();

    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = 'Dalje';

    $scope.hashId = function (id) {
        window.location.hash = id;
    }

    $scope.href = function (x) {
        window.open(x, '_blank');
    }

    $scope.today = new Date;
    $scope.send = function (g) {
        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $http({
            url: $rootScope.config.backend + 'Mail.asmx/Send',
            method: 'POST',
            data: { name: g.name, email: g.email, phone: g.phone, address: g.address, type: g.type, message: g.message, lang: $rootScope.config.language }
        })
     .then(function (response) {
         $scope.sendicon = 'fa fa-check';
         $scope.sendicontitle = 'Poslano';
     },
     function (response) {
         $scope.sendicon = 'fa fa-exclamation-triangle';
         $scope.sendicontitle = 'Greška.';
         alert(response.data.d);
     });
    }

    $scope.showCustomers = false;
    $scope.toggleCustomers = function () {
        $scope.showCustomers = !$scope.showCustomers;
    };

    $scope.premiumUsers = 5;
    $scope.premiumUsers_ = 5;
    var maxUsers = function () {
        $scope.maxUsers = [];
        for (var i = 5; i < 101; i++) {
            $scope.maxUsers.push(i);
        }
    }
    maxUsers();

    $scope.premiumPriceOneYear = 1850;
    $scope.premiumPriceTwoYear = 2960;
    $scope.getPremiumPrice = function (x) {
        $scope.premiumPriceOneYear = x > 5 ? 1850 + ((x - 5) * 500) : 1850;
        $scope.premiumPriceTwoYear = x > 5 ? 2960 + ((x - 5) * 500) : 2960;
        $scope.premiumUsers = x;
        $scope.premiumUsers_ = x;
    }

    $scope.scrollTo = function (id) {
        $anchorScroll(id);
    }

}])

//.controller('webAppCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
//    $rootScope.application = 'Program Prehrane Web';
//    $rootScope.version = 'STANDARD';
//}])

//.controller('pp5Ctrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
//    $rootScope.application = 'Program Prehrane 5.0';
//    $rootScope.version = 'PREMIUM';

//    $scope.gotoForm = function () {
//        $scope.showUserDetails = true;
//    }

//}])

.controller('signupCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.msg = { title: null, css: null, icon: null }
    $scope.signupok = false;

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Init',
            method: 'POST',
            data: ''
        })
        .then(function (response) {
            $scope.user = JSON.parse(response.data.d);
            $scope.passwordConfirm = null;
            $scope.emailConfirm = null;
            $scope.accept = false;
        },
        function (response) {
            alert(response.data.d);
        });
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              init();
          });
    };
    if ($rootScope.config == undefined) {
        getConfig();
    } else {
        init();
    }

    var validationFormDanger = function () {
        $scope.msg.css = 'danger';
        $scope.msg.icon = 'exclamation';
        $scope.sendicon = 'fa fa-sign-in-alt';
        $scope.sendicontitle = 'REGISTRACIJA';
        $scope.signupok = false;
        $scope.isSendButtonDisabled = false;
    }

    var validationFormSuccess = function () {
        $scope.msg.css = 'success';
        $scope.msg.icon = 'check';
        $scope.signupok = true;
        $scope.sendicon = 'fa fa-sign-in-alt';
        $scope.sendicontitle = 'REGISTRACIJA';
        $scope.isSendButtonDisabled = true;
    }

    $scope.sendicon = 'fa fa-sign-in-alt';
    $scope.sendicontitle = 'REGISTRACIJA';
    $scope.isSendButtonDisabled = false;
    $scope.signup = function (user, emailConfirm, passwordConfirm, accept) {
        $scope.msg = { title: null, css: null, icon: null }
        user.userName = user.email;
        if (user.firstName == "" || user.lastName == "" || user.email == "" || user.password == "" || passwordConfirm == "" || emailConfirm == "") {
            $scope.msg.title = 'Sva polja su obavezna.';
            validationFormDanger();
            return false;
        }
        if (user.email != emailConfirm) {
            $scope.msg.title = 'Email adrese moraju biti jednake.';
            validationFormDanger();
            return false;
        }
        if (user.password != passwordConfirm) {
            $scope.msg.title = 'Lozinke moraju biti jednake.';
            validationFormDanger();
            return false;
        }
        if (accept == false) {
            $scope.msg.title = 'Morate prihvatiti uvjete korištenja.';
            validationFormDanger();
            return false;
        }
        $scope.signupok = false;
        $scope.sendicon = 'fas fa-spinner fa-spin';
        $scope.sendicontitle = 'Pričekajte trenutak...';
        $scope.isSendButtonDisabled = true;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Signup',
            method: 'POST',
            data: { x: user, lang: $rootScope.config.language }
        })
       .then(function (response) {
           if (JSON.parse(response.data.d) === 'the email address you have entered is already registered') {
               $scope.msg.title = 'E-mail adresa koju ste upisali je već registrirana';
               validationFormDanger();
               init();
           }
           if (JSON.parse(response.data.d) === 'registration completed successfully') {
               $scope.msg.title = 'Registracija upješno završena';
               validationFormSuccess();
               window.location.hash = 'registration';
               sendSignupMail(user, $rootScope.config.language);
           }
       },
       function (response) {
           alert(response.data.d);
           $scope.signupok = false;
           $scope.sendicon = 'fa fa-sign-in-alt';
           $scope.sendicontitle = 'REGISTRACIJA';
           $scope.isSendButtonDisabled = true;
       });
    }

    var sendSignupMail = function (newUser, lang) {
        debugger;
        $http({
            url: $rootScope.config.backend + 'Users.asmx/SendSignupMail',
            method: 'POST',
            data: { x: newUser, lang: lang }
        })
        .then(function (response) {
        },
        function (response) {
        });
    }

}])

.controller('orderCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.application = 'Program Prehrane Web'; // $rootScope.application; // === undefined ? 'Program Prehrane Web' : $rootScope.application;
    $scope.version = 'PREMIUM'; // $rootScope.version; // === undefined ? 'PREMIUM' : $rootScope.version;
    $scope.userType = 1;
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-angle-double-right';
    $scope.sendicontitle = 'Dalje';
    $scope.showUserDetails = false;
    $scope.showErrorAlert = false;
    $scope.showPaymentDetails = false;

    var init = function () {
        $http({
            url: $rootScope.config.backend + 'Orders.asmx/Init',
            method: 'POST',
            data: ''
        })
     .then(function (response) {
         $scope.user = JSON.parse(response.data.d);
         $scope.user.application = $scope.application;
         $scope.user.version = $rootScope.application == 'Program Prehrane' ? 'PREMIUM' : 'STANDARD';
         $scope.user.licence = 1; // $rootScope.application == 'Program Prehrane' ? '1' : '0';
         $scope.user.licenceNumber = 1;
         $scope.user.userType = $scope.userType;
         $scope.calculatePrice();
     },
     function (response) {
         alert(response.data.d);
     });
    }

    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $rootScope.config = response.data;
              init();
          });
    };

    if ($rootScope.config === undefined) {
        getConfig();
    } else {
        init();
    }

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

        if ($scope.user.application == 'Program Prehrane Web') {
            if ($scope.user.userType == 0) { unitprice = 550; $scope.user.version = 'START'; }
            if ($scope.user.userType == 1) { unitprice = 950; $scope.user.version = 'STANDARD'; }
            if ($scope.user.userType == 2) { unitprice = 1850; $scope.user.version = 'PREMIUM'; }

            if ($scope.user.licence > 1) {
                unitprice = unitprice * $scope.user.licence - ((unitprice * $scope.user.licence) * ($scope.user.licence / 10))
            }

            $scope.user.licenceNumber = 1;
        } else {
            $scope.user.version = $scope.user.version == '' ? 'PREMIUM' : $scope.user.version;
            if ($scope.user.version == 'START') {
                if ($scope.user.licence == '1') {
                    unitprice = 350;
                } else {
                    unitprice = 650;
                }
            }
            if ($scope.user.version == 'PREMIUM') {
                if ($scope.user.licence == '1') {
                    unitprice = 550;
                } else {
                    unitprice = 950;
                }
            }
        }

        totalprice = $scope.user.licenceNumber > 1 ? unitprice * $scope.user.licenceNumber - (unitprice * $scope.user.licenceNumber * 0.1) : unitprice;
        var additionalUsers = $scope.premiumUsers > 5 && $scope.user.userType == 2 ? ($scope.premiumUsers - 5) * 500 : 0;  // 500kn/additional user;
        $scope.user.price = totalprice + additionalUsers;
        $scope.user.priceEur = (totalprice + additionalUsers) / $rootScope.config.eur;

        if ($scope.user.discountCoeff > 0) {
            $scope.user.priceWithDiscount = $scope.user.price - (Math.round($scope.user.price * $scope.user.discountCoeff) * 100 / 100);
            $scope.user.priceWithDiscountEur = $scope.user.priceEur - (Math.round($scope.user.priceEur * $scope.user.discountCoeff) * 100 / 100);
        }
    }

    $scope.order = function (application, version) {
        init();
        window.location.hash = 'order';
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
            $scope.errorMessage = 'Sva polja su obavezna.';
            return false;
        }
        if ($scope.userType == 1) {
            if (user.companyName == '' || user.pin == '') {
                $scope.showErrorAlert = true;
                $scope.errorMessage = 'Sva polja su obavezna.';
                return false;
            }
        }
        if (!(user.country.toLowerCase().startsWith('hr') || user.country.toLowerCase().startsWith('cro'))) {
            user.isForeign = true;
        }
        user.maxNumberOfUsers = $scope.premiumUsers;

        $scope.sendicon = 'fas fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem... pričekajte trenutak.';
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
               $scope.isSendButtonDisabled = false;
               $scope.sendicon = 'fa fa-angle-double-right';
               $scope.sendicontitle = 'Pošalji';
               alert("GREŠKA! Narudžba nije poslana.");
           } else {
               $scope.showAlert = true;
               $scope.showPaymentDetails = true;
               window.location.hash = 'orderform';
           }
       },
       function (response) {
           $scope.showAlert = false;
           $scope.showPaymentDetails = false;
           $scope.isSendButtonDisabled = false;
           $scope.sendicon = 'fa fa-angle-double-right';
           $scope.sendicontitle = 'Pošalji';
           alert(response.data.d);
       });
    }

    $scope.login = function (u, p) {
        $http({
            url: $rootScope.config.backend + 'Users.asmx/Login',
            method: "POST",
            data: {
                userName: u,
                password: p
            }
        })
        .then(function (response) {
            var user = JSON.parse(response.data.d);
            if (user.userId != null) {
                $scope.user.firstName = user.firstName;
                $scope.user.lastName = user.lastName;
                $scope.user.companyName = user.companyName;
                $scope.user.address = user.address;
                $scope.user.postalCode = user.postalCode;
                $scope.user.city = user.city;
                $scope.user.country = user.country;
                $scope.user.pin = user.pin;
                $scope.user.email = user.email;
                $scope.showUserDetails = true;
                $scope.showErrorAlert = false;
            } else {
                $scope.showErrorAlert = true;
                $scope.errorMessage = 'Pogrešna kombinacija korisničkog imena i lozinke'
            }
        },
        function (response) {
            $scope.errorLogin = true;
            $scope.showErrorAlert = true;
            $scope.errorMessage = 'Pogrešna kombinacija korisničkog imena i lozinke'
            $scope.showUserDetails = false;
        });
    }

    $scope.registration = function () {
        window.location.hash = 'registration';
    }

    $scope.gotoForm = function () {
        $scope.showUserDetails = true;
    }

    $scope.forgotPassword = function (x) {
        $scope.showErrorAlert = false;
        $scope.showSuccessAlert = false;
        if (x == null || x == '') {
            $scope.showErrorAlert = true;
            $scope.errorMessage = 'Upišite E-mail koji ste koristili za registraciju.'
        } else {
            $http({
                url: $rootScope.config.backend + 'Users.asmx/ForgotPassword',
                method: "POST",
                data: { email: x, lang: $rootScope.config.language }
            })
          .then(function (response) {
              $scope.showSuccessAlert = true;
              $scope.successMessage = JSON.parse(response.data.d);
          },
          function (response) {
              $scope.showErrorAlert = true;
              $scope.errorMessage = JSON.parse(response.data.d);
          });
        }
    }


}])

.controller('contactCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    $scope.showAlert = false;
    $scope.sendicon = 'fa fa-paper-plane';
    $scope.sendicontitle = 'Pošalji';

    $scope.d = {
        name: '',
        email: '',
        message: ''
    }

    $scope.send = function (d) {
        if ($rootScope.config.backend == undefined) { $rootScope.getConfig(); }
        $scope.isSendButtonDisabled = true;
        $scope.sendicon = 'fa fa-spinner fa-spin';
        $scope.sendicontitle = 'Šaljem';
        $http({
            url: $rootScope.config.backend + 'Mail.asmx/Send',
            method: 'POST',
            data: { name: d.name, email: d.email, messageSubject: 'Program Prehrane - Upit', message: d.message, lang: $rootScope.config.language }
        })
       .then(function (response) {
           $scope.showAlert = true;
           $scope.sendicon = 'fa fa-paper-plane';
           $scope.sendicontitle = 'Pošalji';
           window.location.hash = 'contact';
       },
       function (response) {
           $scope.showAlert = false;
           $scope.sendicon = 'fa fa-paper-plane';
           $scope.sendicontitle = 'Pošalji';
           alert(response.data.d);
       });
    }

}])

.controller('foodCtrl', ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {
    var getConfig = function () {
        $http.get('./config/config.json')
          .then(function (response) {
              $scope.config = response.data;
              load(null);
          });
    };

    $scope.loading = false;
    var load = function (x) {
        $scope.loading = true;
        $http({
            url: $scope.config.backend + 'Foods.asmx/LoadFoods',
            method: 'POST',
            data: { lang: 'hr' }
        })
       .then(function (response) {
           $scope.loading = false;
           $scope.foods = JSON.parse(response.data.d);
       },
       function (response) {
           $scope.loading = false;
           alert(response.data.d);
       });
    };

    getConfig();

    $scope.limit = 50;
    $scope.showMore = function (x) {
        $scope.limit += x;
    }

}])

.controller('bmiCtrl', ['$scope', '$timeout', 'charts', function ($scope, $timeout, charts) {
    $scope.d = {
        height: null,
        weight: null,
        bmi: null,
        description: '',
        css: '',
        calculate: function () {
            this.bmi = (this.weight * 10000 / (this.height * this.height)).toFixed(1);
            this.description = getBmiTitle(this.bmi).des;
            this.css = getBmiTitle(this.bmi).css;
            getCharts();
        }
    }

    var getBmiTitle = function (x) {
        var res = {
            des: '',
            css: ''
        }
        if (x < 18.5) { res.des = 'snižena tjelesna masa', res.css = 'info'; }
        if (x >= 18.5 && x <= 25) { res.des = "normalan tjelesna masa"; res.css = 'success'; }
        if (x > 25 && x < 30) { res.des = "povišena tjelesna masa"; res.css = 'warning'; }
        if (x >= 30) { res.des = "gojaznost"; res.css = 'danger'; }
        return res;
    }

    var bmiChart = function () {
        var id = 'bmiChart';
        var value = $scope.d.bmi;
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

    var getCharts = function () {
        google.charts.load('current', { 'packages': ['gauge'] });
        $timeout(function () {
            bmiChart();
        }, 300);
    }
    getCharts();
}])

/***** Directive *****/
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
              '<div ng-if="strength >= 1 && strength < 2" class="progress-bar progress-bar-danger" style="width:25%">Niska</div>' +
              '<div ng-if="strength >= 2 && strength < 3" class="progress-bar progress-bar-warning" style="width:50%">Srednja</div>' +
              '<div ng-if="strength >= 3 && strength < 4" class="progress-bar progress-bar-warning" style="width:75%">Dobra</div>' +
              '<div ng-if="strength >= 4" class="progress-bar progress-bar-success" style="width:100%">Visoka</div>' +
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
])
.directive('awardDirective', () => {
    return {
        restrict: 'E',
        scope: {
            shortdesc: '=',
            longdesc: '=',
            img: '='
        },
        templateUrl: '../assets/partials/directives/award.html'
    };
})
.directive('discountDirective', () => {
    return {
        restrict: 'E',
        scope: {
            discount: '='
        },
        templateUrl: '../assets/partials/directives/discount.html',
        controller: 'discountCtrl'
    };
})
.controller('discountCtrl', ['$scope', function ($scope) {
    $scope.show = (discount) => {
        return getDateDiff(discount.dateTo) >= 0 && discount.perc > 0 && discount.title;
    }
     var getDateDiff = function (x) {
        var today = new Date();
        var date1 = new Date(x);
        var diffDays = parseInt((date1 - today) / (1000 * 60 * 60 * 24));
        return diffDays;
    }
}])
.directive('priceDirective', () => {
    return {
        restrict: 'E',
        scope: {
            pack: '=',
            discount: '=',
            premiumone: '=',
            premiumtwo: '=',
            eur: '='
        },
        templateUrl: '../assets/partials/directives/price.html'
    };
})
;
/***** Directive *****/

;