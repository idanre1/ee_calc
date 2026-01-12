using EE_Calculator.MathEngine;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;

namespace EE_Calculator.Controls
{
    public sealed partial class CalculatorControl : UserControl
    {
        private PageEngine pageEngine;
        private bool _isPor = false;

        private bool _isResizing;
        private Rectangle _activeSplitter;
        private double _startX;
        private double _startLeftWidth;
        private double _startRightWidth;

        private CoreCursor _previousCursor;
        private bool _isPointerOverSplitter;

        // Public constant used for the initial welcome/example text shown on new tabs
        public const string InitialWelcomeText = "1+2\nx=e0\nx\ny=x+1\ny\n\nShift Left:\n7@<<2\nBitwiseOr:\nb.100 @| b.001\n\n\nNatural Language calc engine:\nhttps://mathparser.org";

        public CalculatorControl() : this(false)
        {
        }

        public CalculatorControl(bool showExampleText)
        {
            InitializeComponent();
            pageEngine = new PageEngine();

            Unloaded += CalculatorControl_Unloaded;
            
            if (showExampleText)
            {
                _isPor = true;
                MathInput.Document.SetText(Windows.UI.Text.TextSetOptions.None, InitialWelcomeText);
            }
        }

        private void CalculatorControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _activeSplitter = null;
            _isResizing = false;
            _isPointerOverSplitter = false;
            RestoreCursor();
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

        private void Splitter_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Rectangle splitter)
            {
                _activeSplitter = splitter;
                _isResizing = true;

                var p = e.GetCurrentPoint(ContentArea);
                _startX = p.Position.X;

                if (ReferenceEquals(_activeSplitter, ResultHexSplitter))
                {
                    _startLeftWidth = ResultColumn.ActualWidth;
                    _startRightWidth = HexColumn.ActualWidth;
                }
                else if (ReferenceEquals(_activeSplitter, HexBinSplitter))
                {
                    _startLeftWidth = HexColumn.ActualWidth;
                    _startRightWidth = BinColumn.ActualWidth;
                }

                splitter.CapturePointer(e.Pointer);
                SetResizeCursor();
                e.Handled = true;
            }
        }

        private void Splitter_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _isPointerOverSplitter = true;
            SetResizeCursor();
        }

        private void Splitter_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isPointerOverSplitter = false;
            if (!_isResizing)
            {
                RestoreCursor();
            }
        }

        private void Splitter_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _activeSplitter = null;
            _isResizing = false;
            RestoreCursor();
        }

        private void Splitter_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isResizing || _activeSplitter == null)
            {
                return;
            }

            var p = e.GetCurrentPoint(ContentArea);
            var dx = p.Position.X - _startX;

            if (ReferenceEquals(_activeSplitter, ResultHexSplitter))
            {
                ResizeColumns(ResultColumn, HexColumn, dx);
            }
            else if (ReferenceEquals(_activeSplitter, HexBinSplitter))
            {
                ResizeColumns(HexColumn, BinColumn, dx);
            }

            e.Handled = true;
        }

        private void Splitter_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_activeSplitter != null)
            {
                _activeSplitter.ReleasePointerCapture(e.Pointer);
            }

            _activeSplitter = null;
            _isResizing = false;
            RestoreCursor();
            e.Handled = true;
        }

        private void SetResizeCursor()
        {
            var coreWindow = Window.Current?.CoreWindow;
            if (coreWindow == null)
            {
                return;
            }

            if (_previousCursor == null)
            {
                _previousCursor = coreWindow.PointerCursor;
            }

            coreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
        }

        private void RestoreCursor()
        {
            var coreWindow = Window.Current?.CoreWindow;
            if (coreWindow == null)
            {
                return;
            }

            // PointerCursor can be null (system default). Setting it back to null can
            // result in the pointer appearing to disappear in some navigation/capture
            // edge cases. Prefer restoring to Arrow when we don't have a concrete cursor.
            coreWindow.PointerCursor = _previousCursor ?? new CoreCursor(CoreCursorType.Arrow, 0);
            _previousCursor = null;
        }

        private void ResizeColumns(ColumnDefinition left, ColumnDefinition right, double dx)
        {
            var minLeft = left.MinWidth;
            var minRight = right.MinWidth;

            var newLeft = _startLeftWidth + dx;
            var newRight = _startRightWidth - dx;

            if (newLeft < minLeft)
            {
                newRight -= (minLeft - newLeft);
                newLeft = minLeft;
            }

            if (newRight < minRight)
            {
                newLeft -= (minRight - newRight);
                newRight = minRight;
            }

            if (newLeft < minLeft || newRight < minRight)
            {
                return;
            }

            left.Width = new GridLength(newLeft, GridUnitType.Pixel);
            right.Width = new GridLength(newRight, GridUnitType.Pixel);
        }
    }
}
