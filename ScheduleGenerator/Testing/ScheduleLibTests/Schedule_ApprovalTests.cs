﻿using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Domain.Algorithms;
using Domain.Conversions;
using Domain.MeetingsParts;
using Infrastructure.GoogleSheetsRepository;
using NUnit.Framework;
using static Infrastructure.SheetConstants;

namespace Testing.ScheduleLibTests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class Schedule_ApprovalTests
    {
        [Test]
        public void CheckMeetingsPlaced_Approval()
        {
            // TODO сейчас это копипаста мейна - пофиксить дублирование

            var inputRequirementsSheetId = 861045221;
            var inputRequirementsSheetUrl = Url + inputRequirementsSheetId;
            var repo = new GsRepository("test", CredentialPath, inputRequirementsSheetUrl);
            repo.SetUpSheetInfo();

            var (requisitions, _, classrooms) = SheetToRequisitionConverter.ConvertToRequisitions(
                repo, InputRequirementsSheetName, LearningPlanSheetName, ClassroomsSheetName);

            var requisition = new Requisition(requisitions.ToArray());

            var estimator = ScheduleCLI.Program.GetDefaultCombinedEstimator();

            var solver = new GreedySolver(estimator, requisition, classrooms, new(42));
            var schedule = solver
                .GetSolution(new(0, 1, 5))
                .Last()
                .Schedule;

            var placedMeetings = schedule
                .Meetings
                .Select(m => $"{m.Discipline} {m.MeetingType} {m.Teacher}")
                .OrderBy(m => m)
                .ToList();
            var notePlacedMeetings = schedule
                .NotUsedMeetings
                .Select(m => $"{m.Discipline} {m.MeetingType} {m.Teacher}")
                .OrderBy(m => m)
                .ToList();
            var unifiedString =
                $"Placed:{placedMeetings.Count} Left:{schedule.NotUsedMeetings.Count}, Placed:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, placedMeetings)}{Environment.NewLine}" +
                $"Not placed:{Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, notePlacedMeetings)}{Environment.NewLine}";
            Approvals.Verify(unifiedString);
        }
    }
}