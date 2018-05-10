using System;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using System.Globalization;

namespace HueShift
{
    public class TimeSpanParser : IValueParser<TimeSpan>
    {
        public Type TargetType { get; } = typeof(TimeSpan);

        Type IValueParser.TargetType => this.TargetType;

        public TimeSpan Parse(string argName, string value, CultureInfo culture)
        {
            if (!TimeSpan.TryParse(value, out var result))
            {
                throw new FormatException($"Invalid value specified for {argName}. '{value}' is not a valid timespan");
            }

            return result;
        }

        object IValueParser.Parse(string argName, string value, CultureInfo culture)
            => this.Parse(argName, value, culture);
    }
}
