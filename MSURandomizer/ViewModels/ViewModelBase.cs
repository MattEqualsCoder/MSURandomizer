using System;
using AvaloniaControls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace MSURandomizer.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public ViewModelBase()
    {
        this.LinkProperties();
        
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != nameof(HasBeenModified))
            {
                HasBeenModified = true;
            }
        };
    }
    
    [Reactive] public bool HasBeenModified { get; set; }
}
