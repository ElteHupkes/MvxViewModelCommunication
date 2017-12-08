using Android.App;
using Android.OS;
using MvvmCross.Droid.Views;
using MvxViewModelCommunication.Core.ViewModels;

namespace MvxViewModelCommunication.Droid.Views
{
    [Activity(Label = "View for ChildViewModel")]
    public class ChildView : MvxActivity<ChildViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.ChildView);
        }
    }
}