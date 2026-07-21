using System.Reflection;

namespace Crabalidator.DependencyInjection
{
    /// <summary>
    /// Configures Crabalidator service registration.
    /// </summary>
    public sealed class CrabalidatorRegistrationBuilder
    {
        private readonly List<Type> _validatorTypes = new List<Type>();

        /// <summary>
        /// Gets the discovered validator types.
        /// </summary>
        public IReadOnlyList<Type> ValidatorTypes => _validatorTypes;

        /// <summary>
        /// Adds a validator type.
        /// </summary>
        /// <typeparam name="TValidator">The validator type.</typeparam>
        /// <returns>The same builder.</returns>
        public CrabalidatorRegistrationBuilder AddValidator<TValidator>()
            => AddValidator(typeof(TValidator));

        /// <summary>
        /// Adds a validator type.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        /// <returns>The same builder.</returns>
        public CrabalidatorRegistrationBuilder AddValidator(Type validatorType)
        {
            if (validatorType == null)
            {
                throw new ArgumentNullException(nameof(validatorType));
            }

            if (!TryGetModelType(validatorType, out _))
            {
                throw new ArgumentException($"Type '{validatorType.FullName}' must derive from CrabValidator<T>.", nameof(validatorType));
            }

            if (!_validatorTypes.Contains(validatorType))
            {
                _validatorTypes.Add(validatorType);
            }

            return this;
        }

        /// <summary>
        /// Discovers and adds validators from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The same builder.</returns>
        public CrabalidatorRegistrationBuilder AddValidators(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            foreach (var validatorType in DiscoverValidatorTypes(assembly))
            {
                AddValidator(validatorType);
            }

            return this;
        }

        internal static bool TryGetModelType(Type validatorType, out Type modelType)
        {
            modelType = null;
            var current = validatorType;

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(CrabValidator<>))
                {
                    modelType = current.GetGenericArguments()[0];
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static IEnumerable<Type> DiscoverValidatorTypes(Assembly assembly)
            => SafeGetExportedTypes(assembly)
                .Where(x => x != null && !x.IsAbstract && !x.IsInterface && !x.ContainsGenericParameters)
                .Where(x => TryGetModelType(x, out _))
                .OrderBy(x => x.FullName, StringComparer.Ordinal);

        private static IEnumerable<Type> SafeGetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.ExportedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null && (x.IsPublic || x.IsNestedPublic));
            }
        }
    }
}
