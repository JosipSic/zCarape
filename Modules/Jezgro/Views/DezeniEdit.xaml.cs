using System.Windows;
using System.Windows.Controls;
using zCarape.Core;

namespace Jezgro.Views
{
    /// <summary>
    /// Interaction logic for DezeniEdit
    /// </summary>
    public partial class DezeniEdit : Window
    {
        public DezeniEdit()
        {
            InitializeComponent();
            Loaded += DezeniEdit_Loaded;
        }

        private void DezeniEdit_Loaded(object sender, RoutedEventArgs e)
        {
            NazivTextBox.Focus();
        }
    }
}
