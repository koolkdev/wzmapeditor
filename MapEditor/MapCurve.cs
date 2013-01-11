/* 
  koolk's Map Editor
 
  Copyright (c) 2009-2013 koolk

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.

     2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.

     3. This notice may not be removed or altered from any source
     distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using WZ;

namespace WZMapEditor
{
    public class MapCurve : IDrawable
    {
        List<Point> points = new List<Point>();

        bool closed = false;

        private static double slipangle = Math.Atan(0.5) * 180 / Math.PI;

        private class ImageDetails
        {
            public int width;
            public int height;
            public string name;
            //public int area;
        }

        private static List<ImageDetails> images = new List<ImageDetails>();

        public static void ProcessObjects(WZDirectory objects)
        {
            foreach (IMGFile file in objects.IMGs.Values)
            {
                foreach (IMGEntry e1 in file.childs.Values)
                {
                    foreach (IMGEntry e2 in e1.childs.Values)
                    {
                        foreach (IMGEntry e3 in e2.childs.Values)
                        {
                            string name = file.Name + "/" + e1.Name + "/" + e2.Name + "/" + e3.Name;
                            if (e3.GetCanvas("0") != null)
                            {
                                WZ.Objects.WZCanvas c = e3.GetCanvas("0");
                                ImageDetails d = new ImageDetails();

                                d.name = name;
                                d.width = c.width;
                                d.height = c.height;

                                /*Bitmap b = c.GetBitmap();
                                int area = 0;
                                for (int x = 0; x < d.width; x++)
                                {
                                    for (int y = 0; y < d.height; y++)
                                    {
                                        area += (b.GetPixel(x, y).A > 0) ? 1 : 0;
                                    }
                                }
                                d.area = area;*/
                                images.Add(d);
                            }
                        }
                    }
                }
                file.Close();
            }
        }
        public MapCurve()
        {
        }

        public void AddPoint(Point point)
        {
            if (!closed) points.Add(point);
        }

        public bool Close()
        {
            closed = true;
            if (points.Count <= 2 || Distance(points.First<Point>(), points.Last<Point>()) > 30)
            {
                return false;
            }
            return true;
        }

        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }

        public void Simplify()
        {
            if (calculateArea() < 0) points.Reverse();
            points = DouglasPeucker(points.ToArray(), 20).ToList<Point>();
            points = TransferStraightLines(points.ToArray()).Take<Point>(points.Count - 1).ToList<Point>();
            points = DouglasPeucker(points.ToArray(), 20).ToList<Point>();
            if (checkComplex(points.Select<Point, double>(x => x.X).ToArray(), points.Select<Point, double>(x => x.Y).ToArray())) throw new Exception("Complex polygon");
            RemoveWrongPoints(false);
            points = FitToTilesSize(points.ToArray());
            MergePoints();
            if(points.Count > 2) RemoveWrongPoints(true);

             
            /*int minx = points[0].X, maxx = minx, miny = points[0].Y, maxy = miny;
            foreach (Point point in points)
            {
                if (point.X < minx) minx = point.X;
                else if (point.X > maxx) maxx = point.X;
                if (point.Y < miny) miny = point.Y;
                else if (point.Y > maxy) maxy = point.Y;
            }
            int width = maxx - minx, height = maxy - miny;

            List<string> objects = images.Where(x => x.width < width * 1.2 && x.width > width * 0.8 && x.height < height * 1.2 && x.height > height * 0.8).Select<ImageDetails, string>(x => x.name).ToList<string>();*/
            
        }

        private void MergePoints()
        {
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i - 1].Equals(points[i]))
                {
                    points.RemoveAt(i);
                    i--;
                }
            }
            if (points.First<Point>().Equals(points.Last<Point>())) points.RemoveAt(points.Count - 1);
        }

        private void RemoveWrongPoints(bool fix)
        {
            List<double> angles = new List<double>();
            for (int i = 1; i < points.Count; i++)
            {
                angles.Add(Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) * 180 / Math.PI);
            }
            angles.Add(Math.Atan2(points[0].Y - points[points.Count - 1].Y, points[0].X - points[points.Count - 1].X) * 180 / Math.PI);

            for (int i = 1; i < points.Count; i++)
            {
                if (Math.Abs(angles[i] - angles[i - 1]) > 180 - slipangle)
                {
                    if (angles[i] == 180 || angles[i] == -180 || angles[i-1] == 180 || angles[i-1] == -180) if (Math.Abs(angles[i] + angles[i - 1]) <= 180 - slipangle) continue;
                    if (fix)
                    {
                        points.RemoveAt(i);
                        angles.Clear();
                        for (int j = 1; j < points.Count; j++)
                        {
                            angles.Add(Math.Atan2(points[j].Y - points[j - 1].Y, points[j].X - points[j - 1].X) * 180 / Math.PI);
                        }
                        angles.Add(Math.Atan2(points[0].Y - points[points.Count - 1].Y, points[0].X - points[points.Count - 1].X) * 180 / Math.PI);
                        i--;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }

        private double calculateArea()
        {
            double area = 0;
            for (int i = 1; i < points.Count; i++)
                area += points[i - 1].X * points[i].Y - points[i].X * points[i - 1].Y;
            area += points.Last<Point>().X * points.First<Point>().Y - points.First<Point>().X * points.Last<Point>().Y;
            return area * 0.5;
        }

        public static bool checkComplex(double[] x, double[] y)
        {
            int i = 0, j;
            for (j = i + 2; j < x.Length - 1; j++)
                if (intersect(x, y, i, j))
                    return true;
            for (i = 1; i < x.Length; i++)
                for (j = i + 2; j < x.Length; j++)
                    if (intersect(x, y, i, j))
                        return true;
            return false;
        }

        public static bool intersect(double[] x, double[] y, int i1, int i2)
        {
            int s1 = (i1 > 0) ? i1 - 1 : x.Length - 1;
            int s2 = (i2 > 0) ? i2 - 1 : x.Length - 1;
            return ccw(x[s1], y[s1], x[i1], y[i1], x[s2], y[s2])
                != ccw(x[s1], y[s1], x[i1], y[i1], x[i2], y[i2])
                && ccw(x[s2], y[s2], x[i2], y[i2], x[s1], y[s1])
                != ccw(x[s2], y[s2], x[i2], y[i2], x[i1], y[i1]);
        }

        public static bool ccw(double p1x, double p1y, double p2x, double p2y, double p3x, double p3y)
        {
            double dx1 = p2x - p1x;
            double dy1 = p2y - p1y;
            double dx2 = p3x - p2x;
            double dy2 = p3y - p2y;
            return dy1 * dx2 < dy2 * dx1;
        }

        private List<Point> FitToTilesSize(Point[] points)
        {
            int minx = points[0].X, maxx = minx, miny = points[0].Y, maxy = miny;
            foreach (Point point in points)
            {
                if (point.X < minx) minx = point.X;
                else if (point.X > maxx) maxx = point.X;
                if (point.Y < miny) miny = point.Y;
                else if (point.Y > maxy) maxy = point.Y;
            }
            int width = maxx - minx, height = maxy - miny;

            //double widthFactor = width - 50;
            //double heightFactor = height - 50;
            int widthFactor = (int)(width / 90.0);

            double newWidth = widthFactor * 90; //+ 50;

            int heightFactor = (int)(height / 60.0);

            double newHeight = heightFactor * 60; //+ 50;

            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = (int)((points[i].X - minx) * (newWidth / width)) + minx;
                points[i].Y = (int)((points[i].Y - miny) * (newHeight / height)) + miny;
            }


            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j <= widthFactor; j++)
                {
                    if (Math.Abs(points[i].X - minx - j * 90) <= 45)
                    {
                        points[i].X = j * 90 + minx;
                        break;
                    }
                }
                for (int j = 0; j <= heightFactor; j++)
                {
                    if (Math.Abs(points[i].Y - miny - j * 60) <= 30)
                    {
                        points[i].Y = j * 60 + miny;
                        break;
                    }
                }
            }

            for (int i = 1; i < points.Length; i++)
            {
                if (points[i].Y != points[i - 1].Y && points[i].X != points[i - 1].X)
                {
                    double m = (double)(points[i].Y - points[i - 1].Y) / (points[i].X - points[i - 1].X);
                    if (Math.Abs(m - (double)60 / 90) > 0.01 )
                    {
                        if (m > 0) points[i].Y = (int)((points[i].X - points[i - 1].X) * 60.0 / 90.0 + points[i - 1].Y);
                        else points[i].Y = (int)(-(points[i].X - points[i - 1].X) * 60.0 / 90.0 + points[i - 1].Y);
                    }
                    // TODO: keep original for what is after that
                }
            }


            return points.ToList<Point>();
        }

        private Point[] TransferStraightLines(Point[] points)
        {
            double[] angles = new double[points.Length];

            for (int i = 1; i < points.Length; i++)
            {
                angles[i] = Math.Atan2(points[i].Y - points[i - 1].Y, points[i].X - points[i - 1].X) * 180 / Math.PI;
            }
            //angles[0] = Math.Atan2(points[0].Y - points[points.Length - 1].Y, points[0].X - points[points.Length - 1].X) * 180 / Math.PI;

            for (int i = 1; i < points.Length; i++)
            {
                if (Math.Abs(-180 - angles[i]) <= 30) angles[i] = -180;
                else if (Math.Abs(-135 - angles[i]) < 15) angles[i] = -180 + slipangle;
                else if (Math.Abs(-90 - angles[i]) <= 30) angles[i] = -90;
                else if (Math.Abs(-45 - angles[i]) < 15) angles[i] = -slipangle;
                else if (Math.Abs(0 - angles[i]) <= 30) angles[i] = 0;
                else if (Math.Abs(45 - angles[i]) < 15) angles[i] = slipangle;
                else if (Math.Abs(90 - angles[i]) <= 30) angles[i] = 90;
                else if (Math.Abs(135 - angles[i]) < 15) angles[i] = 180 - slipangle;
                else if (Math.Abs(180 - angles[i]) <= 30) angles[i] = 180;
            }

            for (int i = 1; i < points.Length; i++)
            {
                double d = Distance(points[i], points[i - 1]);
                if (angles[i] % 180 == 0)
                {
                    points[i].Y = points[i - 1].Y;
                    points[i].X = points[i - 1].X + (int)((angles[i] == 0) ? d : -d);
                }
                else if (angles[i] % 90 == 0)
                {
                    points[i].X = points[i - 1].X;
                    points[i].Y = points[i - 1].Y + (int)((angles[i] > 0) ? d : -d);
                }
                else
                {
                    points[i].X = points[i - 1].X + (int)(d * Math.Cos(angles[i] * Math.PI / 180));
                    points[i].Y = points[i - 1].Y + (int)(d * Math.Sin(angles[i] * Math.PI / 180));
                }
            }

            Point p = findIntersection(points[0], points[1], points[points.Length - 2], points[points.Length - 1]);
            if (p == Point.Empty) { p = findIntersection(points[0], points[1], points[points.Length - 3], points[points.Length - 2]); this.points.Remove(this.points.Last<Point>()); }
            if (p == Point.Empty) throw new Exception("Can't create shape");

            points[0] = p;

            return points;
        }

        public static double DistanceBetweenPointToLine(Point p, Point p1, Point p2)
        {
            return MapFoothold.DistanceBetweenPointToLine(p.X, p.Y, p1.X, p1.Y, p2.X, p2.Y);
        }
        private Point[] DouglasPeucker(Point[] PointList, double epsilon)
        {
            double dmax = 0;
            int index = 0;
            for (int i = 1; i < PointList.Length; i++)
            {
                double d = DistanceBetweenPointToLine(PointList[i], PointList.First<Point>(), PointList.Last<Point>());
                if (d > dmax)
                {
                    index = i;
                    dmax = d;
                }
            }

            Point[] ResultList;
            if (dmax >= epsilon)
            {
                Point[] recResults1 = DouglasPeucker(PointList.Take(index + 1).ToArray<Point>(), epsilon);
                Point[] recResults2 = DouglasPeucker(PointList.Skip(index).ToArray<Point>(), epsilon);

                ResultList = recResults1.Take<Point>(recResults1.Length - 1).Union<Point>(recResults2).ToArray<Point>();
            }
            else
            {
                ResultList = new Point[] { PointList.First<Point>(), PointList.Last<Point>() };
            }
            return ResultList;
        }
        Point findIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            double xD1, yD1, xD2, yD2, xD3, yD3;
            double dot, deg, len1, len2;
            double segmentLen1, segmentLen2;
            double ua, ub, div;

            // calculate differences  
            xD1 = p2.X - p1.X;
            xD2 = p4.X - p3.X;
            yD1 = p2.Y - p1.Y;
            yD2 = p4.Y - p3.Y;
            xD3 = p1.X - p3.X;
            yD3 = p1.Y - p3.Y;

            // calculate the lengths of the two lines  
            len1 = Math.Sqrt(xD1 * xD1 + yD1 * yD1);
            len2 = Math.Sqrt(xD2 * xD2 + yD2 * yD2);

            // calculate angle between the two lines.  
            dot = (xD1 * xD2 + yD1 * yD2); // dot product  
            deg = dot / (len1 * len2);

            // if abs(angle)==1 then the lines are parallell,  
            // so no intersection is possible  
            if (Math.Abs(deg) == 1) return Point.Empty;

            // find intersection Pt between two lines  
            Point pt = new Point(0, 0);
            div = yD2 * xD1 - xD2 * yD1;
            ua = (xD2 * yD3 - yD2 * xD3) / div;
            ub = (xD1 * yD3 - yD1 * xD3) / div;
            pt.X = (int)(p1.X + ua * xD1);
            pt.Y = (int)(p1.Y + ua * yD1);

            // calculate the combined length of the two segments  
            // between Pt-p1 and Pt-p2  
            xD1 = pt.X - p1.X;
            xD2 = pt.X - p2.X;
            yD1 = pt.Y - p1.Y;
            yD2 = pt.Y - p2.Y;
            segmentLen1 = Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2);

            // calculate the combined length of the two segments  
            // between Pt-p3 and Pt-p4  
            xD1 = pt.X - p3.X;
            xD2 = pt.X - p4.X;
            yD1 = pt.Y - p3.Y;
            yD2 = pt.Y - p4.Y;
            segmentLen2 = Math.Sqrt(xD1 * xD1 + yD1 * yD1) + Math.Sqrt(xD2 * xD2 + yD2 * yD2);

            // if the lengths of both sets of segments are the same as  
            // the lenghts of the two lines the point is actually  
            // on the line segment.  

            // if the point isn’t on the line, return null  
            //if (Math.Abs(len1 - segmentLen1) > 0.01 || Math.Abs(len2 - segmentLen2) > 0.01)
            //    return Point.Empty;

            // return the valid intersection  
            return pt;
        }

        public void DrawAnimation(DevicePanel d)
        {

        }

        public void Draw(DevicePanel d)
        {
            for (int i = 1; i < points.Count; i++)
            {
                d.DrawLine(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, Color.Black);
            }
            if (closed)
            {
                d.DrawLine(points.First<Point>().X, points.First<Point>().Y, points.Last<Point>().X, points.Last<Point>().Y, Color.Black);
            }
        }

        public void Draw(Graphics g)
        {
        }
    }
}
