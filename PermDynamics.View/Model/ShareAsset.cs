namespace PermDynamics.View.Model
{
    public class ShareAsset : Asset
    {
        public decimal BuyCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public ulong Count { get; set; }
        public new decimal Cost => CurrentPrice * Count;
    }
}