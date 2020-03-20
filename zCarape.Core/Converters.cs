using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace zCarape.Core
{
    public class SlikaToPunaPutanjaKonverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) 
                return string.Empty;

            string slika = (string)value;
            return Path.Combine(GlobalniKod.SlikeDir, slika);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string punaPutanja = (string)value;
            return Path.GetFileName(punaPutanja);
        }
    }
}
