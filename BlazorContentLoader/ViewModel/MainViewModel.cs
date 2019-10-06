using BlazorContentLoader.Model;
using BlazorContentLoader.ToolWindow;
using DynamicData;
using DynamicData.Binding;
using Microsoft.VisualStudio.Shell.Interop;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BlazorContentLoader.ViewModel
{
    public class MainViewModel : ReactiveObject, IActivatableViewModel
    {
        private AssetWindowState _state;

        private SourceCache<BlazorAssetFile, string> fileCache = new SourceCache<BlazorAssetFile, string>(x => x.Path);

        private ObservableCollectionExtended<ProjectViewModel> projects = new ObservableCollectionExtended<ProjectViewModel>();
        public IObservableCollection<ProjectViewModel> Projects => projects;

        private ProjectViewModel _selectedProject;
        public ProjectViewModel SelectedProject { get => _selectedProject; set => this.RaiseAndSetIfChanged(ref _selectedProject, value); }

        public MainViewModel()
        {
            Activator = new ViewModelActivator();
           
            fileCache.Connect()
                .Group(x => x.ProjectPath)
                .Transform(x => new ProjectViewModel(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(projects)
                .Do(x => { if (SelectedProject == null && projects.Count > 0) SelectedProject = projects.First(); })
                .Subscribe();

        }

        public ViewModelActivator Activator { get; set; }


        public void Initialize(AssetWindowState state)
        {
            _state = state;

            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            foreach (EnvDTE.Project project in ProjectUtils.GetProjects(_state.Solution as IVsSolution))
            {
                
                var staticAssetPaths = this.GetStaticAssets(project.FileName);
                fileCache.AddOrUpdate(staticAssetPaths);
            }
        }

        private IEnumerable<BlazorAssetFile> GetStaticAssets(string projectPath)
        {
            var projectDirectory = Path.GetDirectoryName(projectPath);
            var debugDir = Path.Combine(projectDirectory, "bin\\Debug\\netstandard2.0");
            var releaseDir = Path.Combine(projectDirectory, "bin\\Release\\netstandard2.0");
            DirectoryInfo d = new DirectoryInfo(debugDir);

            List<BlazorAssetSource> sources = new List<BlazorAssetSource>();
            List<BlazorAssetFile> assets = new List<BlazorAssetFile>();
            if (d.Exists)
            {
                FileInfo[] files = d.GetFiles("*.StaticWebAssets.xml");

                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        var document = XDocument.Load(file.FullName);
                        var assetSources = document.Root.Elements().Where(e => e.Name.LocalName == "ContentRoot");
                        foreach (var source in assetSources)
                        {
                            sources.Add(new BlazorAssetSource(source.Attribute("Path").Value, source.Attribute("BasePath").Value));
                        }
                    }
                }
                foreach (var source in sources)
                {
                    DirectoryInfo sourceDir = new DirectoryInfo(source.Path);
                    var assetFiles = sourceDir.GetFiles();
                    foreach (var asset in assetFiles)
                    {
                        assets.Add(new BlazorAssetFile(source, asset.FullName, projectPath));
                    }
                }
            }

            return assets;
        }

    }
}
