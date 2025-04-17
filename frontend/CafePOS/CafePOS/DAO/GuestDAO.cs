using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.GraphQL.State;
using CafePOS.DTO;

namespace CafePOS.DAO
{
    public class GuestDAO
    {
        private static GuestDAO? instance;
        public static GuestDAO Instance
        {
            get { if (instance == null) instance = new GuestDAO(); return instance; }
            private set { instance = value; }
        }

        public GuestDAO() { }

        // ✅ Lấy toàn bộ khách hàng
        public async Task<List<Guest>> GetAllGuestsAsync()
        {
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllGuest.ExecuteAsync();
            var guests = result.Data?.AllGuests.Edges?
                .Where(e => e.Node != null)
                .Select(e => new Guest(e.Node!))
                .ToList() ?? new List<Guest>();
            return guests;
        }

        // ✅ Thêm khách hàng mới
        public async Task<int> AddGuestAsync(string name, string phone, string email, string? notes, int totalPoints, int availablePoints, string? membershipLevel, DateOnly memberSince)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.CreateGuest.ExecuteAsync(
                name,
                phone,
                email,
                totalPoints,
                availablePoints,
                notes,
                membershipLevel,
                memberSince
            );

            return result.Data?.CreateGuest?.Guest?.Id ?? -1;
        }

        // ✅ Cập nhật khách hàng
        public async Task<bool> UpdateGuestAsync(int id, string name, string phone, string email, string? notes, int totalPoints, int availablePoints, string? membershipLevel, DateOnly memberSince)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.UpdateGuest.ExecuteAsync(
                id,
                name,
                phone,
                email,
                totalPoints,
                availablePoints,
                notes,
                membershipLevel,
                memberSince
            );
            return result.Data?.UpdateGuestById?.Guest?.Id > 0;
        }

        // ✅ Xoá khách hàng
        public async Task<bool> DeleteGuestAsync(int id)
        {
            var client = DataProvider.Instance.Client;
            var result = await client.DeleteGuest.ExecuteAsync(id);
            return result.Data?.DeleteGuestById?.DeletedGuestId != null;
        }

        // ✅ Cộng điểm tích lũy + điểm khả dụng
        public async Task<bool> AddPointsAsync(int guestId, int pointsEarned)
        {
            var guests = await GetAllGuestsAsync();
            var guest = guests.FirstOrDefault(g => g.Id == guestId);
            if (guest == null) return false;

            int newTotalPoints = guest.TotalPoints + pointsEarned;
            int newAvailablePoints = guest.AvailablePoints + pointsEarned;
            string newMembershipLevel = Guest.GetMembershipLevel(newTotalPoints);

            return await UpdateGuestAsync(
                guest.Id,
                guest.Name,
                guest.Phone ?? "",
                guest.Email ?? "",
                guest.Notes,
                newTotalPoints,
                newAvailablePoints,
                newMembershipLevel,
                DateOnly.FromDateTime(guest.MemberSince)
            );
        }

        // ✅ Trừ điểm khả dụng khi dùng để thanh toán
        public async Task<bool> DeductAvailablePointsAsync(int guestId, int pointsToDeduct)
        {
            var guests = await GetAllGuestsAsync();
            var guest = guests.FirstOrDefault(g => g.Id == guestId);
            if (guest == null || guest.AvailablePoints < pointsToDeduct) return false;

            return await UpdateGuestAsync(
                guest.Id,
                guest.Name,
                guest.Phone ?? "",
                guest.Email ?? "",
                guest.Notes,
                guest.TotalPoints, // giữ nguyên tổng điểm
                guest.AvailablePoints - pointsToDeduct, // trừ điểm khả dụng
                guest.MembershipLevel,
                DateOnly.FromDateTime(guest.MemberSince)
            );
        }
    }
}
