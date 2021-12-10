using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Domain.Enums;
using Domain.MeetingsParts;
using static Infrastructure.LoggerExtension;

namespace Domain.Algorithms.Solvers
{
    public record BeamNode(Schedule Schedule, Meeting MeetingToAdd, double Score)
    {
        public virtual bool Equals(BeamNode? other)
        {
            if (other is null)
                return false;
            if (Math.Abs(this.Score - other.Score) > 0.00001)
                return false;
            if (GetMeetingsCount() != other.GetMeetingsCount()) return false;
            var s1 = this.GetMeetings()
                .ToHashSet();
            var s2 = other.GetMeetings();
            return s1.SetEquals(s2);
        }

        private int GetMeetingsCount()
        {
            return Schedule.Meetings.Count + MeetingToAdd.GetLinkedMeetings().Count;
        }

        private IEnumerable<string> GetMeetings()
        {
            return this.Schedule.Meetings
                .Select(m => m.ToString())
                .Concat(this.MeetingToAdd.GetLinkedMeetings().Select(m => m.ToString()));
        }

        public override int GetHashCode()
        {
            var hashCode = Schedule.GetHashCode();
            foreach (var linkedMeeting in MeetingToAdd.GetLinkedMeetings()) hashCode ^= linkedMeeting.GetHashCode();

            // hashCode ^= Score.GetHashCode();

            return hashCode;
        }
    }

    public class BeamSolver : ISolver
    {
        private readonly int beamWidth;
        private readonly Dictionary<string, List<RoomSpec>> classroomsWithSpecs;

        private readonly IEstimator estimator;

        //private readonly Random random;
        private readonly Requisition requisition;
        private readonly ISolver solver;

        public BeamSolver(IEstimator estimator, Requisition requisition,
            Dictionary<string, List<RoomSpec>> classroomsWithSpecs, ISolver solver, /*Random random,*/
            int beamWidth = 1)
        {
            this.estimator = estimator;
            this.requisition = requisition;
            this.classroomsWithSpecs = classroomsWithSpecs;
            this.solver = solver;
            //this.random = random;
            this.beamWidth = beamWidth;
        }

        public Solution GetSolution(TimeSpan timeBudget)
        {
            return Solve(new(requisition, classroomsWithSpecs), timeBudget);
        }

        public Solution Solve(Schedule schedule, TimeSpan timeBudget)
        {
            var sw = Stopwatch.StartNew();
            var currentSchedules = new List<Solution> {new(schedule, 0d)};
            var totalCopiesCount = 0;
            var iterationCount = 0;

            while (true)
            {
                iterationCount++;
                var bestMeetings = GetBestMeetings(currentSchedules);
                var iteratedSolutions = GetIteratedSolutions(bestMeetings, out var copyCount);
                totalCopiesCount += copyCount;

                //WriteLog($"total: {iteratedSolutions.Count}");
                if (iteratedSolutions.Count == 0)
                    break;
                //WriteLog($"distinct: {iteratedSolutions.Select(s=>s.Schedule.ToString()).Distinct().Count()}");
                //WriteLog($"distinct: {iteratedSolutions.Select(s=>s.Schedule).Distinct().Count()}");
                WriteLog($"{iteratedSolutions.OrderByDescending(s => s.Score).First().Score}");

                currentSchedules = iteratedSolutions;
            }

            var bestSolution = currentSchedules.OrderByDescending(s => s.Score).First();
            WriteLog($"Beam width: {beamWidth}");
            WriteLog($"Mean copy count: {(double) totalCopiesCount / iterationCount}");
            WriteLog($"Elapsed: {sw.Elapsed}");
            WriteLog($"Best score: {bestSolution.Score}");
            return bestSolution;
        }

        private static List<Solution> GetIteratedSolutions(List<List<BeamNode>> bestMeetings, out int copyCount)
        {
            copyCount = 0;
            var newSchedules = new List<Solution>();
            foreach (var variants in bestMeetings)
            {
                for (var i = 0; i < variants.Count - 1; i++)
                {
                    copyCount++;
                    var copy = variants[i].Schedule.Copy();
                    copy.AddMeeting(variants[i].MeetingToAdd, true);
                    newSchedules.Add(new(copy, variants[i].Score));
                }

                var schedule = variants[^1].Schedule;
                schedule.AddMeeting(variants[^1].MeetingToAdd, true);
                newSchedules.Add(new(schedule, variants[^1].Score));
            }

            return newSchedules;
        }

        private List<List<BeamNode>> GetBestMeetings(List<Solution> currentSchedules)
        {
            return currentSchedules
                .AsParallel()
                .Select(s => s.Schedule)
                .SelectMany(GetBeamNodes)
                .Distinct()
                .OrderByDescending(t => t.Score)
                .Take(beamWidth)
                .GroupBy(t => t.Schedule)
                .Select(g => g.ToList())
                .ToList();
        }

        private double EstimateResult(Schedule schedule, Meeting meeting, double baseScore)
        {
            // schedule.AddMeeting(meeting);
            // var score = estimator.Estimate(schedule);
            // schedule.RemoveMeeting(meeting);
            var scoreDelta = estimator.EstimateMeetingToAdd(schedule, meeting);
            return baseScore + scoreDelta;
        }

        private double EstimateResultByGreedy(Schedule schedule, Meeting meeting)
        {
            var scheduleCopy = schedule.Copy();
            scheduleCopy.AddMeeting(meeting, true);
            var score = solver.Solve(scheduleCopy, TimeSpan.Zero).Score;
            return score;
        }

        private IEnumerable<BeamNode> GetBeamNodes(Schedule schedule)
        {
            return schedule.GetMeetingsToAdd()
                .AsParallel()
                .Select(meeting => new BeamNode(schedule, meeting, EstimateResultByGreedy(schedule, meeting)));
        }
    }
}