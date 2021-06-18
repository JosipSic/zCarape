﻿using System.Windows.Controls;

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
    }
}
