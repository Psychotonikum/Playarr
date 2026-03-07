using System.Threading.Tasks;

namespace Playarr.SignalR
{
    public interface IBroadcastSignalRMessage
    {
        bool IsConnected { get; }
        Task BroadcastMessage(SignalRMessage message);
    }
}
