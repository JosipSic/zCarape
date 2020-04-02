using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace zCarape.Core
{
    public class NumericTextBox: TextBox
    {
        #region Properties

        /// <summary>
        /// Gets or sets character to be used as decimal separator
        /// </summary>
        public string DecimalSeparator { get; set; }


        /// <summary>
        /// Dependency property to store the decimal is allowed to be entered in the textbox
        /// </summary>
        public bool IsDecimalAllowed
        {
            get { return (bool)GetValue(IsDecimalAllowedProperty); }
            set { SetValue(IsDecimalAllowedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDecimalAllowedProperty =
            DependencyProperty.Register("IsDecimalAllowed", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets mask to apply to textbox
        /// </summary>
        public int Scale
        {
            get { return (int)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Scale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(int), typeof(NumericTextBox), new PropertyMetadata(0));


        /// <summary>
        /// Static Constructor
        /// </summary>
        static NumericTextBox()
        {

        }

        /// <summary>
        /// To check the character entered
        /// </summary>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
            if (!e.Handled)
            {
                e.Handled = !MaxLengthReached(e);
            }
            base.OnPreviewTextInput(e); 
        }

        /// <summary>
        /// This method was added to prevent arithmetic overflows while saving in db on decimal part.
        /// </summary>
        bool MaxLengthReached(TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)e.OriginalSource;
            int precision = textBox.MaxLength - Scale - 2;

            string textToValidate = textBox.Text.Insert(textBox.CaretIndex, e.Text).Replace("-", "");
            string[] numericValues = textToValidate.Split(Convert.ToChar(DecimalSeparator));

            if ((numericValues.Length <= 2) && (numericValues[0].Length <= precision) && ((numericValues.Length == 1) || (numericValues[1].Length <= Scale)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AreAllValidNumericChars(string str)
        {
            if (string.IsNullOrEmpty(DecimalSeparator))
            {
                DecimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            }

            bool ret = true;
            if (str==System.Globalization.NumberFormatInfo.CurrentInfo.NegativeSign || 
                str==System.Globalization.NumberFormatInfo.CurrentInfo.PositiveSign)
            {
                return ret;
            }

            if (IsDecimalAllowed && str==DecimalSeparator)
            {
                return ret;
            }

            int I = str.Length;
            for (int i = 0; i < I; i++)
            {
                char ch = str[i];
                ret &= Char.IsDigit(ch);
            }

            return ret;
        }

        #endregion
    }
}
