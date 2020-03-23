using System;
using System.Windows.Forms;
using PermDynamics.View.Model;
using WPFMVVMLib;

namespace PermDynamics.View.ViewModels
{
    public class AssetViewModel : ViewModelBase
    {
        protected Asset Asset;

        public string Name => Asset.Name;

        public decimal Cost => Asset.Cost;

        public AssetViewModel(Asset asset)
        {
            Asset = asset;
        }
    }

    public class ShareAssetViewModel : AssetViewModel
    {
        protected new ShareAsset Asset;

        public decimal BuyCost => Asset.BuyCost;

        public decimal CurrentPrice => Asset.CurrentPrice;

        public ulong Count => Asset.Count;

        public decimal AbsoluteChange => CurrentPrice - BuyCost;

        public double RelativeChange => BuyCost == 0 || Count == 0 ? 0 : Math.Round((double)(AbsoluteChange / BuyCost * Count), 2);

        public ShareAssetViewModel(ShareAsset asset) : base(asset)
        {
            Asset = asset;
        }
    }
}