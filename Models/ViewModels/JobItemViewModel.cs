using AccessMigrationApp.Models.LockerDB;

namespace AccessMigrationApp.Models.ViewModels
{    public class JobItemViewModel
    {
        public int Id { get; set; }
        public string Company { get; set; } = "";
        public string JobClass { get; set; } = "";
        public string InvItem { get; set; } = "";
        public string? InvName { get; set; }
        public double? DefaultQuantity { get; set; }
        public double? IssueQuantity { get; set; }
        public bool? Billable { get; set; }        public static JobItemViewModel FromModel(JobItem model)
        {
            return new JobItemViewModel
            {
                Id = model.Id,
                Company = model.Company ?? "",
                JobClass = model.JobClass ?? "",
                InvItem = model.InvItem ?? "",
                InvName = model.InvName,
                DefaultQuantity = model.DefaultQuantity,
                IssueQuantity = model.IssueQuantity,
                Billable = model.Billable
            };
        }

        public void UpdateModel(JobItem model)
        {
            model.Company = Company;
            model.JobClass = JobClass;
            model.InvItem = InvItem;
            model.InvName = InvName;
            model.DefaultQuantity = DefaultQuantity;
            model.IssueQuantity = IssueQuantity;
            model.Billable = Billable;
        }
    }
}
