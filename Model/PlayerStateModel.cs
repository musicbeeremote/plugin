﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security;
using MusicBeePlugin.Error;
using MusicBeePlugin.Events.Args;

namespace MusicBeePlugin.Model
{
    internal class PlayerStateModel
    {
        public event EventHandler<DataEventArgs> ModelStateEvent;

        private string _artist;
        private string _title;
        private string _album;
        private string _year;
        private string _cover;

        private string _lyrics;

        private Plugin.PlayState _playState;
        private int _volume;

        private bool _shuffleState;
        private Plugin.RepeatMode _repeatMode;
        private bool _muteState;
        private bool _scrobblerState;
        private int _trackRating;

        private void OnModelStateChange(DataEventArgs args)
        {
            EventHandler<DataEventArgs> handler = ModelStateEvent;
            if (handler != null) handler(this, args);
        }

        public int TrackRating
        {
            get { return _trackRating; }
            set
            {
                _trackRating = value;
                OnModelStateChange(new DataEventArgs(DataType.TrackRating));
            }
        }

        public bool ScrobblerState
        {
            get { return _scrobblerState; }
            set
            {
                _scrobblerState = value;
                OnModelStateChange(new DataEventArgs(DataType.ScrobblerState));
            }
        }

        public bool MuteState
        {
            get { return _muteState; }
            set
            {
                _muteState = value;
                OnModelStateChange(new DataEventArgs(DataType.MuteState));
            }
        }

        public Plugin.RepeatMode RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                _repeatMode = value;
                OnModelStateChange(new DataEventArgs(DataType.RepeatMode));
            }
        }

        public bool ShuffleState
        {
            get { return _shuffleState; }
            set
            {
                _shuffleState = value;
                OnModelStateChange(new DataEventArgs(DataType.ShuffleState));
            }
        }

        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnModelStateChange(new DataEventArgs(DataType.Volume));
            }
        }

        public Plugin.PlayState PlayState
        {
            set
            {
                _playState = value;
                OnModelStateChange(new DataEventArgs(DataType.PlayState));
            }
            get { return _playState; }
        }

        public string Artist
        {
            get { return _artist; }
            set
            {
                _artist = value;
                OnModelStateChange(new DataEventArgs(DataType.Artist));
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnModelStateChange(new DataEventArgs(DataType.Title));
            }
        }

        public string Album
        {
            get { return _album; }
            set
            {
                _album = value;
                OnModelStateChange(new DataEventArgs(DataType.Album));
            }
        }

        public string Year
        {
            get { return _year; }
            set
            {
                _year = value;
                OnModelStateChange(new DataEventArgs(DataType.Year));
            }
        }

        public string Cover
        {
            set
            {
                try
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        _cover = string.Empty;
                        return;
                    }
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(value))
                        )
                    using (Image albumCover = Image.FromStream(ms, true))
                    {
                        ms.Flush();
                        int sourceWidth = albumCover.Width;
                        int sourceHeight = albumCover.Height;

                        float nPercentW = (300/(float) sourceWidth);
                        float nPercentH = (300/(float) sourceHeight);

                        var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;
                        int destWidth = (int) (sourceWidth*nPercent);
                        int destHeight = (int) (sourceHeight*nPercent);
                        using (var bmp = new Bitmap(destWidth, destHeight))
                        using (MemoryStream ms2 = new MemoryStream())
                        {
                            Graphics graph = Graphics.FromImage(bmp);
                            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graph.DrawImage(albumCover, 0, 0, destWidth, destHeight);
                            graph.Dispose();

                            bmp.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
                            _cover = Convert.ToBase64String(ms2.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError(ex);
                    _cover = String.Empty;
                }
                finally
                {
                    OnModelStateChange(new DataEventArgs(DataType.Cover));
                }
            }
            get { return _cover; }
        }

        public string Lyrics
        {
            set
            {
                try
                {
                    string lyricsString = value.Trim();
                    if (lyricsString.Contains("\r\r\n\r\r\n"))
                    {
                        /* Convert new line & empty line to xml safe format */
                        lyricsString = lyricsString.Replace("\r\r\n\r\r\n", " &lt;p&gt; ");
                        lyricsString = lyricsString.Replace("\r\r\n", " &lt;br&gt; ");
                    }
                    lyricsString = lyricsString.Replace("\0", " ");
                    lyricsString = lyricsString.Replace("\r\n", "&lt;p&gt;");
                    lyricsString = lyricsString.Replace("\n", "&lt;br&gt;");
                    _lyrics = SecurityElement.Escape(lyricsString);
                }
                catch (Exception ex)
                {
#if DEBUG
                    ErrorHandler.LogError(ex);
#endif
                    _lyrics = String.Empty;
                }
                finally
                {
                    OnModelStateChange(new DataEventArgs(DataType.Lyrics));
                }
            }
            get { return _lyrics; }
        }
    }
}