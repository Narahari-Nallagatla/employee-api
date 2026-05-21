using System.Text.Json;
using AutoMapper;
using EmployeeApi.DTOs;
using EmployeeApi.Helpers;
using EmployeeApi.Interfaces;
using EmployeeApi.Models;
using EmployeeApi.Responses;
using EmployeeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        private readonly IMapper _mapper;

        private readonly ILogger<EmployeesController> _logger;

        private readonly IDistributedCache _cache;

        private readonly CacheService _cacheService;

        public EmployeesController(IEmployeeService service, IMapper mapper, ILogger<EmployeesController> logger, IDistributedCache cache, CacheService cacheService)
        {
            _service = service;

            _mapper = mapper;

            _logger = logger;

            _cache = cache;

            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            try
            {
                var version = await _cache.GetStringAsync("employees_version") ?? "1";

                var cacheKey = $"employees_v{version}_" + $"{pagination.PageNumber}_" + $"{pagination.PageSize}";
                var cacheData = await _cacheService.GetAsync<PagedResponse<List<EmployeeReadDto>>>(cacheKey);

                //var cacheData = await _cache.GetStringAsync(cacheKey);

                //if (!string.IsNullOrEmpty(cacheData))
                //{
                //    _logger.LogInformation("Employees loaded from Redis cache");

                //    var cachedResult = JsonSerializer.Deserialize<PagedResponse<List<EmployeeReadDto>>>(cacheData);

                //    return Ok(cachedResult);
                //}

                if (cacheData != null)
                {
                    _logger.LogInformation("Employees loaded from Redis cache");
                    return Ok(cacheData);
                }


                _logger.LogInformation("Employees loaded from Database");

                var employees = await _service.GetPaged(pagination.PageNumber, pagination.PageSize);

                var totalRecords = await _service.GetCount();

                var totalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize);

                var result = _mapper.Map<List<EmployeeReadDto>>(employees);

                var response = new PagedResponse<List<EmployeeReadDto>>
                {
                    Success = true,

                    Message = "Employees fetched successfully",

                    PageNumber = pagination.PageNumber,

                    PageSize = pagination.PageSize,

                    TotalRecords = totalRecords,

                    TotalPages = totalPages,

                    Data = result
                };

                //await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response),
                //    new DistributedCacheEntryOptions
                //    {
                //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                //    });
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching employees");

                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,

                    Message = "Internal server error",

                    Data = null
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string search, string sortBy = "name")
        {
            var employees = await _service.Search(search, sortBy);

            var result = _mapper.Map<List<EmployeeReadDto>>(employees);

            return Ok(
                new ApiResponse<List<EmployeeReadDto>>
                {
                    Success = true,

                    Message = "Employees fetched successfully",

                    Data = result
                });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cacheKey = $"employee_{id}";

            //  var cacheData = await _cache.GetStringAsync(cacheKey);
            var cacheData = await _cacheService.GetAsync<ApiResponse<EmployeeReadDto>>(cacheKey);


            //if (!string.IsNullOrEmpty(cacheData))
            //{
            //    _logger.LogInformation("Employee loaded from Redis cache");

            //    var cachedEmployee = JsonSerializer.Deserialize<ApiResponse<EmployeeReadDto>>(cacheData);

            //    return Ok(cachedEmployee);
            //}
            if (cacheData != null)
            {
                _logger.LogInformation("Employee loaded from cache");
                return Ok(cacheData);
            }

            var employee = await _service.GetById(id);

            if (employee == null)
            {
                return NotFound(
                    new ApiResponse<string>
                    {
                        Success = false,

                        Message = "Employee not found",

                        Data = null
                    });
            }

            var result = _mapper.Map<EmployeeReadDto>(employee);

            var response = new ApiResponse<EmployeeReadDto>
            {
                Success = true,

                Message = "Employee fetched successfully",

                Data = result
            };

            //await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response),
            //    new DistributedCacheEntryOptions
            //    {
            //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            //    });
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmployeeCreateDto dto)
        {
            var employee = _mapper.Map<Employee>(dto);

            await _service.Create(employee);

            await _cache.SetStringAsync("employees_version", DateTime.UtcNow.Ticks.ToString());

            return Ok(
                new ApiResponse<string>
                {
                    Success = true,

                    Message = "Employee created successfully",

                    Data = null
                });
        }

        [HttpPut]
        public async Task<IActionResult> Update(Employee emp)
        {
            await _service.Update(emp);

            await _cache.RemoveAsync($"employee_{emp.Id}");

            await _cache.SetStringAsync("employees_version", DateTime.UtcNow.Ticks.ToString());

            return Ok(
                new ApiResponse<string>
                {
                    Success = true,

                    Message = "Employee updated successfully",

                    Data = null
                });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);

            await _cache.RemoveAsync($"employee_{id}");

            await _cache.SetStringAsync("employees_version", DateTime.UtcNow.Ticks.ToString());

            return Ok(
                new ApiResponse<string>
                {
                    Success = true,

                    Message = "Employee deleted successfully",

                    Data = null
                });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchEmployee(int id, [FromBody] JsonPatchDocument<EmployeePatchDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var employee = await _service.GetById(id);

            if (employee == null)
            {
                return NotFound(
                    new ApiResponse<string>
                    {
                        Success = false,

                        Message = "Employee not found",

                        Data = null
                    });
            }

            var employeeDto = _mapper.Map<EmployeePatchDto>(employee);

            patchDoc.ApplyTo(employeeDto);

            _mapper.Map(employeeDto, employee);

            await _service.Update(employee);

            await _cache.RemoveAsync($"employee_{id}");

            await _cache.SetStringAsync("employees_version", DateTime.UtcNow.Ticks.ToString());

            return Ok(
                new ApiResponse<string>
                {
                    Success = true,

                    Message = "Employee patched successfully",

                    Data = null
                });
        }
    }
}