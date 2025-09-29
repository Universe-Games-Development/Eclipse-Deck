using UnityEngine;
using Zenject;

public class BoardGameInstaller : MonoInstaller<BoardGameInstaller> {
    [SerializeField] private LoggingSettings loggerSettings = new();
    public override void InstallBindings() {
        Container.Bind<TurnManager>().AsSingle();
        Container.Bind<PlayerHeroFactory>().AsSingle().NonLazy();
        Container.Bind<BoardGame>().FromComponentInHierarchy().AsSingle();
        Container.Bind<TravelManager>().FromComponentInHierarchy().AsSingle();

        Container.Bind<ILogger>().To<GameLogger>().AsSingle().WithArguments(loggerSettings);
        Container.Bind<IOperationManager>().To<OperationManager>().AsSingle();
        Container.Bind<IVisualManager>().To<VisualSequenceManager>().AsSingle();

        
        Container.Bind<IOpponentRegistry>().To<OpponentRegistry>().AsSingle().NonLazy();
        Container.Bind<ICardPlayService>().To<CardPlayService>().AsSingle().NonLazy();
        Container.Bind<ITargetFiller>().To<OperationTargetsFiller>().AsSingle();
        Container.Bind<ITargetValidator>().To<TargetValidator>().AsSingle();
        Container.Bind<Board>().AsSingle();
        
    }
}

[System.Serializable]
public class LoggingSettings {
    [Header("General Settings")]
    public LogLevel minLogLevel = LogLevel.Debug;
    public bool enableUnityConsole = true;
    public bool enableFileLogging = false;
    public bool enableTimestamps = true;
    public bool enableStackTrace = false;

    [Header("Category Settings")]
    public LogCategory enabledCategories = LogCategory.All;

    [Header("Colors (Editor Only)")]
    public Color debugColor = Color.white;
    public Color infoColor = Color.cyan;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
}