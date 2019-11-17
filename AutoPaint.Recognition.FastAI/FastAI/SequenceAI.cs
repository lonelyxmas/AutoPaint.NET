﻿using AutoPaint.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;

namespace AutoPaint.Recognition
{
    public class FastAI : IDisposable
    {
        public List<ILine> Lines { get; set; } = new List<ILine>();
        public ScanMode ScanMode { get; set; } = ScanMode.Rect;
        public int DepthMax { get; set; } = 1000;
        public bool IsCheckAround { get; set; } = false;
        public int AroundLower { get; set; } = 3;
        public int AroundUpper { get; set; } = 7;

        private static readonly int[] OffsetX = new[] { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] OffsetY = new[] { -1, -1, -1, 0, 1, 1, 1, 0 };
        private bool NewLine;

        public FastAI(int[,] bools, ScanMode mode = ScanMode.Rect, bool isCheckAround = false)
        {
            this.ScanMode = mode;
            this.IsCheckAround = isCheckAround;
            CalculateSequence(bools);
        }

        private void CreateNewSequence()
        {
            Lines.Add(new Line());
        }

        private void AddPoint(Vector2 position)
        {
            Lines.Last().Vertices.Add(new Vertex() { Position = position, Size = 1 });
        }

        private void CalculateSequence(int[,] bools)
        {
            if (ScanMode == ScanMode.Rect)
                ScanRect(bools);
            else
                ScanCircle(bools);
        }

        private void ScanCircle(int[,] bools)
        {
            int xlength = bools.GetLength(0);
            int ylength = bools.GetLength(1);
            Vector2 center = new Vector2(System.Convert.ToSingle(xlength / (double)2), System.Convert.ToSingle(ylength / (double)2));
            int radius = 0;
            var loopTo = System.Convert.ToInt32(Math.Sqrt(xlength * xlength + ylength * ylength));
            for (radius = 0; radius <= loopTo; radius++)
            {
                for (float Theat = 0; Theat <= Math.PI * 2; Theat += 1 / (float)radius)
                {
                    int dx = System.Convert.ToInt32(center.X + radius * Math.Cos(Theat));
                    int dy = System.Convert.ToInt32(center.Y + radius * Math.Sin(Theat));
                    if (!(dx >= 0 && dy >= 0 && dx < xlength && dy < ylength))
                        continue;
                    if (bools[dx, dy] == 1)
                    {
                        bools[dx, dy] = 0;
                        this.CreateNewSequence();
                        this.AddPoint(new Vector2(dx, dy));
                        MoveNext(bools, dx, dy, 0);
                        NewLine = true;
                    }
                }
            }
        }

        private void ScanRect(int[,] BolArr)
        {
            int xCount = BolArr.GetUpperBound(0);
            int yCount = BolArr.GetUpperBound(1);
            var loopTo = xCount - 1;
            for (var i = 0; i <= loopTo; i++)
            {
                var loopTo1 = yCount - 1;
                for (var j = 0; j <= loopTo1; j++)
                {
                    int dx = i;
                    int dy = j;
                    if (!(dx > 0 && dy > 0 && dx < xCount && dy < yCount))
                        continue;
                    if (BolArr[dx, dy] == 1)
                    {
                        BolArr[dx, dy] = 0;
                        this.CreateNewSequence();
                        this.AddPoint(new Vector2(dx, dy));
                        MoveNext(BolArr, dx, dy, 0);
                        NewLine = true;
                    }
                }
            }
        }

        private void MoveNext(int[,] bools, int x, int y, int depth)
        {
            if (depth > DepthMax)
                return;
            int xBound = bools.GetUpperBound(0);
            int yBound = bools.GetUpperBound(1);
            int dx, dy;
            if (IsCheckAround)
            {
                int around = GetAroundValue(bools, x, y);
                if (around >= AroundLower && around <= AroundUpper)
                    return;
            }
            for (var i = 0; i <= 7; i++)
            {
                dx = x + OffsetX[i];
                dy = y + OffsetY[i];
                if (!(dx >= 0 && dy >= 0 && dx <= xBound && dy <= yBound))
                    return;
                else if (bools[dx, dy] == 1)
                {
                    bools[dx, dy] = 0;
                    if (NewLine == true)
                    {
                        this.CreateNewSequence();
                        this.AddPoint(new Vector2(dx, dy));
                        NewLine = false;
                    }
                    else
                        this.AddPoint(new Vector2(dx, dy));
                    MoveNext(bools, dx, dy, depth + 1);
                    NewLine = true;
                }
            }
        }

        private int GetAroundValue(int[,] bools, int x, int y)
        {
            int dx, dy, result = 0;
            int xBound = bools.GetUpperBound(0);
            int yBound = bools.GetUpperBound(1);
            for (var i = 0; i <= 7; i++)
            {
                dx = x + OffsetX[i];
                dy = y + OffsetY[i];
                if (dx >= 0 && dy >= 0 && dx <= xBound && dy <= yBound)
                {
                    if (bools[dx, dy] == 1)
                        result += 1;
                }
            }
            return result;
        }

        void IDisposable.Dispose()
        {
            Lines.Clear();
        }
    }
}
