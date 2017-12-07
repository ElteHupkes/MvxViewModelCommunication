using MvxViewModelCommunication.Core.Services;

namespace MvxViewModelCommunication.Core.ViewModels
{
    public class ChildViewModel : BaseViewModel
    {
        public ChildViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}