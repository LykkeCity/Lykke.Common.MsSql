using System;
using Lykke.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class MoneyPaddedConverter : ValueConverter<Money, string>
    {
        public static MoneyPaddedConverter Instance = new MoneyPaddedConverter();

        private MoneyPaddedConverter()
            : base(v => ToPaddedString(v), v => ToMoney(v))
        {
        }

        private static string ToPaddedString(Money money)
        {
            var padFromLeft = 20;
            var padFromRight = 18;
            var delimiter = ".";
            
            var s = money.ToString();

            var delimiterInString = s.Contains(".") ? "." : "";

            if (delimiterInString == "")
            {
                s += ".";
                s += new string('0', padFromRight);
            }
            else
            {
                var delimiterIndex = s.IndexOf(delimiterInString, StringComparison.InvariantCulture);

                var charsAfterDelimiter = s.Length - delimiterIndex;

                s += new string('0', padFromRight - charsAfterDelimiter + 1);
            }

            var delimiterIndexAfterRightPad = s.IndexOf(delimiter, StringComparison.InvariantCulture);
            
            s = new string('0', padFromLeft - delimiterIndexAfterRightPad) + s;

            return s;
        }

        private static Money ToMoney(string str)
        {
            return Money.Parse(str);
        }
    }
}