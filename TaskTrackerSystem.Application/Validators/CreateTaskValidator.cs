using FluentValidation;
using TaskTrackerSystem.Application.DTOs;

namespace TaskTrackerSystem.Application.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Görev başlığı zorunludur.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.DueDate)
             .GreaterThanOrEqualTo(DateTime.Now)
             .WithMessage("Son tarih ve saat geçmişte olamaz.");

        RuleFor(x => x.Priority)
            .IsInEnum();
    }
}