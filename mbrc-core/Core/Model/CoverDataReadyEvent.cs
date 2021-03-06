﻿using TinyMessenger;

namespace MusicBeeRemote.Core.Model
{
    public class CoverDataReadyEvent : ITinyMessage
    {
        public CoverDataReadyEvent(string cover)
        {
            Cover = cover;
        }

        public string Cover { get; }

        public object Sender { get; } = null;
    }
}
