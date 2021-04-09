using KT_Interface.Core.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.ViewModels
{
    class BindableDriveInfo : BindableBase
    {
        public string Name { get; private set; }
        public double TotalSize { get; private set; }

        private double _usageSize;
        public double UsageSize
        {
            get 
            {
                return _usageSize;
            }
            set 
            {
                SetProperty(ref _usageSize, value);
            }
        }

        public BindableDriveInfo(string name, double totalSize)
        {
            Name = name;
            TotalSize = totalSize;
        }
    }

    class UsageViewModel : BindableBase
    {
        public ObservableCollection<BindableDriveInfo> DriveInfos { get; private set; }

        private double _cpuUsage;
        public double CpuUsage
        {
            get 
            {
                return _cpuUsage;
            }
            set 
            {
                SetProperty(ref _cpuUsage, value);
            }
        }

        private double _memoryUsage;
        public double MemoryUsage
        {
            get 
            {
                return _memoryUsage;
            }
            set
            {
                SetProperty(ref _memoryUsage, value);
            }
        }

        private double _memoryTotal;
        public double MemoryTotal
        {
            get
            {
                return _memoryTotal;
            }
            set
            {
                SetProperty(ref _memoryTotal, value);
            }
        }

        public UsageViewModel(UsageService usageService)
        {
            MemoryTotal = usageService.MemoryTotal / 1024;

            DriveInfos = new ObservableCollection<BindableDriveInfo>();
            foreach (var info in usageService.DriveInfos)
                DriveInfos.Add(new BindableDriveInfo(info.Name, info.TotalSize / 1024.0 / 1024.0 / 1024.0));

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    CpuUsage = usageService.CpuUsage;
                    MemoryUsage = MemoryTotal - (usageService.MemoryAvailable / 1024);

                    foreach (var info in usageService.DriveInfos)
                    {
                        var founded = DriveInfos.First(d => d.Name == info.Name);
                        founded.UsageSize = founded.TotalSize - info.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
                    }

                    await Task.Delay(1000);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
