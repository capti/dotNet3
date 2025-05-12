using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dot.Models;
using Avalonia.Threading;

namespace dot.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<OilRig> _oilRigs = new();

        [ObservableProperty]
        private ObservableCollection<Mechanic> _mechanics = new();

        [ObservableProperty]
        private ObservableCollection<Loader> _loaders = new();

        [ObservableProperty]
        private ObservableCollection<string> _logEntries = new();

        [ObservableProperty]
        private string _statusMessage = "Добавьте нефтяную вышку и механика для начала работы";

        [ObservableProperty]
        private double _totalOilExtracted;

        [ObservableProperty]
        private bool _isSimulationStarted;

        public MainViewModel()
        {
            // Не запускаем симуляцию автоматически
        }

        [RelayCommand]
        private void AddOilRig()
        {
            var rig = new OilRig($"Вышка {OilRigs.Count + 1}");

            rig.FireStarted += OnFireStarted;
            rig.MaintenanceRequired += OnMaintenanceRequired;
            rig.OilExtracted += OnOilExtracted;

            OilRigs.Add(rig);
        }

        [RelayCommand]
        private void AddMechanic()
        {
            var mechanic = new Mechanic($"Механик {Mechanics.Count + 1}");

            mechanic.FireExtinguished += (s, e) => OnFireExtinguished(e.Rig);
            mechanic.MaintenanceCompleted += (s, e) => OnMaintenanceCompleted(e.Rig);
            Mechanics.Add(mechanic);
        }

        [RelayCommand]
        private void AddLoader()
        {
            var loader = new Loader($"Погрузчик {Loaders.Count + 1}");

            loader.OilLoaded += OnOilLoaded;
            Loaders.Add(loader);
        }

        [RelayCommand]
        private void StartSimulation()
        {
            if (IsSimulationStarted)
                return;

            if (OilRigs.Count == 0 || Mechanics.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы одну вышку и одного механика!";
                return;
            }

            IsSimulationStarted = true;
            StatusMessage = "Симуляция началась!";
            AddLog("Симуляция началась!");

            foreach (var rig in OilRigs)
            {
                Task.Run(() => rig.StartExtraction());
            }
        }

        private void AddLog(string message)
        {
            Dispatcher.UIThread.Post(() => LogEntries.Add($"{DateTime.Now:HH:mm:ss} {message}"));
        }

        private void OnFireStarted(object? sender, FireEventArgs e)
        {
            if (sender is OilRig rig)
            {
                StatusMessage = $"Пожар на {rig.Name}!";
                AddLog($"Пожар на {rig.Name}!");
                AssignMechanicToFire(rig);
            }
        }

        private void OnFireExtinguished(OilRig rig)
        {
            AddLog($"Пожар на {rig.Name} потушен!");
        }

        private void OnMaintenanceRequired(object? sender, MaintenanceEventArgs e)
        {
            if (sender is OilRig rig)
            {
                StatusMessage = $"{rig.Name} требует обслуживания!";
                AddLog($"{rig.Name} требует обслуживания!");
                AssignMechanicToMaintenance(rig);
            }
        }

        private void OnOilExtracted(object? sender, OilEventArgs e)
        {
            if (sender is OilRig rig)
            {
                if (e.OilLevel >= 100)
                {
                    StatusMessage = $"{rig.Name} добыла {e.OilLevel:F2} единиц нефти";
                    AssignLoaderToRig(rig);
                }
            }
        }

        private void OnOilLoaded(object? sender, OilLoadedEventArgs e)
        {
            if (sender is Loader loader)
            {
                TotalOilExtracted += e.OilAmount;
                StatusMessage = $"Погрузчик {loader.Name} загрузил {e.OilAmount:F2} единиц нефти";
                AddLog($"Погрузчик {loader.Name} загрузил {e.OilAmount:F2} единиц нефти");
            }
        }

        private void AssignMechanicToFire(OilRig rig)
        {
            var availableMechanic = Mechanics.FirstOrDefault(m => !m.IsBusy);
            if (availableMechanic != null)
            {
                Task.Run(() => availableMechanic.ExtinguishFire(rig));
            }
        }

        private void AssignMechanicToMaintenance(OilRig rig)
        {
            var availableMechanic = Mechanics.FirstOrDefault(m => !m.IsBusy);
            if (availableMechanic != null)
            {
                Task.Run(() => availableMechanic.PerformMaintenance(rig));
            }
        }

        private void AssignLoaderToRig(OilRig rig)
        {
            var availableLoader = Loaders.FirstOrDefault(l => !l.IsBusy && l.CurrentOilAmount < l.MaxCapacity);
            if (availableLoader != null)
            {
                Task.Run(() => availableLoader.LoadOil(rig));
            }
        }

        private void OnMaintenanceCompleted(OilRig rig)
        {
            AddLog($"Обслуживание на {rig.Name} завершено!");
            Task.Run(() => rig.StartExtraction());
        }
    }
} 