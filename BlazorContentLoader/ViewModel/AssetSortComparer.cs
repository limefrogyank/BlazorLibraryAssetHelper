
using BlazorContentLoader.Model;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorContentLoader.ViewModel
{
    public class AssetSortComparer : IComparer<AssetViewModel>
    {
        //private SourceList<AssetSaveData> _saveData;
        private IReadOnlyCollection<AssetSaveData> _saveData;

        //public AssetSortComparer(SourceList<AssetSaveData> saveData)
        //{
        //    _saveData = saveData;
        //}

        public AssetSortComparer(IReadOnlyCollection<AssetSaveData> saveData)
        {
            _saveData = saveData;
        }

        public int Compare(AssetViewModel x, AssetViewModel y)
        {
            var first = _saveData.FirstOrDefault(a => a.Path == x.OriginalPath);
            var second = _saveData.FirstOrDefault(a => a.Path == y.OriginalPath);
            return _saveData.IndexOf(first).CompareTo(_saveData.IndexOf(second));
        }
    }
}
