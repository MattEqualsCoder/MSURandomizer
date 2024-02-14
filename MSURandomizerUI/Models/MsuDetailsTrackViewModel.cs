using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MSURandomizerLibrary.Configs;

namespace MSURandomizerUI.Models;

internal class MsuDetailsTrackViewModel : ViewModel
{

    public MsuDetailsTrackViewModel()
    {
        TrackName = "";
        Songs = new List<MsuDetailsSongViewModel>();
    }
    
    public MsuDetailsTrackViewModel(Msu msu, int trackNumber)
    {
        var tracks = msu.Tracks.Where(x => x.Number == trackNumber).ToList();
        if (!tracks.Any())
        {
            TrackNumber = trackNumber;
            TrackName = msu.SelectedMsuType?.Tracks.FirstOrDefault(x => x.Number == trackNumber)?.Name ??
                        $"Track #{trackNumber}";
            Songs = new List<MsuDetailsSongViewModel>();
            return;
        }
        TrackName = tracks.First().TrackName;
        Songs = tracks.OrderBy(x => x.IsAlt).Select(x => new MsuDetailsSongViewModel(x)).ToList();
        TrackNumber = trackNumber;
        HasSongs = true;
    }
    
    public int TrackNumber { get; }
    public string TrackName { get; }
    public bool HasSongs { get; set; }
    public bool ShowSongList => HasSongs;
    public bool ShowNoSongsText => !ShowSongList;
    public string TrackNameText => $"{TrackNumber} - {TrackName}";
    
    public ICollection<MsuDetailsSongViewModel> Songs { get; }
}