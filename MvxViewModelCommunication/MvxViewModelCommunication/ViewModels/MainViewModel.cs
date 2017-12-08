using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvxViewModelCommunication.Core.Services;
using MvxViewModelCommunication.Core.ViewModels.Results;

namespace MvxViewModelCommunication.Core.ViewModels
{
    public class MainViewModel : BaseViewModel, ITransactionRequesterViewModel<TextResult>
    {
        public MainViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
        }

        /// <summary>
        /// Final displayed text
        /// </summary>
        public string FinalText
        {
            get => _finalText;
            set => SetProperty(ref _finalText, value);
        }

        private string _finalText = "Not specified";

        /// <summary>
        /// Moves to the child view model
        /// </summary>
        public ICommand GetChildText =>
            _getChildText ?? (_getChildText = new MvxCommand(DoGetChildText));
        private MvxCommand _getChildText;

        private void DoGetChildText()
        {
            NavigationService.NavigateForResult<ChildViewModel, TextResult>(this);
        }

        /// <summary>
        /// Called when a result message is received from the child
        /// view model.
        /// </summary>
        /// <param name="t"></param>
        public void OnResult(TextResult t)
        {
            FinalText = t.Text;
        }
    }
}