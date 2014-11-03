﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace penToText
{
    public class convertToText2
    {


        //general data storage
        private List<mPoint> originalData;
        private List<mPoint> cleanedData;
        private List<mPoint> cleanedData2; 
        public List<mPoint> scaledList;
        private double minX, minY, scale;

        //canvas stuff
        private Core core;
        private mSectionNode2 root;

        //testing lists and whatnot
        private long[] miliSecondCounts;
        private Stopwatch timer;

        public convertToText2(Core core, int windows)
        {
            timer = new Stopwatch();
            this.core = core;
            originalData = new List<mPoint>();
            cleanedData = new List<mPoint>();

            miliSecondCounts = new long[windows];
            for (int i = 0; i < miliSecondCounts.Length; i++)
            {
                miliSecondCounts[i] = 0;
            }
            /*
             * id 0: Rescaled Original
             * id 1: Original Sections
             * id 2: Current Section Clean
             * id 3: Kasey Section Clean
             * if 4: Dominique Section Clean
             */           

        }

        public void setTree(mSectionNode2 treeRoot)
        {
            root = treeRoot;
        }

        public void getData(BlockingCollection<mPoint> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                mPoint current = item;
                addToScaleList(current);
                mPoint last = null;
                if (originalData.Count > 0)
                {
                    last = originalData[originalData.Count - 1];
                }

                if (originalData.Count == 0 || !(current.X == last.X && current.Y == last.Y && current.line == last.line))
                {
                    
                    originalData.Add(current);

                    timer.Start();
                    addToScaleList(current);
                    timer.Stop();
                    miliSecondCounts[1] += timer.ElapsedMilliseconds;
                    timer.Reset();

                    updateData();
                }
            }
        }

        private double Xmin, Ymin, Xmax, Ymax;
        public void addToScaleList(mPoint adding)
        {
            if (scaledList == null)
            {
                scaledList = new List<mPoint>();
            }
            if (scaledList.Count == 0)
            {
                minX = adding.X;
                minY = adding.Y;
                Xmax = double.NegativeInfinity;
                Ymax = double.NegativeInfinity;
                Xmin = double.PositiveInfinity;
                Ymin = double.PositiveInfinity;
                scale = 1.0;
            }
            double yToAdd = 0.0, xToAdd = 0.0;
            if (minY > adding.Y) {
                yToAdd = Math.Abs(minY - adding.Y)/scale;
                minY = adding.Y;
            }
            if (minX > adding.X)
            {
                xToAdd = Math.Abs(minX - adding.X) / scale;
                minX = adding.X;
            }
            if (xToAdd != 0.0 || yToAdd != 0.0)
            {
                for (int i = 0; i < scaledList.Count; i++)
                {
                    scaledList[i].X += xToAdd;
                    scaledList[i].Y += yToAdd;
                }
            }
            mPoint newPoint = new mPoint((adding.X - minX) / scale, (adding.Y - minY) / scale, adding.line);
            //newPoint.X = (newPoint.X - minX)/scale;
            //newPoint.Y = (newPoint.Y - minY)/scale;
            

            scaledList.Add(newPoint);

            bool changed = false;

            if (Xmin > newPoint.X) { Xmin = newPoint.X; changed = true; }
            if (Xmax < newPoint.X) { Xmax = newPoint.X; changed = true; }
            if (Ymin > newPoint.Y) { Ymin = newPoint.Y; changed = true; }
            if (Ymax < newPoint.Y) { Ymax = newPoint.Y; changed = true; }

            //if (!changed && scaledList.Count == 2) { changed = true; }

            if (scaledList.Count > 1 && changed)
            {
                double Xmin2= double.PositiveInfinity, Ymin2 = double.PositiveInfinity, Xmax2 = double.NegativeInfinity, Ymax2 = double.NegativeInfinity;
                double newScale = (Xmax - Xmin);
                if (Ymax - Ymin > newScale) { newScale = Ymax - Ymin; }
                for (int i = 0; i < scaledList.Count; i++)
                {
                    mPoint tempPoint = new mPoint( (scaledList[i].X - Xmin) / newScale, (scaledList[i].Y - Ymin) / newScale, scaledList[i].line);

                    if (Xmin2 > tempPoint.X) { Xmin2 = tempPoint.X;}
                    if (Xmax2 < tempPoint.X) { Xmax2 = tempPoint.X; }
                    if (Ymin2 > tempPoint.Y) { Ymin2 = tempPoint.Y; }
                    if (Ymax2 < tempPoint.Y) { Ymax2 = tempPoint.Y; }

                    scaledList[i] = tempPoint;
                }

                Xmin = Xmin2;
                Xmax = Xmax2;
                Ymin = Ymin2;
                Ymax = Ymax2;

                //minX += (Xmin * scale);
                //minY += (Ymin * scale);
                scale *= newScale;
            }
        }

        public void resampleAsNew(mPoint adding)
        {

        }

        public List<mPoint> scaleList(List<mPoint> data)
        {
            double newScale;
            bool worked = true;
            double xMin = double.PositiveInfinity, yMin = double.PositiveInfinity, xMax = double.NegativeInfinity, yMax = double.NegativeInfinity;
            List<mPoint> output = new List<mPoint>();
            if (data.Count > 1)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (xMin > data[i].X) { xMin = data[i].X; }
                    if (yMin > data[i].Y) { yMin = data[i].Y; }

                    if (xMax < data[i].X) { xMax = data[i].X; }
                    if (yMax < data[i].Y) { yMax = data[i].Y; }
                }
                newScale = (xMax - xMin);
                if (yMax - yMin > newScale) { newScale = yMax - yMin; }
                worked = !double.IsNaN(newScale) && !double.IsInfinity(newScale) && newScale != 0.0;
                for (int i = 0; i < data.Count && worked; i++)
                {
                    mPoint temp = new mPoint(((data[i].X - xMin) / newScale), ((data[i].Y - yMin) / newScale), data[i].line);
                    output.Add(temp);
                }
            }
            else { worked = false; }

            if (!worked)
            {
                output = data;
            }

            return output;
        }

        public void updateData()
        {
            /*
             * id 0: Rescaled Original
             * id 1: Original Sections
             * id 2: Current Section Clean
             * id 3: Kasey Section Clean
             * if 4: Dominique Section Clean
             */
            int id = 0;



            timer.Start();
            cleanedData = scaleList(new List<mPoint>(originalData));
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();
            core.TextBreakDown[id].newData(new List<mPoint>(cleanedData));
            core.TextBreakDown[id].titleText = "Original ReScale: " + "\nTime: " + (miliSecondCounts[id])+"ms";
            id++;


            timer.Start();
            cleanedData2 =new List<mPoint>(scaledList);
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();
            core.TextBreakDown[id].newData(new List<mPoint>(cleanedData2));
            core.TextBreakDown[id].titleText = "ReScale as new: " + "\nTime: " + (miliSecondCounts[id]) + "ms";
            id++;

            timer.Start();
            List<mPoint> resample1 = resample( scaleList(new List<mPoint>(originalData)),.1);
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();
            core.TextBreakDown[id].newData(new List<mPoint>(resample1));
            core.TextBreakDown[id].titleText = "Original Resample From: " + originalData.Count + "\nTo: " + cleanedData.Count + "\nTicks: " + (miliSecondCounts[id]) + "ms";
            id++;


            /*timer.Start();
            cleanedData2 = new List<mPoint>(scaledList);
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();*/
            core.TextBreakDown[id].newData(new List<mPoint>(cleanedData2));
            core.TextBreakDown[id].titleText = "Resample as new, From: " + originalData.Count + "\nTo: " + cleanedData2.Count + "\nTicks: " + (miliSecondCounts[id]) + "ms";
            id++;

            /*//Make Initial Segments "clean2" id=1
            timer.Start();
            List<mPoint> segments = Dominique(new List<mPoint>(cleanedData));
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();
            core.TextBreakDown[id].newData(new List<mPoint>(segments));
            core.TextBreakDown[id].titleText = "Initial Segments\nTicks: " + miliSecondCounts[id];
            

           List<mPoint> cleanedSections;
            //Current Section Cleaning "SectionClean1" id =2
            timer.Start();
            cleanedSections = minimumLines(cleanSections(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[2] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[2].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[2].titleText = "Current section Cleaning\nTicks: " + miliSecondCounts[2];

            //Kasey Section Cleaning "SectionClean2" id=3
            timer.Start();
            cleanedSections = minimumLines(kaseySectionClean(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[3] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[3].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[3].titleText = "Kasey section Cleaning\nTicks: " + miliSecondCounts[3];

            //Dominique Section Cleaning "SectionClean3" id=4
            timer.Start();
            cleanedSections = minimumLines(dominiqueSectionClean(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[4] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[4].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[4].titleText = "Dominique section Cleaning\nTicks: " + miliSecondCounts[4];*/

            core.draw();
            
        }
        public string getSearchString(string searchFor)
        {
            Queue<mSectionNode2> frontier = new Queue<mSectionNode2>();
            Queue<int> chunkLocs = new Queue<int>();
            List<mSectionNode2> fits = new List<mSectionNode2>();
            int chunkLength = 6;
            frontier.Enqueue(root);
            chunkLocs.Enqueue(0);
            while (frontier.Count > 0)
            {
                mSectionNode2 current = frontier.Dequeue();
                int chunkAt = chunkLocs.Dequeue();
                if (searchFor.Length >= (chunkAt + 1) * chunkLength)
                {
                    String thisChunk = searchFor.Substring(chunkAt * chunkLength, chunkLength);
                    String searchChunk = "";
                    double value = 0.0;
                    if (thisChunk.Equals("Line00"))
                    {
                        searchChunk = thisChunk;
                    }
                    else
                    {
                        searchChunk = thisChunk.Substring(0, 1);
                        value = Double.Parse(thisChunk.Substring(1));
                    }
                    bool found = false;
                    for (int i = 0; i < current.children.Count && !found; i++)
                    {
                        mSectionNode2 child = current.children[i];
                        if (child.SectionLetter.Equals(searchChunk) && value >= child.minValue && value <= child.maxValue)
                        {
                            frontier.Enqueue(child);
                            chunkLocs.Enqueue(chunkAt + 1);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        fits.Add(current);
                    }
                }
                else
                {
                    fits.Add(current);
                }
            }
            string output = "";
            string terminalString = "";
            string possibileString = "";
            for (int i = 0; i < fits.Count; i++)
            {
                //found an endcase
                if (!terminalString.Contains(fits[i].ifStopHere))
                {
                    terminalString += "" + fits[i].ifStopHere;
                }
                possibileString += fits[i].chars;
            }
            if (terminalString.Length != 0)
            {
                output = "If stopped here: " + terminalString + "\n";
            }
            possibileString = new string(possibileString.ToList().Distinct().ToArray());
            output += "Possible Letters: " + possibileString;

            return output;
        }



        /* This might be useful later
         public mSectionNode bestPosibility(String inputString, List<mSectionNode> possibilities)
         {
             String inputLetters = "";
             List<double> inputValues = new List<double>();

             int chunkAt = 0;
             int chunkLength = 6;
             while (inputString.Length >= (chunkAt + 1) * chunkLength)
             {
                 String chunk = inputString.Substring(chunkAt * chunkLength, chunkLength);
                 if(chunk.Equals("Line00")){
                     inputLetters+= chunk;
                     inputValues.Add(0.0);
                 }else{
                     inputLetters+= chunk.Substring(0,1);
                     inputValues.Add(Double.Parse(chunk.Substring(1)));
                 }
                 chunkAt++;
             }

             int bestLoc = 0;
             for (int i = 1; i < possibilities.Count; i++)
             {
                 String a = getTreeString(possibilities[bestLoc]);
                 String b = getTreeString(possibilities[i]);
                 bool sameA = inputLetters.Equals(a);
                 bool sameB = inputLetters.Equals(b);
                 if (sameB && !sameA)
                 {
                     //b is by default closer
                     bestLoc = i;
                 }
                 else if((sameA == sameB) || a.Equals(b))
                 {
                     //which is closer based on values
                     double[] aValues = getValues(possibilities[bestLoc]);
                     double[] bValues = getValues(possibilities[i]);
                     double aClose = 0;
                     double bClose = 0;
                     for (int j = 0; j < inputValues.Count && j < aValues.Length; j++) 
                     {
                         aClose += Math.Abs(inputValues[j] - aValues[j]);
                         bClose += Math.Abs(inputValues[j] - bValues[j]);
                     }

                     if (bClose < aClose)
                     {
                         bestLoc = i;
                     }

                 }
                 else 
                 {
                     //a equal and not b, neither equal to input, and not equal to each other
                     //keep bestLoc as it
                     if (!sameA)
                     {
                         if (Math.Abs(inputLetters.Length - a.Length) > Math.Abs(inputLetters.Length - b.Length))
                         {
                             bestLoc = i;
                         }
                     }
                 }
             }

             return possibilities[bestLoc];
         }
         */
        public List<mPoint> getCleanedData()
        {
            updateData();
            return new List<mPoint>(cleanedData);
        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();
            cleanedData2.Clear();
            scaledList.Clear();

            miliSecondCounts = new long[5];
            for (int i = 0; i < miliSecondCounts.Length; i++)
            {
                miliSecondCounts[i] = 0;
            }
        }

        private double length(List<mPoint> input)
        {
            double length = 0;
            if (input.Count > 1)
            {
                for (int i = 1; i < input.Count; i++)
                {
                    length += distance(input[i - 1], input[i]);
                }
            }
            return length;
        }

        private double distance(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private List<mPoint> resample(List<mPoint> data, double spaceBetweenPoints)
        {
            if (length(data) > 3 * spaceBetweenPoints)
            {
                double bigD = 0;
                double lilD = 0;
                List<mPoint> output = new List<mPoint>();
                output.Add(data[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    if (data[i].line == data[i - 1].line)
                    {
                        lilD = distance(data[i], data[i - 1]);
                        if (bigD + lilD > spaceBetweenPoints)
                        {
                            mPoint temp = new mPoint((data[i - 1].X + ((spaceBetweenPoints - bigD) / lilD) * (data[i].X - data[i - 1].X)),
                                (data[i - 1].Y + ((spaceBetweenPoints - bigD) / lilD) * (data[i].Y - data[i - 1].Y)),
                                data[i].line);
                            output.Add(temp);
                            data.Insert(i, temp);
                            bigD = 0;
                        }
                        else
                        {
                            if (i == data.Count - 1) { output.Add(data[i]); }
                            bigD += lilD;
                        }
                    }
                }

                return output;
            }
            else { return data; }
        }

        public List<mPoint> Dominique(List<mPoint> input)
        {
            List<mPoint> output = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && getDirection(input[sLoc], input[sLoc + 1]) == getDirection(input[i], input[i + 1]);
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

        public List<mPoint> minimumLines(List<mPoint> input)
        {
            int linePoints = 0;
            for (int i = 1; i < input.Count; i++)
            {
                if (input[i - 1].line == input[i].line)
                {
                    linePoints++;
                }
                else
                {
                    if (linePoints == 0)
                    {
                        //do something
                        input.Insert(i, new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                    }
                    else
                    {
                        linePoints = 0;
                        if (i == (input.Count - 1))
                        {
                            input.Add(new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                        }
                    }
                }
            }
            return input;
        }

        public List<mPoint> cleanSections(List<mPoint> input)
        {
            double e = .15; // height over length
            for (int i = 2; i < input.Count; i++)
            {
                if (input[i - 2].line == input[i - 1].line && input[i - 1].line == input[i].line)
                {
                    if (getDirection(input[i - 2], input[i - 1]) == getDirection(input[i - 1], input[i]))
                    {
                        input.RemoveAt(i - 1);
                        i = 2;
                    }
                    else
                    {
                        double lineDistanceA = lineDistance(input[i - 2], input[i], input[i - 1]);
                        double lineDistanceB = lineDistance(input[i - 1], input[i], input[i - 2]);
                        double lineLength;

                        if (lineDistanceA < lineDistanceB)
                        {
                            lineLength = distance(input[i - 2], input[i]);
                            if (lineDistanceA / lineLength <= e)
                            {
                                input.RemoveAt(i - 1);
                                i = 2;
                            }
                        }
                        else
                        {
                            lineLength = distance(input[i - 1], input[i]);
                            if (lineDistanceB / lineLength <= e)
                            {
                                input.RemoveAt(i - 2);
                                i = 2;
                            }
                        }
                    }
                }
            }
            return input;
        }

        public List<mPoint> kaseySectionClean(List<mPoint> input)
        {
            return cleanSections(input);
        }

        public List<mPoint> dominiqueSectionClean(List<mPoint> input)
        {
            return cleanSections(input);
        }

        private double dotProduct(Point a, Point b)
        {
            return ((a.X * b.X) + (a.Y * b.Y));
        }


        private double lineDistance(mPoint a, mPoint b, mPoint c)
        {
            //a and b make line, return distance c from line
            Point u = new Point(b.X - a.X, b.Y - a.Y);
            Point v = new Point(c.X - a.X, c.Y - a.Y);
            double value = dotProduct(u, v) / dotProduct(u, u);
            Point p = new Point(u.X * value + a.X, u.Y * value + a.Y);
            double output = double.PositiveInfinity;
            if (value >= 0.0 && value <= 1.0)
            {
                output = distance(c, new mPoint(p, c.line));
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
    }
}