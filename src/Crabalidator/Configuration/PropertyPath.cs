namespace Crabalidator.Configuration
{
    internal readonly struct PropertyPath
    {
        public PropertyPath(string propertyName, string propertyPath)
        {
            PropertyName = propertyName;
            PropertyPathValue = propertyPath;
        }

        public string PropertyName { get; }

        public string PropertyPathValue { get; }
    }
}
