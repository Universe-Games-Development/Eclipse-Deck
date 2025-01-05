using Cysharp.Threading.Tasks;
using System.Threading;

public interface IEventManager {
    /// <summary>
    /// ������ ������� ���� ��� ����������� ���� ����.
    /// </summary>
    /// <param name="listener">������, ���� ����������� �� ��䳿.</param>
    /// <param name="eventType">��� ��䳿, �� ��� ������ �����.</param>
    /// <param name="executionType">��� ��������� (���������� �� �����������).</param>
    /// <param name="executionOrder">������� ��������� ��� ���������� ��������.</param>
    void RegisterListener(IEventListener listener, EventType eventType, ExecutionType executionType, int executionOrder = 0);

    /// <summary>
    /// ������� ������� ���� ��� ����������� ���� ����.
    /// </summary>
    /// <param name="listener">������, ���� ����� �� �� ��������� �� ��䳿.</param>
    /// <param name="eventType">��� ��䳿, � ����� ����������� ������.</param>
    void UnregisterListener(IEventListener listener, EventType eventType);

    /// <summary>
    /// ��������� ���� ������� ����, ��������� ���������� ��������.
    /// </summary>
    /// <param name="eventType">��� ��䳿, ��� ��������� ���������.</param>
    /// <param name="gameContext">�������� ���, ���� ���� ��������� ��������.</param>
    /// <returns>���������� ��������, ��� ����������� ���� ������� ��䳿 ���� ���������.</returns>
    UniTask TriggerEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default);
}
