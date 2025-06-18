namespace SimpleStore.Models.DTO;

public record UploadFilesDto
{
    public IEnumerable<IFormFile> Files { get; set; }
    public bool UseGuidName { get; set; }
    public bool Private { get; set; }
}