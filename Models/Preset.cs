﻿namespace PlaceKupo.Models
{
    /// <summary>
    /// Preset Model for use in the application.
    /// </summary>
    public class Preset
    {
        /// <summary>
        /// Name of this preset.
        /// </summary>
        //public string Name { get; set; }

        /// <summary>
        /// Map ID of where this preset belongs.
        /// </summary>
        //public uint MapID { get; set; }

        /// <summary>
        /// Waymark values for all of every waymark in the game.
        /// </summary>
        public Waymark A { get; set; }
        public Waymark B { get; set; }
        public Waymark C { get; set; }
        public Waymark D { get; set; }
        public Waymark One { get; set; }
        public Waymark Two { get; set; }

        /// <summary>
        /// Property Changed event handler for this model.
        /// </summary>
#pragma warning disable 67
#pragma warning restore 67
    }
}
