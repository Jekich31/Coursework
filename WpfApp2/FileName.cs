using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ProjectFIN.models;
using ProjectFIN.services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProjectFIN.UI
{
    public class MainViewModel : ObservableObject 
    {
        private ObservableCollection<Vehicle> _vehicles = new();
        private Vehicle? _selectedVehicle;
        private bool _isLoading;
        private double _capacity = 50; 

        public double Capacity
        {
            get => _capacity;
            set => SetProperty(ref _capacity, value);
        }

        private string _vin = string.Empty;
        public string Vin
        {
            get => _vin;
            set => SetProperty(ref _vin, value);
        }

        private string _brand = string.Empty;
        public string Brand
        {
            get => _brand;
            set => SetProperty(ref _brand, value);
        }

        private string _model = string.Empty;
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private double _latitude;
        public double Latitude
        {
            get => _latitude;
            set => SetProperty(ref _latitude, value);
        }

        private double _longitude;
        public double Longitude
        {
            get => _longitude;
            set => SetProperty(ref _longitude, value);
        }

        private string _selectedVehicleType = "GasolineCar";
        public string SelectedVehicleType
        {
            get => _selectedVehicleType;
            set => SetProperty(ref _selectedVehicleType, value);
        }

        public List<string> VehicleTypes { get; } = new() { "GasolineCar", "ElectricCar" };

        public ObservableCollection<Vehicle> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set => SetProperty(ref _selectedVehicle, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public IAsyncRelayCommand LoadVehiclesCommand { get; }
        public IAsyncRelayCommand DeleteVehicleCommand { get; }
        public IAsyncRelayCommand AddVehicleCommand { get; }
        public IAsyncRelayCommand ToggleEngineCommand { get; }
        public IAsyncRelayCommand ToggleDoorsCommand { get; }
        public IRelayCommand<string> ChangeLanguageCommand { get; }
        private DispatcherTimer _fuelTimer;
        public MainViewModel()
        {
            LoadVehiclesCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            DeleteVehicleCommand = new AsyncRelayCommand(DeleteVehicleAsync);
            AddVehicleCommand = new AsyncRelayCommand(AddVehicleAsync);
            ToggleEngineCommand = new AsyncRelayCommand(ToggleEngineAsync);
            ToggleDoorsCommand = new AsyncRelayCommand(ToggleDoorsAsync);

            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);

            ChangeLanguage("UKR");
            _ = LoadVehiclesAsync();
            _fuelTimer = new DispatcherTimer();
            _fuelTimer.Interval = TimeSpan.FromSeconds(2);
            _fuelTimer.Tick += FuelTimer_Tick;
            _fuelTimer.Start();
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
                        hasChanges = true;

                        context.Vehicles.Update(vehicle);
                    }
                }

                if (hasChanges)
                {
                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Помилка збереження палива в БД: {ex.Message}");
                    }
                    OnPropertyChanged(nameof(Vehicles));
                    OnPropertyChanged(nameof(SelectedVehicle));
                }
            }
        }
        private void ChangeLanguage(string? langCode)
        {
            if (string.IsNullOrEmpty(langCode)) return;

            try
            {
                var dict = new ResourceDictionary();
                dict.Source = new Uri($"/WpfApp2;component/Resources/Lang{langCode}.xaml", UriKind.RelativeOrAbsolute);

                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load language dictionary: {ex.Message}", "Language Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetResourceString(string key, string fallback)
        {
            return Application.Current.Resources[key] as string ?? fallback;
        }

        private async Task AddVehicleAsync()
        {
            if (string.IsNullOrWhiteSpace(Brand) || string.IsNullOrWhiteSpace(Model) || string.IsNullOrWhiteSpace(Vin))
            {
                string msg = GetResourceString("msg_FillRequired", "Будь ласка, заповніть VIN, Марку та Модель!");
                string title = GetResourceString("msg_InputErrorTitle", "Помилка введення");
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Vehicle newVehicle;

                if (SelectedVehicleType == "ElectricCar")
                {
                    newVehicle = new ElectricCar(Vin, Brand, Model, Latitude, Longitude, Capacity);
                }
                else
                {
                    newVehicle = new GasolineCar(Vin, Brand, Model, Latitude, Longitude, Capacity);
                }

                using (var context = new AppDbContext())
                {
                    await context.Vehicles.AddAsync(newVehicle);
                    await context.SaveChangesAsync();
                }

                Vehicles.Add(newVehicle);

                Vin = string.Empty;
                Brand = string.Empty;
                Model = string.Empty;
                Latitude = 0;
                Longitude = 0;
                Capacity = 50;

                string successMsg = GetResourceString("msg_AddSuccess", "Автомобіль успішно додано до бази даних!");
                string successTitle = GetResourceString("msg_SuccessTitle", "Успіх");
                MessageBox.Show(successMsg, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbUpdateException dbEx)
            {
                string dbErrorMsg = GetResourceString("msg_DbError", "Помилка бази даних! Можливо, автомобіль з таким VIN вже існує.");
                string dbErrorTitle = GetResourceString("msg_DbErrorTitle", "Помилка запису в БД");

                MessageBox.Show($"{dbErrorMsg}\n\nDetails: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                dbErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                string errorTitle = GetResourceString("msg_ErrorTitle", "Помилка");
                MessageBox.Show($"Unexpected error: {ex.Message}", errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task ToggleEngineAsync()
        {
            if (SelectedVehicle == null) return;
            try
            {

                if (SelectedVehicle.Engine == EngineState.Running)
                {
                    SelectedVehicle.StopEngine();
                }
                else
                {
                    SelectedVehicle.StartEngine();
                }

                using (var context = new AppDbContext())
                {
                    context.Vehicles.Update(SelectedVehicle);
                    await context.SaveChangesAsync();
                }

                OnPropertyChanged(nameof(SelectedVehicle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to toggle engine state: {ex.Message}", "Engine Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task ToggleDoorsAsync()
        {
            if (SelectedVehicle == null) return;

            if (SelectedVehicle.Doors == DoorState.Locked)
            {
                SelectedVehicle.UnlockDoors();
            }
            else
            {
                SelectedVehicle.LockDoors();
            }

            using (var context = new AppDbContext())
            {
                context.Vehicles.Update(SelectedVehicle);
                await context.SaveChangesAsync();
            }

            OnPropertyChanged(nameof(SelectedVehicle));
        }

        private async Task LoadVehiclesAsync()
        {
            IsLoading = true;

            using (var context = new AppDbContext())
            {
                var list = await context.Vehicles
                                        .Include(v => v.LocationData)
                                        .ToListAsync();

                Vehicles.Clear();
                foreach (var vehicle in list)
                {
                    Vehicles.Add(vehicle);
                }
            }

            IsLoading = false;
        }

        private async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            using (var context = new AppDbContext())
            {
                var vehicleToDelete = await context.Vehicles.FindAsync(SelectedVehicle.Vin);
                if (vehicleToDelete != null)
                {
                    context.Vehicles.Remove(vehicleToDelete);
                    await context.SaveChangesAsync();
                }
            }

            Vehicles.Remove(SelectedVehicle);
        }
    }
}