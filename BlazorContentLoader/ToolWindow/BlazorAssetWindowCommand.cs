﻿using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BlazorContentLoader.ToolWindow
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BlazorAssetWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("432535f1-c427-4ac2-b6bb-636691843593");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlazorAssetWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private BlazorAssetWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand((s,e)=> Execute(package), menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BlazorAssetWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in BlazorAssetWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new BlazorAssetWindowCommand(package, commandService);
        }

        private static void Execute(AsyncPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                ToolWindowPane window = await package.ShowToolWindowAsync(
                    typeof(BlazorAssetWindow),
                    0,
                    create: true,
                    cancellationToken: package.DisposalToken);
            });
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        //private void Execute(object sender, EventArgs e)
        //{
        //    ThreadHelper.ThrowIfNotOnUIThread();

        //    // Get the instance number 0 of this tool window. This window is single instance so this instance
        //    // is actually the only one.
        //    // The last flag is set to true so that if the tool window does not exists it will be created.
        //    ToolWindowPane window = this.package.FindToolWindow(typeof(BlazorAssetWindow), 0, true);
        //    if ((null == window) || (null == window.Frame))
        //    {
        //        throw new NotSupportedException("Cannot create tool window");
        //    }

        //    IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
        //    Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        //}
    }
}
