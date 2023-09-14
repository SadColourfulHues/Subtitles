using System;

namespace SadChromaLib.Utils.Parsers;

/// <summary>
/// A structure that defines the timestamp for a subtitle line.
/// </summary>
public struct SubtitleTimestamp: IFormattable
{
    public byte Hours;
    public byte Minutes;
    public byte Seconds;

    public SubtitleTimestamp(
        byte hours,
        byte minutes,
        byte seconds)
    {
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
    }

    /// <summary>
    /// Returns this timestamp as seconds.
    /// </summary>
    /// <returns></returns>
    public readonly int AsSecs()
    {
        return (Hours * 3600) + (Minutes * 60) + Seconds;
    }

    /// <summary>
    /// Returns true if this is greater than 'other'.
    /// </summary>
    /// <returns></returns>
    public readonly bool HasPassed(SubtitleTimestamp other)
    {
        return this.AsSecs() > other.AsSecs();
    }

    public readonly string ToString(string format, IFormatProvider formatProvider)
    {
        return $"{Hours:00}:{Minutes:00}:{Seconds:00}";
    }
}