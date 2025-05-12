using System;
using System.Threading.Tasks;

namespace dot.Models
{
    public class OilRig
    {
        public event EventHandler<OilEventArgs>? OilExtracted;
        public event EventHandler<FireEventArgs>? FireStarted;
        public event EventHandler<MaintenanceEventArgs>? MaintenanceRequired;

        private readonly Random _random = new Random();
        private bool _isOnFire;
        private bool _needsMaintenance;
        private double _oilLevel;
        private readonly object _lockObject = new object();
        private bool _oilReadyForExtraction = false;

        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        public bool IsOnFire => _isOnFire;
        public bool NeedsMaintenance => _needsMaintenance;
        public double OilLevel => _oilLevel;

        public OilRig(string name)
        {
            Name = name;
            _oilLevel = 0;
        }

        public async Task StartExtraction()
        {
            while (true)
            {
                if (_isOnFire || _needsMaintenance)
                {
                    await Task.Delay(1000);
                    continue;
                }

                await Task.Delay(1000);
                
                lock (_lockObject)
                {
                    if (_oilLevel >= 100)
                    {
                        if (!_oilReadyForExtraction)
                        {
                            _oilReadyForExtraction = true;
                            OilExtracted?.Invoke(this, new OilEventArgs(_oilLevel));
                        }
                        continue;
                    }

                    _oilLevel += 5;
                    
                    // 2.15% chance of fire (every ~1.5 minutes)
                    if (_random.NextDouble() < 0.0215)
                    {
                        _isOnFire = true;
                        FireStarted?.Invoke(this, new FireEventArgs());
                    }

                    // 1.67% chance of needing maintenance (every 2 minutes)
                    if (_random.NextDouble() < 0.0167)
                    {
                        _needsMaintenance = true;
                        MaintenanceRequired?.Invoke(this, new MaintenanceEventArgs());
                    }
                }
            }
        }

        public void ExtinguishFire()
        {
            _isOnFire = false;
        }

        public void PerformMaintenance()
        {
            _needsMaintenance = false;
        }

        public double ExtractOil()
        {
            lock (_lockObject)
            {
                var extracted = _oilLevel;
                _oilLevel = 0;
                _oilReadyForExtraction = false;
                return extracted;
            }
        }
    }

    public class OilEventArgs : EventArgs
    {
        public double OilLevel { get; }

        public OilEventArgs(double oilLevel)
        {
            OilLevel = oilLevel;
        }
    }

    public class FireEventArgs : EventArgs
    {
    }

    public class MaintenanceEventArgs : EventArgs
    {
    }
} 