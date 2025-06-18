using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimpleStore.Models;

public class BucketFile
{
    [Key, StringLength(36), Column(TypeName = "TEXT COLLATE NOCASE")]
    public string StorageFileId { get; set; }

    [Required, StringLength(1024), Column(TypeName = "TEXT COLLATE NOCASE")]
    public string FileName { get; set; }

    [Required, StringLength(1024), Column(TypeName = "TEXT COLLATE NOCASE")]
    public string StoredFileName { get; set; }

    [Required, StringLength(2048), Column(TypeName = "TEXT COLLATE NOCASE")]
    public string FilePath { get; set; }

    [Required, StringLength(2048), Column(TypeName = "TEXT COLLATE NOCASE")]
    public string Url { get; set; }

    [Required] public long FileSize { get; set; }
    [Required] public string FileSizeMB { get; set; } // Saves energy to compute once.
    [Required] public DateTimeOffset CreatedAt { get; set; }
    [Required] public long AccessCount { get; set; }
    [Required] public bool Private { get; set; }
    [Required] public DateTimeOffset LastAccess { get; set; }
    [Required] public bool AsDownload { get; set; }

    [JsonIgnore] public Bucket Bucket { get; set; }
    [JsonIgnore] [Required] public string BucketId { get; set; }
}