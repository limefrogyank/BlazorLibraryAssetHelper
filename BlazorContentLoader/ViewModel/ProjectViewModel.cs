using BlazorContentLoader.Model;
using BlazorContentLoader.View;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Cache;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlazorContentLoader.ViewModel
{
    public class ProjectViewModel : ReactiveObject
    {
        private IObservableCache<BlazorAssetFile, string> _fileCache;

        public string ProjectPath { get; set; }

        public string ProjectName => Path.GetFileName(ProjectPath);

        private SourceList<AssetSaveData> _saveData = new SourceList<AssetSaveData>();

        private ObservableCollectionExtended<AssetViewModel> _assets = new ObservableCollectionExtended<AssetViewModel>();
        public IObservableCollection<AssetViewModel> Assets => _assets;

        private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

        public ReactiveCommand<AssetViewModel, Unit> UpCommand { get; }
        public ReactiveCommand<AssetViewModel, Unit> DownCommand { get; }
        public ReactiveCommand<Unit, Unit> GenerateHtmlCommand { get; }

        private AssetViewModel _selectedItem;
        public AssetViewModel SelectedItem { get => _selectedItem; set => this.RaiseAndSetIfChanged(ref _selectedItem, value); }

        public ProjectViewModel(IGroup<BlazorAssetFile, string, string> group)
        {
            _fileCache = group.Cache;
            this.ProjectPath = group.Key;

            var data = LoadStoredData();
            _saveData.AddRange(data);


            _saveData.Connect()
                .Throttle(TimeSpan.FromSeconds(3))
                //.WhenPropertyChanged(x=>x.IsEnabled)
                .Do(x => SaveData())
                .Subscribe();

            _saveData.Connect()
                .Throttle(TimeSpan.FromSeconds(3))
                .WhenPropertyChanged(x=>x.IsEnabled)
                .Do(x => SaveData())
                .Subscribe();

            var sortObservable = _saveData.Connect().ToCollection().Select(x => new AssetSortComparer(x));


            //var filterPredicate = _enabledAssets.Connect().ToCollection().Select(list => (Func<AssetViewModel, bool>)(vm => list.Contains(vm.FileName)));

            var vms = _fileCache.Connect()
                .Filter(x => x.Path.EndsWith(".js") || x.Path.EndsWith(".css"))
                .Transform(x => new AssetViewModel(x, _saveData))
                .Sort(sortObservable)
                .Publish();


            vms.ObserveOn(RxApp.MainThreadScheduler)
                .Bind(_assets)
                .Subscribe();


            vms.WhenPropertyChanged(x => x.IsEnabled)
                .Do(x=>
                {
                    var saveItem = _saveData.Items.FirstOrDefault(saveData => saveData.Path == x.Sender.OriginalPath);
                    saveItem.IsEnabled = x.Value;
                })
                .Subscribe();

            vms.Connect();


            UpCommand = ReactiveCommand.Create<AssetViewModel, Unit>(assetVM =>
            {
                var saveItem = _saveData.Items.FirstOrDefault(x => x.Path == assetVM.OriginalPath);
                var index = _saveData.Items.IndexOf(saveItem);
                if (index > 0)
                    _saveData.Move(index, index - 1);

                SelectedItem = assetVM;
                
                return Unit.Default;
            });

            DownCommand = ReactiveCommand.Create<AssetViewModel, Unit>(assetVM =>
            {
                var saveItem = _saveData.Items.FirstOrDefault(x => x.Path == assetVM.OriginalPath);
                var index = _saveData.Items.IndexOf(saveItem);
                if (index < _saveData.Items.Count() - 1)
                    _saveData.Move(index, index + 1);

                SelectedItem = assetVM;

                return Unit.Default;
            });

            GenerateHtmlCommand = ReactiveCommand.Create<Unit, Unit>(_ =>
            {
                string text = "";
                foreach (var asset in _assets)
                {
                    if (asset.IsEnabled)
                    {
                        if (asset.HtmlPath.EndsWith("js"))
                        {
                            text += $"<script src=\"{asset.HtmlPath}\"></script>{Environment.NewLine}";
                        }
                        else if (asset.HtmlPath.EndsWith("css"))
                        {
                            text += $"<link rel=\"stylesheet\" type=\"text/css\" href=\"{asset.HtmlPath}\" />{Environment.NewLine}";
                        }
                    }
                }

                var dialog = new HtmlDialogWindow();
                dialog.Content = new System.Windows.Controls.TextBox()
                {
                    Text = text,
                    FontFamily = new System.Windows.Media.FontFamily("Consolas")
                };
                dialog.ShowModal();
               
                return Unit.Default;
            });
        }

        List<AssetSaveData> LoadStoredData()
        {
            lockSlim.EnterReadLock();
            var data = new List<AssetSaveData>();
            var projectDirectory = Path.GetDirectoryName(ProjectPath);
            var settingsFile = Path.Combine(projectDirectory, "assetPrefs.xml");
            if (File.Exists(settingsFile))
            {
                using (var fileStream = File.OpenText(settingsFile))
                {
                    var doc = XDocument.Load(fileStream);
                    foreach (var element in doc.Root.Elements())
                    {
                        var isEnabled = bool.Parse(element.Attribute("Enabled").Value);
                        var pathName = element.Attribute("Path").Value;
                        data.Add(new AssetSaveData(pathName, isEnabled));
                    }
                }
            }
            lockSlim.ExitReadLock();
            return data;
        }

        void SaveData()
        {
            lockSlim.EnterWriteLock();
            var projectDirectory = Path.GetDirectoryName(ProjectPath);
            var settingsFile = Path.Combine(projectDirectory, "assetPrefs.xml");
            if (File.Exists(settingsFile))
            {
                File.Delete(settingsFile);
            }

            using (var fileStream = File.Create(settingsFile))
            {
                var doc = new XDocument(new XElement("Root"));
                foreach (var item in _saveData.Items)
                {
                    var element = new XElement("Data");
                    element.SetAttributeValue("Enabled", item.IsEnabled);
                    element.SetAttributeValue("Path", item.Path);
                    doc.Root.Add(element);
                }
                doc.Save(fileStream);
            }
            

            lockSlim.ExitWriteLock();
        }


    }
}
