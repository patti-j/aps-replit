using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class CtpRequest : BaseEntity, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int JobTemplateId { get; set; }
        public virtual JobTemplate JobTemplate { get; set; }
        public int PADetailsId { get; set; }
        public virtual PADetails PADetails { get; set; }
        private int _requestId;
        public int RequestId
        {
            get => _requestId;
            set
            {
                if (_requestId != value)
                {
                    _requestId = value;
                    OnPropertyChanged(nameof(RequestId));
                }
            }
        }

        private string _itemExternalId = string.Empty;
        public string ItemExternalId
        {
            get => _itemExternalId;
            set
            {
                if (_itemExternalId != value)
                {
                    _itemExternalId = value;
                    OnPropertyChanged(nameof(ItemExternalId));
                }
            }
        }

        private string _warehouseExternalId = string.Empty;
        public string WarehouseExternalId
        {
            get => _warehouseExternalId;
            set
            {
                if (_warehouseExternalId != value)
                {
                    _warehouseExternalId = value;
                    OnPropertyChanged(nameof(WarehouseExternalId));
                }
            }
        }

        private decimal _requiredQty;
        public decimal RequiredQty
        {
            get => _requiredQty;
            set
            {
                if (_requiredQty != value)
                {
                    _requiredQty = value;
                    OnPropertyChanged(nameof(RequiredQty));
                }
            }
        }

        private DateTime _needDate = DateTime.MinValue;
        public DateTime NeedDate
        {
            get => _needDate;
            set
            {
                if (_needDate != value)
                {
                    _needDate = value;
                    OnPropertyChanged(nameof(NeedDate));
                }
            }
        }

        private string _requiredPathId = string.Empty;
        public string RequiredPathId
        {
            get => _requiredPathId;
            set
            {
                if (_requiredPathId != value)
                {
                    _requiredPathId = value;
                    OnPropertyChanged(nameof(RequiredPathId));
                }
            }
        }

        private int _priority;
        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        private DateTime _scheduledStart;
        public DateTime ScheduledStart
        {
            get => _scheduledStart;
            set
            {
                if (_scheduledStart != value)
                {
                    _scheduledStart = value;
                    OnPropertyChanged(nameof(ScheduledStart));
                }
            }
        }

        private DateTime _scheduledFinish;
        public DateTime ScheduledFinish
        {
            get => _scheduledFinish;
            set
            {
                if (_scheduledFinish != value)
                {
                    _scheduledFinish = value;
                    OnPropertyChanged(nameof(ScheduledFinish));
                }
            }
        }

        private string _scheduledPath = string.Empty;
        public string ScheduledPath
        {
            get => _scheduledPath;
            set
            {
                if (_scheduledPath != value)
                {
                    _scheduledPath = value;
                    OnPropertyChanged(nameof(ScheduledPath));
                }
            }
        }

        private bool _inventoryInquiry;
        public bool InventoryInquiry
        {
            get => _inventoryInquiry;
            set
            {
                if (_inventoryInquiry != value)
                {
                    _inventoryInquiry = value;
                    OnPropertyChanged(nameof(InventoryInquiry));
                }
            }
        }
        private bool _hotOff;
        public bool HotOff
        {
            get => _hotOff;
            set
            {
                if (_hotOff != value)
                {
                    _hotOff = value;
                    OnPropertyChanged(nameof(HotOff));
                }
            }
        }
        
        private DateTime _reserveCapacityAndMaterialsUntil;
        public DateTime ReserveCapacityAndMaterialsUntil
        {
            get => _reserveCapacityAndMaterialsUntil;
            set
            {
                if (_reserveCapacityAndMaterialsUntil != value)
                {
                    _reserveCapacityAndMaterialsUntil = value;
                    OnPropertyChanged(nameof(ReserveCapacityAndMaterialsUntil));
                }
            }
        }

        private string _schedulingType = string.Empty;
        public string SchedulingType
        {
            get => _schedulingType;
            set
            {
                if (_schedulingType != value)
                {
                    _schedulingType = value;
                    OnPropertyChanged(nameof(SchedulingType));
                }
            }
        }

        private decimal _revenue;
        public decimal Revenue
        {
            get => _revenue;
            set
            {
                if (_revenue != value)
                {
                    _revenue = value;
                    OnPropertyChanged(nameof(Revenue));
                }
            }
        }

        private decimal _throughput;
        public decimal Throughput
        {
            get => _throughput;
            set
            {
                if (_throughput != value)
                {
                    _throughput = value;
                    OnPropertyChanged(nameof(Throughput));
                }
            }
        }

        private bool _isHot;
        public bool IsHot
        {
            get => _isHot;
            set
            {
                if (_isHot != value)
                {
                    _isHot = value;
                    OnPropertyChanged(nameof(IsHot));
                }
            }
        }

        private string _hotReason = string.Empty;
        public string HotReason
        {
            get => _hotReason;
            set
            {
                if (_hotReason != value)
                {
                    _hotReason = value;
                    OnPropertyChanged(nameof(HotReason));
                }
            }
        }

        private string _status = string.Empty;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
    }

    // Define a class for holding results
    public class CtpResult
    {
        public DateTime ScheduledStart { get; set; } = DateTime.MinValue;
        public DateTime ScheduledFinish { get; set; } = DateTime.MinValue;
        public string Status { get; set; } = "Unknown"; // Or "Late"
        public double DaysLate { get; set; } = 0;
        public double Cost { get; set; } = 0;
        public string ScheduledPath { get; set; } = "Unknown";
    }
}
