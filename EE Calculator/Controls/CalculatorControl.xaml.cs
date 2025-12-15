using EE_Calculator.MathEngine;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EE_Calculator.Controls
{
    public sealed partial class CalculatorControl : UserControl
    {
        private PageEngine pageEngine;
        private bool _isPor = false;

        public CalculatorControl() : this(false)
        {
        }

        public CalculatorControl(bool showExampleText)
        {
            InitializeComponent();
            pageEngine = new PageEngine();
            
            if (showExampleText)
            {
                _isPor = true;
                MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, "1+2\nx=e0\nx\ny=x+1\ny\n\nShift Left:\n7@<<2\nBitwiseOr:\nb.100 @| b.001\n\n\nNatural Language calc engine:\nhttps://mathparser.org");
            }
        }

        private void MathInputChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RichEditBox richEditBox)
            {
                string value;
                richEditBox.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);

                // Understand the expressions
                var (dbl, hx, bn, answers) = pageEngine.Calc(value);

                // Print the text in the RichEditBox to the console
                DoubleOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None, dbl);
                HexOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None, hx);
                BinOutput.Document.SetText(Windows.UI.Text.TextSetOptions.None, bn);
                AnswersOutput.Text = answers;
            }
        }

        private void MathInputFocus(object sender, RoutedEventArgs e)
        {
            if (_isPor)
            {
                MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                _isPor = false;
            }
        }
    }
}
