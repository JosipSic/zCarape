using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;

namespace Proizvodnja.Views
{
    /// <summary>
    /// Interaction logic for Masine
    /// </summary>
    public partial class Masine : UserControl
    {
        public Masine()
        {
            InitializeComponent();
        }

        private void PrintButton_ItemClick(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            var link = new PrintableControlLink(view, "Cenovnik") { ReportHeaderTemplate = Resources["reportHeader"] as DataTemplate };

            PrintHelper.ShowRibbonPrintPreview(this, link);
        }
    }
}
