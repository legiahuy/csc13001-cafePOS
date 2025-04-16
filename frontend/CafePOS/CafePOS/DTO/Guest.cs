using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int Points { get; set; } = 0;
        public DateTime MemberSince { get; set; } = DateTime.Now;
        public string MembershipLevel { get; set; } = "Regular";
        public string? Notes { get; set; }

        public Guest() { }

        public Guest(IGetAllGuest_AllGuests_Edges_Node node)
        {
            this.Phone = node.Phone;
            this.Email = node.Email;
            this.Name = node.Name;
            this.Id = node.Id;
            this.Points = node.Points;
            this.MemberSince = node.MemberSince.ToDateTime(TimeOnly.MinValue);
            this.MembershipLevel = node.MembershipLevel!;
        }
    }
}

