using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;

namespace XNAStickyNoteDetector.Presentation
{
    public class TextureManager
    {
        public static Texture2D NoteFrame;
        public static Texture2D StickyTape;
        public static void Load(GraphicsDeviceManager graphics)
        {
            string exeFileName = System.Windows.Forms.Application.ExecutablePath;
            string imgResDirPath = System.IO.Path.GetDirectoryName(exeFileName) + "\\Images\\";
            using (Stream fileName = File.OpenRead(Path.Combine(imgResDirPath, "NoteFrame.png")))
            {
                NoteFrame = Texture2D.FromStream(graphics.GraphicsDevice, fileName);
            }
            using (Stream fileName = File.OpenRead(Path.Combine(imgResDirPath, "StickyTape.png")))
            {
                StickyTape = Texture2D.FromStream(graphics.GraphicsDevice, fileName);
            }
        }
    }
}
