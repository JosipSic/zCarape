using System.Windows.Controls;
using System.Windows.Input;

namespace Jezgro.Views
{
    /// <summary>
    /// Interaction logic for ArtikliEdit
    /// </summary>
    public partial class ArtikliEdit : UserControl
    {
        public ArtikliEdit()
        {
            InitializeComponent();
            this.Loaded += ArtikliEdit_Loaded;
        }

        private void ArtikliEdit_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SifraTextBox.Focus();
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
