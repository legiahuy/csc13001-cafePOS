using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using CafePOS.GraphQL;
using ClosedXML.Excel;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using CafePOS.DAO;
using CafePOS.DTO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Text;
using WinRT.Interop;
using Microsoft.UI.Xaml.Data;

namespace CafePOS
{
    public sealed partial class BillManage : Page, INotifyPropertyChanged
    {
        private readonly ICafePOSClient _client;
        private ObservableCollection<BillViewModel> _allBills = new ObservableCollection<BillViewModel>();
        private ObservableCollection<BillViewModel> _filteredBills = new ObservableCollection<BillViewModel>();
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _searchText = string.Empty;
        private int _currentPage = 1;
        private const int _itemsPerPage = 10;
        private int _totalPages = 1;
        private bool _isUpdating = false;
        private static List<PaymentMethod> _cachedPaymentMethods;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<BillViewModel> Bills
        {
            get => _filteredBills;
            set
            {
                if (_filteredBills != value)
                {
                    _filteredBills = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value && !_isUpdating)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    UpdatePagedBills();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged();
                }
            }
        }

        public BillManage()
        {
            this.InitializeComponent();
            _client = DataProvider.Instance.Client;
            Bills = _allBills;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Clear cache to ensure fresh data
            _cachedPaymentMethods = null;
            foreach (var bill in _allBills)
            {
                bill.RefreshPaymentMethod();
            }
            _ = LoadBillsAsync();
        }

        private async Task LoadBillsAsync()
        {
            try
            {
                // Load payment methods first to ensure they're available
                if (_cachedPaymentMethods == null)
                {
                    _cachedPaymentMethods = await PaymentMethodDAO.Instance.GetAllActivePaymentMethodsAsync();
                }

                var result = await _client.GetAllBills.ExecuteAsync();
                
                if (result.Data?.AllBills?.Edges != null)
                {
                    _allBills.Clear();
                    foreach (var edge in result.Data.AllBills.Edges)
                    {
                        var billVM = new BillViewModel(edge.Node);
                        await billVM.InitializePaymentMethodTextAsync(); // Ensure payment method is loaded
                        _allBills.Add(billVM);
                    }
                    UpdatePagedBills();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể tải danh sách hóa đơn: {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private void UpdatePagedBills()
        {
            try
            {
                _isUpdating = true;

                var filtered = _allBills.AsEnumerable();

                // Apply date range filter
                if (_startDate.HasValue && _endDate.HasValue)
                {
                    filtered = filtered.Where(b => 
                    {
                        var checkInDate = b.DateCheckIn.ToLocalTime();
                        return checkInDate >= _startDate.Value && checkInDate <= _endDate.Value;
                    });
                }

                // Apply ID filter if search text is provided
                if (!string.IsNullOrEmpty(_searchText))
                {
                    filtered = filtered.Where(b => b.Id == _searchText);
                }

                // Sort by date descending
                filtered = filtered.OrderByDescending(b => b.DateCheckIn);

                // Calculate total pages
                var totalItems = filtered.Count();
                TotalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_itemsPerPage));

                // Ensure current page is valid
                _currentPage = Math.Min(Math.Max(1, _currentPage), TotalPages);
                OnPropertyChanged(nameof(CurrentPage));

                // Get items for current page
                var pagedItems = filtered
                    .Skip((_currentPage - 1) * _itemsPerPage)
                    .Take(_itemsPerPage);

                Bills = new ObservableCollection<BillViewModel>(pagedItems);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = 1;
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage = TotalPages;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.Date.HasValue && EndDatePicker.Date.HasValue)
            {
                // Clear ID search
                SearchTextBox.Text = string.Empty;
                _searchText = string.Empty;

                // Set start date to beginning of the day (00:00:00)
                _startDate = StartDatePicker.Date.Value.DateTime.Date;
                // Set end date to end of the day (23:59:59.999)
                _endDate = EndDatePicker.Date.Value.DateTime.Date.AddDays(1).AddTicks(-1);
                UpdatePagedBills();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Get search text
            _searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            // Get date range if selected
            if (StartDatePicker.Date.HasValue && EndDatePicker.Date.HasValue)
            {
                // Set start date to beginning of the day (00:00:00)
                _startDate = StartDatePicker.Date.Value.DateTime.Date;
                // Set end date to end of the day (23:59:59.999)
                _endDate = EndDatePicker.Date.Value.DateTime.Date.AddDays(1).AddTicks(-1);
            }
            else
            {
                _startDate = null;
                _endDate = null;
            }

            _currentPage = 1;
            UpdatePagedBills();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all filters
            StartDatePicker.Date = null;
            EndDatePicker.Date = null;
            SearchTextBox.Text = string.Empty;
            _startDate = null;
            _endDate = null;
            _searchText = string.Empty;

            _currentPage = 1;
            UpdatePagedBills();
        }

        private async Task<List<BillInfoViewModel>> GetBillInfosAsync(string billId)
        {
            try
            {
                var result = await _client.GetBillInfoByBillId.ExecuteAsync(int.Parse(billId));
                if (result.Data?.AllBillInfos?.Edges != null)
                {
                    var billInfos = new List<BillInfoViewModel>();
                    var groupedBillInfos = result.Data.AllBillInfos.Edges
                        .Select(edge => edge.Node)
                        .GroupBy(bi => bi.IdProduct)
                        .Select(g => new BillInfoViewModel
                        {
                            ProductId = g.Key,
                            Count = g.Sum(bi => bi.Count),
                            ProductName = g.First().ProductByIdProduct?.Name ?? "Unknown Product",
                            UnitPrice = g.First().UnitPrice,
                            TotalPrice = g.Sum(bi => bi.TotalPrice)
                        })
                        .ToList();

                    return groupedBillInfos;
                }
                return new List<BillInfoViewModel>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting bill infos: {ex.Message}");
                return new List<BillInfoViewModel>();
            }
        }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BillViewModel bill)
            {
                try
                {
                    var billInfos = await GetBillInfosAsync(bill.Id);
                    var detailsContent = new StringBuilder();
                    detailsContent.AppendLine($"ID: {bill.Id}");
                    detailsContent.AppendLine($"Bàn: {bill.IdTable}");
                    detailsContent.AppendLine($"Ngày vào: {bill.FormattedDateCheckIn}");
                    detailsContent.AppendLine($"Ngày ra: {bill.FormattedDateCheckOut}");
                    detailsContent.AppendLine($"Trạng thái: {bill.FormattedStatus}");
                    detailsContent.AppendLine($"Thanh toán: {bill.FormattedPaymentMethod}");
                    detailsContent.AppendLine();
                    detailsContent.AppendLine("Chi tiết đơn hàng:");
                    detailsContent.AppendLine("----------------------------------------");

                    double subtotal = 0;
                    foreach (var item in billInfos)
                    {
                        detailsContent.AppendLine($"{item.ProductName}");
                        detailsContent.AppendLine($"Số lượng: {item.Count}");
                        detailsContent.AppendLine($"Đơn giá: {item.UnitPrice:N0}đ");
                        var itemTotal = item.TotalPrice;
                        subtotal += itemTotal;
                        detailsContent.AppendLine($"Thành tiền: {itemTotal:N0}đ");
                        detailsContent.AppendLine("----------------------------------------");
                    }

                    detailsContent.AppendLine($"Tổng tiền: {subtotal:N0}đ");
                    detailsContent.AppendLine($"Giảm giá: {bill.FormattedDiscount}");
                    detailsContent.AppendLine($"Thành tiền: {bill.FormattedFinalAmount}");

                    var scrollViewer = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = detailsContent.ToString(),
                            TextWrapping = TextWrapping.Wrap
                        },
                        MaxHeight = 400, // Set maximum height for the ScrollViewer
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Padding = new Thickness(16)
                    };

                    var dialog = new ContentDialog
                    {
                        Title = "Chi tiết hóa đơn",
                        Content = scrollViewer,
                        PrimaryButtonText = "Đóng",
                        XamlRoot = App.MainAppWindow.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể tải chi tiết hóa đơn: {ex.Message}",
                        PrimaryButtonText = "OK",
                        XamlRoot = App.MainAppWindow.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Account.CurrentUserType != 1)
            {
                var dialog = new ContentDialog
                {
                    Title = "Không có quyền",
                    Content = "Bạn không có quyền xóa hóa đơn.",
                    CloseButtonText = "Đóng",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            if (sender is Button button && button.DataContext is BillViewModel bill)
            {
                var dialog = new ContentDialog
                {
                    Title = "Xác nhận xóa",
                    Content = $"Bạn có chắc chắn muốn xóa hóa đơn {bill.Id}?",
                    PrimaryButtonText = "Xóa",
                    SecondaryButtonText = "Hủy",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        var deleteResult = await _client.DeleteBillById.ExecuteAsync(int.Parse(bill.Id));
                        
                        if (deleteResult.Data?.DeleteBillById?.DeletedBillId != null)
                        {
                            _allBills.Remove(bill);
                            UpdatePagedBills();
                            
                            var successDialog = new ContentDialog
                            {
                                Title = "Thành công",
                                Content = "Đã xóa hóa đơn thành công!",
                                PrimaryButtonText = "OK",
                                XamlRoot = App.MainAppWindow.Content.XamlRoot
                            };
                            await successDialog.ShowAsync();
                        }
                        else
                        {
                            throw new Exception("Không thể xóa hóa đơn");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorDialog = new ContentDialog
                        {
                            Title = "Lỗi",
                            Content = $"Không thể xóa hóa đơn: {ex.Message}",
                            PrimaryButtonText = "OK",
                            XamlRoot = App.MainAppWindow.Content.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                }
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            await ExportBillsToExcel(Bills.ToList());
        }

        private async void ExportSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBills = Bills.Where(b => b.IsSelected).ToList();
            if (selectedBills.Count == 0)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Thông báo",
                    Content = "Vui lòng chọn ít nhất một hóa đơn để xuất",
                    CloseButtonText = "Ok",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            await ExportBillsToExcel(selectedBills);
        }

        private async Task ExportBillsToExcel(List<BillViewModel> billsToExport)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Bills");

                // Add headers
                worksheet.Cell(1, 1).Value = "Mã hóa đơn";
                worksheet.Cell(1, 2).Value = "Ngày vào";
                worksheet.Cell(1, 3).Value = "Ngày ra";
                worksheet.Cell(1, 4).Value = "Tổng tiền";
                worksheet.Cell(1, 5).Value = "Giảm giá";
                worksheet.Cell(1, 6).Value = "Thành tiền";
                worksheet.Cell(1, 7).Value = "Trạng thái";
                worksheet.Cell(1, 8).Value = "Thanh toán";

                // Style header row
                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                int currentRow = 2;
                foreach (var bill in billsToExport)
                {
                    // Add bill information
                    worksheet.Cell(currentRow, 1).Value = bill.Id;
                    worksheet.Cell(currentRow, 2).Value = bill.DateCheckIn;
                    worksheet.Cell(currentRow, 3).Value = bill.DateCheckOut;
                    worksheet.Cell(currentRow, 4).Value = bill.TotalAmount;
                    worksheet.Cell(currentRow, 5).Value = bill.Discount;
                    worksheet.Cell(currentRow, 6).Value = bill.FinalAmount;
                    worksheet.Cell(currentRow, 7).Value = bill.FormattedStatus;
                    worksheet.Cell(currentRow, 8).Value = bill.FormattedPaymentMethod;

                    // Format date columns
                    worksheet.Cell(currentRow, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";
                    worksheet.Cell(currentRow, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm:ss";

                    // Format number columns
                    worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";
                    worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";

                    currentRow++;

                    // Add bill details
                    var billDetails = await bill.GetBillDetails();
                    if (billDetails.Any())
                    {
                        // Add details header
                        worksheet.Cell(currentRow, 2).Value = "Chi tiết hóa đơn:";
                        worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                        currentRow++;

                        // Add details headers
                        worksheet.Cell(currentRow, 2).Value = "Tên sản phẩm";
                        worksheet.Cell(currentRow, 3).Value = "Số lượng";
                        worksheet.Cell(currentRow, 4).Value = "Đơn giá";
                        worksheet.Cell(currentRow, 5).Value = "Thành tiền";
                        worksheet.Range(currentRow, 2, currentRow, 5).Style.Font.Bold = true;
                        currentRow++;

                        // Add details
                        foreach (var detail in billDetails)
                        {
                            worksheet.Cell(currentRow, 2).Value = detail.ProductName;
                            worksheet.Cell(currentRow, 3).Value = detail.Count.ToString();  // Convert to string
                            worksheet.Cell(currentRow, 4).Value = detail.UnitPrice;
                            worksheet.Cell(currentRow, 5).Value = detail.TotalPrice;

                            // Format number columns
                            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";
                            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0";

                            currentRow++;
                        }
                    }

                    // Add a blank row between bills
                    currentRow++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save file
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                savePicker.SuggestedFileName = $"Bills_Export_{DateTime.Now:yyyyMMdd_HHmmss}";

                // Initialize the FileSavePicker
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    using var windowsStream = stream.AsStream();
                    workbook.SaveAs(windowsStream);

                    ContentDialog dialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = "Xuất file Excel thành công!",
                        CloseButtonText = "Ok",
                        XamlRoot = App.MainAppWindow.Content.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Có lỗi xảy ra khi xuất file: {ex.Message}",
                    CloseButtonText = "Ok",
                    XamlRoot = App.MainAppWindow.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void BillsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Handle item click if needed
        }

        //private string GetStatusText(int status)
        //{
        //    return status switch
        //    {
        //        0 => "Chưa thanh toán",
        //        1 => "Đã thanh toán",
        //        2 => "Đã h",
        //        _ => "Không xác định"
        //    };
        //}
    }

    public class BillViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _formattedPaymentMethod;
        private static List<PaymentMethod>? _cachedPaymentMethods;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Id { get; set; }
        public string IdTable { get; set; }
        public DateTime DateCheckIn { get; set; }
        public DateTime? DateCheckOut { get; set; }
        public int Status { get; set; }
        public double TotalAmount { get; set; }
        public double Discount { get; set; }
        public double FinalAmount { get; set; }
        public int PaymentMethod { get; set; }

        public string FormattedDateCheckIn => DateCheckIn.ToString("dd/MM/yyyy HH:mm");
        public string FormattedDateCheckOut => DateCheckOut?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string FormattedTotalAmount => $"{TotalAmount:N0}đ";
        public string FormattedDiscount => $"{Discount:N0}đ";
        public string FormattedFinalAmount => $"{FinalAmount:N0}đ";
        public string FormattedStatus => GetStatusText(Status);
        public string FormattedPaymentMethod
        {
            get => _formattedPaymentMethod ?? "Loading...";
            private set
            {
                if (_formattedPaymentMethod != value)
                {
                    _formattedPaymentMethod = value;
                    OnPropertyChanged(nameof(FormattedPaymentMethod));
                }
            }
        }

        public BillViewModel(IGetAllBills_AllBills_Edges_Node bill)
        {
            Id = bill.Id.ToString();
            IdTable = bill.IdTable.ToString();
            DateCheckIn = DateTime.Parse(bill.DateCheckIn).ToLocalTime();
            DateCheckOut = string.IsNullOrEmpty(bill.DateCheckOut) ? null : DateTime.Parse(bill.DateCheckOut).ToLocalTime();
            Status = bill.Status;
            TotalAmount = bill.TotalAmount;
            Discount = bill.Discount ?? 0;
            FinalAmount = bill.FinalAmount;
            PaymentMethod = bill.PaymentMethod ?? 0;
            FormattedPaymentMethod = "Loading..."; // Set initial value
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Chưa thanh toán",
                1 => "Đã thanh toán",
                2 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public async Task InitializePaymentMethodTextAsync()
        {
            try
            {
                if (_cachedPaymentMethods == null)
                {
                    _cachedPaymentMethods = await PaymentMethodDAO.Instance.GetAllActivePaymentMethodsAsync();
                }

                var paymentMethod = _cachedPaymentMethods?.FirstOrDefault(p => p.Id == PaymentMethod);
                FormattedPaymentMethod = paymentMethod?.Name ?? "Không thanh toán";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payment method: {ex.Message}");
                FormattedPaymentMethod = "Khác";
            }
        }

        public void RefreshPaymentMethod()
        {
            _cachedPaymentMethods = null;
            _ = InitializePaymentMethodTextAsync();
        }

        public async Task<List<BillInfoViewModel>> GetBillDetails()
        {
            try
            {
                var result = await DataProvider.Instance.Client.GetBillInfoByBillId.ExecuteAsync(int.Parse(Id));
                if (result.Data?.AllBillInfos?.Edges != null)
                {
                    return result.Data.AllBillInfos.Edges
                        .Select(edge => edge.Node)
                        .GroupBy(bi => bi.IdProduct)
                        .Select(g => new BillInfoViewModel
                        {
                            ProductId = g.Key,
                            Count = g.Sum(bi => bi.Count),
                            ProductName = g.First().ProductByIdProduct?.Name ?? "Unknown Product",
                            UnitPrice = g.First().UnitPrice,
                            TotalPrice = g.Sum(bi => bi.TotalPrice)
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting bill details: {ex.Message}");
            }
            return new List<BillInfoViewModel>();
        }
    }

    public class BillInfoViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Count { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }
}