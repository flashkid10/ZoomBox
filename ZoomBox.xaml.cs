using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AniLyst_5._0.CustomControls
{
    /// <summary>
    /// Interaction logic for ZoomBox.xaml
    /// </summary>
    public partial class ZoomBox : UserControl
    {
        // Taken / Based on the control described on " https://www.codeproject.com/Articles/97871/WPF-simple-zoom-and-drag-support-in-a-ScrollViewer "
        private Point? lastCenterPositionOnTarget;

        private Point? lastMousePositionOnTarget;
        private Point? lastDragPoint;

        public ZoomBox()
        {
            InitializeComponent();
            scrollViewer.MouseUp += ScrollViewer_MouseUp;
            scrollViewer.PreviewMouseUp += ScrollViewer_PreviewMouseUp;
            scrollViewer.PreviewMouseDown += ScrollViewer_PreviewMouseDown;
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.MouseMove += OnMouseMove;
            slider.ValueChanged += OnSliderValueChanged;
            slider.ValueChanged += Slider_ValueChanged;
            SliderGridVis(false);
        }

        new public object Content { get { return CP.Content; } set { CP.Content = value; } }

        #region Slider Grid Visibility

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => SliderGrid.VisibilityCol(slider.Value != 1);

        private void SliderGridVis(bool SG) => SliderGrid.VisibilityCol(SG);

        public double Maximum { get { return slider.Maximum; } set { slider.Maximum = value; } }

        #endregion Slider Grid Visibility

        #region Drag Button

        private DragButton _DragButton = DragButton.Middle;

        public DragButton DragButton { get { return _DragButton; } set { _DragButton = value; } }

        private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e) => DragButtonManager(true, MouseButtonState.Released, sender, e);

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e) => DragButtonManager(true, MouseButtonState.Released, sender, e);

        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e) => DragButtonManager(false, MouseButtonState.Pressed, sender, e);

        private void DragButtonManager(bool ButtonUp, MouseButtonState StatePass, object sender, MouseButtonEventArgs e)
        {
            bool Pass = false;

            switch (_DragButton)
            {
                case DragButton.Left: if (e.LeftButton == StatePass) Pass = true; break;
                case DragButton.Middle: if (e.MiddleButton == StatePass) Pass = true; break;
                case DragButton.Right: if (e.RightButton == StatePass) Pass = true; break;
            }

            if (Pass)
            {
                if (ButtonUp) OnMouseButtonUp(sender, e);
                else OnMouseButtonDown(sender, e);
            }
        }

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        #endregion Drag Button

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);
                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;
                lastDragPoint = posNow;
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);
            if (e.Delta > 0) slider.Value += 1;
            if (e.Delta < 0) slider.Value -= 1;
            e.Handled = true;
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;
            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);
                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);
                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;
                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;
                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;
                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY)) return;
                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
    }

    public enum DragButton { Left, Middle, Right }
}
