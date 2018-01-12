//using System.Text;
//using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

using TouchTracking;

namespace DragOnScrollView
{
    public partial class MainPage : ContentPage
    {
        public class Position
        {
            public double X, Y;
        }
        public class Scroll
        {
            public double X, Y;
        }

        Dictionary<ContentView, DragInfo> dragDictionary = new Dictionary<ContentView, DragInfo>();

        int NoCV = 0;           // The number of the created contentView
        int drag = 0;           // Identification Number of the dragged contentView
        static int max = 20;    // Maximum number of contentViews
        ContentView[] contentViews = new ContentView[max];
        Position[] positions = new Position[max];       // contentViews[i]'s position in scrollView
        Position[] localpositions = new Position[max];  // contentViews[i]'s position in absoluteLayout

        Scroll scroll = new Scroll();   // scroll amount when scroll ON to OFF
                                        //   = absoluteLayout's postion in scroll View
        Scroll ScrollSize = new Scroll { X = 700, Y = 900 }; // Size of ScrollView


        public MainPage()
        {
            InitializeComponent();

            // Initialize positions
            for (int i = 0; i < max; i++)
            {
                positions[i] = new Position { X = 0, Y = 0 };
                localpositions[i] = new Position { X = 0, Y = 0 };
            }

            AddContentView();
            OnScroll();


            /*
            // detect tap event
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if( Math.Abs(absoluteLayout.Scale - 1.0) < Math.Abs(absoluteLayout.Scale * .0001) )
                {
                    absoluteLayout.Scale = 0.5;
                    //absoluteLayout.WidthRequest = scrollView.Width;
                    //absoluteLayout.HeightRequest = scrollView.Height;
                }
                else
                {
                    absoluteLayout.Scale = 1.0;
                }
            };
            scrollView.GestureRecognizers.Add(tap);
            */

        }


        // Scroll Off to On
        void OnScroll()
        {
            // Enable Scroll by enlarging absoluteLayout size
            absoluteLayout.WidthRequest  = ScrollSize.X;
            absoluteLayout.HeightRequest = ScrollSize.Y;
            absoluteLayout.BackgroundColor = Color.LightGray;

            // Scroll Back to the position when scroll ON to OFF
            scrollView.ScrollToAsync( scroll.X, scroll.Y, false );

            // Update dragged contentView's positions[drag]
            positions[drag].X = scroll.X + localpositions[drag].X;
            positions[drag].Y = scroll.Y + localpositions[drag].Y;

            // re-locate all contentViews in "large" absoluteLayout
            for (int i = 0; i < NoCV; i++)
            {
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(contentViews[i]);
                rect.X = positions[i].X;
                rect.Y = positions[i].Y;
                AbsoluteLayout.SetLayoutBounds(contentViews[i], rect);
            }

            Label3.Text = "Display Size of ScrollView: " + scrollView.Width.ToString() +
                           " x " + scrollView.Height.ToString() + "  Label3";
            //DisplayAlert("", "Scroll X = " + scrollView.Width.ToString() +
            // ", Y = " + scrollView.Height.ToString(), "OK");
        }

        // Scroll On to Off
        void OffScroll()
        {
            // Disable Scroll by reducing absoluteLayout size
            absoluteLayout.WidthRequest = scrollView.Width;
            absoluteLayout.HeightRequest = scrollView.Height;
            absoluteLayout.BackgroundColor = Color.White;

            // get scroll amount when scroll On to Off
            scroll.X = scrollView.ScrollX;
            scroll.Y = scrollView.ScrollY;

            // locate contentView in "small" absoluteLayout
            for (int i = 0; i < NoCV; i++)
            {
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(contentViews[i]);
                rect.X = positions[i].X - scroll.X;
                rect.Y = positions[i].Y - scroll.Y;
                AbsoluteLayout.SetLayoutBounds(contentViews[i], rect);
            }

            Label1.Text = "Scroll X = " + scroll.X.ToString() + 
                               ", Y = " + scroll.Y.ToString();
            Label3.Text = "Display Size of ScrollView: " + scrollView.Width.ToString() +
                                                   " x " + scrollView.Height.ToString();
        }



        void AddContentViewButton(object sender, EventArgs args)
        {
            AddContentView();
        }

        /*
        void Button2(object sender, EventArgs args)
        {
            scrollView.ScrollToAsync( 100, 100, false );
        }
        */

        //void AddBoxViewToLayout(AbsoluteLayout absLayout)

        void AddContentView()
        {
            ContentView contentView = new ContentView
            {
                WidthRequest = 200,
                HeightRequest = 100,
                BackgroundColor = Color.Black,
                Padding = new Thickness(2, 2, 2, 2),
                Content = new Editor
                {
                    FontSize = 20,
                    InputTransparent = true,    // Disable to edit
                    Text = "contentViews[" + NoCV + "]"
                }
            };

            contentViews[NoCV] = contentView;
            NoCV++;
            Label4.Text = "The number of ContentView: " + NoCV.ToString();

            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += OnTouchEffectAction;
            contentView.Effects.Add(touchEffect);
            absoluteLayout.Children.Add(contentView);
        }


        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            ContentView view = sender as ContentView;

            // identify the dragged contentView
            for (int i = 0; i < NoCV; i++)
            {
                if (view == contentViews[i]) { drag = i; break; }
            }

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Don't allow a second touch on an already touched BoxView
                    if (!dragDictionary.ContainsKey(view))
                    {
                        dragDictionary.Add(view, new DragInfo(args.Id, args.Location));

                        // Set Capture property to true
                        TouchEffect touchEffect = (TouchEffect)view.Effects.FirstOrDefault(e => e is TouchEffect);
                        touchEffect.Capture = true;
                    }

                    OffScroll();

                    break;

                case TouchActionType.Moved:
                    if (dragDictionary.ContainsKey(view) && dragDictionary[view].Id == args.Id)
                    {
                        Rectangle rect = AbsoluteLayout.GetLayoutBounds(view);
                        Point initialLocation = dragDictionary[view].PressPoint;
                        rect.X += args.Location.X - initialLocation.X;
                        rect.Y += args.Location.Y - initialLocation.Y;
                        AbsoluteLayout.SetLayoutBounds(view, rect);
                    }
                    break;

                case TouchActionType.Released:
                    if (dragDictionary.ContainsKey(view) && dragDictionary[view].Id == args.Id)
                    {
                        Rectangle rect = AbsoluteLayout.GetLayoutBounds(view);

                        localpositions[drag].X = rect.X;
                        localpositions[drag].Y = rect.Y;

                        Label2.Text = "Position in absoluteLayout: X = " + rect.X.ToString() +
                             ", Y = " + rect.Y.ToString();
                        
                        dragDictionary.Remove(view);
                    }

                    OnScroll();

                    break;
            }
        }
    }
}
