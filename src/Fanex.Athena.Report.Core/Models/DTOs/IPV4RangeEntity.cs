namespace Fanex.Athena.Report.Core.Models.DTOs;

public record IPV4RangeEntity
{
    public long IPFrom { get; set; }

    public long IPTo { get; set; }
}