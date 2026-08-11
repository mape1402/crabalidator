using Microsoft.Extensions.DependencyInjection;

namespace Crabalidator.Testing
{
    /// <summary>
    /// Lightweight host for validator tests.
    /// </summary>
    public sealed class CrabalidatorTestHost : IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider _provider;
        private readonly IDisposable _disposable;
        private readonly IAsyncDisposable _asyncDisposable;

        internal CrabalidatorTestHost(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _disposable = provider as IDisposable;
            _asyncDisposable = provider as IAsyncDisposable;
        }

        /// <summary>
        /// Gets the service provider used by the test host.
        /// </summary>
        public IServiceProvider Services => _provider;

        /// <summary>
        /// Creates a test host builder.
        /// </summary>
        /// <returns>A test host builder.</returns>
        public static CrabalidatorTestHostBuilder Create()
            => new CrabalidatorTestHostBuilder();

        /// <summary>
        /// Resolves and runs a validator for an instance.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <returns>The typed validation test result.</returns>
        public ValidationTestResult<T> Validate<T>(T instance)
        {
            var validator = _provider.GetRequiredService<IValidator<T>>();
            return validator.TestValidate(instance);
        }

        /// <summary>
        /// Resolves and runs an async validator for an instance.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="instance">The instance to validate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The typed validation test result.</returns>
        public ValueTask<ValidationTestResult<T>> ValidateAsync<T>(
            T instance,
            CancellationToken cancellationToken = default)
        {
            var validator = _provider.GetRequiredService<IAsyncValidator<T>>();
            return validator.TestValidateAsync(instance, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
            => _disposable?.Dispose();

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (_asyncDisposable != null)
            {
                return _asyncDisposable.DisposeAsync();
            }

            Dispose();
            return default;
        }
    }
}
