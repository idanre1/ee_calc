using EE_Calculator.MathEngine;
using System;
using System.Text;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EE_Calculator.Controls
{
    public sealed partial class CalculatorControl : UserControl
    {
        private PageEngine pageEngine;
        private bool _isPor = false;

        // Public constant used for the initial welcome/example text shown on new tabs
        public const string InitialWelcomeText = "1+2\nx=e0\nx\ny=x+1\ny\n\nShift Left:\n7@<<2\nBitwiseOr:\nb.100 @| b.001\n\n\nNatural Language calc engine:\nhttps://mathparser.org";

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
                MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, InitialWelcomeText);
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
                SetTextWithBoldUnderscores(HexOutput, hx);
                SetTextWithBoldUnderscores(BinOutput, bn);
                AnswersOutput.Text = answers;
            }
        }

        private static void SetTextWithBoldUnderscores(RichEditBox box, string text)
        {
            if (box == null)
            {
                return;
            }

            if (text == null)
            {
                text = string.Empty;
            }
            box.Document.SetText(Windows.UI.Text.TextSetOptions.None, text);

            // Use the document's normalized text (CRLF handling etc.) to match range indexing.
            string docText;
            box.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out docText);
            if (docText == null)
            {
                docText = string.Empty;
            }

            // Apply formatting per line, counting underscores from the right (least-significant group).
            // This avoids underscores on earlier lines affecting the "from the right" counting on later lines.
            var lineStart = 0;
            while (lineStart <= docText.Length)
            {
                var lineEnd = docText.IndexOf('\n', lineStart);
                if (lineEnd < 0)
                {
                    lineEnd = docText.Length;
                }

                var underscoreFromRight = 0;
                for (var i = lineEnd - 1; i >= lineStart; i--)
                {
                    if (docText[i] != '_')
                    {
                        continue;
                    }

                    underscoreFromRight++;
                    try
                    {
                        var range = box.Document.GetRange(i, i + 1);
                        range.CharacterFormat.Bold = FormatEffect.On;
                        range.CharacterFormat.Underline = UnderlineType.Single;

                        // Make every 1st, 3rd, 5th... underscore from the right half font size (1,3,5,...).
                        // (This inverts the previous behavior so the first separator nearest the lowest bits is small.)
                        if (underscoreFromRight % 2 == 1)
                        {
                            var baseSize = range.CharacterFormat.Size;
                            range.CharacterFormat.Size = (float)Math.Max(1.0, baseSize / 2.0);
                        }
                    }
                    catch
                    {
                        // Ignore formatting failures (e.g. if the document's indexing differs on some platforms).
                        break;
                    }
                }

                if (lineEnd >= docText.Length)
                {
                    break;
                }

                lineStart = lineEnd + 1;
            }
        }

        public string GetInputText()
        {
            string value;
            MathInput.Document.GetText(Windows.UI.Text.TextGetOptions.AdjustCrlf, out value);
            return value;
        }

        public void SetInputText(string text)
        {
            MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, text ?? string.Empty);
            // Trigger recalculation after setting text
            MathInputChanged(MathInput, null);
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
