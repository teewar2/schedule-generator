﻿using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Google.Apis.Sheets.v4.Data;
using Infrastructure.GoogleSheetsRepository;

namespace Domain.Conversions
{
    public class ScheduleSpreadsheetConverter
    {
        private readonly GsRepository repository;
        private readonly string sheetName;
        private const int TimeBarRowOffset = 4;
        private const int TimeBarColumnOffset = 0;
        private const int HeadersColumnOffset = 2;
        private const int HeadersRowOffset = 2;

        private static readonly string[] WeekDays = {"ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ"};
        private static readonly int WeekDayCount = WeekDays.Length;

        private static readonly string[] ClassStarts =
        {
            "I 9:00", "II 10:40", "III 12:50",
            "IV 14:30", "V 16:40", "VI 17:50"
        };

        private static readonly int StartIndexesCount = ClassStarts.Length;

        public ScheduleSpreadsheetConverter(GsRepository repo, string sheetName)
        {
            repository = repo;
            this.sheetName = sheetName;
        }

        public void Build(IReadonlySchedule schedule)
        {
            var groupNamesSet = new HashSet<string>();
            var meetingSet = new HashSet<Meeting>();
            foreach (var meeting in schedule.GetMeetings())
            {
                foreach (var group in meeting.Groups!) groupNamesSet.Add(group.GroupName);
                meetingSet.Add(meeting);
            }

            Console.WriteLine($"Прокинется дальше: {meetingSet.Count}");
            var groupNames = groupNamesSet.OrderBy(gn => gn).ToList();

            PrepareSheet();

            BuildSchedulePattern(groupNames);

            FillScheduleData(meetingSet, groupNames);
        }

        private void PrepareSheet()
        {
            repository.ModifySpreadSheet(sheetName)
                .ClearAll()
                .UnMergeAll()
                .Execute();
        }

        private void BuildSchedulePattern(List<string> groups)
        {
            ColorField(groups);
            BuildTimeBar();
            BuildGroupHeaders(groups);
        }

        private void ColorField(List<string> groups)
        {
            var color = new Color {Blue = 15 / 16f, Green = 15 / 16f, Red = 15 / 16f};
            var height = WeekDayCount * StartIndexesCount * 2 - 1;
            var width = groups.Count * 2 - 1;
            var start = (TimeBarRowOffset, HeadersColumnOffset);
            var end = (TimeBarRowOffset + height, HeadersColumnOffset + width);
            // TODO krutovsky: return GsRepository
            repository
                .ModifySpreadSheet(sheetName)
                .ColorizeRange(start, end, color)
                .Execute();
        }

        private void BuildTimeBar()
        {
            var modifier = repository
                .ModifySpreadSheet(sheetName);
            var currentStart = TimeBarRowOffset;
            foreach (var weekDay in WeekDays)
            {
                modifier
                    .WriteRange((currentStart, TimeBarColumnOffset), new() {new() {weekDay}})
                    .AddBorders((currentStart, TimeBarColumnOffset), (currentStart + 11, TimeBarColumnOffset))
                    .MergeCell((currentStart, TimeBarColumnOffset), (currentStart + 11, TimeBarColumnOffset));
                currentStart += 12;
            }

            currentStart = TimeBarRowOffset;
            foreach (var unused in WeekDays)
            foreach (var classStart in ClassStarts)
            {
                modifier
                    .WriteRange((currentStart, TimeBarColumnOffset + 1), new() {new() {classStart}})
                    .AddBorders((currentStart, TimeBarColumnOffset + 1),
                        (currentStart + 1, TimeBarColumnOffset + 1))
                    .MergeCell((currentStart, TimeBarColumnOffset + 1),
                        (currentStart + 1, TimeBarColumnOffset + 1));
                currentStart += 2;
            }

            modifier.Execute();
        }

        private void BuildGroupHeaders(List<string> groups)
        {
            var modifier = repository
                .ModifySpreadSheet(sheetName);
            var currentStart = HeadersColumnOffset;
            foreach (var group in groups)
            {
                modifier
                    .WriteRange((HeadersRowOffset, currentStart), new() {new() {group}})
                    .AddBorders((HeadersRowOffset, currentStart), (HeadersRowOffset, currentStart + 1))
                    .MergeCell((HeadersRowOffset, currentStart), (HeadersRowOffset, currentStart + 1))
                    .WriteRange((HeadersRowOffset + 1, currentStart),
                        new() {new() {group + "-1", group + "-2"}})
                    .AddBorders((HeadersRowOffset + 1, currentStart), (HeadersRowOffset + 1, currentStart))
                    .AddBorders((HeadersRowOffset + 1, currentStart + 1), (HeadersRowOffset + 1, currentStart + 1));
                currentStart += 2;
            }

            modifier.Execute();
        }

        private void FillScheduleData(HashSet<Meeting> meetings, List<string> groups)
        {
            var groupIndexDict = groups
                .Select((g, i) => (g, i))
                .ToDictionary(gi => gi.g, gi => gi.i);
            var modifier = repository
                .ModifySpreadSheet(sheetName);

            foreach (var meeting in meetings) WriteMeeting(meeting, groupIndexDict, modifier);
            modifier.Execute();
        }

        private string FillLocation(Meeting meeting)
        {
            return meeting.Location switch
            {
                Location.Kontur => "Контур",
                Location.PashaEgorov => "ФОК",
                Location.Online => "Онлайн",
                Location.MathMeh => meeting.Classroom ?? "",
                _ => "БИбиба!"
            };
        }

        private void WriteMeeting(Meeting meeting, Dictionary<string, int> groupIndexDict, SheetModifier modifier)
        {
            var horizOffset = 2;
            var vertOffset = 2;

            var weekDayToIntDict = new Dictionary<DayOfWeek, int>
            {
                {DayOfWeek.Monday, 0},
                {DayOfWeek.Tuesday, 1},
                {DayOfWeek.Wednesday, 2},
                {DayOfWeek.Thursday, 3},
                {DayOfWeek.Friday, 4},
                {DayOfWeek.Saturday, 5}
                // { DayOfWeek.Sunday, 6}
            };


            foreach (var (groupName, groupPart) in meeting.Groups!)
            {
                // var data = $"{meeting.Discipline}, {meeting.Teacher?.Name}, {meeting.MeetingTime}";
                var classroom = FillLocation(meeting);
                var data =
                    $"{meeting.Discipline}, " +
                    $"{classroom}, " +
                    $"{meeting.Teacher?.Name}";

                var rowNumOff = weekDayToIntDict[meeting.MeetingTime!.Day] * 12 + vertOffset;
                var rowNum = meeting.MeetingTime.TimeSlotIndex * 2 + rowNumOff;
                var rowsInMeeting = 1;
                if (meeting.WeekType == WeekType.Even) rowNum++;
                if (meeting.WeekType == WeekType.All) rowsInMeeting = 2;

                var colNum = groupIndexDict[groupName] * 2 + horizOffset;
                var columnsInMeeting = 1;
                if (groupPart == GroupPart.Part2) colNum++;
                if (groupPart == GroupPart.FullGroup) columnsInMeeting = 2;
                modifier
                    .WriteRange((rowNum, colNum), new() {new() {data}})
                    .AddBorders((rowNum, colNum), (rowNum + rowsInMeeting - 1, colNum + columnsInMeeting - 1));
                if (rowsInMeeting == 2 || columnsInMeeting == 2)
                    modifier.MergeCell((rowNum, colNum), (rowNum + rowsInMeeting - 1, colNum + columnsInMeeting - 1));
            }
        }
    }
}