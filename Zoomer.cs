using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Boykisser
{
    public class Zoomer : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        private TranslateTransform GetPosition(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScale(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                child_PreviewMouseRightButtonDown);
            }
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var scl = GetScale(child);
                var pos = GetPosition(child);

                double zoom;
                if (e.Delta > 0)
                    zoom = 1.05; 
                else
                    zoom  = 0.95;

                if (((e.Delta < 0) && (scl.ScaleX < .3 || scl.ScaleY < .3)) || ((e.Delta > 0) && (scl.ScaleX > 20 || scl.ScaleY > 20)))
                    return;

                Point relative = e.GetPosition(child);

                pos.X = relative.X * scl.ScaleX * (1-zoom) + pos.X;
                pos.Y = relative.Y * scl.ScaleY * (1-zoom) + pos.Y; 

                scl.ScaleX *= zoom;
                scl.ScaleY *= zoom;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                var st = GetScale(child);
                var tt = GetPosition(child);

                tt.X = 0;
                tt.Y = 0;

                st.ScaleX = 1;
                st.ScaleY = 1;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetPosition(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Reset();
        }

        public void SetOneToOne(double ImageboxWidth)
        {
            var scl = GetScale(child);
            var pos = GetPosition(child);

            double zoom = MainWindow.ImageWidth / SystemParameters.PrimaryScreenWidth;

            pos.X = SystemParameters.PrimaryScreenHeight * 0.5 - MainWindow.ImageWidth * 0.5;
            pos.Y = SystemParameters.PrimaryScreenHeight * 0.5 - MainWindow.ImageHeight * 0.5;

            scl.ScaleX = zoom;
            scl.ScaleY = zoom;
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetPosition(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }
}