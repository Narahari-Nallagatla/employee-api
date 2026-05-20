namespace EmployeeApi.Models
{
    //[Authorize(Roles = "Admin")]
    //[Route("api/[controller]")]
    //[ApiController]
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Department { get; set; }

        public decimal Salary { get; set; }
    }
}
