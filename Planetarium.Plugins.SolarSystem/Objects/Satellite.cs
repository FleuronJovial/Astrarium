﻿using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public abstract class Satellite : SizeableCelestialObject
    {
        /// <summary>
        /// Apparent equatorial coordinates of the moon
        /// </summary>
        //public CrdsEquatorial Equatorial { get; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the moon
        /// </summary>
        //public CrdsRectangular Rectangular { get; }

        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        public double CM { get; set; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Number of the moon
        /// </summary>
        public int Number { get; protected set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; set;  }

        public abstract bool IsEclipsedByPlanet { get; }
    }
}
