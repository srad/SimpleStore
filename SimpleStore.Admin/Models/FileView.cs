namespace SimpleStore.Admin.Models;

public class FileView
{
    public string FileName { get; set; }
    public string FileSizeMB { get; set; }
    public string StorageFileId { get; set; }
    public long AccessCount { get; set; }
    public string LastAccess { get; set; }
    public string CreatedAt { get; set; }
    public bool Selected { get; set; }
    public long FileSize { get; set; }
    public bool Private { get; set; }
    public string AbsoluteUrl { get; set; }
    public bool AsDownload { get; set; }
}