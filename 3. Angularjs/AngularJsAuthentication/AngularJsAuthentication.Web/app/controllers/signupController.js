'use strict';
app.controller('signupController', ['$scope', '$location', '$timeout', 'authService', function ($scope, $location, $timeout, authService) {
    $scope.savedSuccessfully = false;
    $scope.message = "";
    $scope.registration = {
        userName: "",
        password: "",
        confirmPassword: ""
    };
    $scope.signUp = function () {
        authService.saveRegistration($scope.registration).then(function (response) {
            $scope.savedSuccessfully = true;
            $scope.message = "Đăng kí thành công";
            startTimer();
        },
            function (response) {
                var errors = [];
                for (var key in response.data.ModelState) {
                    for (var i = 0; i < response.data.ModelState[key].length; i++) {
                        errors.push(response.data.ModelState[key][i]);
                    }
                }
                $scope.message = "Đăng kí không thành công:" + errors.join(' ');
            });
    };
    var startTimer = function () {
        var timer = $timeout(function () {
            $timeout.cancel(timer);
            $location.path('/login');
        }, 2000);
    }
}]);
