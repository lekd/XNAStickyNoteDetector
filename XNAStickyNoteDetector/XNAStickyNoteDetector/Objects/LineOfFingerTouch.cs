using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Core;
using System.Drawing;
using Microsoft.Xna.Framework;

namespace XNAStickyNoteDetector.Objects
{
    public class LineOfFingerTouch
    {
        public delegate void IntendedLineGenerated(object sender,object args);

        const int MINIMUM_LENGTH = 100;
        List<TouchPoint> _fingerTouches;
        
        public List<TouchPoint> FingerTouches
        {
            get { return _fingerTouches; }
            set { _fingerTouches = value; }
        }
        public LineOfFingerTouch()
        {
            _fingerTouches = new List<TouchPoint>();
        }
        public int getIdOfGeneratingTouch()
        {
            return _fingerTouches[0].Id;
        }
        public static double lengthOfLineOfTouches(List<TouchPoint> lineOfTouches)
        {
            TouchPoint begin = lineOfTouches[0];
            TouchPoint end = lineOfTouches[lineOfTouches.Count - 1];
            double x_diff = end.CenterX - begin.CenterX;
            double y_diff = end.CenterY - begin.CenterY;
            double length = Math.Sqrt(x_diff * x_diff + y_diff * y_diff);
            return length;
        }
        public static bool isIntendedLine(List<TouchPoint> lineOfTouches)
        {
            if (lengthOfLineOfTouches(lineOfTouches) < MINIMUM_LENGTH)
            {
                return false;
            }
            return true;
        }
        public double getShortestLength()
        {
            return lengthOfLineOfTouches(_fingerTouches);
        }
        public bool crossAStickyNote(TouchPoint tagOfNote)
        {
            float top = tagOfNote.CenterY - AppParameters.MARKER_SIZE / 2;
            float left = tagOfNote.CenterX - AppParameters.MARKER_SIZE / 2;
            float bottom = top + AppParameters.NOTE_SIZE;
            float right = left + AppParameters.NOTE_SIZE;
            PointF rotAnchor = new PointF(tagOfNote.CenterX, tagOfNote.CenterY);
            float rotAngle = -1 * tagOfNote.Orientation + (float)Math.PI / 2;

            PointF end = new PointF(_fingerTouches[_fingerTouches.Count - 1].CenterX, _fingerTouches[_fingerTouches.Count - 1].CenterY);
            end = Utilities.rotatePoint(end, rotAngle, rotAnchor);
            if (pointInSideBound(top, left, bottom, right, end))
            {
                //ending point must be outside the note
                Console.WriteLine("Ending in");
                return false;
            }
            PointF beginning = new PointF(_fingerTouches[0].CenterX, _fingerTouches[0].CenterY);
            beginning = Utilities.rotatePoint(beginning, rotAngle, rotAnchor);
            //compute the angle of the line to the standard (non-rotated) posture of the note
            Vector2 beginToEnd = new Vector2(end.X - beginning.X, end.Y - beginning.Y);
            beginToEnd.Normalize();
            double line_angle = Utilities.RadToDegree((float)Math.Acos(beginToEnd.X));
            //if the angle is not around 45 or 135 degrees, then it shouldn't be considered
            if (!(line_angle >= 20 && line_angle <= 60)
                && !(line_angle >= 110 && line_angle <= 150))
            {
                //then it's not note-assigning line
                return false;
            }

            if (pointInSideBound(top, left, bottom, right, beginning))
            {
                //the line starts inside the note, so still acceptable
                return true;
            }
            //if the beginning point is outside the note, then we have to consider the middle point
            PointF middle = new PointF((beginning.X + end.X) / 2, (beginning.Y + end.Y) / 2);
            middle = Utilities.rotatePoint(middle, rotAngle, rotAnchor);
            if (!pointInSideBound(top, left, bottom, right, middle))
            {
                //if the middle point is outside the note, then it's not the correct line
                Console.WriteLine("Middle out");
                return false;
            }
            
            return true;
        }
        bool pointInSideBound(float top, float left, float bottom, float right, PointF p)
        {
            return (p.X >= left && p.X <= right && p.Y >= top && p.Y <= bottom);
        }
    }
}
