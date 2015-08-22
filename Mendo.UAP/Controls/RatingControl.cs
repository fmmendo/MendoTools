using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Mendo.UAP.Controls
{
    public sealed class RatingControl : Control
    {
        public RatingControl()
        {
            this.DefaultStyleKey = typeof(RatingControl);
        }

        public Double Rating
        {
            get { return (Double)GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Rating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register("Rating", typeof(Double), typeof(RatingControl), new PropertyMetadata(0d));
    }
}
