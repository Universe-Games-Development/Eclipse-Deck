using Unity.Android.Gradle.Manifest;
using Zenject;

public interface IEnemyFactory {
    Enemy Create(OpponentData data);
}

public class EnemyFactory : IEnemyFactory {
    private DiContainer _container;

    public EnemyFactory(DiContainer container) {
        _container = container;
    }

    public Enemy Create(OpponentData data) {
        return _container.Instantiate<Enemy>(new object[] { data });
    }
}


