using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAStickyNoteDetector.Objects;
using Microsoft.Xna.Framework.Graphics;

namespace XNAStickyNoteDetector.Managers
{
    public class StickyNoteManager
    {
        Dictionary<int, StickyNote> availableNotes;
        public StickyNoteManager()
        {
            availableNotes = new Dictionary<int, StickyNote>();
        }
        public bool AddNote(StickyNote note)
        {
            if (!availableNotes.ContainsKey(note.Id))
            {
                availableNotes.Add(note.Id, note);
                Console.WriteLine("Note {0} added", note.Id);
                return true;
            }
            return false;
        }
        public bool RemoveNoteWithId(int toBeRemovedNoteID)
        {
            if (availableNotes.ContainsKey(toBeRemovedNoteID))
            {
                availableNotes.Remove(toBeRemovedNoteID);
                Console.WriteLine("Note {0} removed", toBeRemovedNoteID);
                return true;
            }
            return false;
        }
        public void display(SpriteBatch spriteBatch)
        {
            foreach (StickyNote note in availableNotes.Values)
            {
                note.display(spriteBatch);
            }
        }
    }
}
