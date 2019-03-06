﻿using System;
using System.Reactive.Disposables;
using ModSink.UI.ViewModel;
using ModSink.WPF.ViewModel;
using ReactiveUI;

namespace ModSink.WPF.View
{
    public partial class DownloadsView : ReactiveUserControl<DownloadsViewModel>
    {
        public DownloadsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Status, v => v.TbStatus.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Downloads, v => v.IcDownloads.ItemsSource).DisposeWith(d);
            });
        }
    }
}