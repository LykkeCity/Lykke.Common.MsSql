using Falcon.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class Money18Converter : ValueConverter<Money18, string>
    {
        public static readonly Money18Converter Instance = new Money18Converter();

        private Money18Converter()
            : base(v => ToString(v), v => ToMoney18(v))
        {
        }

        private static string ToString(Money18 money)
        {
            return money.ToString().TrimEnd('0');
        }

        private static Money18 ToMoney18(string str)
        {
            return Money18.Parse(str);
        }
    }
}