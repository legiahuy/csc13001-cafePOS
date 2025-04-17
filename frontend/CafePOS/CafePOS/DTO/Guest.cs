using System;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int TotalPoints { get; set; } = 0;       
        public int AvailablePoints { get; set; } = 0;   
        public DateTime MemberSince { get; set; } = DateTime.Now;
        public string MembershipLevel { get; set; } = "Regular";
        public string? Notes { get; set; }

        public Guest() { }

        public Guest(IGetAllGuest_AllGuests_Edges_Node node)
        {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Phone = node.Phone;
            this.Email = node.Email;
            this.TotalPoints = node.TotalPoints;
            this.AvailablePoints = node.AvailablePoints;
            this.MemberSince = node.MemberSince.ToDateTime(TimeOnly.MinValue);
            this.MembershipLevel = node.MembershipLevel ?? GetMembershipLevel(this.TotalPoints); // Auto map lại
            this.Notes = node.Notes;
        }

        public static string GetMembershipLevel(int totalPoints)
        {
            if (totalPoints < 100)
                return "Regular";
            else if (totalPoints < 250)
                return "Silver";
            else if (totalPoints < 500)
                return "Gold";
            else
                return "Platinum";
        }

        public string GetMembershipLevel()
        {
            return GetMembershipLevel(this.TotalPoints);
        }
    }
}
