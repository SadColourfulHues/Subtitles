using System.Diagnostics;
using System.Text;

namespace SadChromaLib.Utils.Parsers;

/// <summary>
/// A utility object for parsing .srt subtitle files.
/// </summary>
public sealed class SRTParser
{
    const int BufferSize = 512;

    private readonly StringBuilder _subtitleTextBuilder;
    private State _state = State.Index;

    private int _tmpIndex;
    private SubtitleTimestamp _tmpStartTime;
    private SubtitleTimestamp _tmpEndTime;

    public SRTParser()
    {
        _subtitleTextBuilder = new();
        Reset();
    }

    #region Main Functions

    public SubtitleData[] ParseFile(string filename)
    {
        Debug.Assert(
            condition: File.Exists(filename),
            message: $"\"{filename}\" does not exist!"
        );

        ReadOnlySpan<char> contents = File.ReadAllText(filename);
        return Parse(contents);
    }

    public SubtitleData[] Parse(ReadOnlySpan<char> text)
    {
        SubtitleData[] subtitles = new SubtitleData[BufferSize];
        int subtitleIdx = 0;

        int lastLineStart = 0;

        for (int i = 0; i < text.Length; ++i) {
            if (text[i] != '\n')
                continue;

            int sliceLen = i - lastLineStart;

            if (sliceLen < 1)
                continue;

            ParseLine(
                line: text.Slice(lastLineStart, sliceLen),
                subtitles: ref subtitles,
                subtitleIdx: ref subtitleIdx
            );

            lastLineStart = i;
        }

        // Complete the final unterminated subtitle line
        if (_state == State.Subtitle &&
            _subtitleTextBuilder.Length > 0)
        {
            PushSubtitle(ref subtitles, ref subtitleIdx);
        }

        return subtitles[..subtitleIdx];
    }

    #endregion

    #region State Machine

    private void ParseLine(
        ReadOnlySpan<char> line,
        ref SubtitleData[] subtitles,
        ref int subtitleIdx)
    {
        switch (_state) {
            case State.Index:
                ParseIndex(line);
                break;

            case State.Timestamps:
                ParseTimestamps(line);
                break;

            case State.Subtitle:
                ParseSubtitle(line, ref subtitles, ref subtitleIdx);
                break;
        }
    }

    private void ParseIndex(ReadOnlySpan<char> line)
    {
        if (!int.TryParse(line, out int index)) {
            return;
        }

        _tmpIndex = index;
        _state = State.Timestamps;
    }

    private void ParseTimestamps(ReadOnlySpan<char> line)
    {
        int offset = 0;

        _tmpStartTime = ParseTimestamp(line, ref offset);
        _tmpEndTime = ParseTimestamp(line, ref offset);

        _state = State.Subtitle;
    }

    private SubtitleTimestamp ParseTimestamp(
        ReadOnlySpan<char> line,
        ref int offsetIdx)
    {
        // Skip unwanted characters
        for (;;) {
            if (!char.IsNumber(line[offsetIdx])) {
                offsetIdx ++;
                continue;
            }

            break;
        }

        Span<byte> values = stackalloc byte[3];
        int startOffset = offsetIdx;
        int valueIdx = 0;

        // Extract timestamp
        for (int i = offsetIdx; i < line.Length; ++i) {
            if (valueIdx >= 3)
                break;

            if (line[i] != ':' && line[i] != ',')
                continue;

            int sliceLen = i - startOffset;
            ReadOnlySpan<char> valueSlice = line.Slice(startOffset, sliceLen);

            if (!byte.TryParse(valueSlice, out byte value)) {
                Console.WriteLine($"Parser error: invalid timestamp \"{line}\"");
                throw new System.InvalidOperationException();
            }

            values[valueIdx] = value;
            valueIdx ++;

            offsetIdx = startOffset + sliceLen + 1;
            startOffset = i + 1;
        }

        // Skip the milliseconds section
        while (offsetIdx < line.Length) {
            if (char.IsNumber(line[offsetIdx])) {
                offsetIdx ++;
                continue;
            }

            break;
        }

        return new(
            hours: values[0],
            minutes: values[1],
            seconds: values[2]
        );
    }

    private void ParseSubtitle(
        ReadOnlySpan<char> line,
        ref SubtitleData[] subtitles,
        ref int subtitleIdx)
    {
        // Subtitle line termination
        if (line.Length <= 1) {
            PushSubtitle(ref subtitles, ref subtitleIdx);
            return;
        }

        if (IsEmptyLine(line))
            return;

        _subtitleTextBuilder.Append(line);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Initialises and pushes a subtitle line into the subtitle array.
    /// </summary>
    private void PushSubtitle(
        ref SubtitleData[] subtitles,
        ref int idx)
    {
        _subtitleTextBuilder.Remove(0, 1);

        subtitles[idx] = new(
            index: _tmpIndex,
            text: _subtitleTextBuilder.ToString(),
            start: _tmpStartTime,
            end: _tmpEndTime
        );

        idx++;

        _state = State.Index;
        _subtitleTextBuilder.Clear();
    }

    private static bool IsEmptyLine(ReadOnlySpan<char> line)
    {
        for (int i = 0; i < line.Length; ++i) {
            if (char.IsWhiteSpace(line[i]))
                continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// Resets the parser state
    /// </summary>
    public void Reset()
    {
        _state = State.Index;
        _subtitleTextBuilder.Clear();

        _tmpIndex = 0;
        _tmpStartTime = new();
        _tmpEndTime = new();
    }

    #endregion

    private enum State
    {
        Index,
        Timestamps,
        Subtitle
    }
}