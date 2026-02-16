using System;
using UnityEngine;

namespace DesertRider.MP3
{
    /// <summary>
    /// Data class representing metadata for a single song in the library.
    /// See TECHNICAL_PLAN.md Section 4.1 for usage details.
    /// </summary>
    [Serializable]
    public class SongData
    {
        /// <summary>
        /// MD5 hash of the MP3 file - used for unique identification and leaderboard matching.
        /// </summary>
        public string Hash;

        /// <summary>
        /// Song title extracted from ID3 tags.
        /// </summary>
        public string Title;

        /// <summary>
        /// Artist name extracted from ID3 tags.
        /// </summary>
        public string Artist;

        /// <summary>
        /// Album name extracted from ID3 tags.
        /// </summary>
        public string Album;

        /// <summary>
        /// Total duration of the song.
        /// </summary>
        public TimeSpan Duration;

        /// <summary>
        /// Date and time when the song was imported into the library.
        /// </summary>
        public DateTime ImportDate;

        /// <summary>
        /// Path to the local copy of the MP3 file.
        /// </summary>
        public string LocalPath;

        /// <summary>
        /// Reference to the analysis data for this song.
        /// </summary>
        public AnalysisData AnalysisData;

        /// <summary>
        /// Player's high score for this specific song (MP3 mode only).
        /// </summary>
        public int HighScore;

        // TODO: Add album art texture reference
        // TODO: Add play count tracking
        // TODO: Add last played timestamp
    }
}
