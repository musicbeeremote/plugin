using TinyMessenger;

namespace MusicBeeRemote.Core.Events.Internal
{
    internal class ForceClientDisconnect : ITinyMessage
    {
        public ForceClientDisconnect(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public object Sender { get; } = null;

        public string ConnectionId { get; }
    }
}
