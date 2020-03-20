using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using zCarape.Core;

namespace Jezgro.Views
{
    /// <summary>
    /// Interaction logic for Slika.xaml
    /// </summary>
    public partial class Slika : Window
    {
        private string _slikaSaPunomPutanjom;


        public Slika(string slika, string opis="", string path="" )
        {
            InitializeComponent();
            _slikaSaPunomPutanjom = System.IO.Path.Combine(GlobalniKod.SlikeDir, slika);
            OpisTextBlock.Text = opis;
            Loaded += Slika_Loaded;
        }

        private void Slika_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(_slikaSaPunomPutanjom))
            {
                MessageBox.Show($"Ne postoji fajl {_slikaSaPunomPutanjom}");
                this.Close();
            }
            else
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(_slikaSaPunomPutanjom);
                bitmapImage.EndInit();

                Image.Source = bitmapImage;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
