namespace SimpleStore.Models.DTO;

public record BucketViewDto
{
    public string BucketId { get; set; }
    public string Name { get; set; }
    public string DirectoryName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastAccess { get; set; }
    public int FileCount { get; set; }
    public bool Private { get; set; }
    public IReadOnlyList<FileViewDto>? Files { get; set; } = [];
    public bool AsDownload { get; set; }
}