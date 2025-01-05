using Cysharp.Threading.Tasks;
using System.Threading;

public interface IEventListener {
    /// <summary>
    /// �����������, ���� �������� ���� ��������� ����.
    /// </summary>
    /// <param name="eventType">��� ��䳿, ��� ������� ��������.</param>
    /// <param name="gameContext">�������� ���, ���� ������ �������� ��� ��� ������� ��䳿.</param>
    /// <returns>���������� ������, ��� ����������� ���� ������� ��䳿.</returns>
    UniTask OnEventAsync(EventType eventType, GameContext gameContext, CancellationToken cancellationToken = default);
}
