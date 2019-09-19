using Lykke.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class MoneyConverter : ValueConverter<Money, string>
    {
        public static MoneyConverter Instance = new MoneyConverter();

        private MoneyConverter()
            : base(v => ToString(v), v => ToMoney(v))
        {
        }

        private static string ToString(Money money)
        {
            return money.ToString().TrimEnd('0');
        }

        private static Money ToMoney(string str)
        {
            return Money.Parse(str);
        }
    }
}