﻿using Autofac;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Bootstrappers
{
    public class AutofacBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IContainer container;

        private object _rootViewModel;
        protected virtual object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            var builder = new ContainerBuilder();
            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);
            this.container = builder.Build();
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(ContainerBuilder builder)
        {
            var viewManager = new ViewManager(this.GetInstance, new List<Assembly>() { this.GetType().Assembly });
            builder.RegisterInstance<IViewManager>(viewManager);

            builder.RegisterInstance<IWindowManagerConfig>(this);
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MessageBoxViewModel>().As<IMessageBoxViewModel>(); // Not singleton!
            builder.RegisterAssemblyTypes(this.GetType().Assembly);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(ContainerBuilder builder) { }

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }

        public override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        public override void Dispose()
        {
            base.Dispose();
            ScreenExtensions.TryDispose(this._rootViewModel);
            if (this.container != null)
                this.container.Dispose();
        }
    }
}
