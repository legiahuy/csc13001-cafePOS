﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CafePOS.GraphQL;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace CafePOS.DTO
{
    public class CafeTable
    {
        public CafeTable(int id, string name, string status) {
            this.Name = name;
            this.Status = status;   
            this.Id = id;
        }

        public CafeTable(IGetAllCafeTables_AllCafeTables_Edges_Node node)
        {
            this.Id = node.Id;
            this.Name = node.Name;
            this.Status = node.Status;
        }

        private string _name;
        private int _id;
        private string _status;

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public SolidColorBrush BackgroundColor =>
            Status.Equals("Trống") ? new SolidColorBrush(Colors.Aqua) : new SolidColorBrush(Colors.LightPink);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
