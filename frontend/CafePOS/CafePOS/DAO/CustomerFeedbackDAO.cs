using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.DTO;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CafePOS.DAO
{
    public class CustomerFeedbackDAO
    {
        private static CustomerFeedbackDAO? instance;
        public static CustomerFeedbackDAO Instance
        {
            get { if (instance == null) instance = new CustomerFeedbackDAO(); return instance; }
            private set { instance = value; }
        }
        private CustomerFeedbackDAO() { }

        // Lấy danh sách tất cả phản hồi
        public async Task<List<CustomerFeedback>> GetAllFeedbacksAsync(string? status)
        {
            var client = DataProvider.Instance.Client;

            // Nếu status là chuỗi rỗng hoặc null, không truyền parameter vào query
            var result = string.IsNullOrEmpty(status)
                ? await client.GetAllFeedbacks.ExecuteAsync(null)
                : await client.GetAllFeedbacks.ExecuteAsync(status);

            Debug.WriteLine($"Raw nodes count: {result.Data?.AllCustomerFeedbacks?.Nodes?.Count()}");
            if (result.Data?.AllCustomerFeedbacks?.Nodes != null)
            {
                var feedbacks = result.Data.AllCustomerFeedbacks.Nodes
                .Select(f => new CustomerFeedback
                {
                    Id = f.Id,
                    Name = f.Name,
                    Email = f.Email,
                    Content = f.Content,
                    SubmittedAt = DateTime.Parse(f.SubmittedAt!),
                    Status = f.Status!,
                    ResponseContent = f.ResponseContent!,
                    RespondedAt = f.RespondedAt != null ? DateTime.Parse(f.RespondedAt) : null,
                    StaffId = f.StaffId
                })
                .ToList();
                return feedbacks;
            }
            return new List<CustomerFeedback>();
        }

        public async Task<List<CustomerFeedback>> GetAllFeedbacksWithoutFilterAsync()
        {
            var client = DataProvider.Instance.Client;

            var result = await client.GetAllFeedbacksWithoutFilter.ExecuteAsync();

            Debug.WriteLine($"Raw nodes count: {result.Data?.AllCustomerFeedbacks?.Nodes?.Count()}");
            if (result.Data?.AllCustomerFeedbacks?.Nodes != null)
            {
                var feedbacks = result.Data.AllCustomerFeedbacks.Nodes
                .Select(f => new CustomerFeedback
                {
                    Id = f.Id,
                    Name = f.Name,
                    Email = f.Email,
                    Content = f.Content,
                    SubmittedAt = DateTime.Parse(f.SubmittedAt!),
                    Status = f.Status!,
                    ResponseContent = f.ResponseContent!,
                    RespondedAt = f.RespondedAt != null ? DateTime.Parse(f.RespondedAt) : null,
                    StaffId = f.StaffId
                })
                .ToList();
                return feedbacks;
            }
            return new List<CustomerFeedback>();
        }

        // Lấy chi tiết một phản hồi theo ID
        public async Task<CustomerFeedback> GetFeedbackByIdAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetFeedbackById.ExecuteAsync(id);

            if (result.Data?.CustomerFeedbackById != null)
            {
                var f = result.Data.CustomerFeedbackById;
                return new CustomerFeedback
                {
                    Id = f.Id,
                    Name = f.Name,
                    Email = f.Email,
                    Content = f.Content,
                    SubmittedAt = DateTime.Parse(f.SubmittedAt!),
                    Status = f.Status!,
                    ResponseContent = f.ResponseContent!,
                    RespondedAt = f.RespondedAt != null ? DateTime.Parse(f.RespondedAt) : null, 
                    StaffId = f.StaffId
                };
            }

            return null;
        }

        // Cập nhật trạng thái phản hồi
        public async Task<bool> UpdateFeedbackStatusAsync(int id, string status)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdateFeedbackStatus.ExecuteAsync(id, status);

            return result.Data?.UpdateCustomerFeedbackById?.CustomerFeedback != null;
        }

        // Gửi phản hồi cho khách hàng
        public async Task<bool> RespondToFeedbackAsync(int id, string responseContent, int staffId)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.RespondToFeedback.ExecuteAsync(id, responseContent, staffId);

            return result.Data?.UpdateCustomerFeedbackById?.CustomerFeedback != null;
        }
    }
}