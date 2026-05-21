using EmployeeApi.Models;

namespace EmployeeApi.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee> GetByIdAsync(int id);
        Task AddAsync(Employee emp);
        Task UpdateAsync(Employee emp);
        Task DeleteAsync(int id);
        Task<List<Employee>> GetPagedAsync(int pageNumber, int pageSize);
        Task<List<Employee>> SearchAsync(string search, string sortBy);
        Task<int> GetCountAsync();

    }
}
