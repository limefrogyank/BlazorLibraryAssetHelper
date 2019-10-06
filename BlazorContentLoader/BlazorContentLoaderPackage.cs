using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BlazorContentLoader.ToolWindow;
using BlazorContentLoader.ViewModel;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using ReactiveUI;
using Splat;
using Task = System.Threading.Tasks.Task;

namespace BlazorContentLoader
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideToolWindow(typeof(BlazorAssetWindow), Style = VsDockStyle.Tabbed, DockedWidth = 300, Window = "DocumentWell", Orientation = ToolWindowOrientation.Right)]
    [Guid(BlazorContentLoaderPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideToolWindow(typeof(BlazorContentLoader.ToolWindow.BlazorAssetWindow))]
    public sealed class BlazorContentLoaderPackage : AsyncPackage
    {
        /// <summary>
        /// BlazorContentLoaderPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "d4813767-a80b-4a17-a74f-e05e56dc1e31";

        public BlazorContentLoaderPackage()
        {
            Splat.ModeDetector.OverrideModeDetector(new CustomModeDetector());

            Splat.Locator.CurrentMutable.Register(() => new View.ProjectView(), typeof(IViewFor<ProjectViewModel>));
            Splat.Locator.CurrentMutable.Register(() => new View.AssetView(), typeof(IViewFor<AssetViewModel>));

            Splat.Locator.CurrentMutable.Register(() => new MainViewModel(), typeof(MainViewModel));
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Since this package might not be initialized until after a solution has finished loading,
            // we need to check if a solution has already been loaded and then handle it.
            bool isSolutionLoaded = await IsSolutionLoadedAsync();

            if (isSolutionLoaded)
            {
                HandleOpenSolution();
            }

            // Listen for subsequent solution events
            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterOpenSolution += HandleOpenSolution;

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await BasicCommand.InitializeAsync(this);

            await BlazorContentLoader.ToolWindow.BlazorAssetWindowCommand.InitializeAsync(this);
        }

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(solService);

            return new AssetWindowState
            {
                Solution = solService
            };
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            if (toolWindowType == typeof(BlazorAssetWindow))
                return "Static Js/CSS Assets for Blazor";
            else
                return base.GetToolWindowTitle(toolWindowType, id);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse(BlazorAssetWindow.WindowGuidString)) ? this : null;
        }

        #endregion

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(solService);

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

       
        //class BlazorAssetSource
        //{
        //    public string Path { get; set; }
        //    public string BasePath { get; set; }

        //    public BlazorAssetSource(string path, string basePath)
        //    {
        //        this.Path = path;
        //        this.BasePath = basePath;
        //    }
        //}
        //class BlazorAssetFile
        //{
        //    public BlazorAssetSource Source { get; set; }
        //    public string Path { get; set; }
            
        //    public BlazorAssetFile(BlazorAssetSource source, string path)
        //    {
        //        this.Source = source;
        //        this.Path = path;
        //    }
        //}

        //IEnumerable<BlazorAssetFile> GetStaticAssets(string projectPath)
        //{
        //    var projectDirectory = Path.GetDirectoryName(projectPath);
        //    var debugDir = Path.Combine(projectDirectory, "bin\\Debug\\netstandard2.0");
        //    var releaseDir = Path.Combine(projectDirectory, "bin\\Release\\netstandard2.0");
        //    DirectoryInfo d = new DirectoryInfo(debugDir);

        //    List<BlazorAssetSource> sources = new List<BlazorAssetSource>();
        //    List<BlazorAssetFile> assets = new List<BlazorAssetFile>();
        //    if (d.Exists)
        //    {
        //        FileInfo[] files = d.GetFiles("*.StaticWebAssets.xml");

        //        if (files.Length > 0)
        //        {
        //            foreach (var file in files)
        //            {
        //                var document = XDocument.Load(file.FullName);
        //                var assetSources = document.Root.Elements().Where(e => e.Name.LocalName == "ContentRoot");
        //                foreach (var source in assetSources)
        //                {
        //                    sources.Add(new BlazorAssetSource(source.Attribute("Path").Value, source.Attribute("BasePath").Value));
        //                }
        //            }
        //        }
        //        foreach (var source in sources)
        //        {
        //            DirectoryInfo sourceDir = new DirectoryInfo(source.Path);
        //            var assetFiles = sourceDir.GetFiles();
        //            foreach (var asset in assetFiles)
        //            {
        //                assets.Add(new BlazorAssetFile(source, asset.FullName));
        //            }
        //        }
        //    }

        //    return assets;
        //}


        private async void HandleOpenSolution(object sender = null, EventArgs e = null)
        {
            // Handle the open solution and try to do as much work
            // on a background thread as possible
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;


            foreach (EnvDTE.Project project in ProjectUtils.GetProjects(solService as IVsSolution))
            {
                //var staticAssetPaths = this.GetStaticAssets(project.FileName);

                Debug.WriteLine(project.FullName);


            }

        }


    }
}
