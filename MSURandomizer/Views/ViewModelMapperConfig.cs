using AvaloniaControls.Models;

namespace MSURandomizer.Views;

using System.Linq;
using System.Reflection;
using AutoMapper;
using ReactiveUI;

public class ViewModelMapperConfig<TAssembly> : Profile
{
    public ViewModelMapperConfig()
    {
        var viewModels = typeof(TAssembly).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ReactiveObject)));
        foreach (var viewModel in viewModels)
        {
            var attribute =
                viewModel.GetCustomAttributes().FirstOrDefault(x => x.GetType() == typeof(MapsToAttribute)) as MapsToAttribute;
            if (attribute?.MapsToType == null)
                continue;
            CreateMap(viewModel, attribute.MapsToType);
            CreateMap(attribute.MapsToType, viewModel);
        }
    }
}