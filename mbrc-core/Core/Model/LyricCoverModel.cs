﻿using System;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MusicBeeRemote.Core.Events;
using MusicBeeRemote.Core.Events.Internal;
using MusicBeeRemote.Core.Threading;
using NLog;
using TinyMessenger;

namespace MusicBeeRemote.Core.Model
{
    internal class LyricCoverModel : IDisposable
    {
        private readonly ITinyMessengerHub _hub;

        /** Singleton **/
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly LimitedTaskScheduler _scheduler = new LimitedTaskScheduler(1);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private string _xHash;
        private string _lyrics;

        public LyricCoverModel(ITinyMessengerHub hub)
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _hub.Subscribe<CoverAvailable>(msg => Task.Factory.StartNew(
                () => SetCover(msg.Cover),
                _cts.Token,
                TaskCreationOptions.PreferFairness,
                _scheduler));
            _hub.Subscribe<LyricsAvailable>(msg => Lyrics = msg.Lyrics);
        }

        public string Cover { get; private set; }

        public string Lyrics
        {
            get => _lyrics;
            private set
            {
                try
                {
                    var lStr = value.Trim();
                    if (lStr.Contains("\r\r\n\r\r\n"))
                    {
                        /* Convert new line & empty line to xml safe format */
                        lStr = lStr.Replace("\r\r\n\r\r\n", " \r\n ");
                        lStr = lStr.Replace("\r\r\n", " \n ");
                    }

                    lStr = lStr.Replace("\0", " ");
                    const string pattern = "\\[\\d:\\d{2}.\\d{3}\\] ";
                    var regEx = new Regex(pattern);
                    _lyrics = SecurityElement.Escape(regEx.Replace(lStr, string.Empty));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Lyrics processing");
                    _lyrics = string.Empty;
                }
                finally
                {
                    _hub.Publish(new LyricsDataReadyEvent(_lyrics));
                }
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
        }

        private void SetCover(string base64)
        {
            var hash = Utilities.ArtworkUtilities.Sha1Hash(base64);

            if (_xHash != null && _xHash.Equals(hash, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            Cover = string.IsNullOrEmpty(base64)
                ? string.Empty
                : Utilities.ArtworkUtilities.ImageResize(base64);
            _xHash = hash;

            _hub.Publish(new CoverDataReadyEvent(Cover));
        }
    }
}
