# BackgroundUpdatable

ReadOnly datastructures that regularly update in a background thread. Like a cache that will never have cache-miss.


## How to use

Create a new instance of `BackgroundUpdatable<T>`:

```csharp
var text = new BackgroundUpdatable<string>(
	"optional initial value",
	TimeSpan.FromMinutes(15),
	UpdateAsync);

Assert.Equals(text.Value(), "optional initial value");
```

The initial value is optional, if it is missing the `UpdateAsync` method will be called the first time to initialize it. **NOTE** initializing with the `UpdateAsync` method will be blocking!

## License

The MIT license