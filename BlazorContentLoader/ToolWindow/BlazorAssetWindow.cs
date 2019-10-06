namespace BlazorContentLoader.ToolWindow
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(WindowGuidString)]
    public class BlazorAssetWindow : ToolWindowPane
    {
        private AssetWindowState _state;
        public const string WindowGuidString = "324cfc94-2378-4834-83d7-6fd8022df893";
        /// <summary>
        /// Initializes a new instance of the <see cref="BlazorAssetWindow"/> class.
        /// </summary>
        public BlazorAssetWindow(AssetWindowState state) : base()
        {
            //IVsSolution solService = this.GetService(typeof(SVsSolution)) as IVsSolution;
            //_state = new AssetWindowState { Solution = solService };
            this._state = state;
            this.Caption = "Blazor JS & CSS Assets";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            var assetControl = new BlazorAssetWindowControl(_state);
            
            this.Content = assetControl;
        }
    }
}
