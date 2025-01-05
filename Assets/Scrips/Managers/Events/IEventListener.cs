using Cysharp.Threading.Tasks;
using System.Threading;

public interface IEventListener {
    /// <summary>
    /// Викликається, коли менеджер подій тригерить подію.
    /// </summary>
    /// <param name="eventType">Тип події, яку потрібно обробити.</param>
    /// <param name="gameContext">Контекст гри, який містить необхідні дані для обробки події.</param>
    /// <returns>Асинхронна задача, яка завершується після обробки події.</returns>
    UniTask OnEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default);
}
