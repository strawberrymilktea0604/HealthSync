using FluentValidation;
using HealthSync.Application.Commands;

namespace HealthSync.Application.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự");

        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Mã xác thực là bắt buộc")
            .Length(6).WithMessage("Mã xác thực phải có 6 ký tự");
    }
}