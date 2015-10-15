using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNAStickyNoteDetector.Objects;
using System.Drawing;

namespace XNAStickyNoteDetector
{
    public class CommandGenerator
    {
        static byte[] ADD_Prefix = new byte[] { (byte)'<', (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
        static byte[] ADD_Postfix = new byte[] { (byte)'<', (byte)'/', (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
        static byte[] DEL_Prefix = new byte[] { (byte)'<', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static byte[] DEL_Postfix = new byte[] { (byte)'<', (byte)'/', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static byte[] UPD_Prefix = new byte[] { (byte)'<', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };
        static byte[] UPD_Postfix = new byte[] { (byte)'<', (byte)'/', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };

        
        static public byte[] GenerateAddCommand(StickyNote note, Bitmap additionalContent)
        {
            byte[] noteFullData = note.getByteData(additionalContent);
            byte[] commandBytes = new byte[ADD_Prefix.Length + noteFullData.Length + ADD_Postfix.Length];
            int index = 0;
            Array.Copy(ADD_Prefix, 0, commandBytes, index, ADD_Prefix.Length);
            index += ADD_Prefix.Length;
            Array.Copy(noteFullData, 0, commandBytes, index, noteFullData.Length);
            index += noteFullData.Length;
            Array.Copy(ADD_Postfix, 0, commandBytes, index, ADD_Postfix.Length);
            Console.WriteLine("Command length: {0}", commandBytes.Length);
            return commandBytes;
        }
        
        static public byte[] GenerateRemoveCommand(long noteID)
        {
            byte[] data = Utilities.Int2Bytes((int)noteID);
            byte[] commandBytes = new byte[DEL_Prefix.Length + data.Length + DEL_Postfix.Length];
            int index = 0;
            Array.Copy(DEL_Prefix, 0, commandBytes, index, DEL_Prefix.Length);
            index += DEL_Prefix.Length;
            Array.Copy(data, 0, commandBytes, index, data.Length);
            index += data.Length;
            Array.Copy(DEL_Postfix, 0, commandBytes, index, DEL_Postfix.Length);
            return commandBytes;
        }
        
        
    }
}
