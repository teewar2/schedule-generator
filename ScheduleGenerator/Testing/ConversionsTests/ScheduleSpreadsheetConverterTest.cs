using System.Linq;
using Domain;
using NUnit.Framework;
using static Domain.Conversions.ScheduleSpreadsheetConverter;
using static Testing.ObjectMother;
using static Infrastructure.SheetConstants;

namespace Testing.ConversionsTests
{
    [TestFixture]
    public class ScheduleSpreadSheetConverterTests
    {
        private const string SheetName = "ScheduleTesting";

        [Test]
        public void ScheduleWriteTest()
        {
            var testSchedule = new Schedule(FullMondayRequisition, ClassRooms);

            while (testSchedule.NotUsedMeetings.Count != 0)
            {
                var meeting = testSchedule.GetMeetingsToAdd().First();
                testSchedule.AddMeeting(meeting, true);
            }

            Repository.ClearCellRange(SheetName, 0, 0, 100, 100);

            BuildSchedule(testSchedule, Repository, SheetName);
        }
    }
}