using System.Windows.Controls;
using System.Windows.Input;

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
            Loaded += Masine_Loaded;
        }

        private void Masine_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            NazivTextBox.Focus();
        }

        private void ObavezanTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(((TextBox)sender).Text))
            {
                ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

    }
}
