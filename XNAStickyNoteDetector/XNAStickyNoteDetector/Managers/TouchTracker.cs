using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using XNAStickyNoteDetector.Objects;

namespace XNAStickyNoteDetector.Managers
{
    public enum TouchType
    {
        Tag, Finger, NotSet
    }
    public class TouchTracker
    {
        public delegate void TouchUnavailable(object sender);
        
        const int OFF_TIME_THRESHOLD_FOR_TAG = 500;
        const int OFF_TIME_THRESHOLD_FOR_FINGER = 100;
        List<TouchPoint> _detectedTouches;
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        TimeSpan _lastTouchUpdatedTime;
        TouchType _type;

        internal TouchType Type
        {
            get { return _type; }
            set { _type = value; }
        }
        bool _available;

        public bool Available
        {
            get { return _available; }
            set { _available = value; }
        }
        public event TouchUnavailable touchUnavailableEventHandler = null;
        public event LineOfFingerTouch.IntendedLineGenerated intendedLineGeneratedHandler = null;
        public TouchTracker(int id)
        {
            _id = id;
            _detectedTouches = new List<TouchPoint>();
            _type = TouchType.NotSet;
            _available = true;
        }
        public void addTouch(TouchPoint touch,GameTime gameTime)
        {
            if (_type == TouchType.NotSet)
            {
                if (touch.IsFingerRecognized)
                {
                    _type = TouchType.Finger;
                }
                else if (touch.IsTagRecognized)
                {
                    _type = TouchType.Tag;
                }
            }
            if (_type == TouchType.Tag)
            {
                _detectedTouches.Clear();
            }
            _detectedTouches.Add(touch);
            _lastTouchUpdatedTime = gameTime.TotalGameTime;
        }
        public TouchPoint getLatestTouch()
        {
            return _detectedTouches[_detectedTouches.Count - 1];
        }
        public void Update(GameTime gameTime)
        {
            if (_detectedTouches.Count == 0)
            {
                return;
            }
            
            if (isUnavailable(gameTime))
            {
                _available = false;
                if (touchUnavailableEventHandler != null)
                {
                    touchUnavailableEventHandler(this);
                }
                //start process line of finger touches when the touch left
                if (_type == TouchType.Finger)
                {
                    processFingerTouches();
                }
                return;
            }
            
        }
        void processFingerTouches()
        {
            if (LineOfFingerTouch.isIntendedLine(_detectedTouches))
            {
                Console.WriteLine("Intended line detected");
                if (intendedLineGeneratedHandler != null)
                {
                    LineOfFingerTouch lineOfTouches = new LineOfFingerTouch();
                    lineOfTouches.FingerTouches = _detectedTouches;
                    intendedLineGeneratedHandler(this, lineOfTouches);
                }
            }
        }
        void processTagTouches()
        {

        }
        bool isUnavailable(GameTime currentGameTime)
        {
            TimeSpan sinceLastUpdate = currentGameTime.TotalGameTime - _lastTouchUpdatedTime;
            if (_type == TouchType.Tag)
            {
                return sinceLastUpdate.TotalMilliseconds >= OFF_TIME_THRESHOLD_FOR_TAG;
            }
            if (_type == TouchType.Finger)
            {
                return sinceLastUpdate.TotalMilliseconds >= OFF_TIME_THRESHOLD_FOR_FINGER;
            }
            return false;
        }
        public int getTrackedTouchID()
        {
            if (_detectedTouches.Count == 0)
            {
                return -1;
            }
            return _detectedTouches[0].Id;
        }

    }
}
