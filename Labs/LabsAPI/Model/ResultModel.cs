namespace LabsAPI.Model;

public class ResultModel
{
    public string? Error { get; set; }
    public string? ProcessedString { get; set; }
    public Dictionary<char, int>? CharacterCounts { get; set; }
    public string? MaxVowelSubstring { get; set; }
    public string? SortedString { get; set; }
    public int RemovedCharIndex { get; set; }
    public char RemovedChar { get; set; }
    public string? TrimmedString { get; set; }
}
