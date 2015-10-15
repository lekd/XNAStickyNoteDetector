using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace XNAStickyNoteDetector.Presentation
{
    public class TouchVisualizer
    {
        public static void VisualizeTouches(SpriteBatch spriteBatch, List<TouchPoint> touches)
        {
            foreach (TouchPoint touch in touches)
            {
                if (touch.IsTagRecognized)
                {
                    VisualizeTag(spriteBatch, touch);
                }
                else if (touch.IsFingerRecognized)
                {
                    //Console.WriteLine("Touch {0}", touch.Id);
                }
            }
        }
        static void VisualizeTag(SpriteBatch spriteBatch, TouchPoint tagTouch)
        {
            float scale = ((float)AppParameters.NOTE_SIZE) / TextureManager.NoteFrame.Width;
            PointF rotCenter = new PointF(tagTouch.CenterX, tagTouch.CenterY);
            PointF topLeft = new PointF(tagTouch.CenterX - AppParameters.MARKER_SIZE / 2, tagTouch.CenterY - AppParameters.MARKER_SIZE / 2);
            PointF noteCenter = new PointF(topLeft.X + AppParameters.NOTE_SIZE / 2, topLeft.Y + AppParameters.NOTE_SIZE / 2);
            float rotAngleInRads = tagTouch.Orientation - (float)Math.PI / 2;
            noteCenter = Utilities.rotatePoint(noteCenter, rotAngleInRads, rotCenter);
            Vector2 origin = new Vector2(TextureManager.NoteFrame.Width/2,TextureManager.NoteFrame.Height/2);
            spriteBatch.Draw(TextureManager.NoteFrame, new Vector2(noteCenter.X, noteCenter.Y),
                            null, Microsoft.Xna.Framework.Color.White, rotAngleInRads, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
