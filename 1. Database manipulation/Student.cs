public class Student
{
    public int StudentId{get;set;}
    public string FirstName{get;set;}
    public string LastName{get;set;}
}
Entity Data Model: class Student <==> table Name: Student(StudentId(PK, int, not null), FirstName(nvarchar(50), null), LastName(nvarchar(50), null));