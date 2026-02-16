namespace DesertRider.Core
{
    /// <summary>
    /// Defines the two main game modes in Desert Rider.
    /// See TECHNICAL_PLAN.md Section 2 for architecture details.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// MP3 Mode: User imports MP3 files into library.
        /// - Deterministic level generation (same song = same level)
        /// - Leaderboards enabled (matched by audio hash)
        /// - Pre-analyzed audio data used for perfect sync
        /// - Seeded terrain, obstacles, and collectibles
        /// </summary>
        MP3,

        /// <summary>
        /// Free Play Mode: Real-time system audio capture.
        /// - Dynamic level generation (unique every time)
        /// - No leaderboards (non-deterministic)
        /// - Real-time beat detection and analysis
        /// - Works with any audio source (Spotify, YouTube, etc.)
        /// </summary>
        FreePlay
    }
}
