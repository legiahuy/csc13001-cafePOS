using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using SkiaSharp;
using ClosedXML.Excel;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using CafePOS.DAO;
using CafePOS.DTO;
using CafePOS.GraphQL;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using WinRT.Interop;
using System.Diagnostics;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace CafePOS
{
    public sealed partial class StatisticPage : Page, INotifyPropertyChanged
    {
        private readonly ICafePOSClient _client;
        private DateTime _selectedDate = DateTime.Today;
        private DateTime _startDate;
        private DateTime _endDate;
        private List<IGetAllBills_AllBills_Edges_Node> _bills = new();
        private List<IGetAllBills_AllBills_Edges_Node> _comparisonBills = new();
        private ISeries[] _series = Array.Empty<ISeries>();
        private ISeries[] _paymentSeries = Array.Empty<ISeries>();
        private Axis[] _xAxes = Array.Empty<Axis>();
        private Axis[] _yAxes = Array.Empty<Axis>();
        private static List<PaymentMethod> _cachedPaymentMethods = new();
        private SemaphoreSlim _dialogSemaphore = new SemaphoreSlim(1, 1);

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ISeries[] Series
        {
            get => _series;
            set
            {
                if (_series != value)
                {
                    _series = value;
                    OnPropertyChanged();
                }
            }
        }

        public ISeries[] PaymentSeries
        {
            get => _paymentSeries;
            set
            {
                if (_paymentSeries != value)
                {
                    _paymentSeries = value;
                    OnPropertyChanged();
                }
            }
        }

        public Axis[] XAxes
        {
            get => _xAxes;
            set
            {
                if (_xAxes != value)
                {
                    _xAxes = value;
                    OnPropertyChanged();
                }
            }
        }

        public Axis[] YAxes
        {
            get => _yAxes;
            set
            {
                if (_yAxes != value)
                {
                    _yAxes = value;
                    OnPropertyChanged();
                }
            }
        }

        public StatisticPage()
        {
            this.InitializeComponent();
            _client = DataProvider.Instance.Client;

            // Initialize empty series
            Series = new ISeries[]
            {
                new ColumnSeries<ObservableValue>
                {
                    Values = new ObservableValue[24] { new(0), new(0), new(0), new(0), new(0), new(0),
                                                      new(0), new(0), new(0), new(0), new(0), new(0),
                                                      new(0), new(0), new(0), new(0), new(0), new(0),
                                                      new(0), new(0), new(0), new(0), new(0), new(0) },
                    Name = "Doanh thu"
                }
            };

            PaymentSeries = Array.Empty<ISeries>();

            // Initialize axes
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Giờ",
                    NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                    Labels = Enumerable.Range(0, 24).Select(i => i.ToString()).ToArray()
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Doanh thu (VND)",
                    NamePadding = new LiveChartsCore.Drawing.Padding(0, 15)
                }
            };

            // Set default date
            _startDate = _selectedDate;
            _endDate = _selectedDate.AddDays(1).AddSeconds(-1);

            // Set DataContext
            this.DataContext = this;

            // Load initial data
            _ = LoadDataAsync();
        }

        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeRangeComboBox.SelectedItem == null) return;

            var now = DateTime.Now;
            var selectedItem = (ComboBoxItem)TimeRangeComboBox.SelectedItem;
            bool isDailyView = selectedItem.Content.ToString() == "Theo ngày";
            
            // Enable/disable comparison buttons based on filter
            CompareDayButton.IsEnabled = isDailyView;
            CompareWeekButton.IsEnabled = isDailyView;
            CompareMonthButton.IsEnabled = isDailyView;

            // Set the opacity to make it visually clear which buttons are disabled
            CompareDayButton.Opacity = isDailyView ? 1.0 : 0.5;
            CompareWeekButton.Opacity = isDailyView ? 1.0 : 0.5;
            CompareMonthButton.Opacity = isDailyView ? 1.0 : 0.5;

            // Hide trend indicators only for non-daily views
            if (!isDailyView)
            {
                HideAllTrendIndicators();
            }

            switch (selectedItem.Content.ToString())
            {
                case "Theo ngày":
                    _startDate = now.Date;
                    _endDate = now.Date.AddDays(1).AddSeconds(-1);
                    break;
                case "Theo tuần":
                    _startDate = now.Date.AddDays(-(int)now.DayOfWeek);
                    _endDate = _startDate.AddDays(7).AddSeconds(-1);
                    break;
                case "Theo tháng":
                    _startDate = new DateTime(now.Year, now.Month, 1);
                    _endDate = _startDate.AddMonths(1).AddSeconds(-1);
                    break;
                case "Theo quý":
                    var quarter = (now.Month - 1) / 3;
                    _startDate = new DateTime(now.Year, quarter * 3 + 1, 1);
                    _endDate = _startDate.AddMonths(3).AddSeconds(-1);
                    break;
                case "Theo năm":
                    _startDate = new DateTime(now.Year, 1, 1);
                    _endDate = _startDate.AddYears(1).AddSeconds(-1);
                    break;
            }

            StartDatePicker.Date = _startDate;
            EndDatePicker.Date = _endDate;
        }

        private void HideAllTrendIndicators()
        {
            // Revenue trends
            RevenueTrendPanel.Visibility = Visibility.Collapsed;
            RevenueTrendIcon.Visibility = Visibility.Collapsed;
            RevenueTrendText.Visibility = Visibility.Collapsed;

            // Bills trends
            BillsTrendPanel.Visibility = Visibility.Collapsed;
            BillsTrendIcon.Visibility = Visibility.Collapsed;
            BillsTrendText.Visibility = Visibility.Collapsed;

            // Average trends
            AverageTrendPanel.Visibility = Visibility.Collapsed;
            AverageTrendIcon.Visibility = Visibility.Collapsed;
            AverageTrendText.Visibility = Visibility.Collapsed;

            // Cancelled trends
            CancelledTrendPanel.Visibility = Visibility.Collapsed;
            CancelledTrendIcon.Visibility = Visibility.Collapsed;
            CancelledTrendText.Visibility = Visibility.Collapsed;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.Date == null || EndDatePicker.Date == null)
            {
                await ShowErrorDialog("Lỗi", "Vui lòng chọn khoảng thời gian");
                return;
            }

            var startDate = StartDatePicker.Date.Value.DateTime;
            var endDate = EndDatePicker.Date.Value.DateTime;

            // If dates are the same (theo ngay), set time range for the whole day
            if (startDate.Date == endDate.Date)
            {
                _startDate = startDate.Date; // 00:00:00
                _endDate = startDate.Date.AddDays(1).AddSeconds(-1); // 23:59:59
            }
            else
            {
                if (startDate > endDate)
                {
                    await ShowErrorDialog("Lỗi", "Ngày bắt đầu không thể sau ngày kết thúc");
                    return;
                }
                _startDate = startDate.Date;
                _endDate = endDate.Date.AddDays(1).AddSeconds(-1);
            }

            await LoadDataAsync();
        }

        private async void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var now = DateTime.Now;
            DateTime compareStartDate;
            DateTime compareEndDate;

            switch (button.Name)
            {
                case "CompareYesterdayButton":
                    compareStartDate = now.AddDays(-1).Date;
                    compareEndDate = compareStartDate.AddDays(1).AddSeconds(-1);
                    break;
                case "CompareLastWeekButton":
                    compareStartDate = now.AddDays(-7).Date;
                    compareEndDate = now.Date.AddSeconds(-1);
                    break;
                case "CompareLastMonthButton":
                    compareStartDate = now.AddMonths(-1).Date;
                    compareEndDate = now.Date.AddSeconds(-1);
                    break;
                default:
                    return;
            }

            var result = await _client.GetAllBills.ExecuteAsync();
            if (result.Data?.AllBills?.Edges != null)
            {
                _comparisonBills = result.Data.AllBills.Edges
                    .Select(edge => edge.Node)
                    .Where(b => {
                        var checkInDate = DateTime.Parse(b.DateCheckIn);
                        return checkInDate >= compareStartDate && 
                               checkInDate <= compareEndDate && 
                               b.Status == 1;
                    })
                .ToList();
                UpdateComparisonUI();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading data for date range: {_startDate:yyyy-MM-dd HH:mm:ss} to {_endDate:yyyy-MM-dd HH:mm:ss}");
                
                // Get all bills
                var result = await _client.GetAllBills.ExecuteAsync();
                
                if (result.Data?.AllBills?.Edges != null)
                {
                    _bills = result.Data.AllBills.Edges
                        .Where(edge => edge?.Node != null)
                        .Select(edge => edge.Node)
                        .Where(b => {
                            if (string.IsNullOrEmpty(b.DateCheckIn)) return false;
                            var checkInDate = DateTime.Parse(b.DateCheckIn);
                            var isInRange = checkInDate >= _startDate && checkInDate <= _endDate;
                            System.Diagnostics.Debug.WriteLine($"Bill {b.Id}: CheckIn={b.DateCheckIn}, Status={b.Status}, InRange={isInRange}");
                            return isInRange && (b.Status == 1 || b.Status == 2); // Include both paid and cancelled bills
                        })
                .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"Found {_bills.Count} bills for date range");

                    var paidBills = _bills.Where(b => b.Status == 1).ToList();
                    var cancelledBills = _bills.Where(b => b.Status == 2).ToList();

                    // Calculate total revenue and bills count (only from paid bills)
                    var totalRevenue = paidBills.Sum(b => b.TotalAmount);
                    var totalPaidBills = paidBills.Count;
                    var totalCancelledBills = cancelledBills.Count;
                    var averageBillValue = totalPaidBills > 0 ? totalRevenue / totalPaidBills : 0;

                    // Update summary cards
                    TotalRevenueText.Text = totalRevenue.ToString("C0", new CultureInfo("vi-VN"));
                    TotalBillsText.Text = totalPaidBills.ToString();
                    CancelledBillsText.Text = totalCancelledBills.ToString();
                    AverageBillValueText.Text = averageBillValue.ToString("C0", new CultureInfo("vi-VN"));

                    // Update best selling product (only from paid bills)
                    var bestSellingProduct = GetBestSellingProduct(paidBills);
                    BestSellingProductText.Text = bestSellingProduct ?? "-";

                    // Calculate hourly revenue (only from paid bills)
                    var hourlyRevenue = new ObservableValue[24];
                    for (int i = 0; i < 24; i++)
                    {
                        hourlyRevenue[i] = new ObservableValue(0);
                    }

                    foreach (var bill in paidBills)
                    {
                        if (!string.IsNullOrEmpty(bill.DateCheckOut))
                        {
                            var checkoutDate = DateTime.Parse(bill.DateCheckOut);
                            var hour = checkoutDate.Hour;
                            hourlyRevenue[hour].Value += bill.TotalAmount;
                            System.Diagnostics.Debug.WriteLine($"Added revenue {bill.TotalAmount} to hour {hour}");
                        }
                    }

                    // Update hourly revenue chart
                    HourlyRevenueChart.Series = new ISeries[]
                    {
                        new ColumnSeries<ObservableValue>
                        {
                            Values = hourlyRevenue,
                            Name = "Doanh thu"
                        }
                    };

                    HourlyRevenueChart.XAxes = new Axis[]
                    {
                        new Axis
                        {
                            Name = "Giờ",
                            Labels = Enumerable.Range(0, 24).Select(i => i.ToString()).ToArray()
                        }
                    };

                    HourlyRevenueChart.YAxes = new Axis[]
                    {
                        new Axis
                        {
                            Name = "Doanh thu (VND)",
                            Labeler = (value) => value.ToString("N0", new CultureInfo("vi-VN")) + " đ"
                        }
                    };

                    // Tính doanh thu theo ngày
                    var groupedByDay = paidBills
                        .Where(b => !string.IsNullOrEmpty(b.DateCheckOut))
                        .GroupBy(b => DateTime.Parse(b.DateCheckOut).Date)
                        .OrderBy(g => g.Key)
                        .ToList();

                    // Tạo nhãn trục X và dữ liệu trục Y
                    var dailyLabels = groupedByDay.Select(g => g.Key.ToString("dd/MM")).ToArray();
                    var dailyValues = groupedByDay.Select(g => g.Sum(b => b.TotalAmount)).ToArray();

                    // Hiển thị biểu đồ
                    DailyRevenueChart.Series = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = dailyValues,
                            Name = "Doanh thu"
                        }
                    };

                    DailyRevenueChart.XAxes = new Axis[]
                    {
                        new Axis
                        {
                            Name = "Ngày",
                            Labels = dailyLabels,
                            LabelsRotation = 15
                        }
                    };

                    DailyRevenueChart.YAxes = new Axis[]
                    {
                        new Axis
                        {
                            Name = "Doanh thu (VND)",
                            Labeler = value => value.ToString("N0", new CultureInfo("vi-VN")) + " đ"
                        }
                    };

                    // Calculate payment methods distribution (only from paid bills)
                    var paymentMethods = paidBills
                        .GroupBy(b => b.PaymentMethod ?? 0)
                        .Select(g => new
                        {
                            Method = g.Key,
                            Amount = g.Sum(b => b.TotalAmount)
                        })
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Found {paymentMethods.Count} payment methods");
                    foreach (var pm in paymentMethods)
                    {
                        System.Diagnostics.Debug.WriteLine($"Payment Method {pm.Method}: {pm.Amount}");
                    }

                    // Get payment method names from database
                    var paymentMethodsResult = await _client.GetPaymentMethods.ExecuteAsync();
                    var paymentMethodNames = paymentMethodsResult.Data?.AllPaymentMethods?.Nodes?
                        .ToDictionary(p => p.Id, p => p.Name) ?? new Dictionary<int, string>();

                    // Update payment methods chart
                    PaymentMethodsChart.Series = paymentMethods.Select(p =>
                    {
                        var methodName = paymentMethodNames.TryGetValue(p.Method, out var name) ? name : "Khác";
                        return new PieSeries<double>
                        {
                            Values = new[] { p.Amount },
                            Name = methodName,
                            Fill = new SolidColorPaint(GetRandomColor())
                        };
                    }).ToArray();

                    // Update category and product revenue charts (only from paid bills)
                    UpdateCategoryRevenueChart(paidBills);
                    UpdateProductRevenueChart(paidBills);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No bills data available");
                    await ShowErrorDialog("Thông báo", "Không có dữ liệu hóa đơn trong khoảng thời gian này");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadDataAsync: {ex}");
                await ShowErrorDialog("Lỗi", $"Không thể tải dữ liệu thống kê: {ex.Message}");
            }
        }

        private void UpdateComparisonUI()
        {
            var currentPaidBills = _bills.Where(b => b.Status == 1).ToList();
            var comparisonPaidBills = _comparisonBills.Where(b => b.Status == 1).ToList();

            var currentRevenue = currentPaidBills.Sum(b => b.TotalAmount);
            var comparisonRevenue = comparisonPaidBills.Sum(b => b.TotalAmount);
            var revenueChange = currentRevenue - comparisonRevenue;
            var revenueChangePercentage = comparisonRevenue > 0 ? (revenueChange / comparisonRevenue) * 100 : 0;

            // Update UI with comparison data
            // You can add comparison indicators to the summary cards
        }

        private string GetBestSellingProduct(List<IGetAllBills_AllBills_Edges_Node> bills)
        {
            var productSales = new Dictionary<string, int>();
            foreach (var bill in bills)
            {
                if (bill.BillInfosByIdBill?.Nodes != null)
                {
                    foreach (var info in bill.BillInfosByIdBill.Nodes)
                    {
                        var productName = info?.ProductByIdProduct?.Name ?? "Unknown Product";
                        if (!productSales.ContainsKey(productName))
                            productSales[productName] = 0;
                        productSales[productName] += info?.Count ?? 0;
                    }
                }
            }

            return productSales.OrderByDescending(p => p.Value).FirstOrDefault().Key ?? "-";
        }

        private void UpdateCategoryRevenueChart(List<IGetAllBills_AllBills_Edges_Node> bills)
        {
            try
            {
                var categoryRevenue = new Dictionary<string, double>();
                foreach (var bill in bills)
                {
                    if (bill.BillInfosByIdBill?.Nodes != null)
                    {
                        foreach (var info in bill.BillInfosByIdBill.Nodes)
                        {
                            if (info?.ProductByIdProduct?.CategoryByIdCategory != null)
                            {
                                var category = info.ProductByIdProduct.CategoryByIdCategory.Name ?? "Unknown Category";
                                if (!categoryRevenue.ContainsKey(category))
                                    categoryRevenue[category] = 0;
                                categoryRevenue[category] += info.TotalPrice;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found {categoryRevenue.Count} categories");
                foreach (var cat in categoryRevenue)
                {
                    System.Diagnostics.Debug.WriteLine($"Category {cat.Key}: {cat.Value}");
                }

                if (categoryRevenue.Any())
                {
                    CategoryRevenueChart.Series = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Values = categoryRevenue.Values.ToArray(),
                            Name = "Doanh thu",
                            Fill = new SolidColorPaint(SKColors.ForestGreen)
                        }
                    };

                    CategoryRevenueChart.XAxes = new Axis[]
                    {
                        new Axis
                        {
                            Labels = categoryRevenue.Keys.ToArray(),
                            LabelsRotation = 45
                        }
                    };

                    CategoryRevenueChart.YAxes = new Axis[]
                    {
                        new Axis
                        {
                            Name = "Doanh thu (VND)",
                            Labeler = (value) => value.ToString("N0", new CultureInfo("vi-VN")) + " đ"
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating category chart: {ex}");
            }
        }

        private void UpdateProductRevenueChart(List<IGetAllBills_AllBills_Edges_Node> bills)
        {
            try
            {
                var productRevenue = new Dictionary<string, double>();
                foreach (var bill in bills)
                {
                    if (bill.BillInfosByIdBill?.Nodes != null)
                    {
                        foreach (var info in bill.BillInfosByIdBill.Nodes)
                        {
                            if (info?.ProductByIdProduct != null)
                            {
                                var productName = info.ProductByIdProduct.Name ?? "Unknown Product";
                                if (!productRevenue.ContainsKey(productName))
                                    productRevenue[productName] = 0;
                                productRevenue[productName] += info.TotalPrice;
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found {productRevenue.Count} products");
                foreach (var prod in productRevenue)
                {
                    System.Diagnostics.Debug.WriteLine($"Product {prod.Key}: {prod.Value}");
                }

                if (productRevenue.Any())
                {
                    ProductRevenueChart.Series = productRevenue.Select(p => new PieSeries<double>
                    {
                        Values = new[] { p.Value },
                        Name = p.Key,
                        Fill = new SolidColorPaint(GetRandomColor())
                    }).ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product chart: {ex}");
            }
        }

        private string GetPaymentMethodName(int? methodId)
        {
            return methodId switch
            {
                1 => "Tiền mặt",
                2 => "Chuyển khoản",
                3 => "Thẻ",
                _ => "Khác"
            };
        }

        private SKColor GetRandomColor()
        {
            var random = new Random();
            return new SKColor(
                (byte)random.Next(256),
                (byte)random.Next(256),
                (byte)random.Next(256)
            );
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileSavePicker();
                
                // Initialize the FileSavePicker with the window handle
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
                
                filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                filePicker.FileTypeChoices.Add("Excel Files", new List<string> { ".xlsx" });
                filePicker.SuggestedFileName = $"BaoCaoDoanhThu_{_startDate:yyyyMMdd}_{_endDate:yyyyMMdd}";

                var file = await filePicker.PickSaveFileAsync();
                if (file != null)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Báo cáo doanh thu");

                        // Add headers
                        worksheet.Cell(1, 1).Value = "Báo cáo doanh thu";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(2, 1).Value = $"Từ ngày: {_startDate:dd/MM/yyyy}";
                        worksheet.Cell(2, 2).Value = $"Đến ngày: {_endDate:dd/MM/yyyy}";

                        // Add summary
                        var paidBills = _bills.Where(b => b.Status == 1).ToList();
                        worksheet.Cell(4, 1).Value = "Tổng doanh thu:";
                        worksheet.Cell(4, 2).Value = paidBills.Sum(b => b.TotalAmount);
                        worksheet.Cell(4, 2).Style.NumberFormat.Format = "#,##0";

                        // Add bill details
                        worksheet.Cell(6, 1).Value = "Chi tiết hóa đơn";
                        worksheet.Cell(6, 1).Style.Font.Bold = true;

                        var headers = new[] { "ID", "Ngày vào", "Ngày ra", "Tổng tiền", "Giảm giá", "Thành tiền", "Trạng thái", "Phương thức thanh toán" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(7, i + 1).Value = headers[i];
                            worksheet.Cell(7, i + 1).Style.Font.Bold = true;
                        }

                        int row = 8;
                        foreach (var bill in paidBills)
                        {
                            worksheet.Cell(row, 1).Value = bill.Id;
                            worksheet.Cell(row, 2).Value = DateTime.Parse(bill.DateCheckIn);
                            worksheet.Cell(row, 3).Value = !string.IsNullOrEmpty(bill.DateCheckOut) ? DateTime.Parse(bill.DateCheckOut) : (DateTime?)null;
                            worksheet.Cell(row, 4).Value = bill.TotalAmount;
                            worksheet.Cell(row, 5).Value = bill.Discount;
                            worksheet.Cell(row, 6).Value = bill.TotalAmount - (bill.Discount ?? 0);
                            worksheet.Cell(row, 7).Value = bill.Status == 1 ? "Đã thanh toán" : "Đã hủy";
                            worksheet.Cell(row, 8).Value = GetPaymentMethodName(bill.PaymentMethod);

                            // Format numbers
                            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0";

                            // Format dates
                            worksheet.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                            worksheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";

                            row++;
                        }

                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();

                        // Save the workbook
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            workbook.SaveAs(stream);
                        }

                        await ShowDialog("Thành công", "Đã xuất báo cáo thành công!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await ShowErrorDialog("Lỗi khi xuất báo cáo", ex.Message);
            }
        }

        private async Task ShowDialog(string title, string content)
        {
            try
            {
                await _dialogSemaphore.WaitAsync();
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
            finally
            {
                _dialogSemaphore.Release();
            }
        }

        private async Task ShowErrorDialog(string title, string content)
        {
            try
            {
                await _dialogSemaphore.WaitAsync();
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
            finally
            {
                _dialogSemaphore.Release();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Set default dates
            var today = DateTime.Now;
            _startDate = today.Date;
            _endDate = today.Date.AddDays(1).AddSeconds(-1);
            
            StartDatePicker.Date = _startDate;
            EndDatePicker.Date = _endDate;
            
            // Set default filter
            TimeRangeComboBox.SelectedIndex = 0; // "Theo ngày"
            
            _ = LoadDataAsync();
        }

        private void ShowComparison(string compareText, double difference, FontIcon trendIcon, TextBlock trendText)
        {
            trendIcon.Visibility = Visibility.Visible;
            trendText.Visibility = Visibility.Visible;

            // Format the difference as percentage
            var percentChange = Math.Abs(difference) * 100;
            
            if (difference > 0)
            {
                trendIcon.Glyph = "\uE74A"; // Up arrow
                trendIcon.Foreground = new SolidColorBrush(Colors.Green);
                trendText.Foreground = new SolidColorBrush(Colors.Green);
                trendText.Text = $"+{percentChange:F1}%";
            }
            else if (difference < 0)
            {
                trendIcon.Glyph = "\uE74B"; // Down arrow
                trendIcon.Foreground = new SolidColorBrush(Colors.Red);
                trendText.Foreground = new SolidColorBrush(Colors.Red);
                trendText.Text = $"-{percentChange:F1}%";
            }
            else
            {
                trendIcon.Glyph = "\uE73E"; // Equal sign
                trendIcon.Foreground = new SolidColorBrush(Colors.Gray);
                trendText.Foreground = new SolidColorBrush(Colors.Gray);
                trendText.Text = "0%";
            }
        }

        private async void CompareDayButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            var todayStats = await GetStatistics(today, today);
            var yesterdayStats = await GetStatistics(yesterday, yesterday);

            if (todayStats != null && yesterdayStats != null)
            {
                // Compare Revenue
                var revenueDiff = (todayStats.TotalRevenue - yesterdayStats.TotalRevenue) / (yesterdayStats.TotalRevenue == 0 ? 1 : yesterdayStats.TotalRevenue);
                ShowComparison("So với hôm qua", revenueDiff, RevenueTrendIcon, RevenueTrendText);

                // Compare Bills Count
                var billsDiff = (todayStats.TotalBills - yesterdayStats.TotalBills) / (double)(yesterdayStats.TotalBills == 0 ? 1 : yesterdayStats.TotalBills);
                ShowComparison("So với hôm qua", billsDiff, BillsTrendIcon, BillsTrendText);

                // Compare Average Bill Value
                var avgDiff = (todayStats.AverageBillValue - yesterdayStats.AverageBillValue) / (yesterdayStats.AverageBillValue == 0 ? 1 : yesterdayStats.AverageBillValue);
                ShowComparison("So với hôm qua", avgDiff, AverageTrendIcon, AverageTrendText);

                // Compare Cancelled Bills
                var cancelledDiff = (todayStats.CancelledBills - yesterdayStats.CancelledBills) / (double)(yesterdayStats.CancelledBills == 0 ? 1 : yesterdayStats.CancelledBills);
                ShowComparison("So với hôm qua", cancelledDiff, CancelledTrendIcon, CancelledTrendText);
            }
        }

        private async void CompareWeekButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
            var lastWeekStart = thisWeekStart.AddDays(-7);
            var lastWeekEnd = thisWeekStart.AddDays(-1);

            var thisWeekStats = await GetStatistics(thisWeekStart, today);
            var lastWeekStats = await GetStatistics(lastWeekStart, lastWeekEnd);

            if (thisWeekStats != null && lastWeekStats != null)
            {
                // Compare Revenue
                var revenueDiff = (thisWeekStats.TotalRevenue - lastWeekStats.TotalRevenue) / (lastWeekStats.TotalRevenue == 0 ? 1 : lastWeekStats.TotalRevenue);
                ShowComparison("So với tuần trước", revenueDiff, RevenueTrendIcon, RevenueTrendText);

                // Compare Bills Count
                var billsDiff = (thisWeekStats.TotalBills - lastWeekStats.TotalBills) / (double)(lastWeekStats.TotalBills == 0 ? 1 : lastWeekStats.TotalBills);
                ShowComparison("So với tuần trước", billsDiff, BillsTrendIcon, BillsTrendText);

                // Compare Average Bill Value
                var avgDiff = (thisWeekStats.AverageBillValue - lastWeekStats.AverageBillValue) / (lastWeekStats.AverageBillValue == 0 ? 1 : lastWeekStats.AverageBillValue);
                ShowComparison("So với tuần trước", avgDiff, AverageTrendIcon, AverageTrendText);

                // Compare Cancelled Bills
                var cancelledDiff = (thisWeekStats.CancelledBills - lastWeekStats.CancelledBills) / (double)(lastWeekStats.CancelledBills == 0 ? 1 : lastWeekStats.CancelledBills);
                ShowComparison("So với tuần trước", cancelledDiff, CancelledTrendIcon, CancelledTrendText);
            }
        }

        private async void CompareMonthButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            var thisMonthStats = await GetStatistics(thisMonthStart, today);
            var lastMonthStats = await GetStatistics(lastMonthStart, lastMonthEnd);

            if (thisMonthStats != null && lastMonthStats != null)
            {
                // Compare Revenue
                var revenueDiff = (thisMonthStats.TotalRevenue - lastMonthStats.TotalRevenue) / (lastMonthStats.TotalRevenue == 0 ? 1 : lastMonthStats.TotalRevenue);
                ShowComparison("So với tháng trước", revenueDiff, RevenueTrendIcon, RevenueTrendText);

                // Compare Bills Count
                var billsDiff = (thisMonthStats.TotalBills - lastMonthStats.TotalBills) / (double)(lastMonthStats.TotalBills == 0 ? 1 : lastMonthStats.TotalBills);
                ShowComparison("So với tháng trước", billsDiff, BillsTrendIcon, BillsTrendText);

                // Compare Average Bill Value
                var avgDiff = (thisMonthStats.AverageBillValue - lastMonthStats.AverageBillValue) / (lastMonthStats.AverageBillValue == 0 ? 1 : lastMonthStats.AverageBillValue);
                ShowComparison("So với tháng trước", avgDiff, AverageTrendIcon, AverageTrendText);

                // Compare Cancelled Bills
                var cancelledDiff = (thisMonthStats.CancelledBills - lastMonthStats.CancelledBills) / (double)(lastMonthStats.CancelledBills == 0 ? 1 : lastMonthStats.CancelledBills);
                ShowComparison("So với tháng trước", cancelledDiff, CancelledTrendIcon, CancelledTrendText);
            }
        }

        private class StatisticsData
        {
            public double TotalRevenue { get; set; }
            public int TotalBills { get; set; }
            public double AverageBillValue { get; set; }
            public int CancelledBills { get; set; }
        }

        private async Task<StatisticsData> GetStatistics(DateTime startDate, DateTime endDate)
        {
            try
            {
                var bills = await GetBillsByDateRange(startDate, endDate);
                if (bills == null) return null;

                var stats = new StatisticsData
                {
                    TotalRevenue = bills.Sum(b => b.FinalAmount),
                    TotalBills = bills.Count(b => b.Status == 1), // Completed bills
                    CancelledBills = bills.Count(b => b.Status == 2), // Cancelled bills
                };
                
                stats.AverageBillValue = stats.TotalBills > 0 ? stats.TotalRevenue / stats.TotalBills : 0;
                
                return stats;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting statistics: {ex.Message}");
                return null;
            }
        }

        private void UpdateAverageTrend(decimal currentAverage, decimal previousAverage)
        {
            if (previousAverage == 0) return;

            var percentChange = ((currentAverage - previousAverage) / previousAverage) * 100;
            ShowComparison("", (double)percentChange / 100, AverageTrendIcon, AverageTrendText);
        }

        private async Task<List<IGetAllBills_AllBills_Edges_Node>> GetBillsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = await _client.GetAllBills.ExecuteAsync();
                if (result.Data?.AllBills?.Edges != null)
                {
                    return result.Data.AllBills.Edges
                        .Where(edge => edge?.Node != null)
                        .Select(edge => edge.Node)
                        .Where(b => {
                            if (string.IsNullOrEmpty(b.DateCheckIn)) return false;
                            var checkInDate = DateTime.Parse(b.DateCheckIn);
                            return checkInDate >= startDate && checkInDate <= endDate;
                        })
                        .ToList();
                }
                return new List<IGetAllBills_AllBills_Edges_Node>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting bills by date range: {ex.Message}");
                return new List<IGetAllBills_AllBills_Edges_Node>();
            }
        }

        private decimal CalculateAverageBillValue(DateTime start, DateTime end)
        {
            var bills = _bills?.Where(b => 
                b.DateCheckIn != null && 
                DateTime.Parse(b.DateCheckIn).Date >= start.Date && 
                DateTime.Parse(b.DateCheckIn).Date <= end.Date &&
                b.Status == 1) // Changed from "Paid" to 1 for paid status
                .ToList();

            if (bills == null || !bills.Any()) return 0;

            var totalAmount = bills.Sum(b => b.TotalAmount);
            return (decimal)(totalAmount / bills.Count);
        }
    }
} 