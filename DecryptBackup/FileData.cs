using System;
using System.Data;

namespace DecryptBackup
{
    internal class FileData
    {
        public string FileID { get; set; }
        public string Domain { get; set; }
        public string RelativePath { get; set; }
        public byte[] BlobArray { get; set; }

        public FileData() { }

        public FileData(DataRow row)
        {
            FillData(row);
        }

        public void Reset()
        {
            FileID = string.Empty;
            Domain = string.Empty;
            RelativePath = string.Empty;
            Array.Clear(BlobArray, 0, BlobArray.Length);
        }

        public void FillData(DataRow row)
        {
            FileID = row["fileID"].ToString();
            Domain = row["domain"].ToString();
            RelativePath = row["relativePath"].ToString();
            BlobArray = row["file"] as byte[];
        }
    }
}
