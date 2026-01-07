namespace Da3wa.WebUI.Models
{
    public class SystemLogsViewModel
    {
        public List<LogFileInfo> LogFiles { get; set; } = new List<LogFileInfo>();
    }

    public class LogFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LogFileContentViewModel
    {
        public string FileName { get; set; } = string.Empty;
        public List<string> Lines { get; set; } = new List<string>();
        public int TotalLines { get; set; }
        public string FileSize { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}
