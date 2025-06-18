namespace SimpleStore.Models.DTO;

public record CreateFileDto
{
    public BucketFile? BucketFile { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}