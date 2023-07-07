using YamlDotNet.Serialization;

namespace MSURandomizerLibrary.Configs;

public class MsuDetailsTrackList
{
    [YamlMember(Alias = "zelda_title")]
    [Smz3TrackNumber(ZeldaFirst = 1, MetroidFirst = 101)]
    public MsuDetailsTrack? ZeldaTitle { get; set; }
    
    [YamlMember(Alias = "light_world")]
    [Smz3TrackNumber(ZeldaFirst = 2, MetroidFirst = 102)]
    public MsuDetailsTrack? LightWorld { get; set; }
    
    [YamlMember(Alias = "rainy_intro")]
    [Smz3TrackNumber(ZeldaFirst = 3, MetroidFirst = 103)]
    public MsuDetailsTrack? RainyIntro { get; set; }
    
    [YamlMember(Alias = "bunny_theme")]
    [Smz3TrackNumber(ZeldaFirst = 4, MetroidFirst = 104)]
    public MsuDetailsTrack? BunnyTheme { get; set; }
    
    [YamlMember(Alias = "lost_woods")]
    [Smz3TrackNumber(ZeldaFirst = 5, MetroidFirst = 105)]
    public MsuDetailsTrack? LostWoods { get; set; }
    
    [YamlMember(Alias = "prologue")]
    [Smz3TrackNumber(ZeldaFirst = 6, MetroidFirst = 106)]
    public MsuDetailsTrack? Prologue { get; set; }
    
    [YamlMember(Alias = "kakariko")]
    [Smz3TrackNumber(ZeldaFirst = 7, MetroidFirst = 107)]
    public MsuDetailsTrack? Kakariko { get; set; }
    
    [YamlMember(Alias = "mirror")]
    [Smz3TrackNumber(ZeldaFirst = 8, MetroidFirst = 108)]
    public MsuDetailsTrack? Mirror { get; set; }
    
    [YamlMember(Alias = "dark_world")]
    [Smz3TrackNumber(ZeldaFirst = 9, MetroidFirst = 109)]
    public MsuDetailsTrack? DarkWorld { get; set; }
    
    [YamlMember(Alias = "pedestal_pull")]
    [Smz3TrackNumber(ZeldaFirst = 10, MetroidFirst = 110)]
    public MsuDetailsTrack? PedestalPull { get; set; }
    
    [YamlMember(Alias = "zelda_game_over")]
    [Smz3TrackNumber(ZeldaFirst = 11, MetroidFirst = 111)]
    public MsuDetailsTrack? ZeldaGameOver { get; set; }
    
    [YamlMember(Alias = "guards")]
    [Smz3TrackNumber(ZeldaFirst = 12, MetroidFirst = 112)]
    public MsuDetailsTrack? Guards { get; set; }
    
    [YamlMember(Alias = "dark_death_mountain")]
    [Smz3TrackNumber(ZeldaFirst = 13, MetroidFirst = 113)]
    public MsuDetailsTrack? DarkDeathMountain { get; set; }
    
    [YamlMember(Alias = "minigame")]
    [Smz3TrackNumber(ZeldaFirst = 14, MetroidFirst = 114)]
    public MsuDetailsTrack? Minigame { get; set; }
    
    [YamlMember(Alias = "dark_woods")]
    [Smz3TrackNumber(ZeldaFirst = 15, MetroidFirst = 115)]
    public MsuDetailsTrack? DarkWoods { get; set; }
    
    [YamlMember(Alias = "hyrule_castle")]
    [Smz3TrackNumber(ZeldaFirst = 16, MetroidFirst = 116)]
    public MsuDetailsTrack? HyruleCastle { get; set; }
    
    [YamlMember(Alias = "pendant_dungeon")]
    [Smz3TrackNumber(ZeldaFirst = 17, MetroidFirst = 117)]
    public MsuDetailsTrack? PendantDungeon { get; set; }
    
    [YamlMember(Alias = "cave_1")]
    [Smz3TrackNumber(ZeldaFirst = 18, MetroidFirst = 118)]
    public MsuDetailsTrack? Cave1 { get; set; }
    
    [YamlMember(Alias = "boss_victory")]
    [Smz3TrackNumber(ZeldaFirst = 19, MetroidFirst = 119)]
    public MsuDetailsTrack? BossVictory { get; set; }
    
    [YamlMember(Alias = "sanctuary")]
    [Smz3TrackNumber(ZeldaFirst = 20, MetroidFirst = 120)]
    public MsuDetailsTrack? Sanctuary { get; set; }
    
    [YamlMember(Alias = "zelda_boss_battle")]
    [Smz3TrackNumber(ZeldaFirst = 21, MetroidFirst = 121)]
    public MsuDetailsTrack? ZeldaBossBattle { get; set; }
    
    [YamlMember(Alias = "crystal_dungeon")]
    [Smz3TrackNumber(ZeldaFirst = 22, MetroidFirst = 122)]
    public MsuDetailsTrack? CrystalDungeon { get; set; }
    
    [YamlMember(Alias = "shop")]
    [Smz3TrackNumber(ZeldaFirst = 23, MetroidFirst = 123)]
    public MsuDetailsTrack? Shop { get; set; }
    
    [YamlMember(Alias = "cave_2")]
    [Smz3TrackNumber(ZeldaFirst = 24, MetroidFirst = 124)]
    public MsuDetailsTrack? Cave2 { get; set; }
    
    [YamlMember(Alias = "zelda_rescued")]
    [Smz3TrackNumber(ZeldaFirst = 25, MetroidFirst = 125)]
    public MsuDetailsTrack? ZeldaRescued { get; set; }
    
    [YamlMember(Alias = "crystal_retrieved")]
    [Smz3TrackNumber(ZeldaFirst = 26, MetroidFirst = 126)]
    public MsuDetailsTrack? CrystalRetrieved { get; set; }
    
    [YamlMember(Alias = "fairy")]
    [Smz3TrackNumber(ZeldaFirst = 27, MetroidFirst = 127)]
    public MsuDetailsTrack? Fairy { get; set; }
    
    [YamlMember(Alias = "agahnims_floor")]
    [Smz3TrackNumber(ZeldaFirst = 28, MetroidFirst = 128)]
    public MsuDetailsTrack? AgahnimsFloor { get; set; }
    
    [YamlMember(Alias = "ganon_reveal")]
    [Smz3TrackNumber(ZeldaFirst = 29, MetroidFirst = 129)]
    public MsuDetailsTrack? GanonReveal { get; set; }
    
    [YamlMember(Alias = "ganons_message")]
    [Smz3TrackNumber(ZeldaFirst = 30, MetroidFirst = 130)]
    public MsuDetailsTrack? GanonsMessage { get; set; }
    
    [YamlMember(Alias = "ganon_battle")]
    [Smz3TrackNumber(ZeldaFirst = 31, MetroidFirst = 131)]
    public MsuDetailsTrack? GanonBattle { get; set; }
    
    [YamlMember(Alias = "triforce_room")]
    [Smz3TrackNumber(ZeldaFirst = 32, MetroidFirst = 132)]
    public MsuDetailsTrack? TriforceRoom { get; set; }
    
    [YamlMember(Alias = "epilogue")]
    [Smz3TrackNumber(ZeldaFirst = 33, MetroidFirst = 133)]
    public MsuDetailsTrack? Epilogue { get; set; }
    
    [YamlMember(Alias = "zelda_credits")]
    [Smz3TrackNumber(ZeldaFirst = 34, MetroidFirst = 134)]
    public MsuDetailsTrack? ZeldaCredits { get; set; }
    
    [YamlMember(Alias = "eastern_palace")]
    [Smz3TrackNumber(ZeldaFirst = 35, MetroidFirst = 135)]
    public MsuDetailsTrack? EasternPalace { get; set; }
    
    [YamlMember(Alias = "desert_palace")]
    [Smz3TrackNumber(ZeldaFirst = 36, MetroidFirst = 136)]
    public MsuDetailsTrack? DesertPalace { get; set; }
    
    [YamlMember(Alias = "agahnims_tower")]
    [Smz3TrackNumber(ZeldaFirst = 37, MetroidFirst = 137)]
    public MsuDetailsTrack? AgahnimsTower { get; set; }
    
    [YamlMember(Alias = "swamp_palace")]
    [Smz3TrackNumber(ZeldaFirst = 38, MetroidFirst = 138)]
    public MsuDetailsTrack? SwampPalace { get; set; }
    
    [YamlMember(Alias = "palace_of_darkness")]
    [Smz3TrackNumber(ZeldaFirst = 39, MetroidFirst = 139)]
    public MsuDetailsTrack? PalaceOfDarkness { get; set; }
    
    [YamlMember(Alias = "misery_mire")]
    [Smz3TrackNumber(ZeldaFirst = 40, MetroidFirst = 140)]
    public MsuDetailsTrack? MiseryMire { get; set; }
    
    [YamlMember(Alias = "skull_woods")]
    [Smz3TrackNumber(ZeldaFirst = 41, MetroidFirst = 141)]
    public MsuDetailsTrack? SkullWoods { get; set; }
    
    [YamlMember(Alias = "ice_palace")]
    [Smz3TrackNumber(ZeldaFirst = 42, MetroidFirst = 142)]
    public MsuDetailsTrack? IcePalace { get; set; }
    
    [YamlMember(Alias = "tower_of_hera")]
    [Smz3TrackNumber(ZeldaFirst = 43, MetroidFirst = 143)]
    public MsuDetailsTrack? TowerOfHera { get; set; }
    
    [YamlMember(Alias = "thieves_town")]
    [Smz3TrackNumber(ZeldaFirst = 44, MetroidFirst = 144)]
    public MsuDetailsTrack? ThievesTown { get; set; }
    
    [YamlMember(Alias = "turtle_rock")]
    [Smz3TrackNumber(ZeldaFirst = 45, MetroidFirst = 145)]
    public MsuDetailsTrack? TurtleRock { get; set; }
    
    [YamlMember(Alias = "ganons_tower")]
    [Smz3TrackNumber(ZeldaFirst = 46, MetroidFirst = 146)]
    public MsuDetailsTrack? GanonsTower { get; set; }
    
    [YamlMember(Alias = "armos_knights")]
    [Smz3TrackNumber(ZeldaFirst = 47, MetroidFirst = 147)]
    public MsuDetailsTrack? ArmosKnights { get; set; }
    
    [YamlMember(Alias = "lanmolas")]
    [Smz3TrackNumber(ZeldaFirst = 48, MetroidFirst = 148)]
    public MsuDetailsTrack? Lanmolas { get; set; }
    
    [YamlMember(Alias = "agahnim_1")]
    [Smz3TrackNumber(ZeldaFirst = 49, MetroidFirst = 149)]
    public MsuDetailsTrack? Agahnim1 { get; set; }
    
    [YamlMember(Alias = "arrghus")]
    [Smz3TrackNumber(ZeldaFirst = 50, MetroidFirst = 150)]
    public MsuDetailsTrack? Arrghus { get; set; }
    
    [YamlMember(Alias = "helmasaur_king")]
    [Smz3TrackNumber(ZeldaFirst = 51, MetroidFirst = 151)]
    public MsuDetailsTrack? HelmasaurKing { get; set; }
    
    [YamlMember(Alias = "vitreous")]
    [Smz3TrackNumber(ZeldaFirst = 52, MetroidFirst = 152)]
    public MsuDetailsTrack? Vitreous { get; set; }
    
    [YamlMember(Alias = "mothula")]
    [Smz3TrackNumber(ZeldaFirst = 53, MetroidFirst = 153)]
    public MsuDetailsTrack? Mothula { get; set; }
    
    [YamlMember(Alias = "kholdstare")]
    [Smz3TrackNumber(ZeldaFirst = 54, MetroidFirst = 154)]
    public MsuDetailsTrack? Kholdstare { get; set; }
    
    [YamlMember(Alias = "moldorm")]
    [Smz3TrackNumber(ZeldaFirst = 55, MetroidFirst = 155)]
    public MsuDetailsTrack? Moldorm { get; set; }
    
    [YamlMember(Alias = "blind")]
    [Smz3TrackNumber(ZeldaFirst = 56, MetroidFirst = 156)]
    public MsuDetailsTrack? Blind { get; set; }
    
    [YamlMember(Alias = "trinexx")]
    [Smz3TrackNumber(ZeldaFirst = 57, MetroidFirst = 157)]
    public MsuDetailsTrack? Trinexx { get; set; }
    
    [YamlMember(Alias = "agahnim_2")]
    [Smz3TrackNumber(ZeldaFirst = 58, MetroidFirst = 158)]
    public MsuDetailsTrack? Agahnim2 { get; set; }
    
    [YamlMember(Alias = "ganons_tower_climb")]
    [Smz3TrackNumber(ZeldaFirst = 59, MetroidFirst = 159)]
    public MsuDetailsTrack? GanonsTowerClimb { get; set; }
    
    [YamlMember(Alias = "light_world_2")]
    [Smz3TrackNumber(ZeldaFirst = 60, MetroidFirst = 160)]
    public MsuDetailsTrack? LightWorld2 { get; set; }
    
    [YamlMember(Alias = "dark_world_2")]
    [Smz3TrackNumber(ZeldaFirst = 61, MetroidFirst = 161)]
    public MsuDetailsTrack? DarkWorld2 { get; set; }
    
    [YamlMember(Alias = "smz3_credits")]
    [Smz3TrackNumber(ZeldaFirst = 99, MetroidFirst = 99)]
    public MsuDetailsTrack? Smz3Credits { get; set; }
    
    [YamlMember(Alias = "samus_fanfare")]
    [Smz3TrackNumber(ZeldaFirst = 101, MetroidFirst = 1)]
    public MsuDetailsTrack? SamusFanfare { get; set; }
    
    [YamlMember(Alias = "item_acquired")]
    [Smz3TrackNumber(ZeldaFirst = 102, MetroidFirst = 2)]
    public MsuDetailsTrack? ItemAcquired { get; set; }
    
    [YamlMember(Alias = "item_room")]
    [Smz3TrackNumber(ZeldaFirst = 103, MetroidFirst = 3)]
    public MsuDetailsTrack? ItemRoom { get; set; }
    
    [YamlMember(Alias = "metroid_opening_with_intro")]
    [Smz3TrackNumber(ZeldaFirst = 104, MetroidFirst = 4)]
    public MsuDetailsTrack? MetroidOpeningWithIntro { get; set; }
    
    [YamlMember(Alias = "metroid_opening_without_intro")]
    [Smz3TrackNumber(ZeldaFirst = 105, MetroidFirst = 5)]
    public MsuDetailsTrack? MetroidOpeningWithoutIntro { get; set; }
    
    [YamlMember(Alias = "crateria_landing_with_thunder")]
    [Smz3TrackNumber(ZeldaFirst = 106, MetroidFirst = 6)]
    public MsuDetailsTrack? CrateriaLandingWithThunder { get; set; }
    
    [YamlMember(Alias = "crateria_landing_without_thunder")]
    [Smz3TrackNumber(ZeldaFirst = 107, MetroidFirst = 7)]
    public MsuDetailsTrack? CrateriaLandingWithoutThunder { get; set; }
    
    [YamlMember(Alias = "crateria_space_pirates_appear")]
    [Smz3TrackNumber(ZeldaFirst = 108, MetroidFirst = 8)]
    public MsuDetailsTrack? CrateriaSpacePiratesAppear { get; set; }
    
    [YamlMember(Alias = "golden_statues")]
    [Smz3TrackNumber(ZeldaFirst = 109, MetroidFirst = 9)]
    public MsuDetailsTrack? GoldenStatues { get; set; }
    
    [YamlMember(Alias = "samus_aran_theme")]
    [Smz3TrackNumber(ZeldaFirst = 110, MetroidFirst = 10)]
    public MsuDetailsTrack? SamusAranTheme { get; set; }
    
    [YamlMember(Alias = "green_brinstar")]
    [Smz3TrackNumber(ZeldaFirst = 111, MetroidFirst = 11)]
    public MsuDetailsTrack? GreenBrinstar { get; set; }
    
    [YamlMember(Alias = "red_brinstar")]
    [Smz3TrackNumber(ZeldaFirst = 112, MetroidFirst = 12)]
    public MsuDetailsTrack? RedBrinstar { get; set; }
    
    [YamlMember(Alias = "upper_norfair")]
    [Smz3TrackNumber(ZeldaFirst = 113, MetroidFirst = 13)]
    public MsuDetailsTrack? UpperNorfair { get; set; }
    
    [YamlMember(Alias = "lower_norfair")]
    [Smz3TrackNumber(ZeldaFirst = 114, MetroidFirst = 14)]
    public MsuDetailsTrack? LowerNorfair { get; set; }
    
    [YamlMember(Alias = "inner_maridia")]
    [Smz3TrackNumber(ZeldaFirst = 115, MetroidFirst = 15)]
    public MsuDetailsTrack? InnerMaridia { get; set; }
    
    [YamlMember(Alias = "outer_maridia")]
    [Smz3TrackNumber(ZeldaFirst = 116, MetroidFirst = 16)]
    public MsuDetailsTrack? OuterMaridia { get; set; }
    
    [YamlMember(Alias = "tourian")]
    [Smz3TrackNumber(ZeldaFirst = 117, MetroidFirst = 17)]
    public MsuDetailsTrack? Tourian { get; set; }
    
    [YamlMember(Alias = "mother_brain_battle")]
    [Smz3TrackNumber(ZeldaFirst = 118, MetroidFirst = 18)]
    public MsuDetailsTrack? MotherBrainBattle { get; set; }
    
    [YamlMember(Alias = "big_boss_battle_1")]
    [Smz3TrackNumber(ZeldaFirst = 119, MetroidFirst = 19)]
    public MsuDetailsTrack? BigBossBattle1 { get; set; }
    
    [YamlMember(Alias = "evacuation")]
    [Smz3TrackNumber(ZeldaFirst = 120, MetroidFirst = 20)]
    public MsuDetailsTrack? Evacuation { get; set; }
    
    [YamlMember(Alias = "chozo_statue_awakens")]
    [Smz3TrackNumber(ZeldaFirst = 121, MetroidFirst = 21)]
    public MsuDetailsTrack? ChozoStatueAwakens { get; set; }
    
    [YamlMember(Alias = "big_boss_battle_2")]
    [Smz3TrackNumber(ZeldaFirst = 122, MetroidFirst = 22)]
    public MsuDetailsTrack? BigBossBattle2 { get; set; }
    
    [YamlMember(Alias = "tension")]
    [Smz3TrackNumber(ZeldaFirst = 123, MetroidFirst = 23)]
    public MsuDetailsTrack? Tension { get; set; }
    
    [YamlMember(Alias = "plant_miniboss")]
    [Smz3TrackNumber(ZeldaFirst = 124, MetroidFirst = 24)]
    public MsuDetailsTrack? PlantMiniboss { get; set; }
    
    [YamlMember(Alias = "ceres_station")]
    [Smz3TrackNumber(ZeldaFirst = 125, MetroidFirst = 25)]
    public MsuDetailsTrack? CeresStation { get; set; }
    
    [YamlMember(Alias = "wrecked_ship_powered_off")]
    [Smz3TrackNumber(ZeldaFirst = 126, MetroidFirst = 26)]
    public MsuDetailsTrack? WreckedShipPoweredOff { get; set; }
    
    [YamlMember(Alias = "wrecked_ship_powered_on")]
    [Smz3TrackNumber(ZeldaFirst = 127, MetroidFirst = 27)]
    public MsuDetailsTrack? WreckedShipPoweredOn { get; set; }
    
    [YamlMember(Alias = "theme_of_super_metroid")]
    [Smz3TrackNumber(ZeldaFirst = 128, MetroidFirst = 28)]
    public MsuDetailsTrack? ThemeOfSuperMetroid { get; set; }
    
    [YamlMember(Alias = "death_cry")]
    [Smz3TrackNumber(ZeldaFirst = 129, MetroidFirst = 29)]
    public MsuDetailsTrack? DeathCry { get; set; }
    
    [YamlMember(Alias = "metroid_credits")]
    [Smz3TrackNumber(ZeldaFirst = 130, MetroidFirst = 30)]
    public MsuDetailsTrack? MetroidCredits { get; set; }
    
    [YamlMember(Alias = "kraid_incoming")]
    [Smz3TrackNumber(ZeldaFirst = 131, MetroidFirst = 31)]
    public MsuDetailsTrack? KraidIncoming { get; set; }
    
    [YamlMember(Alias = "kraid_battle")]
    [Smz3TrackNumber(ZeldaFirst = 132, MetroidFirst = 32)]
    public MsuDetailsTrack? KraidBattle { get; set; }
    
    [YamlMember(Alias = "phantoon_incoming")]
    [Smz3TrackNumber(ZeldaFirst = 133, MetroidFirst = 33)]
    public MsuDetailsTrack? PhantoonIncoming { get; set; }
    
    [YamlMember(Alias = "phantoon_battle")]
    [Smz3TrackNumber(ZeldaFirst = 134, MetroidFirst = 34)]
    public MsuDetailsTrack? PhantoonBattle { get; set; }
    
    [YamlMember(Alias = "draygon_battle")]
    [Smz3TrackNumber(ZeldaFirst = 135, MetroidFirst = 35)]
    public MsuDetailsTrack? DraygonBattle { get; set; }
    
    [YamlMember(Alias = "ridley_battle")]
    [Smz3TrackNumber(ZeldaFirst = 136, MetroidFirst = 36)]
    public MsuDetailsTrack? RidleyBattle { get; set; }
    
    [YamlMember(Alias = "baby_incoming")]
    [Smz3TrackNumber(ZeldaFirst = 137, MetroidFirst = 37)]
    public MsuDetailsTrack? BabyIncoming { get; set; }
    
    [YamlMember(Alias = "the_baby")]
    [Smz3TrackNumber(ZeldaFirst = 138, MetroidFirst = 38)]
    public MsuDetailsTrack? TheBaby { get; set; }
    
    [YamlMember(Alias = "hyper_beam")]
    [Smz3TrackNumber(ZeldaFirst = 139, MetroidFirst = 39)]
    public MsuDetailsTrack? HyperBeam { get; set; }
    
    [YamlMember(Alias = "metroid_game_over")]
    [Smz3TrackNumber(ZeldaFirst = 140, MetroidFirst = 40)]
    public MsuDetailsTrack? MetroidGameOver { get; set; }
}