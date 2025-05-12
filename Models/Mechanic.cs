using System;
using System.Threading.Tasks;

namespace dot.Models
{
    public class Mechanic
    {
        public event EventHandler<MaintenanceCompletedEventArgs>? MaintenanceCompleted;
        public event EventHandler<FireExtinguishedEventArgs>? FireExtinguished;

        private readonly Random _random = new Random();
        private bool _isBusy;
        private readonly object _lockObject = new object();

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public bool IsBusy => _isBusy;

        public Mechanic(string name)
        {
            Name = name;
        }

        public async Task PerformMaintenance(OilRig rig)
        {
            if (_isBusy) return;

            lock (_lockObject)
            {
                _isBusy = true;
            }

            // Simulate maintenance time
            await Task.Delay(_random.Next(2000, 5000));
            
            rig.PerformMaintenance();
            MaintenanceCompleted?.Invoke(this, new MaintenanceCompletedEventArgs(rig));

            lock (_lockObject)
            {
                _isBusy = false;
            }
        }

        public async Task ExtinguishFire(OilRig rig)
        {
            if (_isBusy) return;

            lock (_lockObject)
            {
                _isBusy = true;
            }

            // Simulate fire extinguishing time
            await Task.Delay(_random.Next(3000, 7000));
            
            rig.ExtinguishFire();
            FireExtinguished?.Invoke(this, new FireExtinguishedEventArgs(rig));

            lock (_lockObject)
            {
                _isBusy = false;
            }
        }
    }

    public class MaintenanceCompletedEventArgs : EventArgs
    {
        public OilRig Rig { get; }

        public MaintenanceCompletedEventArgs(OilRig rig)
        {
            Rig = rig;
        }
    }

    public class FireExtinguishedEventArgs : EventArgs
    {
        public OilRig Rig { get; }

        public FireExtinguishedEventArgs(OilRig rig)
        {
            Rig = rig;
        }
    }
} 