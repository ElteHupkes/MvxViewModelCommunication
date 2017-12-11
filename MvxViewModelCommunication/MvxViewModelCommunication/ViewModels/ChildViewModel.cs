using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvxViewModelCommunication.Core.Services.Navigation;
using MvxViewModelCommunication.Core.ViewModels.Results;

namespace MvxViewModelCommunication.Core.ViewModels
{
    public class ChildViewModel : BaseViewModel, ITransactionRequesterViewModel<TextResult>
    {
        public ChildViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        /// <summary>
        /// Text specified in this view model
        /// </summary>
        public string MyText
        {
            get => _myText;
            set => SetProperty(ref _myText, value);
        }

        private string _myText;

        /// <summary>
        /// Text specified in sub view model
        /// </summary>
        public string SubText
        {
            get => _subText;
            set => SetProperty(ref _subText, value);
        }

        private string _subText = "Not specified";

        /// <summary>
        /// Saves text states when the activity is killed
        /// </summary>
        /// <param name="bundle"></param>
        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            bundle.Data.Add("MyText", MyText);
            bundle.Data.Add("SubText", SubText);
        }

        /// <summary>
        /// Reloads text states upon activity restore
        /// </summary>
        /// <param name="state"></param>
        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            if (state?.Data == null) return;

            if (state.Data.TryGetValue("MyText", out var value))
            {
                MyText = value;
            }

            if (state.Data.TryGetValue("SubText", out value))
            {
                SubText = value;
            }
        }

        /// <summary>
        /// Called when a sub result is retrieved
        /// </summary>
        /// <param name="result"></param>
        public void OnResult(TextResult result)
        {
            SubText = result.Text;
        }

        /// <summary>
        /// Command to start obtaining sub text
        /// </summary>
        public ICommand GetSubText =>
            _getSubText ?? (_getSubText = new MvxCommand(DoGetSubText));

        private MvxCommand _getSubText;

        private void DoGetSubText()
        {
            NavigationService.NavigateForResult<SubViewModel, TextResult>(this);
        }

        /// <summary>
        /// Publish result to the parent
        /// </summary>
        public ICommand PublishResult =>
            _publishResult ?? (_publishResult = new MvxCommand(DoPublishResult));

        private MvxCommand _publishResult;

        private void DoPublishResult()
        {
            NavigationService.CloseWithResult(this, new TextResult { Text = $"{MyText} {SubText}" });
        }
    }
}