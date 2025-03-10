using WebSocketGraphql.Repositories;

namespace WebSocketGraphql.Helpers;

public class ChatObservable<T> : IObservable<T>, IDisposable
{
    private readonly int _chatId;
    private readonly IObservable<T> _source;
    private readonly IChat _chat;
    private IDisposable? _disposed;

    public ChatObservable(int chatId, IChat chat, IObservable<T> observable)
    {
        _chatId = chatId;
        _chat = chat;
        _source = observable;
    }

    public void Dispose()
    {
        _disposed?.Dispose();
        _chat.UnSubscribeMessages(_chatId);
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _disposed = _source.Subscribe(observer);
        return this;
    }
}
