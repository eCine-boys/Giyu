using System;

namespace Giyu.Core.Managers
{
    /// <summary>
    /// Facade for legacy audio calls. Wraps the new playback, queue and lyrics services
    /// to ease migration from the former AudioManager implementation.
    /// </summary>
    public class AudioManager
    {
        public PlaybackService Playback { get; }
        public QueueService Queue { get; }
        public LyricsService Lyrics { get; }

        public AudioManager(PlaybackService playback, QueueService queue, LyricsService lyrics)
        {
            Playback = playback ?? throw new ArgumentNullException(nameof(playback));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Lyrics = lyrics ?? throw new ArgumentNullException(nameof(lyrics));
        }
    }
}
