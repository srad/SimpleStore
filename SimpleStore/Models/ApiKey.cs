using System.ComponentModel.DataAnnotations;

namespace SimpleStore.Models;

public class ApiKey
{
    [Key, StringLength(36)] public string Key { get; set; }
    [Required] public string Title { get; set; }
    [Required] public DateTimeOffset CreatedAt { get; set; }
    [Required] public bool AccessTimeLimited { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
}