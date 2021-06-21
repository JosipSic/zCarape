using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;

namespace Proizvodnja.Views
{
    /// <summary>
    /// Interaction logic for Radnici
    /// </summary>
    public partial class Lica : UserControl
    {
        public Lica()
        {
            InitializeComponent();
        }

        private void PrintButton_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var link = new PrintableControlLink(view, "Radnici") { ReportHeaderTemplate = Resources["reportHeader"] as DataTemplate };

            PrintHelper.ShowRibbonPrintPreview(this, link);
        }
    }
}
