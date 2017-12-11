using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvxViewModelCommunication.Core.Services.Navigation;

namespace MvxViewModelCommunication.Core.ViewModels
{
    /// <summary>
    /// Base view model to use with persistent view model communication.
    /// </summary>
    public abstract class BaseViewModel : MvxViewModel, ITransactionViewModel
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
        /// Requester ID for a view model transaction
        /// </summary>
        public string TransactionRequesterId { get; set; }

        /// <summary>
        /// Responder ID for a view model transaction
        /// </summary>
        public string TransactionResponderId { get; set; }

        /// <summary>
        /// The navigation service
        /// </summary>
        public INavigationService NavigationService { get; }

        /// <summary>
        /// Whether or not the view corresponding to this view model
        /// has been created.
        /// </summary>
        public bool ViewIsCreated { get; private set; }

        /// <summary>
        /// Whether or not the view corresponding to this view model
        /// has been destroyed.
        /// </summary>
        public bool ViewIsDestroyed { get; private set; }

        /// <summary>
        /// Whether or not the current view model has reached
        /// the end of initialization.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            protected set
            {
                _isInitialized = value;
                MaybeObtainResult();
            }
        }
        private bool _isInitialized;

        /// <summary>
        /// IoC constructor
        /// </summary>
        /// <param name="navigationService"></param>
        protected BaseViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        /// <summary>
        /// Called when the corresponding view is created
        /// </summary>
        public override void ViewCreated()
        {
            base.ViewCreated();
            ViewIsCreated = true;
            ViewIsDestroyed = false;

            MaybeObtainResult();
        }

        /// <summary>
        /// Called when the corresponding view is destroyed
        /// </summary>
        public override void ViewDestroy()
        {
            base.ViewDestroy();
            ViewIsDestroyed = true;
        }

        /// <summary>
        /// This is the MvvmCross default initialization method. Because
        /// we need to run some code at the end of initialization and it
        /// is customary to call base *first*, we provide an alternative
        /// override (`Setup`) and mark this override as sealed.
        /// </summary>
        /// <returns></returns>
        public sealed override async Task Initialize()
        {
            await base.Initialize();
            await Setup();
            IsInitialized = true;
        }

        /// <summary>
        /// Replacement method for Initialize()
        /// </summary>
        /// <returns></returns>
        public virtual Task Setup()
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Reloads view model state after tombstoning
        /// </summary>
        /// <param name="state"></param>
        protected override void ReloadFromBundle(IMvxBundle state)
        {
            base.ReloadFromBundle(state);
            NavigationService.RestoreTransactionState(this, state);
        }

        /// <summary>
        /// Saves view model state before tombstoning
        /// </summary>
        /// <param name="bundle"></param>
        protected override void SaveStateToBundle(IMvxBundle bundle)
        {
            base.SaveStateToBundle(bundle);
            NavigationService.SaveTransactionState(this, bundle);
        }

        /// <summary>
        /// Whether or not this view model is in an appropriate state
        /// to receive results.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanReceiveResult() => IsInitialized && ViewIsCreated && !ViewIsDestroyed;

        /// <summary>
        /// Checks for a result if the view is in a created state
        /// and the view model is initialized.
        /// </summary>
        protected void MaybeObtainResult()
        {
            if (!CanReceiveResult()) return;

            // Check if there is a result from Navigation.CloseWithResult available for
            // this view model.
            NavigationService.ObtainResult(this);
        }
    }

    /// <summary>
    /// Base view model with parameter initializer
    /// </summary>
    /// <typeparam name="TInit"></typeparam>
    public abstract class BaseViewModel<TInit> : BaseViewModel, IMvxViewModel<TInit>
    {
        /// <summary>
        /// IoC constructor
        /// </summary>
        /// <param name="navigationService"></param>
        protected BaseViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
        }

        public abstract void Prepare(TInit parameter);
    }
}