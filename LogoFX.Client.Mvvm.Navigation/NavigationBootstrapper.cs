﻿using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using LogoFX.Client.Bootstrapping;
using LogoFX.Client.Bootstrapping.Contracts;
using LogoFX.Core;
using Solid.Practices.IoC;

namespace LogoFX.Client.Mvvm.Navigation
{
    public class NavigationBootstrapper<TRootViewModel, TIocContainer> : 
        BootstrapperContainerBase<TRootViewModel, TIocContainer> 
        where TRootViewModel : class where TIocContainer : class, IIocContainer, IBootstrapperAdapter, new()
    {
        private NavigationService _navigationService = new NavigationService();        
        private INavigationService NavigationService
        {
            get { return _navigationService ?? (_navigationService = new NavigationService()); }
        }

        protected NavigationBootstrapper(bool useApplication = true)
            :base(useApplication)
        {
        }

        protected NavigationBootstrapper(TIocContainer iocContainer, bool useApplication=true)
            :base(iocContainer, useApplication)
        {
            
        }

        protected override void OnConfigure(TIocContainer container)
        {
            base.OnConfigure(container);
            container.RegisterInstance(NavigationService);
            OnRegisterRoot(NavigationService);
            OnPrepareNavigation(NavigationService);
        }        

        protected virtual void OnRegisterRoot(INavigationService navigationService)
        {
            navigationService.RegisterViewModel<TRootViewModel>().AsRoot();
        }

        protected virtual void OnPrepareNavigation(INavigationService navigationService)
        {
            //register navigation view-models
            AssemblySource.Instance.ToArray()
                .SelectMany(ass => ass.GetTypes())
                .Where(type => type != typeof(TRootViewModel) && type.IsClass)
                .Select(type => new
                {
                    Type = type,
                    Attr = type.GetCustomAttribute<NavigationViewModelAttribute>()
                })
                .Where(x => x.Attr != null)
                .ForEach(x => _navigationService.RegisterAttr(x.Type, x.Attr));
        }
    }
}