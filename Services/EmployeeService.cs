using EmployeeApi.Interfaces;
using EmployeeApi.Models;

namespace EmployeeApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Employee>> GetAll()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Employee> GetById(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task Create(Employee emp)
        {
            // Business rules can go here
            if (emp.Salary < 0)
                throw new Exception("Salary cannot be negative");

            await _repo.AddAsync(emp);
        }

        public async Task Update(Employee emp)
        {
            await _repo.UpdateAsync(emp);
        }

        public async Task Delete(int id)
        {
            await _repo.DeleteAsync(id);
        }
        public async Task<List<Employee>> GetPaged(int pageNumber, int pageSize)
        {
            return await _repo.GetPagedAsync(pageNumber, pageSize);
        }
        public async Task<List<Employee>> Search(string search, string sortBy)
        {
            return await _repo.SearchAsync(search, sortBy);
        }
        public async Task<int> GetCount()
        {
            return await _repo.GetCountAsync();
        }
    }
}