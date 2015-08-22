using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Mendo.UAP.Behaviors
{
    public class RatingClippingBehavior : DependencyObject, IBehavior
    {
        public DependencyObject AssociatedObject { get { throw new NotImplementedException(); } }

        private FrameworkElement elementReference;

        private bool isCleanedUp = false;

        #region Dependency Properties

        public Double ActualHeight
        {
            get { return (Double)GetValue(ActualHeightProperty); }
            set { SetValue(ActualHeightProperty, value); }
        }
        public static readonly DependencyProperty ActualHeightProperty =
            DependencyProperty.Register("ActualHeight", typeof(Double), typeof(RatingClippingBehavior), new PropertyMetadata(0d));


        public Double ActualWidth
        {
            get { return (Double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }
        public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.Register("ActualWidth", typeof(Double), typeof(RatingClippingBehavior), new PropertyMetadata(0d));


        public int NumberOfStars
        {
            get { return (int)GetValue(NumberOfStarsProperty); }
            set { SetValue(NumberOfStarsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for NumberOfStars.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfStarsProperty =
            DependencyProperty.Register("NumberOfStars", typeof(int), typeof(RatingClippingBehavior), new PropertyMetadata(5, OnNumberOfStarsChanged));


        public double Rating
        {
            get { return (double)GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Rating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof(double), typeof(RatingClippingBehavior), new PropertyMetadata(0d, OnRatingChanged));

        #region Property Changed Callbacks
        private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RatingClippingBehavior)d).HandleClipChanged();
        }

        private static void OnNumberOfStarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RatingClippingBehavior)d).HandleClipChanged();
        }

        #endregion
 
        #endregion

        public void Attach(DependencyObject associatedObject)
        {
            var element = associatedObject as FrameworkElement;
            if (element != null)
            {
                elementReference = element;
                elementReference.SizeChanged += FrameworkElement_SizeChanged;
            }
        }

        public void Detach()
        {
            ClearClip();

            CleanUp();
        }

        private void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var obj = sender as FrameworkElement;

            if (obj != null)
            {
                if (!(obj.ActualHeight > 0) || !(obj.ActualWidth > 0))
                {
                    this.ActualHeight = this.ActualWidth = 0;
                }
                else
                {
                    this.ActualHeight = obj.ActualHeight;
                    this.ActualWidth = obj.ActualWidth;
                }

                HandleClipping();
            }
            else
            {
                this.ActualHeight = this.ActualWidth = 0;
            }
        }

        #region Clipping

        private void HandleClipping()
        {
            if (elementReference != null)
            {
                if (elementReference != null)
                    elementReference.Clip = new RectangleGeometry { Rect = new Rect(0, 0, (ActualWidth / NumberOfStars) * Rating, ActualHeight) };
            }
        }

        private void HandleClipChanged()
        {
            //if (ClipToBounds)
            //{
                HandleClipping();
            //}
            //else
            //{
            //    ClearClip();
            //}
        }

        private void ClearClip()
        {
            if (elementReference != null)
            {
                elementReference.ClearValue(FrameworkElement.ClipProperty);
            }
        }

        #endregion

        void CleanUp()
        {
            if (!isCleanedUp)
            {
                elementReference.SizeChanged -= FrameworkElement_SizeChanged;

                isCleanedUp = true;
            }
        }
    }
}
