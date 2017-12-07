namespace MvxViewModelCommunication.Core.ViewModels
{
    /// <summary>
    /// Interface for a view model that can both receive and send data
    /// to other view models.
    /// </summary>
    public interface ITransactionViewModel : ITransactionRequesterViewModel, ITransactionResponderViewModel
    {

    }
}