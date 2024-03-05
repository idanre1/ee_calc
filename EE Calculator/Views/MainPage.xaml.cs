using System;
using EE_Calculator.MathEngine;

using EE_Calculator.ViewModels;
using Windows.UI.Xaml.Controls;

namespace EE_Calculator.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        private PageEngine pageEngine;
        private bool _isPor = true;
        public MainPage()
        {
            InitializeComponent();
            MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, "1+2\nx=e0\ny=x+1\ny");

            pageEngine = new PageEngine();
        }

        private void MathInputChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RichEditBox)
            {
                RichEditBox richEditBox = (RichEditBox)sender;
                string value;
                richEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);

                // Understand the expressions
                var (dbl, hx, bn, answers) = pageEngine.Calc(value);
                
                // Print the text in the RichEditBox to the console
                DoubleOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None,dbl);
                HexOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None,hx);
                BinOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None,bn);
                AnswersOutput.Text = answers;
            }
        }

        private void MathInputFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_isPor)
            {
                MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                _isPor = false;
            }
        }
    }
}
