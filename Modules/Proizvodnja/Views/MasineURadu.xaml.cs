using System.Windows.Controls;
using System.Windows.Input;

namespace Proizvodnja.Views
{
    /// <summary>
    /// Interaction logic for MasineURadu
    /// </summary>
    public partial class MasineURadu : UserControl
    {
        public MasineURadu()
        {
            InitializeComponent();
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
