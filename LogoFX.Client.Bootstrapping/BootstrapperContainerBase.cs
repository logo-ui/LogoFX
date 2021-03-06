﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Caliburn.Micro;
using LogoFX.Client.Bootstrapping.Contracts;
using Solid.Practices.Composition.Desktop;
using Solid.Practices.IoC;
using Solid.Practices.Modularity;

namespace LogoFX.Client.Bootstrapping
{    
    public class BootstrapperContainerBase<TRootViewModel, TIocContainer> :
#if !WinRT
 BootstrapperBase
#else
        CaliburnApplication
#endif
        where TRootViewModel : class
        where TIocContainer : class, IIocContainer, IBootstrapperAdapter, new()
    {
        private readonly Dictionary<string, Type> _typedic = new Dictionary<string, Type>();
        private IBootstrapperAdapter _bootstrapperAdapter;
        private object _defaultLifetimeScope;
        private TIocContainer _iocContainer;

        protected BootstrapperContainerBase(bool useApplication = true)
            :base(useApplication)
        {
            Initialize();
        }

        protected BootstrapperContainerBase(TIocContainer iocContainer, bool useApplication=true)            
            :base(useApplication)
        {
            _iocContainer = iocContainer;
            Initialize();
        }

        private void DisplayRootView()
        {
            DisplayRootViewFor(typeof(TRootViewModel));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootView();
        }

        /// <summary>
        /// Override to configure the framework and setup your <c>IoC</c> container.
        /// </summary>
        protected override void Configure()
        {
            base.Configure();
            Dispatch.Current.InitializeDispatch();
            //overrided for performance reasons (Assembly caching)
            ViewLocator.LocateTypeForModelType = (modelType, displayLocation, context) =>
            {
                Debug.Assert(modelType != null && modelType.FullName != null);
                var viewTypeName = modelType.FullName.Substring(0, modelType.FullName.IndexOf("`") < 0
                    ? modelType.FullName.Length
                    : modelType.FullName.IndexOf("`")
                    ).Replace("Model", string.Empty);

                if (context != null)
                {
                    viewTypeName = viewTypeName.Remove(viewTypeName.Length - 4, 4);
                    viewTypeName = viewTypeName + "." + context;
                }

                Type viewType;
                if (!_typedic.TryGetValue(viewTypeName, out viewType))
                {
                    _typedic[viewTypeName] = viewType = (from assmebly in AssemblySource.Instance
                                                         from type in assmebly.GetExportedTypes()
                                                         where type.FullName == viewTypeName
                                                         select type).FirstOrDefault();
                }

                return viewType;
            };
            ViewLocator.LocateForModelType = (modelType, displayLocation, context) =>
            {
                var viewType = ViewLocator.LocateTypeForModelType(modelType, displayLocation, context);

                return viewType == null
                    ? new TextBlock { Text = string.Format("Cannot find view for\nModel: {0}\nContext: {1} .", modelType, context) }
                    : ViewLocator.GetOrCreateViewType(viewType);
            };
            _bootstrapperAdapter = _iocContainer;

            RegisterCommon(_iocContainer);
            RegisterViewsAndViewModels(_iocContainer);
            OnConfigure(_iocContainer);
        }        

        private static void RegisterCommon(IIocContainer iocContainer)
        {
            iocContainer.RegisterSingleton<IWindowManager, WindowManager>();
            iocContainer.RegisterSingleton<TRootViewModel, TRootViewModel>();
            iocContainer.RegisterInstance(iocContainer);
        }

        /// <summary>
        /// Called on configure.
        /// </summary>
        /// <param name="container">The container.</param>
        protected virtual void OnConfigure(TIocContainer container)
        {
        }

        #region Overrides

        protected override object GetInstance(Type service, string key)
        {
            return _bootstrapperAdapter.GetInstance(service);
        }
        
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _bootstrapperAdapter.GetAllInstances(service);
        }
        
        protected override void BuildUp(object instance)
        {
            _bootstrapperAdapter.BuildUp(instance);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            if (_iocContainer == null)
            {
                _iocContainer = new TIocContainer();    
            }            
            var initializationFacade = new BootstrapperInitializationFacade<TIocContainer>(GetType(), _iocContainer);
            initializationFacade.Initialize(ModulesPath);
            Modules = initializationFacade.Modules;
            return initializationFacade.AssembliesResolver.GetAssemblies();
        }

        #endregion

        /// <summary>
        /// Gets the modules path that MEF need to search.
        /// </summary>
        /// <value>The modules path.</value>
        public virtual string ModulesPath
        {
            get { return "."; }
        }

        public virtual object CurrentLifetimeScope
        {
            get { return _defaultLifetimeScope; }
        }

        public IEnumerable<ICompositionModule> Modules { get; private set; }

        #region private implementation
        private void RegisterViewsAndViewModels(IIocContainer iocContainer)
        {
            //  register view models
            AssemblySource.Instance.ToArray()
              .SelectMany(ass => ass.GetTypes())
                //  must be a type that ends with ViewModel
              .Where(type => type != typeof(TRootViewModel) && type.Name.EndsWith("ViewModel"))
                //  must be in a namespace ending with ViewModels
              .Where(type => !(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace != null && type.Namespace.EndsWith("ViewModels"))
                //  must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this)
              .Where(type => type.GetInterface(typeof(INotifyPropertyChanged).Name, false) != null)
                //  registered as self
              .Apply(a => iocContainer.RegisterTransient(a, a));

            ////  register views
            //AssemblySource.Instance.ToArray()
            //  .SelectMany(ass => ass.GetTypes())
            //    //  must be a type that ends with View
            //  .Where(type => type.Name.EndsWith("View"))
            //    //  must be in a namespace ending with ViewModels
            //  .Where(type => !(string.IsNullOrWhiteSpace(type.Namespace)) && type.Namespace != null && type.Namespace.EndsWith("Views"))
            //    //  registered as self
            //  .Apply(a => _iocContainer.RegisterPerRequest(a, null, a));

        }
        #endregion
    }    
}
