using EmployeeApi.Interfaces;
using EmployeeApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            IEmployeeRepository employeeRepo,
            ILogger<EmployeesController> logger)
        {
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("GetAll Employees API called");

            var employees = await _employeeRepo.GetAllAsync();

            _logger.LogInformation("GetAll Employees returned {Count} records", employees.Count);

            return Ok(employees);
        }

        // GET: api/employees/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("GetById called with ID: {Id}", id);

            var employee = await _employeeRepo.GetByIdAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found with ID: {Id}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (employee == null)
            {
                _logger.LogWarning("Create Employee called with null data");
                return BadRequest("Invalid employee data");
            }

            await _employeeRepo.AddAsync(employee);

            _logger.LogInformation("Employee created successfully with Name: {Name}", employee.Name);

            return Ok("Employee created successfully");
        }

        // PUT: api/employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                _logger.LogWarning("Update ID mismatch. RouteId: {RouteId}, BodyId: {BodyId}", id, employee.Id);
                return BadRequest("ID mismatch");
            }

            var existing = await _employeeRepo.GetByIdAsync(id);

            if (existing == null)
            {
                _logger.LogWarning("Update failed. Employee not found with ID: {Id}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            await _employeeRepo.UpdateAsync(employee);

            _logger.LogInformation("Employee updated successfully with ID: {Id}", id);

            return Ok("Employee updated successfully");
        }

        // DELETE: api/employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Delete Employee called with ID: {Id}", id);

            var existing = await _employeeRepo.GetByIdAsync(id);

            if (existing == null)
            {
                _logger.LogWarning("Delete failed. Employee not found with ID: {Id}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            await _employeeRepo.DeleteAsync(id);

            _logger.LogInformation("Employee deleted successfully with ID: {Id}", id);

            return Ok("Employee deleted successfully");
        }
    }
}