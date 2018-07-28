﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common
{
    public enum EnumSeaon
    {
        Spring,
        Summer,
        Fall,
        Winter
    }

    public enum EnumMoonPhase
    {
        Empty,
        Grow1,
        Grow2,
        Grow3,
        Full,
        Shrink1,
        Shrink2,
        Shrink3
    }

    public interface IGameCalendar
    {
        string PrettyDate();

        /// <summary>
        /// This acts as a multiplier on how much faster an ingame second passes by compared to a real life second. The default is 60, hence per default a day lasts 24 minutes
        /// This is the sum of all modifiers
        /// </summary>
        int SpeedOfTime { get; }

        /// <summary>
        /// If you want to modify the time speed, set a value here
        /// </summary>
        void SetTimeSpeedModifier(string name, int speed);

        void RemoveTimeSpeedModifier(string name);


        /// <summary>
        /// Amount of hours per day
        /// </summary>
        float HoursPerDay { get; }

        /// <summary>
        /// Amount of days per year
        /// </summary>
        int DaysPerYear { get; }

        /// <summary>
        /// Amount of days per month
        /// </summary>
        int DaysPerMonth { get; }

        /// <summary>
        /// Returns a normalized vector of the sun position
        /// </summary>
        Vec3f SunPositionNormalized { get; }

        /// <summary>
        /// Returns a vector of the sun position
        /// </summary>
        Vec3f SunPosition { get; }

        /// <summary>
        /// Returns a vector of the moon position
        /// </summary>
        Vec3f MoonPosition { get; }


        Vec3f SunColor { get; }

        /// <summary>
        /// Returns a value between 0 (no sunlight) and 1 (full sunlight)
        /// </summary>
        /// <returns></returns>
        float DayLightStrength { get; set; }

        /// <summary>
        /// The current hour of the day as integer
        /// </summary>
        int FullHourOfDay { get; }

        /// <summary>
        /// The current hour of the day as decimal 
        /// </summary>
        float HourOfDay { get; }

        /// <summary>
        /// Total passed hours since the game has started
        /// </summary>
        double TotalHours { get; }

        /// <summary>
        /// Total passed days since the game has started
        /// </summary>
        double TotalDays { get; }

        /// <summary>
        /// The current day of the year (goes from 0 to DaysPerYear)
        /// </summary>
        int DayOfYear { get; }

        
        /// <summary>
        /// Returns the year. Every game begins with 1386
        /// </summary>
        int Year { get; }

        /// <summary>
        /// Returns the current season
        /// </summary>
        EnumSeaon Season { get; }
        

        /// <summary>
        /// Adds given time to the calendar
        /// </summary>
        /// <param name="hours"></param>
        void Add(float hours);



        EnumMoonPhase MoonPhase { get; }
        double MoonPhaseExact { get; }
        float MoonBrightness { get; }
        float MoonSize { get; }
    }
}