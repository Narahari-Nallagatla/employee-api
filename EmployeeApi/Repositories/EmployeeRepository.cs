using EmployeeApi.Data;
using EmployeeApi.Interfaces;
using EmployeeApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeApi.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task AddAsync(Employee emp)
        {
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee emp)
        {
            _context.Employees.Update(emp);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Employee>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Employees
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<List<Employee>> SearchAsync(string search, string sortBy)
        {
            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            query = sortBy?.ToLower() switch
            {
                "salary" => query.OrderBy(x => x.Salary),

                _ => query.OrderBy(x => x.Name)
            };

            return await query.ToListAsync();
        }
        public async Task<int> GetCountAsync()
        {
            return await _context.Employees.CountAsync();
        }       
    }
}   