using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using PermDynamics.View.Model;
using WPFMVVMLib;
using WPFMVVMLib.Commands;

namespace PermDynamics.View.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private AssetsListViewModel _userAssetsList;
        private AssetsListViewModel _virtualBrokerAssetsList;
        private decimal _currentPrice;
        private BuySellShareViewModel _buySellShareViewModel;

        private decimal _lastPrice;
        private bool _lastWasUp;

        public AssetsListViewModel UserAssetsList
        {
            get => _userAssetsList;
            set
            {
                _userAssetsList = value;
                OnPropertyChanged();
            }
        }

        public AssetsListViewModel VirtualBrokerAssetsList
        {
            get => _virtualBrokerAssetsList;
            set
            {
                _virtualBrokerAssetsList = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentPrice
        {
            get => Math.Round(_currentPrice, 5);
            set
            {
                _currentPrice = value;
                var share = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
                if (share is ShareAssetViewModel vm)
                {
                    vm.CurrentPrice = value;
                    UserAssetsList.AssetsChanged();
                }

                share = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
                if (share is ShareAssetViewModel vm2)
                {
                    vm2.CurrentPrice = value;
                    VirtualBrokerAssetsList.AssetsChanged();
                }

                if (_buySellShareViewModel != null) _buySellShareViewModel.CurrentPrice = value;
                VirtualBroker_PriceChanged();
                OnPropertyChanged();
            }
        }

        public DelegateCommand BuyShareCommand => new DelegateCommand(() =>
        {
            var money = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Денежные средства");
            if (money == null) return;

            var window = new BuySellShareWindow();
            _buySellShareViewModel = new BuySellShareViewModel { AvailableSum = money.Cost, IsBuyAction = true, CurrentPrice = CurrentPrice };
            window.DataContext = _buySellShareViewModel;
            var result = window.ShowDialog();
            if (result.HasValue && result.Value)
            {
                using (var transaction = new TransactionScope())
                {
                    var share = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
                    if (share == null)
                    {
                        var shareNew = new ShareAssetViewModel(new ShareAsset
                        { Name = "Perm Dynamics", Count = _buySellShareViewModel.Count, BuyCost = CurrentPrice, CurrentPrice = CurrentPrice });
                        UserAssetsList.Assets.Add(shareNew);
                        if (money.ChangeCost(-CurrentPrice * _buySellShareViewModel.Count))
                            transaction.Complete();
                        OnPropertyChanged(nameof(UserAssetsList));
                    }
                    else if (share is ShareAssetViewModel permD)
                    {
                        permD.BuyShares(_buySellShareViewModel.Count);
                        if (money.ChangeCost(-CurrentPrice * _buySellShareViewModel.Count))
                            transaction.Complete();

                        UserAssetsList.AssetsChanged();
                    }
                }
            }

            _buySellShareViewModel = null;
        });

        public DelegateCommand SellShareCommand => new DelegateCommand(() =>
        {
            var money = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Денежные средства");
            if (money == null) return;

            var share = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
            if (share == null) return;
            if (share is ShareAssetViewModel permD)
            {
                var window = new BuySellShareWindow();
                _buySellShareViewModel = new BuySellShareViewModel { AvailableCount = permD.Count, IsBuyAction = false, CurrentPrice = CurrentPrice };
                window.DataContext = _buySellShareViewModel;
                var result = window.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    using (var transaction = new TransactionScope())
                    {
                        permD.SellShares(_buySellShareViewModel.Count);
                        if (permD.Count == 0)
                            UserAssetsList.Assets.Remove(permD);
                        if (money.ChangeCost(CurrentPrice * _buySellShareViewModel.Count))
                            transaction.Complete();
                        UserAssetsList.AssetsChanged();
                    }
                }
            }
        });

        private void VirtualBrokerBuyShares(ulong count)
        {
            var money = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Денежные средства");
            if (money == null) return;
            var share = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
            if (share == null)
                return;
            if (share is ShareAssetViewModel permD)
            {
                permD.BuyShares(count);
                money.ChangeCost(-CurrentPrice * count);
                VirtualBrokerAssetsList.AssetsChanged();
            }
        }

        private void VirtualBrokerSellShares(ulong count)
        {
            var money = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Денежные средства");
            if (money == null) return;
            var share = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
            if (share is ShareAssetViewModel permD)
            {
                if (count < permD.Count) return;
                permD.SellShares(count);
                money.ChangeCost(CurrentPrice * count);
                VirtualBrokerAssetsList.AssetsChanged();
            }
        }

        private void VirtualBroker_PriceChanged()
        {
            var money = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Денежные средства");
            if (money == null) return;
            var share = VirtualBrokerAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");

            if (share is ShareAssetViewModel permD && _lastWasUp && CurrentPrice > _lastPrice && permD.AbsoluteChange > 0)
            {
                VirtualBrokerSellShares(permD.Count);
            }
            else if (!_lastWasUp && CurrentPrice < _lastPrice)
            {
                var count = (ulong)(money.Cost / CurrentPrice / 4);
                if (count != 0) VirtualBrokerBuyShares(count);
            }

            _lastWasUp = CurrentPrice > _lastPrice;
            _lastPrice = CurrentPrice;
        }

        public MainWindowViewModel()
        {
            UserAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = 400000 }) }));
            VirtualBrokerAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = 400000 }) }));
            var shareNew = new ShareAssetViewModel(new ShareAsset
            { Name = "Perm Dynamics", Count = 0, BuyCost = 0, CurrentPrice = CurrentPrice });
            VirtualBrokerAssetsList.Assets.Add(shareNew);
        }
    }
}