﻿using System;
using Domain.ScheduleLib;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Rules
{
    class NumberOfClassesInARow : IRule
    {
        public readonly double UnitPenalty;
        public int MaxClassesInARow { get; set; }

        public NumberOfClassesInARow(double unitPenalty = 1500, int maxClassesInARow = 5)
        {
            UnitPenalty = unitPenalty;
            MaxClassesInARow = maxClassesInARow;
        }

        public double Evaluate(LearningPlan learningPlan, Requisition requisition, Schedule schedule, Meeting meetingToAdd)
        {
            var classesInARowDifference = FindClassesInARowDifference(schedule, meetingToAdd);
            if (classesInARowDifference < 0)
            {
                return 0;
            }
            var totalPenalty = UnitPenalty * classesInARowDifference;
            return totalPenalty;
        }

        private static int FindClassesInARowDifference(Schedule schedule, Meeting meetingToAdd)
        {
            var todaysMeetings = schedule.Meetings
                .Where(m => m.WeekType == meetingToAdd.WeekType)
                .Where(m => m.MeetingTime.Day == meetingToAdd.MeetingTime.Day)
                .Where(m => m.Groups.First() == meetingToAdd.Groups.First());

            var maxCountInSchedule = FindMaxClassesInARow(todaysMeetings);
            var newTodaysMeetings = todaysMeetings.ToHashSet();
            newTodaysMeetings.Add(meetingToAdd);
            var maxCountInNewSchedule = FindMaxClassesInARow(newTodaysMeetings);
            return maxCountInNewSchedule - maxCountInSchedule;
        }

        private static int FindMaxClassesInARow(IEnumerable<Meeting> meetings)
        {
            var orderedMeetings = meetings.OrderBy(m => m.MeetingTime.TimeSlotIndex);

            var lastPointer = orderedMeetings.First().MeetingTime.TimeSlotIndex;
            var currentPointer = lastPointer;
            var meetingCountInARow = 0;
            var maxCount = 0;
            foreach (var meeting in orderedMeetings)
            {
                currentPointer = meeting.MeetingTime.TimeSlotIndex;
                if (currentPointer - lastPointer < 2)
                {
                    meetingCountInARow++;
                    if (meetingCountInARow > maxCount)
                    {
                        maxCount = meetingCountInARow;
                    }
                }
                else
                {
                    meetingCountInARow = 1;
                }
                lastPointer = currentPointer;
            }

            return maxCount;
        }

        //public EvaluationResult Evaluate(Schedule schedule, Requisition requisition)
        //{
        //    var badMeetings = GetBadMeetings(schedule);
        //    return new EvaluationResult
        //    (
        //        badMeetings.Length * UnitPenalty,
        //        badMeetings,
        //        "Превышено допустимое количество пар подряд"
        //    );
        //}

        //private Meeting[] GetBadMeetings(Schedule schedule)
        //{
        //    var badMeetings = new List<Meeting>();
        //    foreach (var weekTypeGrouping in schedule.Meetings.GroupBy(m => m.WeekType))
        //    {
        //        // Should be exactly one group specified in meeting
        //        foreach (var groupGrouping in weekTypeGrouping.GroupBy(m => m.Groups.First()))
        //        {
        //            foreach (var dayGrouping in groupGrouping.GroupBy(m => m.MeetingTime.Day))
        //            {
        //                var currentBadMeetings = AnalizeDay((IEnumerable<Meeting>)dayGrouping.GetEnumerator());
        //                badMeetings.AddRange(currentBadMeetings);
        //            }
        //        }
        //    }
        //    return badMeetings.ToArray();
        //}

        //private IEnumerable<Meeting> AnalizeDay(IEnumerable<Meeting> meetings)
        //{
        //    if (!meetings.Any())
        //    {
        //        return meetings;
        //    }
        //    var todaysBadMeetings = new List<Meeting>();
        //    var orderedMeetings = meetings.OrderBy(m => m.MeetingTime.TimeSlotIndex);
        //    var lastPointer = orderedMeetings.First().MeetingTime.TimeSlotIndex;
        //    var currentPointer = lastPointer;
        //    var meetingCountInARow = 0;
        //    foreach (var meeting in orderedMeetings)
        //    {
        //        currentPointer = meeting.MeetingTime.TimeSlotIndex;
        //        if (currentPointer - lastPointer < 2)
        //        {
        //            meetingCountInARow++;
        //        }
        //        else
        //        {
        //            meetingCountInARow = 1;
        //        }
        //        if (meetingCountInARow > MaxClassesInARow)
        //        {
        //            todaysBadMeetings.Add(meeting);
        //        }
        //        lastPointer = currentPointer;
        //    }

        //    return todaysBadMeetings;
        //}
    }
}