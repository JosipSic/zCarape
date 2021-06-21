using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Printing;
using System.Drawing;
using System.Windows.Controls;

namespace Proizvodnja.Views
{
    /// <summary>
    /// Interaction logic for Pivot
    /// </summary>
    public partial class Pivot : UserControl
    {
        public Pivot()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            PivotGrid.BestFit();
        }

        private void PrintButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string DocumentName = "Pivot izvestaj";
            //string Title = "Pivot tabela";

            //PivotGrid.ShowPrintPreview(this, DocumentName, Title);


            var link = new PrintableControlLink(PivotGrid, DocumentName);
            link.PaperKind = System.Drawing.Printing.PaperKind.A4;
            link.Margins = new System.Drawing.Printing.Margins(45, 35, 30, 30);
            link.Landscape = true;
            PrintHelper.ShowRibbonPrintPreview(this, link);

        }

        private void ChartControl_CustomDrawCrosshair(object sender, CustomDrawCrosshairEventArgs e)
        {
            foreach (CrosshairElementGroup g in e.CrosshairElementGroups)
            {
                foreach (CrosshairElement el in g.CrosshairElements)
                {
                    el.Visible = !double.IsNaN(el.SeriesPoint.Value) && el.SeriesPoint.Value != 0;
                }
            }
        }
    }
}
