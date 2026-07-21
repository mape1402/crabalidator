using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Runtime
{
    /// <summary>
    /// Default implementation of <see cref="ICrabalidator"/>.
    /// </summary>
    internal sealed class Crabalidator : ICrabalidator
    {
        private readonly IServiceProvider _services;

        public Crabalidator(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public ValidationResult Validate<T>(T instance)
            => _services.GetRequiredService<IValidator<T>>().Validate(instance);

        public ValueTask<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default)
            => _services.GetRequiredService<IAsyncValidator<T>>().ValidateAsync(instance, cancellationToken);
    }
}
