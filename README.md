# Persistent view model to view model communication with MvvmCross 5.x

MvvmCross 5.x introduces a new `NavigationService`, which includes a mechanism for passing results between view models as follows.
You would do something like this in a parent view model (I paraphrase from the [MvvmCross documentation](https://www.mvvmcross.com/documentation/fundamentals/navigation)):

```csharp
    public async Task SomeMethod()
    {
        var result = await _navigationService.Navigate<NextViewModel, MyObject, MyReturnObject>(new MyObject());
        //Do something with the result MyReturnObject that you get back
    }
```

And return a result like this in a child view model:
```
    public async Task SomeMethodToClose()
    {
        await _navigationService.Close(this, new MyReturnObject());
    }
```

However, there is a problem with this approach. When your app gets suspended (tombstoned, or whatever you like to call it) the view model objects are discarded, and result passing breaks entirely. It gets even more complicated on Android, where individual activities may be killed to save memory. This scenario isn't very likely in real life, but there's a developer option called "Don't keep activities" that is the most straightforward way to test activity suspension without going out of your way. When enabled, this option kills any Android activity as soon as it is disappears from view even if it is technically still present in the stack. Upon returning to it, the activity will be restored with a bundle passed to `OnCreate` (which previously would have to be populated in `OnSaveInstanceState`). "Don't keep activities" doesn't test real suspension though, because the service layer remains in tact and MvvmCross may actually cache the view model there to speed up this kind of quick activity recovery, which also occurs when the device is rotated.

All that being said, app suspension accounts for a non-insignificant amount of complaints about unexpected behavior or even crashes in our app. Therefore I would personally like to work with a result mechanism that can handle all kinds of activity (/ view model) suspension scenarios. In the past we experimented mostly with message passing using `MvxMessenger` (something I think was popularized by [this article](https://gregshackles.com/returning-results-from-view-models-in-mvvmcross/)). The problem here is again Android, where when returning from a suspended state the receiving view model may not be initialized. We therefore had to implement separate mechanism using `StartActivityForResult` for that platform, which resulted in duplicate code and all other sorts of complications. Much rather we'd have a solution which:

- Works on all platforms without platform specific code
- Supports all sorts of freaky suspension scenarios
- Is straightforward to use

Fortunately, I think I've implemented such a solution!

## What it looks like
Let's say we have `ParentViewModel` receiving a result from a `ChildViewModel`. The relevant code would look as follows. First we have a class which
contains the result (this could actually just be s `string` in this case, but let's pretend there's more there):

```csharp
public class TextResult 
{
    public string Text { get; set; }
}
```

Then there's the parent view model:
```csharp
public class ParentViewModel : BaseViewModel, ITransactionRequesterViewModel<TextResult> 
{
    // .....

    // Launches the ChildViewModel to obtain a result 
    public Task DoGetChildText()
    {
        return NavigationService.NavigateForResult<ChildViewModel, TextResult>(this);
    }
	
    // Called when a result is available
    public void OnResult(TextResult result) 
    {
    }
    
    // ....
}
```

And the child view model, which will return the result:
```csharp
public class ChildViewModel : BaseViewModel
{
    // ...

    private void DoPublishResult()
    {
        NavigationService.CloseWithResult(this, new TextResult { Text = "My result text" });
    }
	
	// ...
}
```

That's it! When the `ChildViewModel` calls `CloseWithResult`, the result will be delivered to the parent view model
either directly, or when it comes back from its suspended state.

## How it works
Conceptually, whenever a parent view model would like to get some data as an object of type `TResult` from a child it opens, it engages in a _transaction_ with
that child. The parent view model needs to implement [`ITransactionRequesterViewModel<TResult>`](MvxViewModelCommunication/MvxViewModelCommunication/Services/Navigation/ITransactionRequesterViewModel.cs),
whereas the child needs to implement [`ITransactionResponderViewModel`](MvxViewModelCommunication/MvxViewModelCommunication/Services/Navigation/ITransactionRequesterViewModel.cs). 
The transaction is mediated by a custom [`NavigationService`](MvxViewModelCommunication/MvxViewModelCommunication/Services/Navigation/NavigationService.cs) which extends `MvxNavigationService`. I did not
want to override the default generic "for result" `Navigate` and `Close` methods, because the semantics of this implementation differ from the default. Some functionality
is required in each view model as well in order to save transaction IDs upon suspension and to indicate to the `NavigationService` whether or not it can receive a result. In this code sample,
a [`BaseViewModel`](MvxViewModelCommunication/MvxViewModelCommunication/ViewModels/BaseViewModel.cs) is included that handles all the required logic.

Upon receiving a transaction request, the navigation service generates a transaction UUID and sets it on the implemented interface variables on both the parent and the child view models. It also stores
a weak reference to the parent view model, so that it can return a result to it immediately if it is still alive when the result is available. The child does whatever it needs to do to get the data
and calls `NavigationService.CloseWithResult`. The navigation service checks its weak reference table for the transaction ID to see if the receiving view model is still present and alive. If so, the
result is returned immediately. If not, it stores the result object in a temporary table for the revived view model to retrieve later. Because the receiving view model should be alive very soon 
(it should now be the top level view model and hence cannot be in a suspended state) the result object is only held in memory shortly and need not be in a serializable format, 
a convenient advantage over any method using `StartActivityForResult` in Android.

If our receiving view model was not alive to receive the result when it became available, there are two possible scenarios:

1. The platform specific view has been destroyed, but our fully initialized view model is still cached to be reused.
2. Both the platform view and the view model objects are gone.

Keeping this in mind, we have our view model check the navigation service for a result in two locations:

1. In the `ViewCreated` callback. If the view model had been cached, it is already initialized at this point
   and ready to retrieve data. If the view model is not yet fully initialized the result is not retrieved.
2. At the end of `Initialize`.

Note that we let the `NavigationService` do all the heavy lifting of calling the `OnResult(TResult)` callback implemented by the receiving view model. No matter how many result types
it can potentially get, it only needs to call `NavigationService.ObtainResult(this)` once. The correct result method to call will be determined using reflection.

So that's it! Activities, view controllers, etc can now be killed in weird ways by whatever supported operating system, but view model communication still works.