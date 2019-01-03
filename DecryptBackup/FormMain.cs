using DecryptBackup.SQLiteHelper;
using Claunia.PropertyList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DecryptBackup
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private string FolderPath
        {
            get => tbPath.Text;
            set => tbPath.Text = value;
        }

        private string tempPath = string.Empty;
        private string Password => tbPwd.Text;
        private const string manifestFile = "Manifest.plist";
        private const string manifestDBFile = "Manifest.db";
        private const string infoFile = "Info.plist";
        private const string statusFile = "Status.plist";
        private string decryptedFolderPath = string.Empty;
        private string manifestFilePath = string.Empty;

        private Dictionary<string, TargetTime> FileTime = new Dictionary<string, TargetTime>();
        private Dictionary<string, TargetTime> FolderTime = new Dictionary<string, TargetTime>();
        private DateTimeOffset ZeroTime = new DateTimeOffset(new DateTime(1970, 1, 1));

        private Keybag keyBag;
        private Thread tDecrypted;

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDerypting)
            {
                DialogResult dr = MessageBox.Show("还在解密中，是否退出");
                if (dr.Equals(DialogResult.Cancel) || dr.Equals(DialogResult.No))
                {
                    e.Cancel = true;
                }
                else
                {
                    tDecrypted?.Abort();
                }
            }
        }

        private bool isDecrypting = false;

        private bool IsDerypting
        {
            get => isDecrypting;
            set
            {
                isDecrypting = value;
                this.Invoke(new Action(() =>
                {
                    tbPath.ReadOnly = value;
                    tbPwd.ReadOnly = value;
                    tbShowInfo.ReadOnly = value;
                    btnDecrypte.Enabled = !value;
                    btnFolder.Enabled = !value;
                }));
            }
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            tbShowInfo.Clear();
            if (isDecrypting)
            {
                MessageBox.Show("正在解密中");
                return;
            }
            if (string.IsNullOrEmpty(FolderPath))
            {
                MessageBox.Show("请选择备份文件目录");
                return;
            }
            manifestFilePath = Path.Combine(FolderPath, manifestFile);
            if (!File.Exists(manifestFilePath))
            {
                MessageBox.Show("请确定这是一个正确的备份文件目录");
                return;
            }
            tDecrypted = new Thread(DecryptedBackupFolder);
            tDecrypted.Start();
        }

        private void DecryptedBackupFolder()
        {
            try
            {
                IsDerypting = true;
                FileStream stream = File.OpenRead(manifestFilePath);
                NSObject root = BinaryPropertyListParser.Parse(stream);
                NSDictionary dicRoot = root as NSDictionary;
                foreach (string item in dicRoot.Keys)
                {
                    AppendToTextbox($"{item}\t{dicRoot[item]}{Environment.NewLine}");
                }
                bool wasPwdSet = ((NSNumber)dicRoot["WasPasscodeSet"]).ToBool();
                bool isEncrypted = ((NSNumber)dicRoot["IsEncrypted"]).ToBool();
                if (isEncrypted)
                {
                    NSData data = (NSData)dicRoot["BackupKeyBag"];
                    keyBag = new Keybag(data.Bytes);
                    AppendToTextbox(keyBag.PrintClassKeys());
                    bool isDecrypted = keyBag.UnlockWithPassword(Password);
                    if (!isDecrypted)
                    {
                        MessageBox.Show("密码解密错误，请重新输入密码");
                        isDecrypting = false;
                        return;
                    }
                    AppendToTextbox(keyBag.PrintClassKeys());
                    NSData pManifestKey = (NSData)dicRoot["ManifestKey"];
                    Decryptmetadata(pManifestKey.Bytes);
                    foreach (KeyValuePair<string, TargetTime> item in FileTime)
                    {
                        if (File.Exists(item.Key))
                        {
                            FileInfo file = new FileInfo(item.Key)
                            {
                                LastWriteTime = item.Value.LastModifiedTime,
                                CreationTime = item.Value.CreateTime
                            };
                        }
                    }
                    foreach (KeyValuePair<string, TargetTime> item in FolderTime)
                    {
                        DirectoryInfo directory = new DirectoryInfo(item.Key)
                        {
                            LastWriteTime = item.Value.LastModifiedTime,
                            CreationTime = item.Value.CreateTime
                        };
                    }
                }
                else
                {
                    MessageBox.Show("该备份文件未加密！");
                }
                IsDerypting = false;
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }

        private void Decryptmetadata(byte[] manifestKey)
        {
            byte[] manifestKeyArray = new byte[manifestKey.Length - 4];
            Array.ConstrainedCopy(manifestKey, 4, manifestKeyArray, 0, manifestKeyArray.Length);

            byte[] manifestClassArray = new byte[4];
            Array.ConstrainedCopy(manifestKey, 0, manifestClassArray, 0, manifestClassArray.Length);
            int intManifestClass = manifestClassArray.ToInt32(true);
            byte[] key = keyBag.UnwrapKeyForClass(intManifestClass, manifestKeyArray);

            CreateDecryptedFolder();
            string encryptedDBFile = $"{FolderPath}\\{manifestDBFile}";
            byte[] encryptedDBArray = ReadFileToArray(encryptedDBFile);
            //解密后的字节流
            byte[] decryptedDBData = Security.DecrypteCBC(encryptedDBArray, key, null);
            string decryptedDBFile = WriteDecryptedDBToLocal(decryptedDBData);
            if (!string.IsNullOrEmpty(decryptedDBFile))
            {
                FileInfo dbFile = new FileInfo(encryptedDBFile);
                FileTime[decryptedDBFile] = new TargetTime()
                {
                    CreateTime = dbFile.CreationTime,
                    LastModifiedTime = dbFile.LastWriteTime
                };
                AppendToTextbox($"解密数据库文件成功,解密目录:{Path.GetFullPath(decryptedDBFile)}{Environment.NewLine}");
                DecryptEachFile(decryptedDBFile);
                AppendToTextbox($"备份文件解密完成。{Environment.NewLine}");
            }
        }

        private void DecryptEachFile(string decryptedDBFile)
        {
            DataTable fileTable = SQLiteDataOperate.ReadFiles(decryptedDBFile);
            FileData eachFile = new FileData();
            foreach (DataRow item in fileTable.Rows)
            {
                eachFile.FillData(item);
                DecryptFile(eachFile);
            }
        }

        private readonly Regex pathRegex = new Regex("[:*?\"<>|]+?");

        private void DecryptFile(FileData fileInfo)
        {
            try
            {
                NSDictionary root = BinaryPropertyListParser.Parse(fileInfo.BlobArray) as NSDictionary;

                NSArray objects = root["$objects"] as NSArray;
                NSDictionary top = root["$top"] as NSDictionary;
                int objectsIndex = (top["root"] as UID).Bytes[0];
                NSDictionary fileData = objects[objectsIndex] as NSDictionary;
                int fileSize = (fileData["Size"] as NSNumber).ToInt();

                long unixModifiedTime = (fileData["LastModified"] as NSNumber).ToLong() * 10000000;
                DateTimeOffset targetLastModifed = ZeroTime.Add(new TimeSpan(unixModifiedTime));
                long unixBirthTime = (fileData["Birth"] as NSNumber).ToLong() * 10000000;
                DateTimeOffset targetBirth = ZeroTime.Add(new TimeSpan(unixBirthTime));

                fileInfo.RelativePath = fileInfo.RelativePath.Replace('/', Path.DirectorySeparatorChar);
                fileInfo.RelativePath = pathRegex.Replace(fileInfo.RelativePath, "_");
                string targetPath = Path.Combine(decryptedFolderPath, fileInfo.Domain, fileInfo.RelativePath);

                int mode = (fileData["Mode"] as NSNumber).ToInt();

                if (mode == 33188)
                {
                    AppendToTextbox($"解密文件 {fileInfo.Domain}\\{fileInfo.RelativePath}{Environment.NewLine}");
                    FileTime[targetPath] = new TargetTime()
                    {
                        CreateTime = targetBirth.ToLocalTime().DateTime,
                        LastModifiedTime = targetLastModifed.ToLocalTime().DateTime
                    };
                    int protectionClassFlag = (fileData["ProtectionClass"] as NSNumber).ToInt();
                    int encryptionKeyInObjects = (fileData["EncryptionKey"] as UID).Bytes[0];
                    NSDictionary encryptiontKeyDic = objects[encryptionKeyInObjects] as NSDictionary;
                    NSData nsData = encryptiontKeyDic["NS.data"] as NSData;
                    byte[] nsDataArray = nsData.Bytes;
                    byte[] encryptionKeyArray = new byte[nsData.Length - 4];
                    Array.ConstrainedCopy(nsData.Bytes, 4, encryptionKeyArray, 0, encryptionKeyArray.Length);

                    byte[] key = keyBag.UnwrapKeyForClass(protectionClassFlag, encryptionKeyArray);

                    string encryptedFilePath = Path.Combine(FolderPath, fileInfo.FileID.Substring(0, 2), fileInfo.FileID);

                    byte[] data = ReadFileToArray(encryptedFilePath);
                    byte[] decryptedDataArray = Security.DecrypteCBC(data, key, null);
                    //byte[] decryptedFileData = new byte[decryptedDataArray.Length];
                    //Array.ConstrainedCopy(decryptedDataArray, 0, decryptedFileData, 0, decryptedDataArray.Length);
                    WriteFile(decryptedDataArray, targetPath);
                }
                else if (mode == 16877)//判断是一个文件夹
                {
                    AppendToTextbox($"解密文件文件夹 {fileInfo.Domain}\\{fileInfo.RelativePath}{Environment.NewLine}");
                    Directory.CreateDirectory(targetPath);
                    FolderTime[targetPath] = new TargetTime()
                    {
                        CreateTime = targetBirth.ToLocalTime().DateTime,
                        LastModifiedTime = targetLastModifed.ToLocalTime().DateTime
                    };
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                AppendToTextbox(ex.Message);
            }
        }

        private byte[] ReadFileToArray(string filePath)
        {
            List<byte> encryptedDBArray = new List<byte>();
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] tempArray = new byte[2048];
                while (stream.Read(tempArray, 0, tempArray.Length) > 0)
                {
                    encryptedDBArray.AddRange(tempArray);
                }
            }

            return encryptedDBArray.ToArray();
        }

        private string WriteDecryptedDBToLocal(byte[] decryptedDBData)
        {
            try
            {
                string decryptedDBFile = Path.Combine(decryptedFolderPath, manifestDBFile);
                using (FileStream writeStream = new FileStream(decryptedDBFile, FileMode.Create))
                {
                    writeStream.Write(decryptedDBData, 0, decryptedDBData.Length);
                }
                return decryptedDBFile;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void CreateDecryptedFolder()
        {
            string fullPath = Path.GetFullPath(FolderPath);
            string directoryName = Path.GetDirectoryName(fullPath);
            string rootPath = Path.GetPathRoot(FolderPath);
            decryptedFolderPath = fullPath.Replace(directoryName, rootPath);
            if (Directory.Exists(decryptedFolderPath))
            {
                Directory.Move(decryptedFolderPath, $"{decryptedFolderPath}-{DateTime.Now.ToString("MMddHHmmss")}-bak");
            }
            Directory.CreateDirectory(decryptedFolderPath);
            DirectoryInfo sourceDirectory = new DirectoryInfo(fullPath);
            FolderTime[decryptedFolderPath] = new TargetTime()
            {
                CreateTime = sourceDirectory.CreationTime,
                LastModifiedTime = sourceDirectory.LastWriteTime
            };
            File.Copy($"{FolderPath}\\{infoFile}", $"{decryptedFolderPath}\\{infoFile}");
            File.Copy($"{FolderPath}\\{manifestFile}", $"{decryptedFolderPath}\\{manifestFile}");
            File.Copy($"{FolderPath}\\{statusFile}", $"{decryptedFolderPath}\\{statusFile}");
        }

        private const int BLOCK_SIZE = 2048;
        private void WriteFile(byte[] decryptedData, string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);
            CheckDirectory(folderPath);
            using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                stream.Write(decryptedData, 0, decryptedData.Length);
            }
        }

        private void CheckDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                CheckDirectory(Path.GetDirectoryName(folderPath));
                Directory.CreateDirectory(folderPath);
            }
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBD = new FolderBrowserDialog
            {
                Description = "请选择备份文件路径",
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (!string.IsNullOrEmpty(tempPath))
            {
                folderBD.SelectedPath = tempPath;
            }
            DialogResult dResult = folderBD.ShowDialog();
            if (dResult == DialogResult.OK)
            {
                tempPath = folderBD.SelectedPath;
                FolderPath = folderBD.SelectedPath;
            }
        }

        private void AppendToTextbox(string message)
        {
            tbShowInfo.Invoke(new Action<string>((text) =>
            {
                tbShowInfo.AppendText(text);
                tbShowInfo.ScrollToCaret();
            }), message);
            Application.DoEvents();
        }
    }
}
