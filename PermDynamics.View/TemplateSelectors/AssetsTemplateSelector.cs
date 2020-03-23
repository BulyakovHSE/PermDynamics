using System.Windows;
using System.Windows.Controls;
using PermDynamics.View.ViewModels;

namespace PermDynamics.View.TemplateSelectors
{
    public class AssetsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AssetTemplate { get; set; }

        public DataTemplate ShareAssetTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item?.GetType() == typeof(ShareAssetViewModel)) return ShareAssetTemplate;
            return AssetTemplate;
        }
    }
}