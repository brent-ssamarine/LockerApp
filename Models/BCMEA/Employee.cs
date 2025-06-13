using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LockerApp.Models.BCMEA;

[Table("employees")]
public class Employee
{
    [Key]
    [Column("recnum")]
    public int Id { get; set; }

    [Column("id")]
    public string? EmployeeNumber { get; set; }

    [Column("ai_status")]
    public string? AiStatus { get; set; }

    [Column("lastname")]
    public string? LastName { get; set; }

    [Column("firstname")]
    public string? FirstName { get; set; }

    [Column("initial")]
    public string? Initial { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("homeport")]
    public string? HomePort { get; set; }

    [Column("homeport_locked_by_user")]
    public bool? HomePortLockedByUser { get; set; }

    [Column("wes_regular")]
    public bool? WesRegular { get; set; }

    [Column("class")]
    public string? Class { get; set; }

    [Column("add_user")]
    public string? AddUser { get; set; }

    [Column("add_date")]
    public DateTime? AddDate { get; set; }

    [Column("mod_user")]
    public string? ModUser { get; set; }

    [Column("mod_date")]
    public DateTime? ModDate { get; set; }
}
