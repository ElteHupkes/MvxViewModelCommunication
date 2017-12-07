using MvvmCross.Core.ViewModels;

namespace MvxViewModelCommunication.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            base.Initialize();

            RegisterNavigationServiceAppStart<>();
        }
    }
}