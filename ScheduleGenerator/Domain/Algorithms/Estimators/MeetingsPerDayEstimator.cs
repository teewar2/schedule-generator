using System;

namespace Domain.Algorithms.Estimators
{
    public class MeetingsPerDayEstimator : IEstimator
    {
        public double Estimate(Schedule schedule, Meeting meetingToAdd)
        {
            throw new NotImplementedException();
        }

        public double Estimate(Schedule schedule)
        {
            var penalty = 0;
            const int maxMeetingsPerDay = 3;
            foreach (var day in schedule.GroupsMeetingsTimesByDay.Keys)
            foreach (var group in schedule.GroupsMeetingsTimesByDay[day].Keys)
            {
                //TODO четные и нечетные недели оценивать отдельно и складывать их результаты
                //TODO 0 пар в день и от 2 до 4 пар в день -> penalty = 0.
                var currentDif = schedule.GroupsMeetingsTimesByDay[day][@group].Count - maxMeetingsPerDay;
                penalty += currentDif;
            }

            return
                -penalty; // TODO поделить на количество половинок групп и количество дней и 2 (количество четностей недель)
        }
    }
}