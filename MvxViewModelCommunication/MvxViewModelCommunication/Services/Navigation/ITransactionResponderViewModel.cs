using MvvmCross.Core.ViewModels;

namespace MvxViewModelCommunication.Core.Services.Navigation
{
    /// <summary>
    /// Interface for a view model that is involved in parent-to-child
    /// view model communication as a child (i.e. responding to
    /// a request).
    /// </summary>
    public interface ITransactionResponderViewModel : IMvxViewModel
    {
        /// <summary>
        /// ID of the transaction this view model is
        /// responding to.
        /// </summary>
        string TransactionResponderId { get; set; }
    }
}