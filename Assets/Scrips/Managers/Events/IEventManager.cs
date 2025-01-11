using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public interface IEventManager {
    /// <summary>
    /// ������ ������� ���� ��� ����������� ���� ����.
    /// </summary>
    /// <param name="listener">������, ���� ����������� �� ��䳿.</param>
    /// <param name="eventType">��� ��䳿, �� ��� ������ �����.</param>
    /// <param name="executionType">��� ��������� (���������� �� �����������).</param>
    /// <param name="executionOrder">������� ��������� ��� ���������� ��������.</param>
    void RegisterListener<T>(IEventListener listener, T eventType, ExecutionType executionType, int executionOrder = 0, bool isPhantomListener = false) where T : Enum;

    /// <summary>
    /// ������� ������� ���� ��� ����������� ���� ����.
    /// </summary>
    /// <param name="listener">������, ���� ����� �� �� ��������� �� ��䳿.</param>
    /// <param name="eventType">��� ��䳿, � ����� ����������� ������.</param>
    void UnregisterListener<T>(IEventListener listener, T eventType, bool isPhantomListener = false) where T : Enum;

    /// <summary>
    /// ��������� ���� ������� ����, ��������� ���������� ��������.
    /// </summary>
    /// <param name="eventType">��� ��䳿, ��� ��������� ���������.</param>
    /// <param name="gameContext">�������� ���, ���� ���� ��������� ��������.</param>
    /// <returns>���������� ��������, ��� ����������� ���� ������� ��䳿 ���� ���������.</returns>
    UniTask TriggerEventAsync<T>(T eventType, GameContext gameContext, CancellationToken cancellationToken = default, bool isPhantomCall = false) where T : Enum;
}

public enum ExecutionType {
    Sequential,
    Parallel
}
