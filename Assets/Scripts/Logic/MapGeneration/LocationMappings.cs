using System.Collections.Generic;

public static class LocationMappings {
    public static readonly Dictionary<string, Location> SceneNameToLocation = new Dictionary<string, Location> {
        { "MainMenu", Location.MainMenu },
        { "Sewers", Location.Sewers },
        { "Cave", Location.Cave },
        { "FloodedCave", Location.FloodedCave },
        { "Lab", Location.Lab },
        { "Hell", Location.Hell },
        { "GameLoading", Location.GameLoading },
        { "Loading", Location.Loading }
    };

    public static readonly Dictionary<Location, string> LocationToScene = new Dictionary<Location, string> {
        { Location.MainMenu, "MainMenu" },
        { Location.Sewers, "Sewers" },
        { Location.Cave, "Cave" },
        { Location.FloodedCave, "FloodedCave" },
        { Location.Lab, "Lab" },
        { Location.Hell, "Hell" },
        { Location.GameLoading, "GameLoading" },
        { Location.Loading, "Loading" }
    };
}
