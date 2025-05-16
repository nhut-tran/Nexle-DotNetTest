using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators;

public class SignInDtoValidator : AbstractValidator<SignInDto>
{
    public SignInDtoValidator()
    {
        RuleFor(s => s.Email).NotEmpty().EmailAddress();
        RuleFor(s => s.Password).NotEmpty();
    }
}