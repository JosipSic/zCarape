using DevExpress.Xpf.Printing;
using Proizvodnja.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Proizvodnja.Views
{
    public partial class PregledRnDijalog : UserControl
    {
        public PregledRnDijalog()
        {
            InitializeComponent();
        }

        private void PrintButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var link = new PrintableControlLink(this.TableView, "RadniNalog");
            link.PaperKind = System.Drawing.Printing.PaperKind.A4;
            link.Margins = new System.Drawing.Printing.Margins(30, 30, 30, 30);
            link.PageHeaderTemplate = (DataTemplate)Resources["PrintPageHeader"];
            link.PageHeaderData = (this.DataContext as PregledRnDijalogViewModel).Title;
            PrintHelper.ShowRibbonPrintPreview(this, link);
        }
    }
}
