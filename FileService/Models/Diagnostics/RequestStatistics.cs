namespace FileService.Models.Diagnostics;

public class RequestStatistics
{
    public string Endpoint { get; set; } = null!;
    public long Count { get; set; }
    public DateTime LastRequestAt { get; set; }
    public int TotalTime { get; set; }
    public int LastTime { get; set; }
    public int AvgTime => Convert.ToInt32(TotalTime / Count);
}
