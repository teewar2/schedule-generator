﻿namespace Domain.ScheduleLib
{
    public class EvaluationResult
    {
        public double Penalty;
        public Meeting[] BadMeetings;
        public string PenaltyDescription;
        
        public EvaluationResult(double penalty, Meeting[] badMeetings, string penalityDescription)
        {
            Penalty = penalty;
            BadMeetings = badMeetings;
            PenaltyDescription = penalityDescription;
        }
    }

    public interface IRule
    {
        double Evaluate(LearningPlan learningPlan, Requisition requisition, Schedule schedule, Meeting meetingToAdd);
    }
}
