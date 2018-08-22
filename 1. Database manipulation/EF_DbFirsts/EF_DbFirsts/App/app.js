var app=angular.module('myApp', ['ngRoute']);

app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/StudentsList', {
        templateUrl: '/App/Views/StudentsList.html',
        controller: 'StudentController'
    })
        .when('/AddStudent', {
            templateUrl: '/App/Views/AddStudent.html',
            controller: 'StudentController'
        })
        .when('/EditStudent/:stId', {
            templateUrl: '/App/Views/EditStudent.html',
            controller: 'StudentController'
        })
        .when('/DeleteStudent/:stId', {
            templateUrl: '/App/Views/DeleteStudent.html',
            controller: 'StudentController'
        })
        .otherwise({
            controller:'StudentController'
        })
}])
