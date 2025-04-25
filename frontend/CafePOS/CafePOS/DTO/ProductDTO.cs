using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CafePOS.DTO
{
    public class ProductDTO : INotifyPropertyChanged
    {
        private string _formattedPrice;
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public string FormattedPrice
        {
            get => _formattedPrice;
            set
            {
                if (_formattedPrice != value)
                {
                    _formattedPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 