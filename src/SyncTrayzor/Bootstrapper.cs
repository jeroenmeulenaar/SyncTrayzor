﻿using Stylet;
using StyletIoC;
using SyncTrayzor.NotifyIcon;
using SyncTrayzor.Pages;
using SyncTrayzor.Services;
using SyncTrayzor.SyncThing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SyncTrayzor
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void Configure()
        {
            Stylet.Logging.LogManager.Enabled = true;
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<IApplicationState>().ToInstance(new ApplicationState(this.Application));
            builder.Bind<IConfigurationProvider>().To<ConfigurationProvider>().InSingletonScope();
            builder.Bind<AutostartProvider>().ToSelf().InSingletonScope();
            builder.Bind<ConfigurationApplicator>().ToSelf().InSingletonScope();
            builder.Bind<ISyncThingApiClient>().To<SyncThingApiClient>().InSingletonScope();
            builder.Bind<ISyncThingEventWatcher>().To<SyncThingEventWatcher>().InSingletonScope();
            builder.Bind<ISyncThingProcessRunner>().To<SyncThingProcessRunner>().InSingletonScope();
            builder.Bind<ISyncThingManager>().To<SyncThingManager>().InSingletonScope();
            builder.Bind<ISyncThingConnectionsWatcher>().To<SyncThingConnectionsWatcher>().InSingletonScope();
            builder.Bind<INotifyIconManager>().To<NotifyIconManager>().InSingletonScope();
            builder.Bind<IWatchedFolderMonitor>().To<WatchedFolderMonitor>().InSingletonScope();
        }

        protected override void Launch()
        {
            var notifyIconManager = this.Container.Get<INotifyIconManager>();
            notifyIconManager.Setup((INotifyIconDelegate)this.RootViewModel);
            this.Container.Get<ConfigurationApplicator>().ApplyConfiguration();

            if (this.Args.Length > 0 && this.Args[0] == "-minimized")
                this.Container.Get<INotifyIconManager>().EnsureIconVisible();
            else
                base.Launch();

            var config = this.Container.Get<IConfigurationProvider>().Load();
            if (config.StartSyncThingAutomatically)
                ((ShellViewModel)this.RootViewModel).Start();
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            this.Container.Dispose();
        }

        protected override void OnUnhandledExecption(DispatcherUnhandledExceptionEventArgs e)
        {
            var windowManager = this.Container.Get<IWindowManager>();

            var configurationException = e.Exception as ConfigurationException;
            if (configurationException != null)
            {
                windowManager.ShowMessageBox(String.Format("Configuration Error: {0}", configurationException.Message), "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                windowManager.ShowMessageBox(String.Format("Unhandled error: {0}", e.Exception.Message), "Unhandled error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
