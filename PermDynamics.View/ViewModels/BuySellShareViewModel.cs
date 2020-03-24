using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using WPFMVVMLib;

namespace PermDynamics.View.ViewModels
{
    public class BuySellShareViewModel : ViewModelBase, IDataErrorInfo
    {
        private decimal _currentPrice;
        private ulong _count;
        
        public decimal CurrentPrice
        {
            get => Math.Round(_currentPrice, 2);
            set
            {
                _currentPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sum));
                OnPropertyChanged(nameof(IsOk));
            }
        }

        public ulong Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Sum));
                OnPropertyChanged(nameof(IsOk));
            }
        }

        public decimal Sum => Math.Round(CurrentPrice * Count);

        public decimal AvailableSum { get; set; }

        public ulong AvailableCount { get; set; }

        public bool IsBuyAction { get; set; }

        public bool IsSellAction => !IsBuyAction;

        public string OkBtnText => IsBuyAction ? "Купить" : "Продать";

        public bool IsOk => IsBuyAction ? Sum <= AvailableSum : Count <= AvailableCount;

        public ReadOnlyObservableCollection<ValidationError> Errors { get; set; }

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;
                switch (columnName)
                {
                    case "Count":
                    {
                        if (Count > AvailableCount && IsSellAction) error = "kf";
                    }break;
                    case "Sum":
                    {
                        if (IsBuyAction && Sum > AvailableSum) error = "ds";
                    }break;
                }
                return error;
            }
        }

        public string Error => throw new NotImplementedException();

        public BuySellShareViewModel()
        {
        }
    }
}