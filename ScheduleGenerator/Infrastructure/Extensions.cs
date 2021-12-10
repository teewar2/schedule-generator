﻿using System;
using System.Collections;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;

namespace Infrastructure
{
    public static class Extensions
    {
        public static CellData CommonCellData(string value)
        {
            return new()
            {
                UserEnteredValue = new()
                {
                    StringValue = value
                },
                UserEnteredFormat = new()
                {
                    TextFormat = new()
                    {
                        FontSize = 9
                    },
                    VerticalAlignment = "middle",
                    HorizontalAlignment = "center",
                    WrapStrategy = "wrap"
                }
            };
        }

        public static CellData HeaderCellData(string value)
        {
            var cellData = CommonCellData(value);
            cellData.UserEnteredFormat.TextFormat.Bold = true;
            return cellData;
        }

        public static CellData CommonBoolCellData(bool value = false)
        {
            return new()
            {
                UserEnteredValue = new()
                {
                    BoolValue = value
                },
                DataValidation = new()
                {
                    Condition = new()
                    {
                        Type = "BOOLEAN"
                    }
                }
            };
        }

        private static readonly DateTime Beginning = new(1899, 12, 30);

        public static CellData CommonTimeCellData(DateTime dateTime)
        {
            return new()
            {
                UserEnteredFormat = new()
                {
                    NumberFormat = new()
                    {
                        Type = "DATE_TIME",
                        Pattern = "dd.mm.yyyy hh:mm:ss"
                    }
                },
                UserEnteredValue = new()
                {
                    NumberValue = (dateTime - Beginning).TotalDays
                }
            };
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>();
            foreach (var e in source)
            {
                batch.Add(e);
                if (batch.Count < batchSize) continue;
                yield return batch;
                batch = new();
            }
            if (batch.Count > 0)
                yield return batch;
        }
            
    }
}