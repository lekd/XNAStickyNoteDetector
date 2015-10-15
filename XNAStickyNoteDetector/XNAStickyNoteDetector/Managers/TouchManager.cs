using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using XNAStickyNoteDetector.Objects;

namespace XNAStickyNoteDetector.Managers
{
    public class TouchManager
    {
        public delegate void StickyNoteCrossedByALine(int noteID, float notePosX, float notePosY, float noteOrientation);
        public delegate void TagRemoved(int removedTagID);

        Dictionary<int, TouchTracker> _trackers;

        public Dictionary<int, TouchTracker> Trackers
        {
            get { return _trackers; }
            set { _trackers = value; }
        }

        
        List<TouchPoint> _currentTouches;

        public List<TouchPoint> CurrentTouches
        {
            get { return _currentTouches; }
            set { _currentTouches = value; }
        }

        public event StickyNoteCrossedByALine noteCrossedByALineEventHandler = null;
        public event TagRemoved tagRemovedEventHandler = null;
        public void Init()
        {
            _currentTouches = new List<TouchPoint>();
            _trackers = new Dictionary<int, TouchTracker>();
        }
        public void UpdateCurrentTouches(List<TouchPoint> touches, GameTime gameTime)
        {
            _currentTouches.Clear();
            foreach (TouchPoint touch in touches)
            {
                if (touch.IsTagRecognized || touch.IsFingerRecognized)
                {
                    _currentTouches.Add(touch);
                    //if no tracker for this, then create one
                    if (!_trackers.ContainsKey(touch.Id))
                    {
                        TouchTracker tracker = new TouchTracker(touch.Id);
                        tracker.intendedLineGeneratedHandler += new Objects.LineOfFingerTouch.IntendedLineGenerated(tracker_intendedLineGeneratedHandler);
                        tracker.touchUnavailableEventHandler += new TouchTracker.TouchUnavailable(tracker_touchUnavailableEventHandler);
                        _trackers.Add(tracker.Id, tracker);
                    }
                    //put this into corresponding tracker
                    TouchTracker corresTracker = _trackers[touch.Id];
                    corresTracker.addTouch(touch,gameTime);
                }
            }
            //update all trackers
            List<int> unavailableKeys = new List<int>();
            foreach (TouchTracker tracker in _trackers.Values)
            {
                tracker.Update(gameTime);
                if (!tracker.Available)
                {
                    unavailableKeys.Add(tracker.getTrackedTouchID());
                }
            }
            //remove all unavaiable trackers
            foreach (int key in unavailableKeys)
            {
                _trackers.Remove(key);
            }
        }

        void tracker_touchUnavailableEventHandler(object sender)
        {
            TouchTracker tracker = (TouchTracker)sender;
            if (tracker.Type == TouchType.Tag)
            {
                if (tagRemovedEventHandler != null)
                {
                    TouchPoint touch = tracker.getLatestTouch();
                    int tagID = (int)(((TagData)touch.Tag).Value);
                    tagRemovedEventHandler(tagID);
                }
            }
        }

        void tracker_intendedLineGeneratedHandler(object sender, object args)
        {
            LineOfFingerTouch generatedLine = (LineOfFingerTouch)args;
            foreach (TouchTracker tracker in _trackers.Values)
            {
                if (tracker.Type == TouchType.Tag)
                {
                    //check if this intentional line crosses a sticky note or not
                    if (generatedLine.crossAStickyNote(tracker.getLatestTouch()))
                    {
                        TouchPoint touch = tracker.getLatestTouch();
                        int tagID = (int)(((TagData)touch.Tag).Value);
                        if (noteCrossedByALineEventHandler != null)
                        {
                            noteCrossedByALineEventHandler(tagID, touch.CenterX, touch.CenterY, touch.Orientation);
                        }
                    }
                }
            }
        }
    }
}
