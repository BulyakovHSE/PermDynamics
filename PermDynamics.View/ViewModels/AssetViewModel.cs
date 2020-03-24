using System;
using System.Transactions;
using System.Windows.Forms;
using PermDynamics.View.Model;
using WPFMVVMLib;

namespace PermDynamics.View.ViewModels
{
    public class AssetViewModel : ViewModelBase
    {
        protected Asset Asset;

        public string Name => Asset.Name;

        public decimal Cost => Math.Round(Asset.Cost, 2);

        public AssetViewModel(Asset asset)
        {
            Asset = asset;
        }

        public bool ChangeCost(decimal amount)
        {
            if (amount < 0 && Math.Abs(amount) > Cost) return false;
            Asset.Cost += amount;
            OnPropertyChanged(nameof(Cost));
            return true;
        }
    }

    public class ShareAssetViewModel : AssetViewModel, IEnlistmentNotification
    {
        private ShareAsset _current;
        private ShareAsset _original;
        private bool _enlisted;

        protected new ShareAsset Asset
        {
            get => _current;
            set
            {
                if (!Enlist())
                    _original = value;
                _current = value;

            }
        }

        public decimal BuyCost => Math.Round(Asset.BuyCost, 2);

        public decimal CurrentPrice
        {
            get => Math.Round(Asset.CurrentPrice, 2);
            set
            {
                Asset.CurrentPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AbsoluteChange));
                OnPropertyChanged(nameof(RelativeChange));
                OnPropertyChanged(nameof(Cost));
            }
        }

        public ulong Count => Asset.Count;

        public decimal AbsoluteChange => Math.Round((CurrentPrice - BuyCost) * Count, 2);

        public double RelativeChange => BuyCost == 0 || Count == 0 ? 0 : Math.Round((double)(AbsoluteChange / (BuyCost * Count)) * 100, 2);

        public new decimal Cost => Math.Round(Asset.Cost, 2);

        public ShareAssetViewModel(ShareAsset asset) : base(asset)
        {
            Asset = asset;
        }

        public void BuyShares(ulong count)
        {
            Asset.BuyCost = (Asset.BuyCost * Asset.Count + Asset.CurrentPrice * count) / (Asset.Count + count);
            Asset.Count += count;
            OnPropertyChanged(nameof(BuyCost));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(AbsoluteChange));
            OnPropertyChanged(nameof(RelativeChange));
            OnPropertyChanged(nameof(Cost));
        }

        public void SellShares(ulong count)
        {
            if (Asset.Count < count) return;
            Asset.Count -= count;
            OnPropertyChanged(nameof(BuyCost));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(AbsoluteChange));
            OnPropertyChanged(nameof(RelativeChange));
            OnPropertyChanged(nameof(Cost));
        }

        private bool Enlist()
        {
            if (_enlisted)
                return true;
            var currentTx = System.Transactions.Transaction.Current;
            if (currentTx == null)
                return false;
            currentTx.EnlistVolatile(this, EnlistmentOptions.None);
            _enlisted = true;
            return true;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            _original = _current;
            _enlisted = false;
        }

        public void Rollback(Enlistment enlistment)
        {
            _current = _original;
            _enlisted = false;
        }

        public void InDoubt(Enlistment enlistment)
        {
            _enlisted = false;
        }
    }
}