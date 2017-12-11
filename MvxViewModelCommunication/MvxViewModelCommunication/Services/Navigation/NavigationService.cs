using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Platform;

namespace MvxViewModelCommunication.Core.Services.Navigation
{
    public class NavigationService : MvxNavigationService, INavigationService
    {
        /// <summary>
        /// Bundle key used for the transaction ID
        /// </summary>
        public const string TransactionRequestIdKey = "_tqId";

        /// <summary>
        /// Bundle key used for response transaction ID
        /// </summary>
        public const string TransactionResponderIdKey = "_trId";

        /// <summary>
        /// IoC constructor
        /// </summary>
        /// <param name="navigationCache"></param>
        /// <param name="viewModelLoader"></param>
        public NavigationService(IMvxNavigationCache navigationCache, 
            IMvxViewModelLoader viewModelLoader) : base(navigationCache, viewModelLoader)
        {
        }

        /// <summary>
        /// Maps transaction IDs to their receivers. If a transaction result is set and
        /// the receiver in this map is still alive 
        /// </summary>
        private readonly Dictionary<string, WeakReference<ITransactionRequesterViewModel>> _transactionMap =
            new Dictionary<string, WeakReference<ITransactionRequesterViewModel>>();

        /// <summary>
        /// Intermediary result map for when the result cannot be delivered immediately
        /// </summary>
        private readonly Dictionary<string, Tuple<Type, bool, object>> _resultMap = new Dictionary<string, Tuple<Type, bool, object>>();

        /// <summary>
        /// Generates a view model transaction ID
        /// </summary>
        /// <returns></returns>
        private static string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Generates a transaction ID and logs the parent in the table.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        private void GenerateTransaction(ITransactionRequesterViewModel parent, ITransactionResponderViewModel child)
        {
            var transactionId = GenerateTransactionId();
            parent.TransactionRequesterId = transactionId;
            child.TransactionResponderId = transactionId;

            _transactionMap[transactionId] = new WeakReference<ITransactionRequesterViewModel>(parent);
        }

        ///
        /// <inheritdoc />
        ///
        public Task NavigateForResult<TViewModel, TResult>(ITransactionRequesterViewModel<TResult> receiver)
            where TViewModel : ITransactionResponderViewModel
        {
            var request = new MvxViewModelInstanceRequest(typeof(TViewModel));
            var viewModel = ViewModelLoader.LoadViewModel(request, null);
            GenerateTransaction(receiver, (ITransactionViewModel)viewModel);

            request.ViewModelInstance = viewModel;
            return Navigate(request, viewModel);
        }

        ///
        /// <inheritdoc />
        ///
        public Task NavigateForResult<TViewModel, TParameter, TResult>(ITransactionRequesterViewModel<TResult> receiver,
            TParameter parameter)
            where TViewModel : ITransactionResponderViewModel, IMvxViewModel<TParameter>
        {
            var request = new MvxViewModelInstanceRequest(typeof(TViewModel));
            var vm = (IMvxViewModel<TParameter>)ViewModelLoader.LoadViewModel(request, parameter, null);
            GenerateTransaction(receiver, (ITransactionViewModel)vm);

            request.ViewModelInstance = vm;
            return Navigate(request, vm);
        }

        ///
        /// <inheritdoc />
        ///
        public Task CloseWithResult<TResult>(ITransactionResponderViewModel viewModel, TResult result)
        {
            var transactionId = viewModel.TransactionResponderId;
            viewModel.TransactionResponderId = null;

            var hasValue = _transactionMap.TryGetValue(transactionId, out var vmReference);
            if (hasValue)
            {
                _transactionMap.Remove(transactionId);
            }

            // Check if the weak reference has not expired and the transaction ID is still valid
            if (hasValue && vmReference.TryGetTarget(out var target) && target.TransactionRequesterId == transactionId && target.CanReceiveResult())
            {
                if (!(target is ITransactionRequesterViewModel<TResult> tTarget))
                {
                    throw new NotSupportedException($"Trying to set a result of type {typeof(TResult)} on view model" +
                                                    $" of type {target}, which doesn't implement TRequestViewModel<{typeof(TResult)}>");
                }

                // Set the result directly
                tTarget.OnResult(result);
                tTarget.TransactionRequesterId = null;
                return Close(viewModel);
            }

            // Store the result in the result table, and let the parent view model retrieve it
            // when it is rehydrated.
            _resultMap[transactionId] = new Tuple<Type, bool, object>(typeof(TResult), true, result);
            return Close(viewModel);
        }

        /// <summary>
        /// Cancels an ongoing transaction from a sending view model
        /// </summary>
        /// <param name="tr"></param>
        public void CancelTransaction(ITransactionViewModel tr)
        {
            if (tr.TransactionRequesterId == null) return;

            var transactionId = tr.TransactionRequesterId;
            tr.TransactionRequesterId = null;

            if (!_transactionMap.TryGetValue(transactionId, out var vmReference)) return;
            _transactionMap.Remove(transactionId);

            if (vmReference.TryGetTarget(out var target) && target.CanReceiveResult())
            {
                // Cancel the transaction directly on the view model
                target.TransactionRequesterId = null;
            }
            else
            {
                // Set a negative transaction result for the new / rehydrated view model
                // to pick up.
                _resultMap.Add(transactionId, new Tuple<Type, bool, object>(null, false, null));
            }
        }

        /// <summary>
        /// Checks if this view model was in a transaction and closes it.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public override Task<bool> Close(IMvxViewModel viewModel)
        {
            if (viewModel is ITransactionViewModel tr && tr.TransactionRequesterId != null)
            {
                CancelTransaction(tr);
            }

            return base.Close(viewModel);
        }

        ///
        /// <inheritdoc />
        ///
        public bool ObtainResult(ITransactionRequesterViewModel viewModel)
        {
            if (!viewModel.CanReceiveResult())
            {
                MvxTrace.Warning($"ObtainResult called for a viewmodel of type {viewModel.GetType()} in a non-susceptible transaction state. ");
                return false;
            }

            // Check if there is a transaction for this view model, and whether there is
            // a result available for it.
            var transactionId = viewModel.TransactionRequesterId;
            if (transactionId == null || !_resultMap.TryGetValue(transactionId, out var resultTuple)) return false;

            // Only clear the transaction ID if there is a result. For one thing, this view model
            // may be a transaction sender and not receiver, and this method may also be called
            // too early in the lifetime. This does mean that IDs for unfinished transactions
            // might linger, but this would seem harmless to me.
            viewModel.TransactionRequesterId = null;
            _resultMap.Remove(transactionId);

            if (!resultTuple.Item2)
            {
                // Result was cancelled
                return true;
            }

            var type = resultTuple.Item1;
            var obj = resultTuple.Item3;

            // Find the result method on the view model
            const string onResultName = nameof(ITransactionRequesterViewModel<object>.OnResult);
            var resultMethod = viewModel.GetType().GetMethods()
                .Where(m =>
                {
                    var name = m.Name;
                    var parameters = m.GetParameters();
                    return name == onResultName && parameters.Length == 1 &&
                           parameters[0].ParameterType == type;
                })
                .FirstOrDefault();

            if (resultMethod == null)
            {
                throw new NotSupportedException($"Trying to set a transaction result of type ${type}" +
                                                $" on a view model of type ${viewModel.GetType()}, but" +
                                                $" the view model has no method {onResultName}({type})");
            }

            // Invoke the result method
            resultMethod.Invoke(viewModel, new[] { obj });
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void SaveTransactionState(ITransactionRequesterViewModel viewModel, IMvxBundle bundle)
        {
            bundle.Data.Add(TransactionRequestIdKey, viewModel.TransactionRequesterId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void SaveTransactionState(ITransactionResponderViewModel viewModel, IMvxBundle bundle)
        {
            bundle.Data.Add(TransactionResponderIdKey, viewModel.TransactionResponderId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void SaveTransactionState(ITransactionViewModel viewModel, IMvxBundle bundle)
        {
            SaveTransactionState((ITransactionRequesterViewModel)viewModel, bundle);
            SaveTransactionState((ITransactionResponderViewModel)viewModel, bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void RestoreTransactionState(ITransactionRequesterViewModel viewModel, IMvxBundle bundle)
        {
            if (bundle?.Data == null) return;
            if (bundle.Data.TryGetValue(TransactionRequestIdKey, out var value))
            {
                viewModel.TransactionRequesterId = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void RestoreTransactionState(ITransactionResponderViewModel viewModel, IMvxBundle bundle)
        {
            if (bundle?.Data == null) return;
            if (bundle.Data.TryGetValue(TransactionResponderIdKey, out var value))
            {
                viewModel.TransactionResponderId = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="bundle"></param>
        public void RestoreTransactionState(ITransactionViewModel viewModel, IMvxBundle bundle)
        {
            RestoreTransactionState((ITransactionRequesterViewModel)viewModel, bundle);
            RestoreTransactionState((ITransactionResponderViewModel)viewModel, bundle);
        }
    }
}