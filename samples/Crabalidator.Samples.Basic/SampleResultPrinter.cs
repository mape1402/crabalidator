using Crabalidator;

namespace Crabalidator.Samples.Basic
{
    internal static class SampleResultPrinter
    {
        public static void Print(string label, ValidationResult result)
        {
            Console.WriteLine();
            Console.WriteLine(label);
            Console.WriteLine(result.IsValid ? "Valid" : "Invalid");

            foreach (var failure in result.Errors)
            {
                Console.WriteLine($"- {failure.PropertyPath}: {failure.ErrorMessage}");
            }
        }
    }
}
