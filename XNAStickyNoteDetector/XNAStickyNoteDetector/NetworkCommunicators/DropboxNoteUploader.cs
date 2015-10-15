using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Drawing;

namespace XNAStickyNoteDetector.NetworkCommunicators
{
    public class DropboxNoteUploader
    {
        CloudStorage storage;
        ICloudStorageAccessToken storageToken;
        public DropboxNoteUploader()
        {
            storage = new CloudStorage();
            var dropboxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
            ICloudStorageAccessToken accessToken;
            using (var fs = File.Open("DropBoxStorage.Token", FileMode.Open, FileAccess.Read, FileShare.None))
            {
                accessToken = storage.DeserializeSecurityToken(fs);
            }
            storageToken = storage.Open(dropboxConfig, accessToken);
        }
        Bitmap bitmapToUpload = null;
        string targetFileName = string.Empty;
        public void setFileAndFileNameToUpload(Bitmap toBeUploaded, string fileName)
        {
            bitmapToUpload = toBeUploaded;
            targetFileName = fileName;
        }
        public void UploadNoteBitmap()
        {
            if (bitmapToUpload == null || targetFileName == string.Empty)
            {
                return;
            }
            var targetFolder = storage.GetFolder("/Notes");
            Stream bmpStream = Utilities.ImageToStream(bitmapToUpload);
            storage.UploadFile(bmpStream, targetFileName, targetFolder);
        }
    }
}
