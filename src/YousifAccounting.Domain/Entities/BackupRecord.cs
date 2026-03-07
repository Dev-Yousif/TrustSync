using YousifAccounting.Domain.Common;
using YousifAccounting.Domain.Enums;

namespace YousifAccounting.Domain.Entities;

public sealed class BackupRecord : BaseEntity
{
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public BackupType Type { get; set; }
    public string? Checksum { get; set; }
    public bool IsEncrypted { get; set; }
    public string? Notes { get; set; }
}
