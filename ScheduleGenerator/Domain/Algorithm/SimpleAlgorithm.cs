using System;
using System.Collections.Generic;
using System.Linq;
using Domain.ScheduleLib;
using Domain.Conversions;


namespace Domain.Algorithm
{
    class SimpleAlgorithm
    {
        private Dictionary<AdditionalMeetingInfo, HashSet<Meeting>> infoMeetingDict;
        private HashSet<Meeting> allMeetings;

        public SimpleAlgorithm(Dictionary<AdditionalMeetingInfo, HashSet<Meeting>> infoMeetingDict)
        {
            this.infoMeetingDict = infoMeetingDict;
            allMeetings = infoMeetingDict.Values.SelectMany(m => m).ToHashSet();
        }

        public void StartSolving()
        {
            var stack = new Stack<ScheduleFrame>();
            // sort AddInfos to peek most spec
            var sortedAdditional = infoMeetingDict.Keys
                .OrderBy(ai => ai.possibleWeekType)
                .ThenBy(ai => ai.possibleGroups.First().Count)
                .ThenBy(ai => ai.possibleMeetingTimes.First().Count)
                .ToList();
            // start frame setUp
            var startFrame = new ScheduleFrame(
                sortedAdditional[0], 0, 0, 0, 0, 0,
                new HashSet<Meeting>(), new HashSet<Meeting>());

            while (true)
            {
                var currentFrame = stack.Peek();

                var addInfo = currentFrame.additionalMeetingInfo;
                var weekTypes = addInfo.possibleWeekType;
                var groupPriors = addInfo.possibleGroups;
                var timePriors = addInfo.possibleMeetingTimes;
                while (currentFrame.possibleWeekTypesPointer < weekTypes.Count)
                {
                    while (currentFrame.possibleGroupsPriorityPointer < groupPriors.Count)
                    {
                        while (currentFrame.possibleGroupsPointer <
                                groupPriors[currentFrame.possibleGroupsPriorityPointer].Count)
                        {
                            while (currentFrame.possibleTimesPointer < timePriors.Count)
                            {
                                var groups = groupPriors[currentFrame.possibleGroupsPriorityPointer][currentFrame.possibleGroupsPointer].ToList();
                                var times = addInfo.possibleMeetingTimes[currentFrame.possibleTimesPointer];
                                var needToPlace = infoMeetingDict[addInfo].Except(currentFrame.currentPlaced).ToList();
                                var needToPlacePointer = 0;
                                foreach (var group in groups)
                                {
                                    foreach (var time in times)
                                    {
                                        // Check if place free
                                        // if collision load stack top






                                        if (needToPlacePointer > needToPlace.Count)
                                        {
                                            // GOTO
                                            goto Out;
                                        }

                                        var meetingToPlace = needToPlace[needToPlacePointer];
                                        needToPlacePointer++;

                                        meetingToPlace.WeekType = addInfo.possibleWeekType[currentFrame.possibleWeekTypesPointer];
                                        meetingToPlace.Groups = new[] { group };
                                        meetingToPlace.MeetingTime = time;
                                        currentFrame.currentPlaced.Add(meetingToPlace);
                                    }
                                }

                                currentFrame.possibleTimesPointer++;
                            }

                            currentFrame.possibleGroupsPointer++;
                        }

                        currentFrame.possibleGroupsPriorityPointer++;
                    }

                    currentFrame.possibleWeekTypesPointer++;
                Out:
                    break;
                }



                // Push current frame to stack
                // var newFrame = new ScheduleFrame();

                // SetUp new frame
                // freeze meetings
                //newFrame.frozenMeetings = new HashSet<Meeting>(currentFrame.frozenMeetings);
                //newFrame.frozenMeetings.UnionWith(currentFrame.currentPlaced);

                //stack.Push(newFrame);
            }
        }
    }

    class ScheduleFrame
    {
        public AdditionalMeetingInfo additionalMeetingInfo;
        public int additionalInfosPointer;

        public int possibleGroupsPriorityPointer;
        public int possibleGroupsPointer;

        public int possibleWeekTypesPointer;
        public int possibleTimesPointer;

        public HashSet<Meeting> currentPlaced;
        public HashSet<Meeting> frozenMeetings;

        public ScheduleFrame(AdditionalMeetingInfo additionalMeetingInfo, int additionalInfosPointer,
                int possibleGroupsPriorityPointer, int possibleGroupsPointer,
                int possibleWeekTypesPointer, int possibleTimesPointer,
                HashSet<Meeting> currentPlaced, HashSet<Meeting> frozenMeetings)
        {
            this.additionalMeetingInfo = additionalMeetingInfo;
            this.possibleGroupsPriorityPointer = possibleGroupsPriorityPointer;
            this.additionalInfosPointer = additionalInfosPointer;
            this.possibleGroupsPointer = possibleGroupsPointer;
            this.possibleWeekTypesPointer = possibleWeekTypesPointer;
            this.possibleTimesPointer = possibleTimesPointer;
            this.currentPlaced = currentPlaced;
            this.frozenMeetings = frozenMeetings;
        }
    }
}
