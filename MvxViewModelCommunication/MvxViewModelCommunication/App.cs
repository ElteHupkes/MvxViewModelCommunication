using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvxViewModelCommunication.Core.Services;
using MvxViewModelCommunication.Core.Services.Navigation;
using MvxViewModelCommunication.Core.ViewModels;

namespace MvxViewModelCommunication.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            Mvx.LazyConstructAndRegisterSingleton<IMvxNavigationCache, MvxNavigationCache>();
            Mvx.LazyConstructAndRegisterSingleton<INavigationService, NavigationService>();
            Mvx.LazyConstructAndRegisterSingleton<IMvxNavigationService>(Mvx.Resolve<INavigationService>);
            RegisterNavigationServiceAppStart<MainViewModel>();
        }
    }
}