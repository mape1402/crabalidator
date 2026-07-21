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
                var code = string.IsNullOrWhiteSpace(failure.ErrorCode) ? string.Empty : $" [{failure.ErrorCode}]";
                Console.WriteLine($"- {failure.PropertyPath}{code}: {failure.ErrorMessage}");
            }
        }
    }
}
