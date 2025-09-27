public interface IGenericFactory {
    T Create<T>(params object[] args) where T : class;
}