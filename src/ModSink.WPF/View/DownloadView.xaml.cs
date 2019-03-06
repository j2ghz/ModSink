﻿using System.Reactive.Disposables;
using Humanizer;
using ModSink.Common.Client;
using ModSink.UI.ViewModel;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class DownloadView : ReactiveUserControl<DownloadViewModel>
    {
        public DownloadView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.GroupBox.Header).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.State, v => v.TbState.Text, ts=>ts.Humanize(LetterCasing.Sentence)).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Status, v => v.TbStatus.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Progress, v => v.ProgressBar.Value).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.State, v => v.ProgressBar.IsIndeterminate,
                    s => s != DownloadProgress.TransferState.Downloading).DisposeWith(d);
            });
        }
    }
}