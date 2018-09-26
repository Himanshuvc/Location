using Plugin.Geolocator;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace WorkingWithMaps
{
	public class PinPage : ContentPage
	{
		Map map;
        Position pos = new Position();
		public PinPage ()
		{
			map = new Map { 
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
            getmap();

            //        map.MoveToRegion (MapSpan.FromCenterAndRadius (
            //new Position (36.9628066,-122.0194722), Distance.FromMiles (3))); // Santa Cruz golf course
            // create map style buttons
            var street = new Button { Text = "Street" };
            var hybrid = new Button { Text = "Hybrid" };
            var satellite = new Button { Text = "Satellite" };
            street.Clicked += HandleClicked;
            hybrid.Clicked += HandleClicked;
            satellite.Clicked += HandleClicked;
            var segments = new StackLayout
            {
                Spacing = 30,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { street, hybrid, satellite }
            };


            // put the page together
            var stack = new StackLayout { Spacing = 0 };
            stack.Children.Add(map);
            //stack.Children.Add(slider);
            stack.Children.Add(segments);
            Content = stack;


            // for debugging output only
            map.PropertyChanged += (sender, e) => {
                Debug.WriteLine(e.PropertyName + " just changed!");
                if (e.PropertyName == "VisibleRegion" && map.VisibleRegion != null)
                    CalculateBoundingCoordinates(map.VisibleRegion);
            };



            // create buttons
            //var morePins = new Button { Text = "Add more pins" };
            //morePins.Clicked += (sender, e) => {
            //	map.Pins.Add(new Pin {
            //		Position = new Position(36.9641949,-122.0177232),
            //		Label = "Boardwalk"
            //	});
            //	map.Pins.Add(new Pin {
            //		Position = new Position(36.9571571,-122.0173544),
            //		Label = "Wharf"
            //	});
            //	map.MoveToRegion (MapSpan.FromCenterAndRadius (
            //		new Position (36.9628066,-122.0194722), Distance.FromMiles (1.5)));

            //};
            //var reLocate = new Button { Text = "Re-center" };
            //reLocate.Clicked += (sender, e) => {
            //	map.MoveToRegion (MapSpan.FromCenterAndRadius (
            //		new Position (36.9628066,-122.0194722), Distance.FromMiles (3)));
            //};
            //var buttons = new StackLayout {
            //	Orientation = StackOrientation.Horizontal,
            //	Children = {
            //		morePins, reLocate
            //	}
            //};

            // put the page together
            Content = new StackLayout { 
				Spacing = 0,
				Children = {
					map,
					//buttons
				}};
		}
        void HandleClicked(object sender, EventArgs e)
        {
            var b = sender as Button;
            switch (b.Text)
            {
                case "Street":
                    map.MapType = MapType.Street;
                    break;
                case "Hybrid":
                    map.MapType = MapType.Hybrid;
                    break;
                case "Satellite":
                    map.MapType = MapType.Satellite;
                    break;
            }
        }


        /// <summary>
        /// In response to this forum question http://forums.xamarin.com/discussion/22493/maps-visibleregion-bounds
        /// Useful if you need to send the bounds to a web service or otherwise calculate what
        /// pins might need to be drawn inside the currently visible viewport.
        /// </summary>
        static void CalculateBoundingCoordinates(MapSpan region)
        {
            // WARNING: I haven't tested the correctness of this exhaustively!
            var center = region.Center;
            var halfheightDegrees = region.LatitudeDegrees / 2;
            var halfwidthDegrees = region.LongitudeDegrees / 2;

            var left = center.Longitude - halfwidthDegrees;
            var right = center.Longitude + halfwidthDegrees;
            var top = center.Latitude + halfheightDegrees;
            var bottom = center.Latitude - halfheightDegrees;

            // Adjust for Internation Date Line (+/- 180 degrees longitude)
            if (left < -180) left = 180 + (180 + left);
            if (right > 180) right = (right - 180) - 180;
            // I don't wrap around north or south; I don't think the map control allows this anyway

            Debug.WriteLine("Bounding box:");
            Debug.WriteLine("                    " + top);
            Debug.WriteLine("  " + left + "                " + right);
            Debug.WriteLine("                    " + bottom);
        }
       
        private async void getmap()
        {
            var locator = CrossGeolocator.Current;
            TimeSpan ts = TimeSpan.FromTicks(10000);
            var position = await locator.GetPositionAsync(ts);

            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude),
                                                         Distance.FromMiles(1)));

            pos = new Position(position.Latitude, position.Longitude); // Latitude, Longitude
            var pin = new Pin
            {
                Type = PinType.Place,
                Position = pos,
                Label = "Vc",
                Address = "Guru"
            };
            map.Pins.Add(pin);
        }
    }
}

