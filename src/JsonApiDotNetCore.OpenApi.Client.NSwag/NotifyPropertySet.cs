using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// Adapted from https://github.com/PrismLibrary/Prism/blob/9.0.537/src/Prism.Core/Mvvm/BindableBase.cs for JsonApiDotNetCore.

#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
#pragma warning disable AV1554 // Method contains optional parameter in type hierarchy
#pragma warning disable AV1562 // Do not declare a parameter as ref or out

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

/// <summary>
/// Implementation of <see cref="INotifyPropertyChanged" /> that doesn't detect changes.
/// </summary>
[PublicAPI]
public abstract class NotifyPropertySet : INotifyPropertyChanged
{
    /// <summary>
    /// Occurs when a property is set.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the property and notifies listeners.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the property.
    /// </typeparam>
    /// <param name="storage">
    /// Reference to a property with both getter and setter.
    /// </param>
    /// <param name="value">
    /// Desired value for the property.
    /// </param>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners. This value is optional and can be provided automatically when invoked from compilers that support
    /// CallerMemberName.
    /// </param>
    /// <returns>
    /// Always <c>true</c>.
    /// </returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Raises this object's PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">
    /// Name of the property used to notify listeners. This value is optional and can be provided automatically when invoked from compilers that support
    /// <see cref="CallerMemberNameAttribute" />.
    /// </param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises this object's PropertyChanged event.
    /// </summary>
    /// <param name="args">
    /// The <see cref="PropertyChangedEventArgs" />.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(this, args);
    }
}
