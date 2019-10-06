namespace BlazorContentLoader.View
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
    public partial class AssetView : ReactiveUserControl<AssetViewModel>
    {

        public AssetView()
        {

            this.InitializeComponent();

            this.WhenActivated(d =>
              {
                  this.OneWayBind(this.ViewModel, x => x.FileName, x => x.fileName.Text).DisposeWith(d);
                  this.Bind(this.ViewModel, x => x.IsEnabled, x => x.enabledBox.IsChecked).DisposeWith(d);
                  //this.Bind(this.ViewModel, x => x.SelectedProject, x => x.projectBox.SelectedItem).DisposeWith(d);
                  //this.OneWayBind(this.ViewModel, x => x.SelectedProject, x => x.selectedProjectView.ViewModel).DisposeWith(d);
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