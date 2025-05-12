using System;
using System.Threading.Tasks;

namespace dot.Models
{
    public class Loader
    {
        public event EventHandler<OilLoadedEventArgs>? OilLoaded;

        private readonly Random _random = new Random();
        private bool _isBusy;
        private readonly object _lockObject = new object();
        private double _currentOilAmount;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public bool IsBusy => _isBusy;
        public double CurrentOilAmount => _currentOilAmount;
        public double MaxCapacity { get; } = 1000;

        public Loader(string name)
        {
            Name = name;
            _currentOilAmount = 0;
        }

        public async Task LoadOil(OilRig rig)
        {
            if (_isBusy) return;

            lock (_lockObject)
            {
                _isBusy = true;
            }

            // Simulate loading time
            await Task.Delay(_random.Next(2000, 4000));

            var oilAmount = rig.ExtractOil();
            _currentOilAmount = oilAmount;
            OilLoaded?.Invoke(this, new OilLoadedEventArgs(oilAmount));

            lock (_lockObject)
            {
                _isBusy = false;
            }
        }

        public void UnloadOil()
        {
            _currentOilAmount = 0;
        }
    }

    public class OilLoadedEventArgs : EventArgs
    {
        public double OilAmount { get; }

        public OilLoadedEventArgs(double oilAmount)
        {
            OilAmount = oilAmount;
        }
    }
} 