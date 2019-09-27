using System;
using Falcon.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lykke.Common.MsSql.Converters
{
    public class NullableMoney18PaddedConverter : ValueConverter<Money18?, string>
    {
        public static readonly NullableMoney18PaddedConverter Instance = new NullableMoney18PaddedConverter();

        private NullableMoney18PaddedConverter()
            : base(v => ToPaddedString(v), v => ToNullableMoney18(v))
        {
        }

        private static string ToPaddedString(Money18? money)
        {
            if (!money.HasValue)
                return null;

            var padFromLeft = 20;
            var padFromRight = 18;
            var delimiter = ".";

            var s = money.ToString();

            var delimiterIndex = s.IndexOf(".", StringComparison.InvariantCulture);

            if (delimiterIndex == -1)
            {
                s += ".";
                s += new string('0', padFromRight);
            }
            else
            {
                var charsAfterDelimiter = s.Length - delimiterIndex;

                s += new string('0', padFromRight - charsAfterDelimiter + 1);
            }

            var delimiterIndexAfterRightPad = s.IndexOf(delimiter, StringComparison.InvariantCulture);

            s = new string('0', padFromLeft - delimiterIndexAfterRightPad) + s;

            return s;
        }

        private static Money18? ToNullableMoney18(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? default(Money18?) : Money18.Parse(str);
        }
    }
}