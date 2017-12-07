using MvxViewModelCommunication.Core.Services;

namespace MvxViewModelCommunication.Core.ViewModels
{
    public class SubViewModel : BaseViewModel
    {
        public SubViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}