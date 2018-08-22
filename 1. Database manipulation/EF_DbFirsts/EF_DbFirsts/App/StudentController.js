app.controller('StudentController', ['$scope', '$http', '$location', '$routeParams', function ($scope, $http, $location, $routeParams) {
    $scope.ListStudents;
    $scope.Status;

    $scope.Close = function () {
        $location.path('/StudentsList');
    }

    //get all 
    var get = function () {
        $http.get("api/students/GetAllStudents").success(function (data) {
            $scope.ListStudents = data;
        })
            .error(function (data) {
                $scope.Status = "Data not found";
            });
    }
    var getdetail = function () {
        if ($routeParams.stId) {
            $http.get('api/students/GetStudent/'+$routeParams.stId).success(function (data) {
                console.log(data);
                $scope.Name = data.Name;
                $scope.Adress = data.Adress;
                $scope.Date = data.Date;
                $scope.Email = data.Email;
                $scope.Phone = data.Phone;
                $scope.StudentId = data.StudentId;
            });
        }
    }
    get();
    getdetail();
    //add new
    $scope.Add = function () {
        var StudentData = {
            Name: $scope.Name,
            Adress: $scope.Adress,
            Date: $scope.Date,
            Email: $scope.Email,
            Phone: $scope.Phone
        };
        $http.post("api/students/AddStudents", StudentData).success(function (data) {
            $location.path('/StudentsList');
        })
            .error(function (data) {
                console.log(data);
                $scope.error = "error adding" + data.ExceptionMessage;
            });
    }
    $scope.Update = function () {
        var StudentData = {
            Name: $scope.Name,
            Adress: $scope.Adress,
            Date: $scope.Date,
            Email: $scope.Email,
            Phone: $scope.Phone,
            StudentId: $scope.StudentId

        };
        if ($routeParams.stId> 0) {
            $http.put("api/students/EditStudents", StudentData).success(function (data) {
                $location.path('/StudentsList');
            })
                .error(function (data) {
                    $scope.error = "Error editing" + data.ExceptionMessage;
                });
        }
    }
    //delete
    $scope.Delete = function () {
        if ($routeParams.stId) {
            if ($routeParams.stId > 0) {
                $http.delete('api/students/DeleteStudents/?id=' + $routeParams.stId).success(function (data) {
                    $location.path('/StudentsList');
                })
                    .error(function (data) {
                        $scope.error = "Error deleting" + data.ExceptionMessage;
                    })
            }
        }
        
    }

}]);