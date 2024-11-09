using System.Collections.Generic;
using System.Linq;
using AvaloniaControls.Models;
using MSURandomizerLibrary.Configs;

namespace MSURandomizer.ViewModels;

public class MsuTrackViewModel : BaseViewModel
{
    public MsuTrackViewModel()
    {
        TrackName = "";
        Songs = new List<MsuSongViewModel>();
    }
    
    public MsuTrackViewModel(MsuTypeTrack msuTypeTrack, ICollection<Track> songs)
    {
        TrackNumber = msuTypeTrack.Number;
        TrackName = msuTypeTrack.Name;
        Songs = songs.Where(x => x.Number == msuTypeTrack.Number && !x.IsCopied).OrderBy(x => x.IsAlt)
            .Select(x => new MsuSongViewModel(x)).ToList();
        Display = Songs.Count != 0;
    }
    
    public MsuTrackViewModel(Track msuTrack, ICollection<Track> songs)
    {
        TrackNumber = msuTrack.Number;
        TrackName = msuTrack.TrackName;
        Songs = songs.Where(x => x.Number == msuTrack.Number && !x.IsCopied).OrderBy(x => x.IsAlt)
            .Select(x => new MsuSongViewModel(x)).ToList();
        Display = Songs.Count != 0;
    }

    public string TrackDisplay => $"{TrackNumber} - {TrackName}";
    
    public int TrackNumber { get; init; }
    
    public string TrackName { get; init; }
    
    public bool Display { get; init; }
    
    public List<MsuSongViewModel> Songs { get; init; }
}