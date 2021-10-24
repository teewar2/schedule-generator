﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Infrastructure.SheetPatterns.SheetTableErrorPainter;
using static Infrastructure.SheetConstants;

namespace Testing.SheetPatternsTest
{
    [TestFixture]
    internal class SheetTableErrorPainterTest
    {
        private readonly List<((int, int), string)> errors = new()
        {
            ((0, 0), "error msg1"), ((1, 1), "error msg2"), ((4, 2), "error msg3")
        };

        [SetUp]
        [TearDown]
        public void SetUp()
        {
            Repository.ClearCellRange(SheetName, (0, 0), (10, 10));
        }

        [Test]
        public void PaintErrorsWhenNoMergerCells()
        {
            PaintErrors(Repository, SheetName, (1, 0), errors);
            var coordsToClear = errors.Select(e => e.Item1);
            ClearErrorPaint(Repository, SheetName, (1, 0), coordsToClear);
            //TODO: test should probably check something
        }

        [Test]
        public void PaintErrorsAndClear()
        {
            PaintErrors(Repository, SheetName, (5, 0), errors);
            var coordsToClear = errors.Select(e => e.Item1);
            ClearErrorPaint(Repository, SheetName, (5, 0), coordsToClear);
            //TODO: test should probably check something
        }
    }
}