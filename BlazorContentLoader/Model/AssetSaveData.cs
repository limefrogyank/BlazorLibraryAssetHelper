using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.Model
{
    public class AssetSaveData : ReactiveObject
    {
        public string Path { get; set; }
        //public int Index { get; set; }

        private bool _isEnabled;
        public bool IsEnabled { get => _isEnabled; set => this.RaiseAndSetIfChanged(ref _isEnabled, value); }

        public AssetSaveData(string path, bool enabled)
        {
            Path = path;
            //Index = index;
            IsEnabled = enabled;
        }
    }
}
