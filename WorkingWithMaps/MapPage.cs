using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Plugin.Geolocator;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace WorkingWithMaps
{
	public class MapPage : ContentPage
	{
		Map map;
        Position pos = new Position();
		public MapPage ()
		{
			map = new Map { 
				IsShowingUser = true,
				HeightRequest = 100,
				WidthRequest = 960,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

            getmap();
            
            

            // You can use MapSpan.FromCenterAndRadius 
            //map.MoveToRegion (MapSpan.FromCenterAndRadius (new Position (37, -122), Distance.FromMiles (0.3)));
            // or create a new MapSpan object directly
                     //   map.MoveToRegion (new MapSpan (new Position (0,0), 360, 360) );

			// add the slider
			var slider = new Slider (1, 18, 1);
			slider.ValueChanged += (sender, e) => {
				var zoomLevel = e.NewValue; // between 1 and 18
				var latlongdegrees = 360 / (Math.Pow(2, zoomLevel));
				Debug.WriteLine(zoomLevel + " -> " + latlongdegrees);
				if (map.VisibleRegion != null)
					map.MoveToRegion(new MapSpan (map.VisibleRegion.Center, latlongdegrees, latlongdegrees)); 
			};


			// create map style buttons
			var street = new Button { Text = "Street" };
			var hybrid = new Button { Text = "Hybrid" };
			var satellite = new Button { Text = "Satellite" };
			street.Clicked += HandleClicked;
			hybrid.Clicked += HandleClicked;
			satellite.Clicked += HandleClicked;
			var segments = new StackLayout { Spacing = 30,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				Orientation = StackOrientation.Horizontal, 
				Children = {street, hybrid, satellite}
			};


			// put the page together
			var stack = new StackLayout { Spacing = 0 };
			stack.Children.Add(map);
			stack.Children.Add (slider);
			stack.Children.Add (segments);
			Content = stack;


			// for debugging output only
			map.PropertyChanged += (sender, e) => {
				Debug.WriteLine(e.PropertyName + " just changed!");
				if (e.PropertyName == "VisibleRegion" && map.VisibleRegion != null)
					CalculateBoundingCoordinates (map.VisibleRegion);
			};

            
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
           // getAlljobs("","");
        }
        private async void getAlljobs(string lat,string longi)
        {
            try
            {
                string s = Plugin.DeviceInfo.CrossDeviceInfo.Current.Id;
                string URL = "https://globe24-7.info/test/insert.php";   // "https://globe24-7.info/api/v1/jobs/job-list";
                try
                {
                    var formContent = new FormUrlEncodedContent(new[]
                        {
                new KeyValuePair<string, string>("udid", s),
                new KeyValuePair<string, string>("lat", lat),
                new KeyValuePair<string, string>("long", longi),
            });


                    var myHttpClient = new HttpClient();
                    //myHttpClient.DefaultRequestHeaders.Add("Authorization", "");

                    var response = await myHttpClient.PostAsync(URL, formContent);

                    var json = await response.Content.ReadAsStringAsync();


                    URL = "https://globe24-7.info/test/select.php";

            //         formContent = new FormUrlEncodedContent(new[]
            //        {
            //    new KeyValuePair<string, string>("udid", "),
            //});

                    response = await myHttpClient.PostAsync(URL,null);
                     json = await response.Content.ReadAsStringAsync();
                    JArray results = JArray.Parse(json.ToString());

                    double laty=0;
                    double longy = 0;
                    int already = 0;

                    for (int i = 0; i < results.Count; i++)
                    {
                        already = 0;
                        foreach (Newtonsoft.Json.Linq.JProperty result in results[i])
                        {
                            if (result.Name.ToString() == "udid")
                            {
                                if (result.Value.ToString() == s)
                                {
                                    already = 1;
                                    break;
                                }
                            }

                            if (result.Name.ToString() == "lat")
                            {
                                laty = Convert.ToDouble(result.Value);
                            }
                            if (result.Name.ToString() == "long")
                            {
                                longy = Convert.ToDouble(result.Value);
                            }

                            
                        }
                        if (already == 0)
                        {
                            pos = new Position(laty, longy); // Latitude, Longitude
                            var pin = new Pin
                            {
                                Type = PinType.Place,
                                Position = pos,
                                Label = Plugin.DeviceInfo.CrossDeviceInfo.Current.DeviceName
                                //Address = "Guru"
                            };
                            map.Pins.Add(pin);
                        }
                    }
                    ////JObject results = JObject.Parse(json.ToString());


                    //List<JobList> j = new List<JobList>();

                    //var ContentObj = JsonConvert.DeserializeObject<RootObject1>(json);

                    //j = ContentObj.job_list;

                    //try
                    //{
                    //    Commonclass.AllJobs = j;
                    //    lstAllJobs.ItemsSource = Commonclass.AllJobs;
                    //    getMyjobs();
                    //    this.indi.IsRunning = false;
                    //    this.indi.IsVisible = false;
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                }
                catch (Exception ex)
                {
                   
                }
                //lstAllJobs.ItemsSource = Commonclass.AllJobs;
            }
            catch (Exception ex)
            {
             
            }

            
        }

        private async void getmap()
        {
            var locator = CrossGeolocator.Current;
            TimeSpan ts = TimeSpan.FromTicks(10000);
            var position = await locator.GetPositionAsync(ts);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude),
                                                         Distance.FromMiles(1)));
            getAlljobs(position.Latitude.ToString(), position.Longitude.ToString());
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

        void HandleClicked (object sender, EventArgs e)
		{
			var b = sender as Button;
			switch (b.Text) {
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
		static void CalculateBoundingCoordinates (MapSpan region)
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

			Debug.WriteLine ("Bounding box:");
			Debug.WriteLine ("                    " + top);
			Debug.WriteLine ("  " + left + "                " + right);
			Debug.WriteLine ("                    " + bottom);
		}
	}
}
