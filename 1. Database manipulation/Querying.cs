use linq
var context =new SchoolContext();
var students=(from s in context.Students
where s.FirstName=="ajaj" select s).toList();
context.SaveChanges();

Entity Framework Api chuyển sang câu lệnh truy vấn sql hoặc mysql.. sử dụng data provider và
trả lại kết quả 
<===>> select * from Students where FirstName="ajaj";
Sử dụng SaveChanges() để lưu thay đổi
