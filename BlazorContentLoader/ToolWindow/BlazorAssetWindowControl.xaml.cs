namespace BlazorContentLoader.ToolWindow
{
    using ReactiveUI;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reactive.Disposables;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModel;

    /// <summary>
    /// Interaction logic for BlazorAssetWindowControl.
    /// </summary>
    public partial class BlazorAssetWindowControl : ReactiveUserControl<MainViewModel>
    {
        private AssetWindowState _state;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlazorAssetWindowControl"/> class.
        /// </summary>
        public BlazorAssetWindowControl(AssetWindowState state)
        {

            _state = state;
            //ViewModel = new BlazorAssetsViewModel(state);
            this.InitializeComponent();

            this.WhenActivated(d =>
              {
                  this.ViewModel = (MainViewModel)Splat.Locator.Current.GetService(typeof(MainViewModel));
                  this.ViewModel.Initialize(_state);

                  this.OneWayBind(this.ViewModel, x => x.Projects, x => x.projectBox.ItemsSource).DisposeWith(d);
                  this.Bind(this.ViewModel, x => x.SelectedProject, x => x.projectBox.SelectedItem).DisposeWith(d);
                  this.OneWayBind(this.ViewModel, x => x.SelectedProject, x => x.selectedProjectView.ViewModel).DisposeWith(d);
              });
        }

       

        ///// <summary>
        ///// Handles click on the button by displaying a message box.
        ///// </summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event args.</param>
        //[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        //[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        //private void button1_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show(
        //        string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
        //        "BlazorAssetWindow");
        //}

    }
}