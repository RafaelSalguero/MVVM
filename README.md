# Tonic.MVVM
MVVM Library for WPF

##Design time view model + dependency injection

The `Tonic.MVVM.ViewModelLocator` is a markup extension that its function 
is to get ViewModel instances given the name of the view model.

The basic use is to inherit from the class and to have two `IServiceProvider` methods, one for design time dependencies and other for run time dependencies:

```c#
//Design time dependencies, mostly mocks
public static IServiceProvider Designer()
{
    var Kernel = new Ninject.StandardKernel();
    Kernel.Bind<Func<IDatabase>>().ToMethod<Func<IDatabase>>(x => () => new InMemoryDatabase(new MockContext()));
    Kernel.Bind<IDateTimeProvider>().ToConstant(new MockDateTime(new DateTime(2013, 05, 27)));
    return Kernel;
}

//Run time dependencies
public static IServiceProvider Runtime()
{
    var Kernel = new Ninject.StandardKernel();
    Kernel.Bind<Func<IDatabase>>().ToMethod<Func<IDatabase>>(x => () => new ContextDatabase(new Db()));
    Kernel.Bind<IDateTimeProvider>().ToConstant(new SystemDateTime());

    return Kernel;
}

//Return true if the program is in design time
public static bool IsInDesignMode
=>  System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

//Map view models with views
private static INameLocator GetLocator(IServiceProvider Provider)
{
    var P = new PairLocator(typeof(View.Dummy), typeof(ViewModel.Dummy), Provider);
    return P;
}

public Locator(string Name)
    : base(GetLocator(IsInDesignMode ? Designer() : Runtime()), Name) { }
    

```

The `PairLocator` class is an implementation of the `INameLocator` 
interface that traverse all assembly types that follows the name convention, 
but any other implementation of INameLocator that gets ViewModel instances by name will work with the ViewModelLocator
