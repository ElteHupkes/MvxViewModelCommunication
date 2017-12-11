using MvvmCross.Core.ViewModels;

namespace MvxViewModelCommunication.Core.Services.Navigation
{
    /// <summary>
    /// Interface for a view model that is involved in a parent-to-child
    /// communication as a parent.
    /// </summary>
    public interface ITransactionRequesterViewModel : IMvxViewModel
    {
        /// <summary>
        /// The ID of the transaction the view model is requesting.
        /// </summary>
        string TransactionRequesterId { get; set; }

        /// <summary>
        /// Whether or not this view model is in a state suitable
        /// to retrieve a transaction result.
        /// </summary>
        /// <returns></returns>
        bool CanReceiveResult();
    }

    /// <summary>
    /// Interface to be implemented by a parent view model
    /// that will request a certain TResult from a child.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface ITransactionRequesterViewModel<TResult> : ITransactionRequesterViewModel
    {
        /// <summary>
        /// Called with the result of the request
        /// </summary>
        /// <param name="result"></param>
        void OnResult(TResult result);
    }
}