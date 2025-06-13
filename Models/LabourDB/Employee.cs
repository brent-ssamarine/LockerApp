using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LockerApp.Models.LabourDB;

[Table("employees")]
public class Employee
{
    [Key]
    [Column("recnum")]
    public int Id { get; set; }

    [Column("id")]
    [StringLength(5)]
    public string? EmployeeId { get; set; }

    [Column("ai_status")]
    [StringLength(1)]
    public string? AiStatus { get; set; }

    [Column("lastname")]
    [StringLength(16)]
    public string? LastName { get; set; }

    [Column("firstname")]
    [StringLength(12)]
    public string? FirstName { get; set; }

    [Column("initial")]
    [StringLength(1)]
    public string? Initial { get; set; }

    [Column("status")]
    [StringLength(1)]
    public string? Status { get; set; }

    [Column("homeport")]
    [StringLength(3)]
    public string? HomePort { get; set; }

    [Column("homeport_locked_by_user")]
    public bool? HomePortLockedByUser { get; set; }

    [Column("wes_regular")]
    public bool? WesRegular { get; set; }

    [Column("class")]
    [StringLength(6)]
    public string? Class { get; set; }

    [Column("add_user")]
    [StringLength(16)]
    public string? AddUser { get; set; }

    [Column("add_date")]
    public DateTime? AddDate { get; set; }

    [Column("mod_user")]
    [StringLength(16)]
    public string? ModUser { get; set; }

    [Column("mod_date")]
    public DateTime? ModDate { get; set; }
}
