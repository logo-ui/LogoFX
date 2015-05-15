﻿namespace LogoFX.UI.ViewModels.Interfaces
{
    public interface IViewModelFactory
    {
        TViewModel CreateModelWrapper<TModel, TViewModel>(TModel model) where TViewModel : IModelWrapper<TModel>;
    }
}
