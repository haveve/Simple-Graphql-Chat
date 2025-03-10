using WebSocketGraphql.Repositories;
using WebSocketGraphql.ViewModels;

namespace WebSocketGraphql.Helpers;

public class UserOnlineObservable<T> : IObservable<T>, IDisposable
{
    private readonly int _userId;
    private readonly IObservable<T> _source;
    private readonly IChat _chat;
    private IDisposable? _disposed;

    public UserOnlineObservable(int userId, IEnumerable<int> chatIds, IChat chat, IObservable<T> observable)
    {
        this._userId = userId;
        this._chat = chat;
        this._source = observable;

        _chat.SetOnile(_userId).ContinueWith(x =>
        {
            _chat.NotifyAllChats(chatIds, new ChatParticipant() { Id = _userId, Online = true });
        });
    }

    public void Dispose()
    {
        _disposed?.Dispose();
        _chat.SetOffline(_userId).ContinueWith(x =>
        {
            _chat.NotifyAllChats(x.Result, new ChatParticipant() { Id = _userId, Online = false });
        });

        _chat.UnSubscribeUserNotification(_userId);
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _disposed = _source.Subscribe(observer);
        return this;
    }
}
