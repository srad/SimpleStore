namespace SimpleStore.Models.DTO;

public record FileViewDto
{
    public string FileName { get; set; }
    public string RelativeUrl { get; set; }
    public string AbsoluteUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string FileSizeMB { get; set; }
    public DateTimeOffset LastAccess { get; set; }
    public bool Private { get; set; }
    public string StorageFileId { get; set; }
    public long FileSize { get; set; }
    public long AccessCount { get; set; }
    public bool AsDownload { get; set; }
}