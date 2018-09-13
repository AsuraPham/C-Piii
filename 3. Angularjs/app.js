var app=angular.module('app',[]);
app.controller("myCtrl", function($scope,$http){
    $scope.getData=function(){
        $http({
            method:'GET',
            url:'https://training.gemvietnam.com/dummy-api/users.json'
        })
       
    }
})