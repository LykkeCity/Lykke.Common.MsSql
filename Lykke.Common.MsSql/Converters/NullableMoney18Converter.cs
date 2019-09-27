using Falcon.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class NullableMoney18Converter : ValueConverter<Money18?, string>
    {
        public static readonly NullableMoney18Converter Instance = new NullableMoney18Converter();

        private NullableMoney18Converter()
            : base(v => ToString(v), v => ToNullableMoney18(v))
        {
        }

        private static string ToString(Money18? money)
        {
            return money?.ToString();
        }

        private static Money18? ToNullableMoney18(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? default(Money18?) : Money18.Parse(str);
        }
    }
}