using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SimpleStore.Models;

[Index(nameof(Name), IsUnique = true), Index(nameof(DirectoryName), IsUnique = true)]
public class Bucket
{
    [Key, StringLength(36), Column(TypeName = "TEXT COLLATE NOCASE")] public string BucketId { get; set; }
    [Required, Column(TypeName = "TEXT COLLATE NOCASE")] public string Name { get; set; }
    [Required, Column(TypeName = "TEXT COLLATE NOCASE")] public string DirectoryName { get; set; }
    [Required] public DateTimeOffset CreatedAt { get; set; }
    [Required] public DateTimeOffset LastAccess { get; set; }
    [NotMapped] public int Size { get; set; }
    [Required] public bool Private { get; set; }
    [Required] public bool AsDownload { get; set; }

    public ICollection<BucketFile> Files { get; set; }
}