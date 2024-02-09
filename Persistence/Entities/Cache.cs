using System.ComponentModel.DataAnnotations;

namespace Persistence.Entities;

public class Cache
{
    [Required] [StringLength(500)] public string CacheKey { get; set; }

    [Required] public string CacheValue { get; set; }

    [Required] public DateTime CacheExpirationUtc { get; set; }
}