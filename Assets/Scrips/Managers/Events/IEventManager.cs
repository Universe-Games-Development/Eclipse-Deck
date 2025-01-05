using Cysharp.Threading.Tasks;
using System.Threading;

public interface IEventManager {
    /// <summary>
    /// Реєструє слухача подій для конкретного типу подій.
    /// </summary>
    /// <param name="listener">Слухач, який реагуватиме на події.</param>
    /// <param name="eventType">Тип події, на яку слухач реагує.</param>
    /// <param name="executionType">Тип виконання (послідовний чи паралельний).</param>
    /// <param name="executionOrder">Порядок виконання для послідовних слухачів.</param>
    void RegisterListener(IEventListener listener, EventType eventType, ExecutionType executionType, int executionOrder = 0);

    /// <summary>
    /// Видаляє слухача подій для конкретного типу подій.
    /// </summary>
    /// <param name="listener">Слухач, який більше не має реагувати на події.</param>
    /// <param name="eventType">Тип події, з якого видаляється слухач.</param>
    void UnregisterListener(IEventListener listener, EventType eventType);

    /// <summary>
    /// Тригерить подію певного типу, передаючи додатковий контекст.
    /// </summary>
    /// <param name="eventType">Тип події, яку необхідно тригерити.</param>
    /// <param name="gameContext">Контекст гри, який буде переданий слухачам.</param>
    /// <returns>Асинхронне завдання, яке завершується після обробки події усіма слухачами.</returns>
    UniTask TriggerEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default);
}
