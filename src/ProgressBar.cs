class ProgressBar {
    private string PrefixText { get; }
    private int Total { get; }
    private int LogDelay { get; }
    private DateTime LastLogDateTime { get; set; }

    public ProgressBar(string prefixText, int total, int logDelayMilliSec) {
        PrefixText = prefixText;
        Total = total;
        LogDelay = logDelayMilliSec;
        LastLogDateTime = new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();
    }

    public void WriteProgress(int current, bool forceWrite = false) {
        var curDateTime = DateTime.UtcNow;
        var diffMs = curDateTime.Subtract(LastLogDateTime).TotalMilliseconds;
        if (!forceWrite && diffMs <= LogDelay) {
            return;
        }
        var percentage = (int)((double)current / Total * 100);
        ClearConoleLastLine();
        Console.Write($"{PrefixText} [{current}/{Total}] {percentage}%");
        LastLogDateTime = DateTime.UtcNow;
    }

    public static void ClearConoleLastLine() {
        Console.SetCursorPosition(0, Console.CursorTop);
    }
}
