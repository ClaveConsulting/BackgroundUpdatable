# BackgroundUpdatable

ReadOnly datastructures that regularly update in a background thread. Like a cache that will never have cache-miss.

BackgroundUpdatable has several advantages:
* It is thread safe
* Async work is done in a background thread, so it will not block the main thread
* While it is waiting for update it will use the previous value
* There will never be a cache-miss
* If update throws an exception, the previous values are used
* If the update fails it will wait for another period, so doesn't hammer the data provider

## How to use

Create a new instance of `BackgroundUpdatable<T>`:

```csharp
var text = new BackgroundUpdatable<string>(
	"optional initial value",
	TimeSpan.FromMinutes(15),
	UpdateAsync);

Assert.Equals(text.Value(), "optional initial value");

// Force update the value
await text.Update();

// Log that it is updating
// Most likely you would not 
text.BackgroundUpdateStarted += new BackgroundUpdateStartedHandler(
  () => Logger.Info("Started updating text");
);

text.BackgroundUpdateSucceeded += new BackgroundUpdateSucceededHandler(
  value => Logger.Info($"Updated text completed, new value is {value}");
);

text.BackgroundUpdateFailed += new BackgroundUpdateFailedHandler(
  exception => Logger.Error("Update failed", exception);
);

//
```

## Public methods

### `public BackgroundUpdatable(TimeSpan period, Func<Task<T>> update)`

Construct a new BackgroundUpdatable of type `T` without an initial value. See notes below about the initial update.

* `TimeSpan period`: How long to wait until the vaule is updated
* `Func<Task<T>> update`: An async function that returns the new value

### `public BackgroundUpdatable(T initialValue, TimeSpan period, Func<Task<T>> update)`

Construct a new BackgroundUpdatable of type `T` with an initial value.

* `T initialValue`: The initial value to use
* `TimeSpan period`: How long to wait until the vaule is updated
* `Func<Task<T>> update`: An async function that returns the new value

### `public T Value()`

Returns the current value synchronously. If it is longer than the `TimeSpan` period since the value was updated, and an update isn't currently in progress, it will start an update in a background thread.

### `public async Task Update()`

Updates the value and waits for it to complete, unless an update is currently in progress. After the update it will reset the timeout period. If the update throws an exception, it will be thrown from this method too.

## Events

BackgroundUpdatable contains three events that can be useful to log debug and error messages: 

* `BackgroundUpdateStarted()` - Called when a background update is started. 
* `BackgroundUpdateFailed(Exception exception)` - Called if the update throws an exception, with the exception.
* `BackgroundUpdateSucceeded(object value)` - Called if the update succeeds, with the new value.

## Useful subclasses

This package contains some useful subclasses of BackgroundUpdatable<T>
	
### BackgroundUpdatableCollection<T>
	
Implements the `IReadOnlyCollection<T>` interface. This subclass is useful if you need something that behaves like `ISet<T>`, for example for using `.Contains(value)` efficiently.

### BackgroundUpdatableList<T>
	
Implements the `IReadOnlyList<T>` interface. This subclass is useful if you need something that behaves like `IList<T>`, for example for looking up based on numeric index.

### BackgroundUpdatableDictionary<TKey, TValue>

Implements the `IReadOnlyDictionary<TKey, TValue>` interface. This subclass is useful if you need a key-value store that is very fast, can be slightly out of date and can fit all the data in memory.

## Usage with IoC

Here is a sample of how to use BackgroundUpdatable<T> in an IoC environment:
	
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ITextProvider, TextProvider>();
}

// TextProvider.cs
public interface ITextProvider
{
    // Users of this interface only need to
    // know of this getter, the details are
    // hidden in the implementation.
    string Text { get; }
}

public class TextProvider : ITextProvider
{
    private readonly TimeSpan _refreshPeriod = TimeSpan.FromMinutes(10);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly BackgroundUpdatable<string> _updater;

    // Set up everything in the constructor
    // This class should be registered as a singleton,
    // so this will only happen once
    public TextProvider(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _updater = new BackgroundUpdatable<string>(
            _refreshPeriod,
            UpdateAsync
        );
        _updater.BackgroundUpdateStarted += UpdateStarted;
        _updater.BackgroundUpdateFailed += UpdateFailed;
        _updater.BackgroundUpdateSucceeded += UpdateSucceeded;
    }

    // This is the implementation of the interface
    public string Text => _updater.Value();

    // Fetch a new value using HttpClient
    // Probably you would do something more advanced here
    private async Task<string> UpdateAsync()
    {
        var client = _httpClientFactory.CreateClient();
        return await client.GetStringAsync("/path/to/api");
    }

    // Three very simple logging handlers
    private void UpdateStarted()
        => _logger.LogInformation("Started fetching text");
    
    private void UpdateFailed(Exception exception) 
        => _logger.LogError(exception, "Failed to fetch text");

    private void UpdateSucceeded(object value)
        => _logger.LogInformation("Successfully fetched text");
}
```

## Important notes to keep in mind

### If you don't supply an initial value, the first usage will block the thread

While the update method is async, the usage is always sync, so the initialization will block the thread. Until the update resolves with a value all threads calling `.Value()` will be blocked. Once the update resolves all threads will be released.

### If the initial update fails it will throw an exception

If the initial update fails (the async update method throws an exception), then BackgroundUpdatable will remain uninitialized and the next call to it will attempt to initialize it again. Any blocking call will rethrow the exception, so you need to handle it yourself.

### When the timeout occures the existing value is used until the update resolves

When the timeout happens the main thread continues using the existing value. The background thread will fetch a new value and until it is done working the existing value is returned by `.Value()`.

### Only one update can happen at a time

Even if multiple threads call `.Value()` right at the same time, only one will cause the async update function to be called. It is therefore safe to use BackgroundUpdatable in multi-threaded applications that experience heavy load.

### If the update fails the existing value is reused

If the update function throws an exception the timeout is reset and the existing value is used. This is because usually you want to fetch something over the network, and you might have a temporary network issue that will resolve itself in a short while. 

### You can manually update the value by calling the async `.Update()` method

This is a good way to force the data to update, if you know that it has likely updated. Unlike `.Value()` this method will wait until the update is done. If you call this method while an update is already underway, it will return right away. 

### Update will not happen automatically

The update of data will not happen by itself, it will only happen as a result of calling `.Value()` after the timeout period.

## License

The MIT license
