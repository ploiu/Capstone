﻿using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Capstone.Common
{
    public static class DateTimeParser
    {
        private enum DaysOfWeek
        {
            // used to determine if there's an error
            NONE = -1,
            SUNDAY = DayOfWeek.Sunday,
            MONDAY = DayOfWeek.Monday,
            TUESDAY = DayOfWeek.Tuesday,
            WEDNESDAY = DayOfWeek.Wednesday,
            THURSDAY = DayOfWeek.Thursday,
            FRIDAY = DayOfWeek.Friday,
            SATURDAY = DayOfWeek.Saturday
        }

        private enum Months
        {
            // used to indicate an error
            NONE = -1,
            JANUARY = 1,
            FEBRUARY = 2,
            MARCH = 3,
            APRIL = 4,
            MAY = 5,
            JUNE = 6,
            JULY = 7,
            AUGUST = 8,
            SEPTEMBER = 9,
            OCTOBER = 10,
            NOVEMBER = 11,
            DECEMBER = 12
        }

        /// <summary>
        /// Represents supported units of time for this parser class
        /// </summary>
        private enum Units
        {
            // used to represent an error
            NONE = -1,
            SECOND = 0,
            MINUTE = 1,
            HOUR = 2,
            DAY = 3,
            WEEK = 4,
            MONTH = 5,
            YEAR = 6
        }

        /// <summary>
        /// Formats a time-like piece of string to be in a parseable format by <see cref="DateTime.Parse(string)"/>
        /// The rules for parsing are as follows:
        /// <list type="bullet">
        /// <item>If the time has 1 or 2 digits, those digits are used as the hour and 00 is set as the minutes</item>
        /// <item>If the time has exactly 3 digits, the first digit is used as the hour and the other 2 are used as the minutes</item>
        /// <item>If the time has exactly 4 digits, the first 2 digits are used as the hour and the other 2 are used as the minutes</item>
        /// <item>if the original time has either am/pm in it, am/pm will be added to the end of the time respectively</item>
        /// </list>
        /// This method does not validate that the time is ok, only that it's formatted correctly
        /// </summary>
        /// <param name="time">The time-like string to be properly formatted</param>
        /// <returns></returns>
        public static string FormatTime(string time)
        {
            // a copy of the passed time so that we don't screw up the original for later
            var cleanRegex = new Regex("(am|pm)|[ :]");
            string timeCopy = cleanRegex.Replace(time.ToLower(), "");
            // the length of the string changes where we need to insert characters
            if (timeCopy.Length == 1 || timeCopy.Length == 2)
            {
                // it's the hour; need to add ":00"
                timeCopy += ":00";
            }
            else if (timeCopy.Length == 3)
            {
                // full time with a single-digit hour; add a colon after the first character
                timeCopy = timeCopy.Substring(0, 1) + ":" + timeCopy.Substring(1);
            }
            else if (timeCopy.Length == 4)
            {
                // full time with a double-digit hour; add a colon after the second character
                timeCopy = timeCopy.Substring(0, 2) + ":" + timeCopy.Substring(2);
            }
            // re-add the am/pm if the original time has it
            if (time.ToLower().Contains("am"))
            {
                timeCopy += " am";
            }
            else if (time.ToLower().Contains("pm"))
            {
                timeCopy += " pm";
            }
            return timeCopy;
        }

        /// <summary>
        /// Returns a date that is on the specified <paramref name="nextDay"/> 1 week after the passed <paramref name="date"/>
        /// </summary>
        /// <param name="nextDay">The weekday to get the date for</param>
        /// <param name="date">The date to get the weekday for relative to</param>
        /// <returns>a new date 1 week after the passed date on the passed nextDay</returns>
        public static DateTime GetDateForNextWeekDay(System.DayOfWeek nextDay, DateTime date)
        {
            // get the number of days in between the date's day of the week and the passed nextDay
            DayOfWeek today = date.DayOfWeek;
            // formula to get the number of days until the specified next day = 7 - (today - nextDay)
            int numberOfDaysUntilNextDay = 7 - (today - nextDay);
            // create a new DateTime from the passed date so that we don't modify the passed date
            DateTime newDate = new DateTime(date.Ticks).AddDays(numberOfDaysUntilNextDay);
            return newDate;
        }

        /// <summary>
        /// Gets a date from the passed text that contains a relative date (e.g. "set an alarm for 5 days from now") and returns it
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onlyUsedForTests">the date used as the starting point to create the returned date from</param>
        /// <returns></returns>
        /// <exception cref="DateParseException" />
        public static DateTime GetDateForRelativeOffset(string text, DateTime onlyUsedForTests)
        {
            DateTime offsetDateTime;
            var now = onlyUsedForTests;
            Units specifiedUnit = ParseUnitFromText(text);
            if (specifiedUnit == Units.NONE)
            {
                throw new DateParseException($"could not find a unit in the passed string [{text}]");
            }
            // create a regex to parse out the number before the returned unit
            var regexString = $"(?i)[0-9]{{1,3}}(?= {specifiedUnit.ToString()})(?-i)";
            var regex = new Regex(regexString);
            var match = regex.Match(text);
            if (match.Success)
            {
                int parsedNumber = int.Parse(match.Value);
                // figure out which unit to increment
                switch (specifiedUnit)
                {
                    case Units.SECOND:
                        offsetDateTime = now.AddSeconds(parsedNumber);
                        break;
                    case Units.MINUTE:
                        offsetDateTime = now.AddMinutes(parsedNumber);
                        break;
                    case Units.HOUR:
                        offsetDateTime = now.AddHours(parsedNumber);
                        break;
                    case Units.DAY:
                        offsetDateTime = now.AddDays(parsedNumber);
                        break;
                    case Units.WEEK:
                        offsetDateTime = now.AddDays(7 * parsedNumber);
                        break;
                    case Units.MONTH:
                        offsetDateTime = now.AddMonths(parsedNumber);
                        break;
                    case Units.YEAR:
                        offsetDateTime = now.AddYears(parsedNumber);
                        break;
                    default:
                        throw new DateParseException("Something went wrong with adding the unit to the date");
                }
            }
            else
            {
                throw new DateParseException($"could not find number of unit for text [{text}]");
            }
            return offsetDateTime;
        }

        /// <summary>
        /// Gets the date for a specific day that has yet to occur during this week. To get the next date for a day that has already happened, see <see cref="GetDateForNextWeekDay"/>
        /// </summary>
        /// <param name="nextDay">The day of the week to get the date for</param>
        /// <param name="date">the date that the day of the week should be determined starting from</param>
        /// <returns></returns>
        /// <exception cref="DateParseException" />
        public static DateTime GetDateForThisWeekDay(System.DayOfWeek nextDay, DateTime date)
        {
            // get the number of days in between the date's day of the week and the passed nextDay
            DayOfWeek today = date.DayOfWeek;
            // if today is after the passed nextDay, then throw an error
            if (today > nextDay)
            {
                throw new DateParseException($"[{nextDay.ToString()}] has already passed");
            }
            // formula to get the number of days until the specified next day = Math.Abs(today - nextDay)
            int numberOfDaysUntilNextDay = Math.Abs(today - nextDay);
            // create a new DateTime from the passed date so that we don't modify the passed date
            DateTime newDate = new DateTime(date.Ticks).AddDays(numberOfDaysUntilNextDay);
            return newDate;
        }

        public static DateTime GetNextOccurrenceOfDate(int day, int month, DateTime onlyUsedForTests)
        {
            var now = onlyUsedForTests;
            var year = (int)month < now.Month || (int)month == now.Month && day < now.Day ? now.Year + 1 : now.Year;
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Finds the first instance of text in the string that matches a time-like format. Examples of this format are:
        /// <list type="bullet">
        /// <item>7 am</item>
        /// <item>7:30 pm</item>
        /// <item>2359</item>
        /// <item>19 o'clock</item>
        /// </list>
        /// This method does not ensure that the time is in proper format to be parsed by <see cref="System.DateTime.Parse(string)"/>
        /// </summary>
        /// <param name="text">the string to find a time-like structure in</param>
        /// <returns>the found time if it was found, else an empty string</returns>
        public static string GetTimePartOfString(string text)
        {
            string timePart = "";
            text = text.ToLower();
            // use a regex to parse out the time. This regex can handle partial times, full times, am, pm, and o'clock formats.
            var timeRegex = new Regex("([0-2]?[0-9][: ]?[0-5][0-9]( ?[ap]m)?)|([0-9]{1,4} ?[ap]m)|([0-9]{1,2}(?=( ?)o([' -]?)clock))|([0-9]{1,4}$|[0-9]{1,2}[: ][0-9]{1,2}$)");
            var match = timeRegex.Match(text);
            if (match.Success)
            {
                timePart = match.Value;
            }
            return timePart;
        }

        public static DateTime ParseExactDate(string text)
        {
            return ParseExactDate(text, DateTime.Now);
        }

        /// <summary>
        /// Parses an exact date (e.g. "March 4th 2014", "March 4th", "4th of March", etc.) from a string of text and returns it
        /// </summary>
        /// <param name="text">The text to parse the date from</param>
        /// <param name="startingDate">Today's date, used to determine if the date references next year or not. Passed as a parameter to allow for unit testing</param>
        /// <returns></returns>
        /// <exception cref="DateParseException" />
        public static DateTime ParseExactDate(string text, DateTime startingDate)
        {
            // get the first occurrence of a month within the text
            Months referencedMonth = GetMonthFromString(text);
            if (referencedMonth == Months.NONE)
            {
                throw new DateParseException("Month not found in text!");
            }
            // the datetime to return
            DateTime dateTime;
            // get the index of the month in the string and determine where the day of the month is using a regex
            string pattern = $"[0-9]{{1,2}}(?=(th|rd|st|nd) of {referencedMonth.ToString().ToLower()})|(?<={referencedMonth.ToString().ToLower()} )[0-9]{{1,2}}";
            Regex dayOfMonthRegex = new Regex(pattern);
            var match = dayOfMonthRegex.Match(text.ToLower());
            if (match.Success)
            {
                // check if the month has already passed. If so, we need to shift the year forward
                var matchedDay = int.Parse(match.Value);
                dateTime = GetNextOccurrenceOfDate(matchedDay, (int)referencedMonth, startingDate);
            }
            else
            {
                throw new DateParseException("Regex match could not find the month's day!");
            }
            return dateTime;
        }

        public static DateTime ParseSlashOrDashNotation(string text)
        {
            return ParseSlashOrDashNotation(text, DateTime.Now);
        }

        /// <summary>
        /// Parses a date from a piece of text in a format with a separator and returns it. The allowed separators are:
        /// <list type="bullet">
        /// <item>slash</item>
        /// <item>period</item>
        /// <item>space</item>
        /// <item>dash</item>
        /// </list>
        /// </summary>
        /// <param name="text">The text to parse the date from</param>
        /// <param name="onlyUsedForTests">The date used to determine if the date refers to next year, if no year is specified. This is passed as a parameter to allow for unit tests to work</param>
        /// <returns></returns>
        /// <exception cref="DateParseException" />
        public static DateTime ParseSlashOrDashNotation(string text, DateTime onlyUsedForTests)
        {
            DateTime parsedDate;
            // matches a month/day/year pattern using either dashes, slashes, spaces, or periods as the separator
            var withYear = new Regex("[0-9]{1,2}[-/ .][0-9]{1,2}[-/ .][0-9]{4}");
            var withoutYear = new Regex("[0-9]{1,2}[-/ .][0-9]{1,2}");
            var withYearMatch = withYear.Match(text);
            var withoutYearMatch = withoutYear.Match(text);
            if (withYearMatch.Success)
            {
                // parse the date outright, there's no need to determine if the year needs to be shifted as it was specified
                parsedDate = DateTime.Parse(withYearMatch.Value);
            }
            else if (withoutYearMatch.Success)
            {
                // use our method to get the next occurrence of that date
                var dateWithWrongYear = DateTime.Parse(withoutYearMatch.Value);
                parsedDate = GetNextOccurrenceOfDate(dateWithWrongYear.Day, dateWithWrongYear.Month, onlyUsedForTests);
            }
            else
            {
                throw new DateParseException("A date could not be parsed from " + text);
            }
            return parsedDate;
        }

        /// <summary>
        /// scans the pased text for one of the days of the week and returns which <see cref="System.DayOfWeek"/> matches the string. The check is case-insensitive.
        /// <br />
        /// If no day of week was matched, a <see cref="DateParseException"/> will be thrown
        /// </summary>
        /// <param name="text">the text to check</param>
        /// <returns>the matching day of week.</returns>
        /// <exception cref="DateParseException" />
        public static System.DayOfWeek ParseWeekDayFromString(string text)
        {
            text = text.ToLower();
            DaysOfWeek matchedDay = DaysOfWeek.NONE;
            var dayOfWeekList = Enum.GetValues(typeof(DaysOfWeek));
            foreach (DaysOfWeek day in dayOfWeekList)
            {
                string dayName = day.ToString().ToLower();
                if (text.Contains(dayName))
                {
                    matchedDay = day;
                }
            }
            // figure out which System.DayOfWeek to return
            switch (matchedDay)
            {
                case DaysOfWeek.SUNDAY:
                    return DayOfWeek.Sunday;
                case DaysOfWeek.MONDAY:
                    return DayOfWeek.Monday;
                case DaysOfWeek.TUESDAY:
                    return DayOfWeek.Tuesday;
                case DaysOfWeek.WEDNESDAY:
                    return DayOfWeek.Wednesday;
                case DaysOfWeek.THURSDAY:
                    return DayOfWeek.Thursday;
                case DaysOfWeek.FRIDAY:
                    return DayOfWeek.Friday;
                case DaysOfWeek.SATURDAY:
                    return DayOfWeek.Saturday;
                case DaysOfWeek.NONE:
                default:
                    throw new DateParseException($"The string [{text}] does not contain a day of the week!");
            }
        }

        private static int FindFirstIndexOfMonth(string text, Months month)
        {
            text = text.ToLower();
            int matchingIndex = text.ToLower().IndexOf(month.ToString().ToLower());
            return matchingIndex;
        }

        private static DateTime GetDateForNextWeekDay(System.DayOfWeek nextDay)
        {
            return GetDateForNextWeekDay(nextDay, DateTime.Now);
        }

        private static DateTime GetDateForRelativeOffset(string text)
        {
            return GetDateForRelativeOffset(text, DateTime.Now);
        }

        private static Months GetMonthFromString(string text)
        {
            text = text.ToLower();
            Months foundMonth = Months.NONE;
            // get an array of the months
            var months = Enum.GetValues(typeof(Months));
            foreach (Months month in months)
            {
                if (text.Contains(month.ToString().ToLower()))
                {
                    foundMonth = month;
                    break;
                }
            }
            return foundMonth;
        }

        /// <summary>
        /// takes a piece of text and replaces all references of full month names with their numeric counterparts (e.g. January becomes 1, and December becomes 12). The replacing is done ignoring case.
        /// </summary>
        /// <param name="text">the text to have all month occurrences replaced in</param>
        /// <returns></returns>
        private static DateTime GetNextOccurrenceOfDate(int day, int month)
        {
            return GetNextOccurrenceOfDate(day, month, DateTime.Now);
        }

        private static Units ParseUnitFromText(string text)
        {
            Units parsedUnit = Units.NONE;
            var unitsArray = Enum.GetValues(typeof(Units));
            foreach (Units unit in unitsArray)
            {
                var lowercaseUnit = unit.ToString().ToLower();
                if (text.ToLower().Contains(lowercaseUnit))
                {
                    parsedUnit = unit;
                    break;
                }
            }
            return parsedUnit;
        }
        private static string ReplaceMonthNamesWithMonthNumber(string text)
        {
            string result = text;
            // for each month, create a case-insensitive regex for it and use it to replace the text
            var months = Enum.GetValues(typeof(Months));
            foreach (Months month in months)
            {
                var monthRegex = new Regex($"(?i){month.ToString()}(?-i)");
                result = monthRegex.Replace(result, ((int)month).ToString());
            }
            return result;
        }
    }

    public class DateParseException : Exception
    {
        public DateParseException()
        {
        }

        public DateParseException(string message) : base(message)
        {
        }

        public DateParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DateParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}