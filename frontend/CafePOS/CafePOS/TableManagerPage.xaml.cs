using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CafePOS.DAO;
using CafePOS.DTO;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CafePOS
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TableManagerPage : Page
    {
        public TableManagerPage()
        {
            this.InitializeComponent();
            LoadTable();
        }

        private async void LoadTable() {
            List<CafeTable> tableList = await CafeTableDAO.Instance.GetAllCafeTablesAsync();

            foreach (CafeTable table in tableList) {
                Button btn = new Button {
                    Width = CafeTableDAO.tableWidth,
                    Height = CafeTableDAO.tableHeight,
                    Margin = new Thickness(5),
                    Content = new TextBlock
                    {
                        Text = table.Name + "\n" + table.Status,
                        TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                        VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                    }
                };

                switch (table.Status) {
                    case "Trống":
                        btn.Background = new SolidColorBrush(Colors.Aqua);
                        break;
                    default:
                        btn.Background = new SolidColorBrush(Colors.LightPink);
                        break;
                }

                TableItemsControl.Items.Add(btn);
            }
        }
    }
}
