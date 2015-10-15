using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNAStickyNoteDetector.Presentation;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace XNAStickyNoteDetector.Objects
{
    public class StickyNote
    {
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        float _anchorX;

        public float AnchorX
        {
            get { return _anchorX; }
            set { _anchorX = value; }
        }
        float _anchorY;

        public float AnchorY
        {
            get { return _anchorY; }
            set { _anchorY = value; }
        }
        float _orientation;

        public float Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }
        public void display(SpriteBatch spriteBatch)
        {
            int displayStickyTapeSize = 120;
            float scale = ((float)displayStickyTapeSize)/TextureManager.StickyTape.Width;
            PointF topleft = new PointF(_anchorX - AppParameters.MARKER_SIZE/2, _anchorY - AppParameters.MARKER_SIZE/2);
            PointF tapePosition = new PointF(topleft.X + AppParameters.NOTE_SIZE - displayStickyTapeSize / 6, topleft.Y + displayStickyTapeSize / 6);
            float rotAngleInRads = _orientation - (float)Math.PI / 2;
            tapePosition = Utilities.rotatePoint(tapePosition,rotAngleInRads,new PointF(_anchorX,_anchorY));
            Vector2 origin = new Vector2(TextureManager.StickyTape.Width/2,TextureManager.StickyTape.Height/2);
            spriteBatch.Draw(TextureManager.StickyTape, new Vector2(tapePosition.X, tapePosition.Y),
                                null,
                                Microsoft.Xna.Framework.Color.White,
                                rotAngleInRads,
                                origin,
                                scale,
                                SpriteEffects.None, 0);
        }
        public byte[] getByteData(Bitmap extractedContent)
        {
            //extractedContent = Utilities.ResizeImage(extractedContent, new Size(extractedContent.Width/2,extractedContent.Height/2));
            byte[] numberBuffer = null;
            byte[] dataBuffer = Utilities.ImageToBytes(extractedContent);
            Console.WriteLine("Bitmap size {0}", dataBuffer.Length);
            byte[] allDataBytes = new byte[sizeof(Int32) + 3 * sizeof(float) + sizeof(Int32) + 4 + dataBuffer.Length];
            int index = 0;
            //parse ID
            numberBuffer = Utilities.Int2Bytes(IDGenerator.getHashedID(_id));
            Array.Copy(numberBuffer, 0, allDataBytes, index, numberBuffer.Length);
            index += numberBuffer.Length;
            //parse X
            float relativeX = _anchorX / Program.WindowSize.Width;
            numberBuffer = Utilities.Float2Bytes(relativeX);
            Array.Copy(numberBuffer, 0, allDataBytes, index, numberBuffer.Length);
            index += numberBuffer.Length;
            //parse Y
            float relativeY = _anchorY / Program.WindowSize.Height;
            numberBuffer = Utilities.Float2Bytes(relativeY);
            Array.Copy(numberBuffer, 0, allDataBytes, index, numberBuffer.Length);
            index += numberBuffer.Length;
            //parse orientation
            numberBuffer = Utilities.Float2Bytes(_orientation);
            Array.Copy(numberBuffer, 0, allDataBytes, index, numberBuffer.Length);
            index += numberBuffer.Length;
            //parse content size
            numberBuffer = Utilities.Int2Bytes(dataBuffer.Length);
            Array.Copy(numberBuffer, 0, allDataBytes, index, numberBuffer.Length);
            index += numberBuffer.Length;
            //put content type into the array
            byte[] contentType = new byte[] { (byte)'@', (byte)'B', (byte)'M', (byte)'P' };
            Array.Copy(contentType, 0, allDataBytes, index, contentType.Length);
            index += contentType.Length;
            //put content into the array
            Array.Copy(dataBuffer, 0, allDataBytes, index, dataBuffer.Length);
            return allDataBytes;
        }
    }
}
