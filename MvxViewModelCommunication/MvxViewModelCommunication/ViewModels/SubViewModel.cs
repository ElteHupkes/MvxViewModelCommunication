using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using MvxViewModelCommunication.Core.Services.Navigation;
using MvxViewModelCommunication.Core.ViewModels.Results;

namespace MvxViewModelCommunication.Core.ViewModels
{
    public class SubViewModel : BaseViewModel
    {
        public SubViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        /// <summary>
        /// Text specified in this view model
        /// </summary>
        public string SubText
        {
            get => _subText;
            set => SetProperty(ref _subText, value);
        }

        private string _subText;

        /// <summary>
        /// Saves text states when the activity is killed
        /// </summary>
        /// <param name="bundle"></param>
        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
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

            if (state.Data.TryGetValue("SubText", out var value))
            {
                SubText = value;
            }
        }

        /// <summary>
        /// Publish result to the parent
        /// </summary>
        public ICommand PublishResult =>
            _publishResult ?? (_publishResult = new MvxCommand(DoPublishResult));

        private MvxCommand _publishResult;

        private void DoPublishResult()
        {
            NavigationService.CloseWithResult(this, new TextResult {Text = SubText});
        }
    }
}