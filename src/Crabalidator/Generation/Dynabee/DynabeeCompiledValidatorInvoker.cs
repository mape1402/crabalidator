using DynaBee.FluentApi.Invocation;

namespace Crabalidator.Generation.Dynabee
{
    /// <summary>
    /// Adapts DynaBee object invocation to Crabalidator's invoker contract.
    /// </summary>
    internal sealed class DynabeeCompiledValidatorInvoker : ICompiledValidatorInvoker
    {
        private readonly Func<IReadOnlyList<object>, object> _invoke;

        public DynabeeCompiledValidatorInvoker(IDynaBeeObjectMethodAdapter adapter)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            _invoke = adapter.Invoke;
        }

        public ValidationResult Invoke(object instance)
            => (ValidationResult)_invoke(new[] { instance });
    }
}
