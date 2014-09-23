﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Diagnostics;

namespace penToText
{
    public class convertToText3
    {

        private delegate void drawingDelegate();

        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<Point> originalData;
        private List<Point> cleanedData;
        private List<Point> cleanedData2;
        private List<Point> slopeData1;
        private List<Point> slopeData2;
        public Size inputSize;

        //canvas stuff
        //private lineDrawCanvas inputCopy;
        private lineDrawCanvas clean1;
        private lineDrawCanvas clean2;
        private lineDrawCanvas slopes1;
        private lineDrawCanvas slopes2;
        public Size canvasSizes;

        //testing lists and whatnot
        private double goalClean;
        private double e, e2;
        private long c1, c2;
        private Stopwatch timer;

        public convertToText3(dynamicDisplay display, Size inputSize)
        {
            this.inputSize = inputSize;
            timer = new Stopwatch();

            goalClean = (1 / 15.0);

            originalData = new List<Point>();            
            cleanedData = new List<Point>();
            cleanedData2 = new List<Point>();
            slopeData1 = new List<Point>();
            slopeData2 = new List<Point>();
            e = 0;
            e2 = 0;

            c1 = 0;
            c2 = 0;

            //thisDynamicDisplay = new dynamicDisplay();
            thisDynamicDisplay = display;

            double side = 250;

            //copy of input
            /*inputCopy = new lineDrawCanvas(0, 0, display, "Threaded Copy Input");
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.myPanel.Width = side;
            inputCopy.myPanel.Height = side;
            inputCopy.toAddCircles = false;
            thisDynamicDisplay.addCanvas(inputCopy);*/

            clean1 = new lineDrawCanvas(0, 0, display, "Clean All");
            clean1.outOfX = inputSize.Width;
            clean1.outOfy = inputSize.Height;
            clean1.myPanel.Width = side;
            clean1.myPanel.Height = side;
            clean1.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean1);

            clean2 = new lineDrawCanvas(1, 0, display, "Clean All quicker");
            clean2.outOfX = inputSize.Width;
            clean2.outOfy = inputSize.Height;
            clean2.myPanel.Width = side;
            clean2.myPanel.Height = side;
            clean2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean2);

            slopes1 = new lineDrawCanvas(0, 1, display, "Slopes1");
            slopes1.outOfX = inputSize.Width;
            slopes1.outOfy = inputSize.Height;
            slopes1.myPanel.Width = side;
            slopes1.myPanel.Height = side;
            slopes1.toAddCircles = true;
            thisDynamicDisplay.addCanvas(slopes1);

            slopes2 = new lineDrawCanvas(1, 1, display, "Slopes2");
            slopes2.outOfX = inputSize.Width;
            slopes2.outOfy = inputSize.Height;
            slopes2.myPanel.Width = side;
            slopes2.myPanel.Height = side;
            slopes2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(slopes2);
        }

        public void getData(BlockingCollection<Point> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                Point current = item;
                originalData.Add(current);
                cleanedData2.Add(current);
                updateData();
            }
        }

        public void updateData()
        {
            //Thread.Sleep(1000);
            /*inputCopy.newData(originalData);
            inputCopy.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(inputCopy.updateDraw));*/

            double testClean = goalClean + 1;
            double step = .1;
            //if (e != 0) { e -= step; }

            double originalClean = cleanliness(originalData);

            int i = 0;
            int iterations = 100;
            if (e != 0) {
                int steps = (int)(e / step);
                steps = (int)((double)steps * .75);
                e = (double)(steps * step);

            }

            timer.Start(); 
            while (testClean > goalClean && i < iterations)
            {
                e += step;
                i++;

                cleanedData = RDPclean(originalData, e);
                //cleanedData = cleanedData.Distinct().ToList();
                testClean = cleanliness(cleanedData);
                
                //Console.WriteLine("e: " + e + " cleanliness: " + testClean +" out ouf: " + goalClean);
            }

            if (i == iterations)
            {
                e = 0;
                cleanedData = originalData;
            }
            timer.Stop();
            c1 += timer.ElapsedTicks;
            timer.Reset();
            clean1.newData(cleanedData);
            clean1.titleText = "Clean from: " + originalData.Count + " e: " + e + "\nClean: " + testClean +" out of: " +goalClean + "\nTicks: " + c1;
            clean1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean1.updateDraw));


            testClean = goalClean + 1;
            //if (e2 != 0) { e2 -= step; }
            //if (e2 != 0) { e2 /= 2; }
            //e2 = 0;

            if (e2 != 0)
            {
                int steps = (int)(e2 / step);
                steps = (int)((double)steps * .75);
                e2 = (double)(steps * step);

            }

            i = 0;
            List<Point> temp = cleanedData2;
            //goalClean *= 1.25;
            String newTitle = "Clean e: ";
            timer.Start();
            while (testClean > goalClean && i < iterations)
            {
                e2 += step;
                i++;

                temp = RDPclean(cleanedData2, e2);
                //temp = temp.Distinct().ToList();
                testClean = cleanliness(temp);

                //Console.WriteLine("e: " + e + " cleanliness: " + testClean +" out ouf: " + goalClean);
            }

            if (i == iterations)
            {
                e2 = 0;
                newTitle += "F";
                //cleanedData = originalData;
            }
            else
            {
                cleanedData2 = temp;
                newTitle += e + "\nClean: " + testClean + " out of: " + goalClean;
            }

            timer.Stop();
            c2 += timer.ElapsedTicks;
            timer.Reset();
            newTitle += "\nTicks: " + c2;
            clean2.newData(cleanedData2);
            clean2.titleText = newTitle;
            clean2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean2.updateDraw));

            slopeData1 = Dominique2(cleanedData);
            slopes1.newData(slopeData1);
            slopes1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(slopes1.updateDraw));

            slopeData2 = Dominique2(cleanedData2);
            slopes2.newData(slopeData2);
            slopes2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(slopes2.updateDraw));
        }

        

        public void resize()
        {
            /*inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.updateDraw();*/

            clean1.outOfX = inputSize.Width;
            clean1.outOfy = inputSize.Height;
            clean1.updateDraw();

            clean2.outOfX = inputSize.Width;
            clean2.outOfy = inputSize.Height;
            clean2.updateDraw();

            slopes1.outOfX = inputSize.Width;
            slopes1.outOfy = inputSize.Height;
            slopes1.updateDraw();

            slopes2.outOfX = inputSize.Width;
            slopes2.outOfy = inputSize.Height;
            slopes2.updateDraw();
        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();
            cleanedData2.Clear();
            slopeData1.Clear();
            slopeData2.Clear();

            c1 = 0;
            c2 = 0;
            e = 0;
            e2 = 0;


            /*inputCopy.newData(originalData);
            inputCopy.updateDraw();*/

            clean1.newData(originalData);
            clean1.updateDraw();

            clean2.newData(originalData);
            clean2.updateDraw();

            slopes1.newData(originalData);
            slopes1.updateDraw();

            slopes2.newData(originalData);
            slopes2.updateDraw();
        }

        private List<Point> RDPclean(List<Point> input, double epsilon)
        {
            List<Point> output = new List<Point>();
            if (input.Count > 2)
            {
                double maxDistance = 0.0;
                int pos = 0;
                //find the greatest distance
                for (int i = 1; i < input.Count - 1; i++)
                {
                    double d = lineDistance(input[i], input[0], input[input.Count - 1]);
                    if (d > maxDistance)
                    {
                        maxDistance = d;
                        pos = i;
                    }
                }

                if (maxDistance > epsilon)
                {


                    output = RDPclean(input.GetRange(0, pos + 1), epsilon);
                    output.RemoveAt(output.Count - 1);
                    output.AddRange(RDPclean(input.GetRange(pos, (input.Count - pos)), epsilon));

                    //output = output.Distinct().ToList();

                }
                else
                {
                    //no points in list are long enough, so just return first and last point
                    output.Add(input[0]);
                    output.Add(input[input.Count - 1]);
                }
            }
            else
            {
                output = input;
            }
            return output;
        }

        private double lineDistance(Point a, Point lineA, Point lineB)
        {
            double X0, X1, X2, Y0, Y1, Y2;
            X0 = a.X;
            Y0 = a.Y;
            X1 = lineA.X;
            Y1 = lineA.Y;
            X2 = lineB.X;
            Y2 = lineB.Y;

            double numerator = Math.Abs((X2 - X1) * (Y1 - Y0) - (X1 - X0) * (Y2 - Y1));
            double denominator = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));

            return (numerator / denominator);
        }

        private double cleanliness(List<Point> input)
        {
            double length = 0;
            if (input.Count > 1)
            {
                for (int i = 1; i < input.Count; i++)
                {
                    length += distance(input[i - 1], input[i]);
                }
            }
            return (input.Count / length);
        }

        private double distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private List<Point> Dominique2(List<Point> input)
        {
            List<Point> output = new List<Point>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = ((xChange(input[sLoc], input[sLoc + 1]) == xChange(input[i], input[i + 1])) && (yChange(input[sLoc], input[sLoc + 1]) == yChange(input[i], input[i + 1])));
                    if (!sameSlope)
                    {
                        output.Add(input[i]);
                        sLoc = i;
                    }
                }
                output.Add(input[input.Count - 1]);
            }
            else
            {
                output = input;
            }
            return output;
        }

        private int xChange(Point a, Point b)
        {
            //0 is same, 1 a is greater, 2 b is greater;
            int output = 0;
            if (a.X > b.X)
            {
                output = 1;
            }
            else if (a.X < b.X)
            {
                output = 2;
            }
            return output;
        }

        private int yChange(Point a, Point b)
        {
            int output = 0;
            if (a.Y > b.Y)
            {
                output = 1;
            }
            else if (a.Y < b.Y)
            {
                output = 2;
            }
            return output;
        }
    }
}