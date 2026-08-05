namespace Crabalidator.Configuration
{
    internal interface IKnownRuleBuilder<T, TProperty>
    {
        string PropertyName { get; }

        Type PropertyType { get; }

        ICrabRuleBuilder<T, TProperty> AddKnownRule(RuleDescriptor rule);
    }
}
