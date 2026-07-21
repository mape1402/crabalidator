namespace Crabalidator.Samples.Basic
{
    public sealed class RegistrationRequestValidator : CrabValidator<RegistrationRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync(IsUsernameAvailableAsync)
                .WithMessage("Username is already taken.");

            RuleFor(x => x.Email)
                .NotNull()
                .MaximumLength(40);
        }

        private static async ValueTask<bool> IsUsernameAvailableAsync(
            string username,
            CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            return username != "taken";
        }
    }
}
