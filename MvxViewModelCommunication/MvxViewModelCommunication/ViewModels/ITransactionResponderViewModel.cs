namespace MvxViewModelCommunication.Core.ViewModels
{
    /// <summary>
    /// Interface for a view model that is involved in parent-to-child
    /// view model communication as a child (i.e. responding to
    /// a request).
    /// </summary>
    public interface ITransactionResponderViewModel
    {
        /// <summary>
        /// ID of the transaction this view model is
        /// responding to.
        /// </summary>
        string TransactionResponderId { get; set; }
    }
}