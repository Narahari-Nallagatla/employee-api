using EmployeeApi.Models;

namespace EmployeeApi.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAll();
        Task<Employee> GetById(int id);
        Task Create(Employee emp);
        Task Update(Employee emp);
        Task Delete(int id);
        Task<List<Employee>> GetPaged(int pageNumber, int pageSize);
        Task<List<Employee>> Search(string search, string sortBy);
        Task<int> GetCount();
    }
}