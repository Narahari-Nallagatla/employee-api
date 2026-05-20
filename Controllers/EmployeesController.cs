using AutoMapper;
using EmployeeApi.DTOs;
using EmployeeApi.Helpers;
using EmployeeApi.Interfaces;
using EmployeeApi.Models;
using EmployeeApi.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

namespace EmployeeApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeesController> _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IEmployeeService service, ILogger<EmployeesController> logger, IMapper mapper)
        {
            _service = service;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var employees = await _service.GetPaged(pagination.PageNumber, pagination.PageSize);

            var totalRecords = await _service.GetCount();

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize);

            var result = _mapper.Map<List<EmployeeReadDto>>(employees);

            return Ok(new PagedResponse<List<EmployeeReadDto>>
            {
                Success = true,
                Message = "Employees fetched successfully",

                PageNumber = pagination.PageNumber,

                PageSize = pagination.PageSize,

                TotalRecords = totalRecords,

                TotalPages = totalPages,

                Data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _service.GetById(id);

            if (emp == null)
                return NotFound();

            return Ok(emp);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            var employee = _mapper.Map<Employee>(dto);

            await _service.Create(employee);

            return Ok("Created");
        }

        [HttpPut]
        public async Task<IActionResult> Update(Employee emp)
        {
            await _service.Update(emp);
            return Ok("Updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return Ok("Deleted");
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string search, string sortBy = "name")
        {
            var employees = await _service.Search(search, sortBy);

            return Ok(
                _mapper.Map<List<EmployeeReadDto>>(employees));
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEmployee(int id, [FromBody] JsonPatchDocument<EmployeePatchDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var employee =
                await _service.GetById(id);

            if (employee == null)
            {
                return NotFound();
            }

            var employeeDto =
                _mapper.Map<EmployeePatchDto>(employee);

            patchDoc.ApplyTo(employeeDto);

            _mapper.Map(employeeDto, employee);

            await _service.Update(employee);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Employee updated successfully",
                Data = null
            });
        }

    }
}