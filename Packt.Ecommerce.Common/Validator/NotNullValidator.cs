using System;
using System.Diagnostics.CodeAnalysis;

namespace Packt.Ecommerce.Common.Validator
{
    public class NotNullValidator
    {
        public static void ThrowIfNull([NotNull] object input, string name)
        {
            if (input == null)
            {
                throw new ArgumentException(name);
            }
        }

        public static void ThrowIfNullOrEmpty([NotNull] string input, string name)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
