using TinyMessenger;

namespace MusicBeeRemote.Core.Events.Status.Internal
{
    public class BroadcastEventAvailable : ITinyMessage
    {
        public BroadcastEventAvailable(BroadcastEvent broadcastEvent)
        {
            BroadcastEvent = broadcastEvent;
        }

        public object Sender { get; } = null;

        public BroadcastEvent BroadcastEvent { get; }
    }
}
