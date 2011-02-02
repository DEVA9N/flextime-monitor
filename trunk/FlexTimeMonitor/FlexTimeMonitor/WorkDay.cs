﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace A9N.FlexTimeMonitor
{
    /// <summary>
    /// Represents a work day
    /// </summary>
    public class WorkDay
    {
        private DateTime end;

        /// <summary>
        /// Creates Workday instance with start time now
        /// </summary>
        public WorkDay()
        {
            Start = DateTime.Now;
            End = DateTime.Now;
        }

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// End time 
        /// </summary>
        public DateTime End
        {
            get
            {
                Boolean isToday = this.Start.Date == DateTime.Now.Date;

                if (isToday)
                {
                    end = DateTime.Now;
                }

                return end;
            }
            set
            {
                end = value;
            }
        }

        #region Studpid input hacks

        // Note: there is something like Dependency Property which may or may not be suitable to handle the input data. But I have no time to check that further.
        // Non the less: there has to be a way to handle the input, maybe with a direct data access (like WCF: Message Handling).

        /// <summary>
        /// Temporarily used to access Start.
        /// Will prevent a unwanted date change. You can't just simply replace "Start" 
        /// since there would be no nice way to set the date (well you could create
        /// another date property but that is even worse).
        /// </summary>
        [XmlIgnore]
        public DateTime StartHack { get { return Start; } set { Start = new DateTime(Start.Year, Start.Month, Start.Day, value.Hour, value.Minute, value.Second); } }

        /// <summary>
        /// Temporarily used to access End.
        /// Will prevent a unwanted date change. You can't just simply replace "End" 
        /// since there would be no nice way to set the date (well you could create
        /// another date property but that is even worse).
        /// </summary>
        [XmlIgnore]
        public DateTime EndHack { get { return End; } set { End = new DateTime(End.Year, End.Month, End.Day, value.Hour, value.Minute, value.Second); } }

        #endregion

        /// <summary>
        /// The difference between Difference and the complete workday (including break period)
        /// </summary>
        public TimeSpan OverTime
        {
            get
            {
                // This will prevent output clutter ("T" will result in "-01:23:45:123245", "hh\:mm" in "01:23" ignoring the negative value)
                TimeSpan result = Elapsed - (Properties.Settings.Default.WorkPeriod + Properties.Settings.Default.BreakPeriod);
                return new TimeSpan(result.Hours, result.Minutes, result.Seconds);
            }
        }

        /// <summary>
        /// Difference between start and now
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                // This will prevent output clutter ("T" will result in "-01:23:45:123245", "hh\:mm" in "01:23" ignoring the negative value)
                TimeSpan result = End - Start;
                return new TimeSpan(result.Hours, result.Minutes, result.Seconds);
            }
        }

        /// <summary>
        /// Estimated end time
        /// </summary>
        public DateTime Estimated
        {
            get { return Start + (Properties.Settings.Default.WorkPeriod + Properties.Settings.Default.BreakPeriod); }
        }

        /// <summary>
        /// Remaining time - oppoosite of OverTime (5min Remaining == -5min OverTime)
        /// </summary>
        public TimeSpan Remaining
        {
            get { return -OverTime; }
        }

        /// <summary>
        /// Additional note
        /// </summary>
        public String Note { get; set; }
    }
}