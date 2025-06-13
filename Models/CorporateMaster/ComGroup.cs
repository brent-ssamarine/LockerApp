using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LockerApp.Models.CorporateMaster;

[Table("comgroup")]
public class ComGroup
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;

    [Column("name")]
    public string? Name { get; set; }

    [Column("billingsuffix")]
    public char? BillingSuffix { get; set; }

    [Column("add_user")]
    public string? AddUser { get; set; }

    [Column("add_date")]
    public DateTime? AddDate { get; set; }

    [Column("mod_user")]
    public string? ModUser { get; set; }

    [Column("mod_date")]
    public DateTime? ModDate { get; set; }
}
