using BlazorContentLoader.Model;
using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.ViewModel
{
    public class AssetViewModel : ReactiveObject
    {
        private BlazorAssetFile _blazorAssetFile;

        public string OriginalPath => _blazorAssetFile.Path;

        public string HtmlPath => _blazorAssetFile.Source.BasePath + "/" + Path.GetFileName(_blazorAssetFile.Path);

        public string FileName => _blazorAssetFile.Source.BasePath.Split('/').Last() + "/" + Path.GetFileName(_blazorAssetFile.Path);


        private bool _isEnabled = false;
        public bool IsEnabled { get => _isEnabled; set => this.RaiseAndSetIfChanged(ref _isEnabled, value); }

        //private int _index = -1;
        //public int Index { get => _index; set => this.RaiseAndSetIfChanged(ref _index, value); }


        private SourceList<AssetSaveData> _saveData;

        public AssetViewModel(BlazorAssetFile blazorAssetFile, SourceList<AssetSaveData> saveData)
        {
            _blazorAssetFile = blazorAssetFile;
            _saveData = saveData;

            var initialSetting = _saveData.Items.FirstOrDefault(x => x.Path == _blazorAssetFile.Path);
            if (initialSetting != null)
            {
                IsEnabled = initialSetting.IsEnabled;
                //Index = initialSetting.Index;
            }
            else
            {
                //this.Index = _saveData.Count;
            }

            //this.WhenAnyValue(x => x.IsEnabled, x => x.Index).Subscribe<(bool,int)>(comb =>
            //  {
            //      _saveData.AddOrUpdate(new AssetSaveData(_blazorAssetFile.Path, comb.Item2, comb.Item1));
            //  });
            //this.WhenAnyValue(x => x.IsEnabled).Subscribe(isEnabled =>
            //{
            //    _saveData.AddOrUpdate(new AssetSaveData(_blazorAssetFile.Path, comb.Item2, comb.Item1));
            //});
        }

    }
}
