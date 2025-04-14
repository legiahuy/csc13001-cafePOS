using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Staff
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
        public float Salary { get; set; }
        public string? UserName { get; set; }

        public Staff() { }
        public string SalaryFormatted => string.Format("{0:N0} VNĐ", Salary);

        public Staff(IGetAllStaff_AllStaff_Nodes node)
        {
            Id = node.Id;
            Name = node.Name;
            Dob = node.Dob?.ToString("yyyy-MM-dd");
            Gender = node.Gender;
            Phone = node.Phone;
            Email = node.Email;
            Position = node.Position;
            Salary = (float)(node.Salary ?? 0);
            UserName = node.UserName;
        }
    }
}
