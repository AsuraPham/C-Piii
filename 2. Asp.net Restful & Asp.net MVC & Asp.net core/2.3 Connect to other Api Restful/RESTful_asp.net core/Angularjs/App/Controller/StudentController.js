(function () {
    'use strict';

    angular
        .module('app')
        .controller('StudentController', StudentController);

    StudentController.$inject = ['$location','StudentService'];

    function StudentController($location, StudentService) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'StudentController';

        GetStudentList();

        function GetStudentList() {
            StudentService.getData().
        }
    }
})();
