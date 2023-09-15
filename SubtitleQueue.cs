using System;

namespace SadChromaLib.Utils.Parsers;

/// <summary>
/// A queue of subtitle lines.
/// </summary>
public sealed class SubtitleQueue
{
    private readonly SubtitleData[] _subtitles;
    private int _subtitleIdx;

    #if GODOT4
    /// <summary>
    /// Creates a queue from a specified .srt file
    /// </summary>
    public SubtitleQueue(string path, bool isGodotPath = true)
    {
        SRTParser parser = new();
        _subtitleIdx = 0;

        if (isGodotPath) {
            _subtitles = parser.ParseGodotFile(path);
            return;
        }

        _subtitles = parser.ParseFile(path);
    }
    #else
    /// <summary>
    /// Creates a queue from a specified .srt file
    /// </summary>
    public SubtitleQueue(string path)
    {
        SRTParser parser = new();
        _subtitleIdx = 0;

        _subtitles = parser.ParseFile(path);
    }
    #endif

    /// <summary>
    /// Creates a queue from the contents of an .srt file
    /// </summary>
    public SubtitleQueue(ReadOnlySpan<char> srtContents)
    {
        _subtitleIdx = 0;

        SRTParser parser = new();
        _subtitles = parser.Parse(srtContents);
    }

    /// <summary>
    /// Returns whether or not the queue has more subtitle lines in it.
    /// </summary>
    /// <returns></returns>
    public bool CanDequeue()
    {
        return _subtitleIdx <= _subtitles.Length;
    }

    /// <summary>
    /// Returns the current active subtitle line
    /// </summary>
    /// <returns></returns>
    public SubtitleData? GetActive()
    {
        if (_subtitleIdx >= _subtitles.Length)
            return null;

        return _subtitles[_subtitleIdx];
    }

    /// <summary>
    /// Dequeues a subtitle line.
    /// </summary>
    /// <returns></returns>
    public SubtitleData? Dequeue()
    {
        if (_subtitleIdx >= _subtitles.Length)
            return null;

        SubtitleData current = _subtitles[_subtitleIdx];
        _subtitleIdx ++;

        return current;
    }

    public void Reset()
    {
        _subtitleIdx = 0;
    }
}
