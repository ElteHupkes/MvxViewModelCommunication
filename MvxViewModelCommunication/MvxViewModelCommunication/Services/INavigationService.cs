using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvxViewModelCommunication.Core.ViewModels;

namespace MvxViewModelCommunication.Core.Services
{
    /// <summary>
    /// Interface for a custom navigation service with extra methods for
    /// tombstone-safe result passing.
    /// </summary>
    public interface INavigationService : IMvxNavigationService
    {
        /// <summary>
        /// Initializes a transaction between the receiver view model and
        /// the yet to be opened child view model for a result of type TResult.
        /// </summary>
        /// <typeparam name="TRequestViewModel"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="receiver"></param>
        /// <returns></returns>
        Task NavigateForResult<TRequestViewModel, TViewModel, TResult>(TRequestViewModel receiver)
            where TRequestViewModel : ITransactionRequesterViewModel<TResult>
            where TViewModel : ITransactionResponderViewModel;

        /// <summary>
        /// Initializes a transaction between the receiver view model
        /// and the yet to be opened parameterized child view model,
        /// for a result of type TResult.
        /// </summary>
        /// <typeparam name="TRequestViewModel"></typeparam>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TParameter"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="receiver"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task NavigateForResult<TRequestViewModel, TViewModel, TParameter, TResult>(TRequestViewModel receiver,
            TParameter parameter)
            where TRequestViewModel : ITransactionRequesterViewModel<TResult>
            where TViewModel : ITransactionResponderViewModel, IMvxViewModel<TParameter>;

        /// <summary>
        /// Sets the result of a transaction and closes the view model
        /// </summary>
        /// <typeparam name="TResultViewModel"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="result"></param>
        Task CloseWithResult<TResultViewModel, TResult>(TResultViewModel viewModel, TResult result)
            where TResultViewModel : ITransactionResponderViewModel, IMvxViewModel;

        /// <summary>
        /// Checks if a transaction result is available for this view model
        /// and sets it using its `OnResult` method. Returns whether or not
        /// a result was available. If no result is available, `OnResult` is
        /// not called.
        /// </summary>
        /// <param name="viewModel"></param>
        bool ObtainResult(ITransactionRequesterViewModel viewModel);

        /// <summary>
        /// Persists view model transaction state for the given
        /// view model to the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void SaveTransactionState(ITransactionRequesterViewModel viewModel, IMvxBundle bundle);

        /// <summary>
        /// Persists view model transaction state for the given
        /// view model to the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void SaveTransactionState(ITransactionResponderViewModel viewModel, IMvxBundle bundle);

        /// <summary>
        /// Persists view model transaction state for the given
        /// view model from the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void SaveTransactionState(ITransactionViewModel viewModel, IMvxBundle bundle);

        /// <summary>
        /// Persists view model transaction state for the given
        /// view model from the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void RestoreTransactionState(ITransactionRequesterViewModel viewModel, IMvxBundle bundle);

        /// <summary>
        /// Restores view model transaction state for the given
        /// view model from the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void RestoreTransactionState(ITransactionResponderViewModel viewModel, IMvxBundle bundle);

        /// <summary>
        /// Restores view model transaction state for the given
        /// view model from the given bundle.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        void RestoreTransactionState(ITransactionViewModel viewModel, IMvxBundle bundle);
    }
}