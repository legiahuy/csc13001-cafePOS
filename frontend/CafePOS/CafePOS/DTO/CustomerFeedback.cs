using System;
using System.Collections.Generic;
using System.Linq;
using CafePOS.GraphQL;

namespace CafePOS.DTO
{
    public class CustomerFeedback
    {
        public CustomerFeedback() { }

        public CustomerFeedback(int id, string name, string email, string content,
            DateTime submittedAt, string status, string responseContent = null,
            DateTime? respondedAt = null, int? staffId = null)
        {
            this.Id = id;
            this.Name = name;
            this.Email = email;
            this.Content = content;
            this.SubmittedAt = submittedAt;
            this.Status = status;
            this.ResponseContent = responseContent;
            this.RespondedAt = respondedAt;
            this.StaffId = staffId;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Content { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; }
        public string ResponseContent { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? StaffId { get; set; }

        // Phương thức hỗ trợ để lấy trạng thái hiển thị
        public string StatusDisplay
        {
            get
            {
                switch (Status)
                {
                    case "pending":
                        return "Chưa xử lý";
                    case "in_progress":
                        return "Đang xử lý";
                    case "completed":
                        return "Đã xử lý";
                    default:
                        return Status;
                }
            }
        }
    }
}