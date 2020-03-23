using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PermDynamics.View.Model;
using WPFMVVMLib;

namespace PermDynamics.View.ViewModels
{
    public class AssetsListViewModel : ViewModelBase
    {
        private ObservableCollection<AssetViewModel> _assets;

        public decimal TotalValue => Assets.Sum(x => x.Cost);

        public decimal AbsoluteChange => Assets.Sum(x => x.GetType() == typeof(AssetViewModel) ? 0 : ((ShareAssetViewModel)x).AbsoluteChange);

        public double RelativeChange
        {
            get
            {
                var sum = Assets.Sum(x => x.GetType() == typeof(AssetViewModel) ? x.Cost : ((ShareAssetViewModel)x).BuyCost * ((ShareAssetViewModel)x).Count);
                return sum == 0 ? 0 : Math.Round((double)(AbsoluteChange / sum), 2);
            }
        }

        public ObservableCollection<AssetViewModel> Assets
        {
            get => _assets;
            set
            {
                _assets = value;
                OnPropertyChanged();
            }
        }

        public AssetsListViewModel(IList<AssetViewModel> assets)
        {
            Assets = new ObservableCollection<AssetViewModel>(assets);
        }
    }
}