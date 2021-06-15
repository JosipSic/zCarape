using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
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
            PrintHelper.ShowPrintPreview(this, link);
        }
    }
}
