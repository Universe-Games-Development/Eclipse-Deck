using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public interface IEventManager {
    /// <summary>
    /// Реєструє слухача подій для конкретного типу подій.
    /// </summary>
    /// <param name="listener">Слухач, який реагуватиме на події.</param>
    /// <param name="eventType">Тип події, на яку слухач реагує.</param>
    /// <param name="executionType">Тип виконання (послідовний чи паралельний).</param>
    /// <param name="executionOrder">Порядок виконання для послідовних слухачів.</param>
    void RegisterListener<T>(IEventListener listener, T eventType, ExecutionType executionType, int executionOrder = 0, bool isPhantomListener = false) where T : Enum;

    /// <summary>
    /// Видаляє слухача подій для конкретного типу подій.
    /// </summary>
    /// <param name="listener">Слухач, який більше не має реагувати на події.</param>
    /// <param name="eventType">Тип події, з якого видаляється слухач.</param>
    void UnregisterListener<T>(IEventListener listener, T eventType, bool isPhantomListener = false) where T : Enum;

    /// <summary>
    /// Тригерить подію певного типу, передаючи додатковий контекст.
    /// </summary>
    /// <param name="eventType">Тип події, яку необхідно тригерити.</param>
    /// <param name="gameContext">Контекст гри, який буде переданий слухачам.</param>
    /// <returns>Асинхронне завдання, яке завершується після обробки події усіма слухачами.</returns>
    UniTask TriggerEventAsync<T>(T eventType, GameContext gameContext, CancellationToken cancellationToken = default, bool isPhantomCall = false) where T : Enum;
}

public enum ExecutionType {
    Sequential,
    Parallel
}
