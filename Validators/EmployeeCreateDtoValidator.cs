using EmployeeApi.DTOs;
using FluentValidation;

namespace EmployeeApi.Validators
{
    public class EmployeeCreateDtoValidator
        : AbstractValidator<EmployeeCreateDto>
    {
        public EmployeeCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(3);

            RuleFor(x => x.Department)
                .NotEmpty();

            RuleFor(x => x.Salary)
                .GreaterThan(0);
        }
    }
}