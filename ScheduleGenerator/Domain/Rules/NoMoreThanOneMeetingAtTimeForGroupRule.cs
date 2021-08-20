﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ScheduleLib;

namespace Domain.Rules
{
    public class NoMoreThanOneMeetingAtTimeForGroupRule : IRule
    {
        public readonly double UnitPenalty;

        public NoMoreThanOneMeetingAtTimeForGroupRule(double unitPenalty = 1500)
        {
            UnitPenalty = unitPenalty;
        }

        public double Evaluate(LearningPlan learningPlan, Requisition requisition, Schedule schedule, Meeting meetingToAdd)
        {
            var badMeetings = GetCollidedMeetings(schedule, meetingToAdd);
            var totalPenalty = UnitPenalty * badMeetings.Count;
            return totalPenalty;
        }
        
        public static List<Meeting> GetCollidedMeetings(Schedule schedule, Meeting meetingToAdd)
        {
            var meetingToAddGroupsNames = meetingToAdd.Groups.Select(e => e.GroupName).ToHashSet();
            var meetingsWithSameGroup = schedule.Meetings
                .Where(m => m.Groups.Select(e=>e.GroupName).Intersect(meetingToAddGroupsNames).ToList().Count != 0)
                .Where(m => m.WeekType == meetingToAdd.WeekType || m.WeekType == WeekType.All || meetingToAdd.WeekType == WeekType.All)
                .Where(m => m.MeetingTime.Equals(meetingToAdd.MeetingTime))
                .ToList();
            return meetingsWithSameGroup;
        }
    }
}
