using System;
using Falcon.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class Money18PaddedConverter : ValueConverter<Money18, string>
    {
        public static Money18PaddedConverter Instance = new Money18PaddedConverter();

        private Money18PaddedConverter()
            : base(v => ToPaddedString(v), v => ToMoney18(v))
        {
        }

        private static string ToPaddedString(Money18 money)
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

        private static Money18 ToMoney18(string str)
        {
            return Money18.Parse(str);
        }
    }
}