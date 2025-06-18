namespace SimpleStore.Admin.Models;

public class BucketView
{
    public string BucketId { get; set; }
    public string CreatedAt { get; set; }
    public string LastAccess { get; set; }
    public bool Private { get; set; }
    public string Name { get; set; }
    public int FileCount { get; set; }
    public string StorageSizeMB => String.Format("{0:0.00}", (double)Size / 1024 / 1024);
    public List<FileView> Files { get; set; } = [];
    public long Size => Files.Sum(x => x.FileSize);
    public bool AsDownload { get; set; }
}