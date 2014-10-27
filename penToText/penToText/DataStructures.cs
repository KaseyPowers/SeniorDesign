﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace penToText
{    
    public class mPoint
    {
        public double X;
        public double Y;
        public int line;

        public mPoint(double x, double y, int line)
        {
            X = x;
            Y = y;
            this.line = line;
        }

        public mPoint(Point input, int line){
            X = input.X;
            Y = input.Y;
            this.line = line;
        }

        public Point getPoint()
        {
            return new Point(X, Y);
        }

    }

    public class mLetterSections
    {
        public List<mPoint> points;

        public mLetterSections(List<mPoint> points)
        {
            this.points = points;
        }

        public String getString( bool length)
        {
            String output = "";
            double firstLength = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                double thisLength = distance(points[i], points[i + 1]);
                if (i == 0)
                {
                    firstLength = distance(points[i], points[i + 1]);
                }

                int direction = getDirection(points[i], points[i + 1]);
                switch (direction)
                {
                    case 4:
                        output += "A";
                        break;
                    case 5:
                        output += "B";
                        break;
                    case 6:
                        output += "C";
                        break;
                    case 7:
                        output += "D";
                        break;

                }
                if (length)
                {
                    output += (thisLength / firstLength).ToString("F2");
                }

            }

            return output;
        }
        private int getDirection(mPoint startPoint, mPoint endPoint)
        {
            /*
             * 0: up
             * 1: down
             * 2: left
             * 3: right
             * 4: up-left
             * 5: up-right
             * 6: down-left
             * 7: down-right
             */
            int direction = -1;
            double deltaX = xChange(startPoint, endPoint);
            double deltaY = yChange(startPoint, endPoint);

            if (deltaY < 0)
            {
                //up
                if (deltaX > 0)
                {
                    //right
                    direction = 5;
                }
                else
                {
                    //left
                    direction = 4;
                }
            }
            else
            {
                //down
                if (deltaX > 0)
                {
                    //right
                    direction = 7;
                }
                else
                {
                    //left
                    direction = 6;
                }
            }

            return direction;
        }

        private double xChange(mPoint a, mPoint b)
        {

            return (a.X - b.X);
        }

        private double yChange(mPoint a, mPoint b)
        {
            return (a.Y - b.Y);
        }
        private double distance(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }
    }
}
