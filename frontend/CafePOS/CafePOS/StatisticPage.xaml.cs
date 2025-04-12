using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using SkiaSharp;

namespace CafePOS.Views
{
    public sealed partial class StatisticPage : Page
    {
        private ISeries[] _series;
        private ICartesianAxis[] _xAxes;
        private ICartesianAxis[] _yAxes;

        public ISeries[] Series
        {
            get => _series;
            set
            {
                _series = value;
                this.Bindings?.Update();
            }
        }

        public ICartesianAxis[] XAxes
        {
            get => _xAxes;
            set
            {
                _xAxes = value;
                this.Bindings?.Update();
            }
        }

        public ICartesianAxis[] YAxes
        {
            get => _yAxes;
            set
            {
                _yAxes = value;
                this.Bindings?.Update();
            }
        }

        public int TotalBills { get; set; }
        public int CanceledBills { get; set; }
        public int TotalItemsSold { get; set; }
        public double AvgItemsPerBill { get; set; }
        public double AvgRevenuePerBill { get; set; }
        public List<DrinkSalesSummary> ItemReports { get; set; }
        public List<HourlyRevenue> HourlyRevenues { get; set; }
        public DateTimeOffset SelectedDate { get; set; } = DateTimeOffset.Now;

        // Instances of DAOs
        private BillDAO billDAO;
        private BillInfoDAO billInfoDAO;
        private DrinkDAO drinkDAO;

        public StatisticPage()
        {
            this.InitializeComponent();
            billDAO = new BillDAO();
            billInfoDAO = new BillInfoDAO();
            drinkDAO = new DrinkDAO();

            _ = LoadDataAsync(DateTime.Today); // Initial load with today's date
        }

        private async Task LoadDataAsync(DateTime date)
        {
            // Get the selected date from the picker
            DateTime selectedDate = date;
            
            Debug.WriteLine($"LoadDataAsync called with date: {selectedDate:yyyy-MM-dd}");

            // Fetch all bills
            var client = DataProvider.Instance.Client;
            var result = await client.GetAllFullBills.ExecuteAsync();
            var allBills = result.Data?.AllBills?.Edges?
                .Select(e => new FullBill(e.Node))
                .ToList() ?? new List<FullBill>();

            // Filter bills by date and status
            var paidBills = allBills.Where(b => 
                b.DateCheckOut?.Date == selectedDate.Date && 
                b.Status == 1).ToList();
            var canceledBills = allBills.Where(b => 
                b.DateCheckOut?.Date == selectedDate.Date && 
                b.Status == 2).ToList();

            Debug.WriteLine($"Paid Bills fetched: {paidBills.Count}");

            // Calculate hourly revenue
            HourlyRevenues = new List<HourlyRevenue>();
            for (int hour = 0; hour < 24; hour++)
            {
                HourlyRevenues.Add(new HourlyRevenue
                {
                    Hour = $"{hour:00}:00",
                    Revenue = 0
                });
            }

            var billInfos = new List<BillInfo>();
            float totalRevenue = 0;

            Debug.WriteLine("\nProcessing bills and their revenues:");
            foreach (var bill in paidBills)
            {
                var infos = await billInfoDAO.GetBillInfoByBillIdAsync(bill.Id);
                billInfos.AddRange(infos);

                if (bill.DateCheckOut.HasValue)
                {
                    int hour = bill.DateCheckOut.Value.Hour;
                    float billRevenue = (float)bill.FinalAmount;
                    Debug.WriteLine($"Bill ID: {bill.Id}, Hour: {hour:00}:00, Revenue: {billRevenue:N0}đ");

                    totalRevenue += billRevenue;
                    HourlyRevenues[hour].Revenue += billRevenue;
                }
            }

            Debug.WriteLine("\nHourly Revenue Summary:");
            foreach (var hourlyRev in HourlyRevenues.Where(h => h.Revenue > 0))
            {
                Debug.WriteLine($"Hour {hourlyRev.Hour}: {hourlyRev.Revenue:N0}đ");
            }
            Debug.WriteLine($"Total Revenue: {totalRevenue:N0}đ");

            // Update statistics
            TotalBills = paidBills.Count;
            CanceledBills = canceledBills.Count;
            TotalItemsSold = billInfos.Sum(bi => bi.Count);
            AvgItemsPerBill = TotalBills > 0 ? (double)TotalItemsSold / TotalBills : 0;
            AvgRevenuePerBill = TotalBills > 0 ? totalRevenue / TotalBills : 0;

            // Update UI text blocks
            TotalBillsText.Text = TotalBills.ToString();
            CanceledBillsText.Text = CanceledBills.ToString();
            TotalItemsText.Text = TotalItemsSold.ToString();
            AvgItemsText.Text = $"{AvgItemsPerBill:F1}";
            AvgRevenueText.Text = $"{AvgRevenuePerBill:N0}đ";

            // Update the chart
            UpdateHourlyRevenueChart();
        }

        private void UpdateHourlyRevenueChart()
        {
            var values = HourlyRevenues.Select(x => (double)x.Revenue).ToArray();
            var labels = HourlyRevenues.Select(x => x.Hour).ToArray();

            Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Doanh thu theo giờ",
                    Values = values,
                    Fill = new SolidColorPaint(SKColors.DodgerBlue)
                }
            };

            XAxes = new ICartesianAxis[]
            {
                new Axis
                {
                    Name = "Giờ",
                    Labels = labels,
                    LabelsRotation = 45,
                    TextSize = 12
                }
            };

            YAxes = new ICartesianAxis[]
            {
                new Axis
                {
                    Name = "Doanh thu (đ)",
                    Labeler = value => $"{value:N0}đ",
                    TextSize = 12
                }
            };
        }

        private async void ApplyDateButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ApplyDateButton_Click called");

            if (StatisticDatePicker.Date is DateTimeOffset selectedDate)
            {
                Debug.WriteLine($"Selected Date: {selectedDate.Date:yyyy-MM-dd}");
                await LoadDataAsync(selectedDate.LocalDateTime.Date);
            }
        }
    }
}
