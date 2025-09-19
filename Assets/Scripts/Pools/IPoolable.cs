public interface IPoolable {
    void OnPoolCreate();   // Викликається при створенні
    void OnPoolGet();                   // Викликається при взятті з пулу
    void OnPoolRelease();              // Викликається при поверненні в пул
    void OnPoolDestroy();              // Викликається при знищенні
}
