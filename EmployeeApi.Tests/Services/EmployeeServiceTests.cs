using EmployeeApi.Interfaces;
using EmployeeApi.Models;
using EmployeeApi.Services;
using Moq;
using Xunit;

namespace EmployeeApi.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository>
            _repoMock;

        private readonly EmployeeService _service;

        public EmployeeServiceTests()
        {
            _repoMock =
                new Mock<IEmployeeRepository>();

            _service =
                new EmployeeService(
                    _repoMock.Object);
        }

        [Fact]
        public async Task GetById_ShouldReturnEmployee()
        {
            // Arrange

            var employee =
                new Employee
                {
                    Id = 1,
                    Name = "Hari",
                    Department = "IT",
                    Salary = 50000
                };

            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(employee);

            // Act

            var result =
                await _service.GetById(1);

            // Assert

            Assert.NotNull(result);

            Assert.Equal(
                "Hari",
                result.Name);
        }
    }
}