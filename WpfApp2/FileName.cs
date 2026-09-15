using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProjectFIN.models;
using ProjectFIN.services;
using ProjectFIN.InterFaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class MainViewModel : ObservableObject
    {
        private DispatcherTimer _fuelTimer;

        public ObservableCollection<Vehicle> Vehicles { get; set; } = new();

        private Vehicle? _selectedVehicle;
        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set => SetProperty(ref _selectedVehicle, value);
        }

        private string _vin = string.Empty;
        public string Vin { get => _vin; set => SetProperty(ref _vin, value); }

        private string _brand = string.Empty;
        public string Brand { get => _brand; set => SetProperty(ref _brand, value); }

        private string _model = string.Empty;
        public string Model { get => _model; set => SetProperty(ref _model, value); }

        private double _latitude;
        public double Latitude { get => _latitude; set => SetProperty(ref _latitude, value); }

        private double _longitude;
        public double Longitude { get => _longitude; set => SetProperty(ref _longitude, value); }

        private double _capacity = 60;
        public double Capacity { get => _capacity; set => SetProperty(ref _capacity, value); }

        public List<string> VehicleTypes => new()
        {
            GetResourceString("m_TypeGasoline", "GasolineCar"),
            GetResourceString("m_TypeElectric", "ElectricCar")
        };

        private string _selectedVehicleType;
        public string SelectedVehicleType
        {
            get => _selectedVehicleType;
            set => SetProperty(ref _selectedVehicleType, value);
        }

        public IAsyncRelayCommand LoadVehiclesCommand { get; }
        public IAsyncRelayCommand AddVehicleCommand { get; }
        public IAsyncRelayCommand DeleteVehicleCommand { get; }
        public IAsyncRelayCommand<Vehicle> ToggleEngineCommand { get; }
        public IAsyncRelayCommand<Vehicle> ToggleDoorsCommand { get; }
        public IRelayCommand<string> ChangeLanguageCommand { get; }

        public MainViewModel()
        {
            _selectedVehicleType = VehicleTypes[0];

            LoadVehiclesCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            AddVehicleCommand = new AsyncRelayCommand(AddVehicleAsync);
            DeleteVehicleCommand = new AsyncRelayCommand(DeleteVehicleAsync);

            ToggleEngineCommand = new AsyncRelayCommand<Vehicle>(ToggleEngineAsync);
            ToggleDoorsCommand = new AsyncRelayCommand<Vehicle>(ToggleDoorsAsync);
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);

            _fuelTimer = new DispatcherTimer();
            _fuelTimer.Interval = TimeSpan.FromSeconds(2);
            _fuelTimer.Tick += FuelTimer_Tick;
            _fuelTimer.Start();

            _ = LoadVehiclesAsync();
        }

        private async Task LoadVehiclesAsync()
        {
            using (var context = new AppDbContext())
            {
                await context.Database.EnsureCreatedAsync();

                var list = await context.Vehicles.Include(v => v.LocationData).ToListAsync();

                Vehicles.Clear();
                foreach (var v in list)
                {
                    Vehicles.Add(v);
                }
            }
        }

        private async Task AddVehicleAsync()
        {
            if (string.IsNullOrWhiteSpace(Vin) || string.IsNullOrWhiteSpace(Brand) || string.IsNullOrWhiteSpace(Model))
            {
                MessageBox.Show(GetResourceString("msg_FillRequired", "Заповніть поля!"),
                                GetResourceString("msg_InputErrorTitle", "Помилка"),
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Vehicle newVehicle;
            string electricType = GetResourceString("m_TypeElectric", "ElectricCar");

            if (SelectedVehicleType == electricType)
                newVehicle = new ElectricCar(Vin, Brand, Model, Latitude, Longitude, Capacity);
            else
                newVehicle = new GasolineCar(Vin, Brand, Model, Latitude, Longitude, Capacity);

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Vehicles.Add(newVehicle);
                    await context.SaveChangesAsync();
                }

                Vehicles.Add(newVehicle);
                Vin = Brand = Model = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetResourceString("msg_DbError", "Помилка запису в БД"),
                                GetResourceString("msg_DbErrorTitle", "Помилка"),
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            using (var context = new AppDbContext())
            {
                context.Vehicles.Remove(SelectedVehicle);
                await context.SaveChangesAsync();
            }

            Vehicles.Remove(SelectedVehicle);
        }

        private async Task ToggleEngineAsync(Vehicle? targetVehicle)
        {
            var vehicle = targetVehicle ?? SelectedVehicle;
            if (vehicle == null) return;

            try
            {
                if (vehicle.Engine == EngineState.Running)
                {
                    vehicle.StopEngine();
                }
                else
                {
                    if (vehicle.CurrentLevel <= 0)
                    {
                        MessageBox.Show(GetResourceString("msg_NoFuel", "Паливо на нулі!"),
                                        GetResourceString("msg_EngineErrorTitle", "Помилка"),
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    vehicle.StartEngine();
                }

                using (var context = new AppDbContext())
                {
                    context.Vehicles.Update(vehicle);
                    await context.SaveChangesAsync();
                }

                OnPropertyChanged(nameof(Vehicles));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, GetResourceString("msg_ErrorTitle", "Помилка"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ToggleDoorsAsync(Vehicle? targetVehicle)
        {
            var vehicle = targetVehicle ?? SelectedVehicle;
            if (vehicle == null) return;

            if (vehicle.Doors == DoorState.Locked)
                vehicle.UnlockDoors();
            else
                vehicle.LockDoors();

            using (var context = new AppDbContext())
            {
                context.Vehicles.Update(vehicle);
                await context.SaveChangesAsync();
            }

            OnPropertyChanged(nameof(Vehicles));
        }

        private async void FuelTimer_Tick(object? sender, EventArgs e)
        {
            bool hasChanges = false;

            using (var context = new AppDbContext())
            {
                foreach (var vehicle in Vehicles)
                {
                    if (vehicle.Engine == EngineState.Running)
                    {
                        vehicle.ConsumeResource(0.2);
                        context.Vehicles.Update(vehicle);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    await context.SaveChangesAsync();
                    OnPropertyChanged(nameof(Vehicles));
                }
            }
        }

        private void ChangeLanguage(string? langCode)
        {
            if (string.IsNullOrEmpty(langCode)) return;

            var dict = new ResourceDictionary
            {
                Source = new Uri($"/WpfApp2;component/Resources/Lang{langCode}.xaml", UriKind.RelativeOrAbsolute)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            OnPropertyChanged(nameof(VehicleTypes));
        }

        private string GetResourceString(string key, string fallback)
        {
            return Application.Current.Resources[key] as string ?? fallback;
        }
    }
}