﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace A9N.FlexTimeMonitor
{
    /// <summary>
    /// Represents a work day
    /// </summary>
    public sealed class WorkDay : INotifyPropertyChanged
    {
        private int _weekNumber;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates Workday instance with start time now
        /// </summary>
        public WorkDay()
        {
            Data = new WorkDayData
            {
                Date = DateTime.Now,
                Start = DateTime.Now,
                End = DateTime.Now
            };
        }

        /// <summary>
        /// Converts the TimeSpan to a DateTime instance for the current workday.
        /// </summary>
        /// <remarks>
        /// This methods is a workaround that solves the problems that TimeSpans that are entered in the grid view by
        /// the user do not contain a proper date component. In order to store a DateTime that matches the current
        /// date of the workday this method takes Date to create a valid DateTime object.
        /// 
        /// BTW: for DateTime objects that are entered in the grid view there is another issue. If only the time is 
        /// entered the DateTime object will always use the date of today. It makes it impossible to alter old times
        /// from the history without making the date invalid.
        /// </remarks>
        /// <param name="time">The time.</param>
        /// <returns>DateTime.</returns>
        private DateTime ConvertToDateTime(TimeSpan time)
        {
            return new DateTime(this.Date.Year, this.Date.Month, this.Date.Day, time.Hours, time.Minutes, time.Seconds);
        }


        /// <summary>
        /// Gets or sets the data object that stores the workday data.
        /// </summary>
        /// <value>The data.</value>
        public WorkDayData Data { get; set; }

        /// <summary>
        /// Gets or sets the date of the workday
        /// </summary>
        /// <value>The date.</value>
        [XmlIgnore]
        public DateTime Date
        {
            get => Data.Date;
            set => Data.Date = value;
        }

        /// <summary>
        /// Gets the week number.
        /// </summary>
        /// <value>The week number.</value>
        [XmlIgnore]
        public int WeekOfYear
        {
            get
            {
                if (_weekNumber == 0)
                {
                    var calendar = System.Globalization.DateTimeFormatInfo.CurrentInfo?.Calendar;

                    _weekNumber = calendar?.GetWeekOfYear(Date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday) ?? 0;
                }

                return _weekNumber;
            }
        }

        /// <summary>
        /// Start time
        /// </summary>
        /// <value>The start.</value>
        [XmlIgnore]
        public TimeSpan Start
        {
            get => Data.Start.TimeOfDay;
            set => Data.Start = ConvertToDateTime(value);
        }

        /// <summary>
        /// End time
        /// </summary>
        /// <value>The end.</value>
        [XmlIgnore]
        public TimeSpan End
        {
            get => IsToday ? DateTime.Now.TimeOfDay : Data.End.TimeOfDay;
            set => Data.End = ConvertToDateTime(value);
        }


        /// <summary>
        /// The difference between Difference and the complete workday (including break period)
        /// </summary>
        /// <value>The over time.</value>
        [XmlIgnore]
        public TimeSpan OverTime => Elapsed - (Properties.Settings.Default.WorkPeriod + Properties.Settings.Default.BreakPeriod);

        /// <summary>
        /// Gets or sets the discrepancy. Discrepancy is a positive or negative time offset that is taken into account
        /// when calculating the total workday time. For example a skipped lunch break can be set by +1h or a doctor's
        /// appointment can be set by -1h.
        /// </summary>
        /// <value>The discrepancy.</value>
        [XmlIgnore]
        public TimeSpan Discrepancy
        {
            get => Data.Discrepancy.TimeOfDay;
            set => Data.Discrepancy = ConvertToDateTime(value);
        }

        /// <summary>
        /// Difference between start and now also considering a possible discrepancy.
        /// </summary>
        /// <value>The elapsed.</value>
        [XmlIgnore]
        public TimeSpan Elapsed => End - Start + Discrepancy;

        /// <summary>
        /// Estimated end time
        /// </summary>
        /// <value>The estimated.</value>
        [XmlIgnore]
        public TimeSpan Estimated => Start - Discrepancy + (Properties.Settings.Default.WorkPeriod + Properties.Settings.Default.BreakPeriod);

        /// <summary>
        /// Remaining time - opposite of OverTime (5min Remaining == -5min OverTime)
        /// </summary>
        /// <value>The remaining.</value>
        [XmlIgnore]
        public TimeSpan Remaining => -OverTime;

        /// <summary>
        /// Additional note
        /// </summary>
        /// <value>The note.</value>
        [XmlIgnore]
        public String Note
        {
            get => Data.Note;
            set => Data.Note = value;
        }

        /*
         * Instead of using enhanced WPF patterns to achieve certain benefits I took the fast approach and am using some
         * helper properties instead. These helper methods are used in the datagrid for trigger and display purposes.
         * 
         */

        /// <summary>
        /// Gets a value indicating whether this instance is odd week.
        /// </summary>
        /// <remarks>
        /// This value is used to highlight the odd weeks in the datagrid.
        /// </remarks>
        /// <value><c>true</c> if this instance is odd week; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool IsOddWeek
        {
            get
            {
                if (WeekOfYear > 0)
                {
                    return WeekOfYear % 2 > 0;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is today.
        /// </summary>
        /// <remarks>
        /// This value is used to highlight today in the datagrid.
        /// </remarks>
        /// <value><c>true</c> if this instance is today; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool IsToday => Date.Date == DateTime.Now.Date;

        /// <summary>
        /// Gets a value indicating whether this instance has discrepancy which is either Discrepancy != Zero or negative OverTim.
        /// </summary>
        /// <value><c>true</c> if this instance has discrepancy; otherwise, <c>false</c>.</value>
        [XmlIgnore]
        public bool HasNegativeOvertime => OverTime < TimeSpan.Zero && !IsToday;

        /// <summary>
        /// This is a workaround for the missing capability of TimeSpan formating to handle negative values.
        /// This property is used as a binding in the xaml code.
        /// </summary>
        /// <value>The over time string.</value>
        [XmlIgnore]
        public String OverTimeString => TimeSpanExtension.ToHhmmss(OverTime);
    }
}