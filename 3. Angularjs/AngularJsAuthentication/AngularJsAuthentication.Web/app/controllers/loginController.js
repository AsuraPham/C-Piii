'use strict'
app.controller('loginController', ['$scope', '$location', 'authService', function ($scope, $loacation, authService) {
    $scope.loginData = {
        userName: "",
        password: ""
    };
    $scope.message = "";
    $scope.login = function () {
        authService.login($scope.loginData).then(function (response) {
            $loacation.path('/home');
        },
            function (err) {
                $scope.message = err.error_description;
            }
        );
    };
}]);