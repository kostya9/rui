using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;
using StackExchange.Redis;
using WinUi.Annotations;

namespace WinUi;

public record RedisServer(string Name, string Address, int Port, string Username, string Password)
{
    public Guid Id { get; } = Guid.NewGuid();
};

public record RedisKey(string Key);

public record ConnectedRedisServer(RedisServerListEntry ServerEntry, ConnectionMultiplexer Connection) : INotifyPropertyChanged
{
    public enum State_ { Idle, Refreshing }

    public ObservableCollection<RedisKey> VisibleKeys { get; set; } = new();

    private State_ _state = State_.Idle;

    public State_ State
    {
        get => _state;
        set
        {
            if(_state == value)
                return;

            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsBusy => _state is State_.Refreshing;

    public bool IsNotBusy => !IsBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
};

public record RedisServerListEntry(RedisServer Server) : INotifyPropertyChanged
{
    public enum State { Connected, Connecting, Disconnected, Disconnecting, BeingDeleted }

    private State _entryState = State.Disconnected;
    public State EntryState
    {
        get
        {
            return _entryState;
        }
        set
        {
            if(_entryState == value)
                return;

            _entryState = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(GoIcon));
            OnPropertyChanged(nameof(DeleteIcon));
        }
    }

    public bool IsNotBusy => !IsBusy;
    public bool IsBusy => _entryState is State.Connecting or State.Disconnecting or State.BeingDeleted;

    public Symbol GoIcon => IsBusy ? Symbol.Sync : Symbol.Go;

    public Symbol DeleteIcon => IsBusy ? Symbol.Sync : Symbol.Delete;

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}