using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using PermDynamics.Model;
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
        private User _user;
        private User _virtualUser;
        private int _lastOperationId;

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
                var share = UserAssetsList.Assets.FirstOrDefault(x => x.Name == "Perm Dynamics");
                if (share == null)
                {
                    var shareNew = new ShareAssetViewModel(new ShareAsset
                    { Name = "Perm Dynamics", Count = _buySellShareViewModel.Count, BuyCost = CurrentPrice, CurrentPrice = CurrentPrice });
                    UserAssetsList.Assets.Add(shareNew);
                    AddOperation(_buySellShareViewModel.Count, true, _user.Id);
                    money.ChangeCost(-CurrentPrice * _buySellShareViewModel.Count);
                    OnPropertyChanged(nameof(UserAssetsList));
                }
                else if (share is ShareAssetViewModel permD)
                {
                    permD.BuyShares(_buySellShareViewModel.Count);
                    AddOperation(_buySellShareViewModel.Count, true, _user.Id);
                    money.ChangeCost(-CurrentPrice * _buySellShareViewModel.Count);

                    UserAssetsList.AssetsChanged();
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
                    permD.SellShares(_buySellShareViewModel.Count);
                    AddOperation(_buySellShareViewModel.Count, false, _user.Id);
                    if (permD.Count == 0)
                        UserAssetsList.Assets.Remove(permD);
                    money.ChangeCost(CurrentPrice * _buySellShareViewModel.Count);
                    UserAssetsList.AssetsChanged();
                }
            }
        });

        public DelegateCommand RestartCommand => new DelegateCommand(() =>
        {
            SharesModel.DeleteTables();
            SharesModel.CreateTables();
            _user = new User() { Id = 0, Name = "Пользователь", Money = 400000, ShareCount = 0 };
            _virtualUser = new User() { Id = 1, Name = "Виртуальный брокер", Money = 400000, ShareCount = 0 };
            SharesModel.AddUser(_user);
            SharesModel.AddUser(_virtualUser);
            UserAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = _user.Money }) }));
            if (_user.ShareCount > 0)
            {
                var newShare = new ShareAssetViewModel(new ShareAsset
                    { Name = "Perm Dynamics", Count = _user.ShareCount, BuyCost = 0, CurrentPrice = CurrentPrice });
                UserAssetsList.Assets.Add(newShare);
            }

            VirtualBrokerAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = _virtualUser.Money }) }));
            var shareNew = new ShareAssetViewModel(new ShareAsset
                { Name = "Perm Dynamics", Count = _virtualUser.ShareCount, BuyCost = 0, CurrentPrice = CurrentPrice });
            VirtualBrokerAssetsList.Assets.Add(shareNew);
        });

        public DelegateCommand ArchiveCommand => new DelegateCommand(() =>
        {
            var archiveWindow = new ArchiveWindow();
            archiveWindow.ShowDialog();
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
                AddOperation(count, true, _virtualUser.Id);
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
                AddOperation(count, false, _virtualUser.Id);
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

        private void AddOperation(ulong count, bool isBuying, int userId)
        {
            SharesModel.AddOperation(new Operation()
            { Count = count, IsBuying = isBuying, Price = CurrentPrice, UserId = userId, Id = ++_lastOperationId });
            if (userId == _user.Id)
            {
                if (isBuying)
                {
                    _user.Money -= count * CurrentPrice;
                    _user.ShareCount += count;
                }
                else
                {
                    _user.Money += count * CurrentPrice;
                    _user.ShareCount -= count;
                }

                SharesModel.UpdateUser(_user);
            }
            else if (userId == _virtualUser.Id)
            {
                if (isBuying)
                {
                    _virtualUser.Money -= count * CurrentPrice;
                    _virtualUser.ShareCount += count;
                }
                else
                {
                    _virtualUser.Money += count * CurrentPrice;
                    _virtualUser.ShareCount -= count;
                }
                SharesModel.UpdateUser(_virtualUser);
            }
        }

        public MainWindowViewModel()
        {
            var users = SharesModel.GetUsers();
            _user = users.First(x => x.Name == "Пользователь");
            _virtualUser = users.First(x => x.Name == "Виртуальный брокер");
            var operations = SharesModel.GetOperations();
            _lastOperationId = operations.Count == 0 ? 0 : operations.Last().Id;
            var userBuyCost = _user.ShareCount == 0 ? 0 : operations.Sum(x => x.IsBuying && x.UserId == _user.Id ? x.Count * x.Price : x.UserId == _user.Id ? x.Count * -x.Price : 0) / _user.ShareCount;
            var virtualUserBuyCost = _virtualUser.ShareCount == 0 ? 0 : operations.Sum(x => x.IsBuying && x.UserId == _virtualUser.Id ? x.Count * x.Price : x.UserId == _virtualUser.Id ? x.Count * -x.Price : 0) / _virtualUser.ShareCount;

            UserAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = _user.Money }) }));
            if (_user.ShareCount > 0)
            {
                var newShare = new ShareAssetViewModel(new ShareAsset
                { Name = "Perm Dynamics", Count = _user.ShareCount, BuyCost = userBuyCost, CurrentPrice = CurrentPrice });
                UserAssetsList.Assets.Add(newShare);
            }

            VirtualBrokerAssetsList = new AssetsListViewModel(new List<AssetViewModel>(
                new[] { new AssetViewModel(new Asset { Name = "Денежные средства", Cost = _virtualUser.Money }) }));
            var shareNew = new ShareAssetViewModel(new ShareAsset
            { Name = "Perm Dynamics", Count = _virtualUser.ShareCount, BuyCost = virtualUserBuyCost, CurrentPrice = CurrentPrice });
            VirtualBrokerAssetsList.Assets.Add(shareNew);
        }
    }
}