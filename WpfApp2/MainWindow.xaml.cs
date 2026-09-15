using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectFIN.models;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
            InitializeComponent();

            var vm = new MainViewModel();
            DataContext = vm;

            Loaded += async (s, e) =>
            {
                await mapWebView.EnsureCoreWebView2Async();
                mapWebView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) MyWpfApp/1.0";
                UpdateMapMarkers(vm.Vehicles);
            };

            vm.Vehicles.CollectionChanged += (s, e) => UpdateMapMarkers(vm.Vehicles);
            vm.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedVehicle) && vm.SelectedVehicle?.LocationData != null)
                {
                    var v = vm.SelectedVehicle;
                    var lat = v.LocationData.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var lng = v.LocationData.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    if (mapWebView.CoreWebView2 != null)
                    {
                        await mapWebView.ExecuteScriptAsync($"focusOnVehicle('{v.Vin}', {lat}, {lng});");
                    }
                }
            };
            this.Closing += MainWindow_Closing;
        }

        private void UpdateMapMarkers(IEnumerable<Vehicle> vehicles)
        {
            if (mapWebView.CoreWebView2 == null) return;

            var markersJs = string.Join(",", vehicles
                .Where(v => v.LocationData != null)
                .Select(v => $"{{ vin: '{v.Vin}', lat: {v.LocationData.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                             $"lng: {v.LocationData.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                             $"title: '{v.Brand} {v.Model} ({v.Vin})', " +
                             $"status: '{v.Engine}' }}"));

            string htmlContent = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"" />
        <script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
        <style>
            #map {{ height: 100vh; width: 100%; margin: 0; padding: 0; }}
            body {{ margin: 0; }}
        </style>
    </head>
    <body>
        <div id=""map""></div>
        <script>
            var map = L.map('map').setView([49.2331, 28.4682], 12);

            L.tileLayer('https://tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                maxZoom: 19,
                attribution: '&copy; OpenStreetMap contributors'
            }}).addTo(map);

            var vehicles = [{markersJs}];
            var bounds = [];
            var markers = {{}};

            vehicles.forEach(function(v) {{
                var marker = L.marker([v.lat, v.lng]).addTo(map)
                    .bindPopup('<b>' + v.title + '</b><br>Engine: ' + v.status);
                
                markers[v.vin] = marker;
                bounds.push([v.lat, v.lng]);
            }});

            if (bounds.length > 0) {{
                map.fitBounds(bounds);
            }}

            function focusOnVehicle(vin, lat, lng) {{
                map.flyTo([lat, lng], 16, {{ duration: 1.5 }});
                if (markers[vin]) {{
                    markers[vin].openPopup();
                }}
            }}
        </script>
    </body>
    </html>";

            mapWebView.NavigateToString(htmlContent);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                bool hasUnsafeVehicles = vm.Vehicles.Any(v =>
                    v.Engine == EngineState.Running ||
                    v.Doors == DoorState.Unlocked
                );

                if (hasUnsafeVehicles)
                {
                    string title = Application.Current.Resources["msg_ConfirmExitTitle"] as string ?? "Увага!";
                    string message = Application.Current.Resources["msg_ConfirmExitWarning"] as string ?? "У вас залишилися автомобілі з працюючим двигуном або відкритими дверима!";

                    var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}