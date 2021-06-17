using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;

namespace Proizvodnja.Views
{
    /// <summary>
    /// Interaction logic for Izvestaji
    /// </summary>
    public partial class Izvestaji : UserControl
    {
        public Izvestaji()
        {
            InitializeComponent();
        }

        private void PrintButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var link = new PrintableControlLink(IzvestajTableView, "Izvestaj");
            link.PaperKind = System.Drawing.Printing.PaperKind.A4;
            link.Landscape = true;
            link.Margins = new System.Drawing.Printing.Margins(30, 30, 30, 30);
            link.PageHeaderTemplate = (DataTemplate)Resources["PrintPageHeader"];
            link.PageHeaderData = cboPeriod.SelectedIndex == 0 ? "Svi datumi" : "Za period od " + DatumOdDateEdit.EditText + " do " + DatumDoDateEdit.EditText;
            PrintHelper.ShowRibbonPrintPreview(this, link);
        }
    }
}
