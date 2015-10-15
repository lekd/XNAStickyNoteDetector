using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface;
using System.Drawing;
using System.Runtime.InteropServices;
using XNAStickyNoteDetector.Presentation;
using XNAStickyNoteDetector.Managers;
using XNAStickyNoteDetector.Objects;
using XNAStickyNoteDetector.NetworkCommunicators;
using System.Threading;

namespace XNAStickyNoteDetector
{
    public class MainCanvas_2 : Microsoft.Xna.Framework.Game
    {
        System.Drawing.Imaging.ColorPalette pal;

        private readonly GraphicsDeviceManager graphics;
        private TouchTarget touchTarget;
        private SpriteBatch foregroundBatch;
        private bool applicationLoadCompleteSignalled;
        private volatile bool normalizedImageUpdated;

        // For Normalized Raw Image
        private byte[] normalizedImage;
        private ImageMetrics normalizedMetrics;

        // For Scaling the RawImage back to full screen.
        private float scale;

        // Something to lock to deal with asynchronous frame updates
        private readonly object syncObject = new object();


        TouchManager touchManager = new TouchManager();

        StickyNoteManager noteManager = new StickyNoteManager();

        //Cloud handler
        DropboxNoteUploader cloudNoteUploader;
        public MainCanvas_2()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            
        }
        protected override void Initialize()
        {

            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 60f);

            base.Initialize();
            IntPtr hWnd = this.Window.Handle;
            var control = System.Windows.Forms.Control.FromHandle(hWnd);
            var form = control.FindForm();
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            IDGenerator.Initialize();
            NetworkSettings.Load();
            //WifiCommunicator.Connect(NetworkSettings.HUB_IP, NetworkSettings.HUB_PORT);
            cloudNoteUploader = new DropboxNoteUploader();
        }
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            // Turn raw image back on again
            touchTarget.EnableImage(ImageType.Normalized);
        }
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
        }
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            // If the app isn't active, there's no need to keep the raw image enabled
            touchTarget.DisableImage(ImageType.Normalized);
        }
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();

        }
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();

            // Enable the normalized raw-image.
            touchTarget.EnableImage(ImageType.Normalized);

            // Hook up a callback to get notified when there is a new frame available
            touchTarget.FrameReceived += OnTouchTargetFrameReceived;
        }
        private void OnTouchTargetFrameReceived(object sender, FrameReceivedEventArgs e)
        {
            // Lock the syncObject object so normalizedImage isn't changed while the Update method is using it
            lock (syncObject)
            {
                if (normalizedImage == null)
                {
                    // get rawimage data for a specific area
                    if (e.TryGetRawImage(
                            ImageType.Normalized,
                            0, 0,
                            InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth,
                            InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight,
                            out normalizedImage,
                            out normalizedMetrics))
                    {
                        
                        scale = (InteractiveSurface.PrimarySurfaceDevice == null)
                                    ? 1.0f
                                    : (float)InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth / normalizedMetrics.Width;
                        normalizedImageUpdated = true;
                    }
                }
                else
                {
                    // get the updated rawimage data for the specified area
                    normalizedImageUpdated = e.UpdateRawImage(
                        ImageType.Normalized,
                        normalizedImage,
                        0, 0,
                        InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth,
                        InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight);
                }

                
            }
        }
        protected override void LoadContent()
        {
            TextureManager.Load(graphics);
            AppParameters.Load();
            foregroundBatch = new SpriteBatch(graphics.GraphicsDevice);
            touchManager.Init();
            touchManager.noteCrossedByALineEventHandler += new TouchManager.StickyNoteCrossedByALine(touchManager_noteCrossedByALineEventHandler);
            touchManager.tagRemovedEventHandler += new TouchManager.TagRemoved(touchManager_tagRemovedEventHandler);
        }

        void touchManager_noteCrossedByALineEventHandler(int noteID, float notePosX, float notePosY, float noteOrientation)
        {
            StickyNote newNote = new StickyNote();
            newNote.Id = noteID;
            newNote.AnchorX = notePosX;
            newNote.AnchorY = notePosY;
            newNote.Orientation = noteOrientation;
            bool addResult = noteManager.AddNote(newNote);
            if (addResult)
            {
                //extract corresponding image of the note
                Bitmap screenshot = getScreenShotBitmap(true);
                Bitmap noteImage = StickyNoteExtractor.ProcessToExtract(new Emgu.CV.Image<Emgu.CV.Structure.Gray, byte>(screenshot), newNote.AnchorX, newNote.AnchorY, newNote.Orientation);
                //construct ADD command
                //byte[] addNoteCommandData = CommandGenerator.GenerateAddCommand(newNote, noteImage);
                //send it
                //WifiCommunicator.Send(addNoteCommandData);
                string fileName = String.Format("{0}.png", IDGenerator.getHashedID(noteID));
                cloudNoteUploader.setFileAndFileNameToUpload(noteImage, fileName);
                Thread uploadThread = new Thread(new ThreadStart(cloudNoteUploader.UploadNoteBitmap));
                uploadThread.Start();
            }
            
        }
        void touchManager_tagRemovedEventHandler(int removedTagID)
        {
            bool removeResult = noteManager.RemoveNoteWithId(removedTagID);
            if (removeResult)
            {
                //construct REMOVE command
                byte[] removeNoteCommandData = CommandGenerator.GenerateRemoveCommand(IDGenerator.getHashedID(removedTagID));
                //send it
                //WifiCommunicator.Send(removeNoteCommandData);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {

                // get the current state of touches
                ReadOnlyTouchPointCollection touches = touchTarget.GetState();
                List<TouchPoint> currentTouches = new List<TouchPoint>(touches);
                touchManager.UpdateCurrentTouches(currentTouches,gameTime);
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            // This controls the color of the image data.
            graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.FromNonPremultiplied(85,89,92,255));
            foregroundBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            TouchVisualizer.VisualizeTouches(foregroundBatch,touchManager.CurrentTouches);
            noteManager.display(foregroundBatch);
            foregroundBatch.End();
            base.Draw(gameTime);
        }

        private Bitmap getScreenShotBitmap(bool scaleFullSize)
        {
            lock (syncObject)
            {
                // Don't bother if the app isn't visible, or if the image hasn't been updates since the last update
                if (normalizedImageUpdated &&
                    (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable))
                {
                    if (normalizedMetrics != null)
                    {
                        GCHandle h = GCHandle.Alloc(normalizedImage, GCHandleType.Pinned);
                        IntPtr ptr = h.AddrOfPinnedObject();
                        Bitmap imageBitmap = new Bitmap(normalizedMetrics.Width,
                                              normalizedMetrics.Height,
                                              normalizedMetrics.Stride,
                                              System.Drawing.Imaging.PixelFormat.Format8bppIndexed,
                                              ptr);
                        if (h.IsAllocated)
                        {
                            h.Free();
                        }
                        Convert8bppBMPToGrayscale(imageBitmap);
                        if (scaleFullSize)
                        {
                            Size fullSize = new Size((int)(scale * normalizedMetrics.Width), (int)(scale * normalizedMetrics.Height));
                            imageBitmap = Utilities.ResizeImage(imageBitmap, fullSize);
                        }
                        return imageBitmap;
                    }
                }
            }
            return null;
        }
        private void Convert8bppBMPToGrayscale(Bitmap bmp)
        {
            if (pal == null) // pal is defined at module level as --- ColorPalette pal;
            {
                pal = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pal.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                }
            }
            bmp.Palette = pal;
        }
        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
        }
        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release  managed resources.
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                }

                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
            }

            // Release unmanaged Resources.

            base.Dispose(disposing);
        }

        #endregion
    }
}
