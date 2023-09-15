using System;

namespace SadChromaLib.Utils.Parsers;

/// <summary>
/// A struct that represents a subtitle line.
/// </summary>
public readonly struct SubtitleData: IFormattable
{
    public readonly int Index;
    public readonly string Text;
    public readonly SubtitleTimestamp Start;
    public readonly SubtitleTimestamp End;

    public SubtitleData(
        int index,
        string text,
        SubtitleTimestamp start,
        SubtitleTimestamp end)
    {
        Index = index;
        Text = text;
        Start = start;
        End = end;
    }

    public SubtitleData(
        int index,
        string text,
        byte startHour,
        byte startMinutes,
        byte startSeconds,
        byte endHour,
        byte endMinutes,
        byte endSeconds)
    {
        Index = index;
        Text = text;
        Start = new(startHour, startMinutes, startSeconds);
        End = new(endHour, endMinutes, endSeconds);
    }

    public readonly bool HasStarted(SubtitleTimestamp current, float syncDelay = 0.0f)
    {
        return current.HasPassed(Start, syncDelay);
    }

    public readonly bool HasEnded(SubtitleTimestamp current, float syncDelay = 0.0f)
    {
        return current.HasPassed(End, syncDelay);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return $"#{Index} {Start} to {End}:\n{Text}";
    }
}