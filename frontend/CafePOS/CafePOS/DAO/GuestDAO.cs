using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL.State;
using CafePOS.DTO;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;

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

        public async Task<int> AddGuestAsync(string name, string phone, string email, string? notes, int points, string? memberLevel, DateOnly date)
        {
            var client = DataProvider.Instance.Client;

            var result = await client.CreateGuest.ExecuteAsync(
                name,
                phone,
                email,
                points,
                notes,
                memberLevel,
                date
            );

            return result.Data?.CreateGuest?.Guest?.Id ?? -1;
        }


    }
}
