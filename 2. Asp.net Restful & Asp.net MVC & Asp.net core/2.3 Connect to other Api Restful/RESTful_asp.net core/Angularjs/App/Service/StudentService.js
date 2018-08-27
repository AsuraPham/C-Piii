(function () {
    'use strict';

    angular
        .module('app')
        .factory('StudentService', StudentService);

    StudentService.$inject = ['$http'];

    function StudentService($http) {
        var service = {
            getData: getData
        };

        return service;

        function getData() {
            return $http.get("api/student");
        }
    }
})();