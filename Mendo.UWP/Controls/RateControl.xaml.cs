using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Mendo.UWP.Controls
{
    public sealed partial class RateControl : UserControl
    {


        public Double UserRating
        {
            get { return (Double)GetValue(UserRatingProperty); }
            set { SetValue(UserRatingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserRating.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserRatingProperty =
            DependencyProperty.Register("UserRating", typeof(Double), typeof(RateControl), new PropertyMetadata(0d));



        public RateControl()
        {
            this.InitializeComponent();
        }
    }
}
