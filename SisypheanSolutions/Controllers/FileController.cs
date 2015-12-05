using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace SisypheanSolutions.Controllers
{
    public class FileController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FileDownload(string uniqueID)
        {
            try
            {
                var files = Directory.GetFiles(GetFileLocation());
                string path = files.FirstOrDefault(item => item.Contains(uniqueID));
                string fileName = GetFileName(path);

                if (path.EndsWith(EncryptedExtension()))
                {
                    return PartialView("_FileDownload");
                }

                else
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                    return ReturnFile(fileName, fileBytes);
                }
            }

            catch
            {
                return PartialView("_DownloadError");
            }
        }

        [HttpPost]
        public ActionResult FileDownload(string uniqueID, string password)
        {
            try
            {
                var files = Directory.GetFiles(GetFileLocation());
                string path = files.FirstOrDefault(item => item.Contains(uniqueID));
                string fileName = GetFileName(path);

                byte[] decryptedBytes = DecryptFile(path, password);

                return ReturnFile(fileName, decryptedBytes);
            }

            catch
            {
                return PartialView("_DownloadError");
            }
        }

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase[] files, string password = "")
        {
            try
            {
                Guid uniqueID = Guid.NewGuid();
                string link = GenerateDownloadLink(uniqueID);

                //More than one file, probably should be zipped.
                if (files.Length > 1)
                {
                    using (var compressedFileStream = new MemoryStream())
                    {
                        //Create an archive and store the stream in memory.
                        using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, false))
                        {
                            foreach (var file in files)
                            {
                                //Create a zip entry for each attachment.
                                var zipEntry = zipArchive.CreateEntry(file.FileName);

                                byte[] fileBytes = FileToByteArray(file);

                                //Get the stream of the attachment
                                using (var originalFileStream = new MemoryStream(fileBytes))
                                {
                                    using (var zipEntryStream = zipEntry.Open())
                                    {
                                        //Copy the attachment stream to the zip entry stream
                                        originalFileStream.CopyTo(zipEntryStream);
                                    }
                                }
                            }

                        }

                        byte[] zipBytes = compressedFileStream.ToArray();
                        string fileName = "BundledArchive.zip";
                        if (password != "")
                        {
                            zipBytes = EncryptFile(zipBytes, password);
                            fileName = fileName + EncryptedExtension();
                        }

                        SaveFile(uniqueID, fileName, zipBytes);
                    }
                }

                //Doesn't need zipping.
                else
                {
                    byte[] fileBytes = FileToByteArray(files[0]);

                    if (password != "")
                    {
                        fileBytes = EncryptFile(fileBytes, password);
                    }

                    SaveFile(uniqueID, files[0].FileName, fileBytes);
                }

                return Json(new { success = true });
            }

            catch
            {
                string[] errors = { "There was an error when processing your files. Please try re-uploading." };
                return Json(new { success = false, errors });
            }
        }

        #region Encryption
        private byte[] EncryptFile(byte[] bytesToBeEncrypted, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return AES_Encrypt(bytesToBeEncrypted, passwordBytes);
        }

        private byte[] DecryptFile(string path, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesToBeDecrypted = System.IO.File.ReadAllBytes(path);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            return bytesDecrypted;
        }

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = GetRandomBytes();

            using (MemoryStream memorystream = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cryptostream = new CryptoStream(memorystream, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptostream.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cryptostream.Close();
                    }

                    encryptedBytes = memorystream.ToArray();
                }
            }

            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = GetRandomBytes();

            using (MemoryStream memorystream = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cryptostream = new CryptoStream(memorystream, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptostream.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cryptostream.Close();
                    }

                    decryptedBytes = memorystream.ToArray();
                }
            }

            return decryptedBytes;
        }

        private byte[] GetRandomBytes()
        {
            //int size = 16;
            //byte[] bytes = new byte[size];

            //RNGCryptoServiceProvider.Create().GetBytes(bytes);

            //Not Random Currently.
            byte[] bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8 };

            return bytes;
        }
        #endregion

        #region Private Methods

        private string GetFileName(string file)
        {
            int delimiterIndex = file.IndexOf('.', 0);

            string fileName = file.Substring(delimiterIndex + 1);

            if (fileName.EndsWith(EncryptedExtension()))
            {
                fileName = fileName.Substring(0, fileName.Length - EncryptedExtension().Length);
            }

            return fileName;
        }

        private string GetFileLocation()
        {
            return "D:\\EncryptedFiles\\";
        }

        private string EncryptedExtension()
        {
            return ".encrypted";
        }

        private byte[] FileToByteArray(HttpPostedFileBase file)
        {
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(file.InputStream))
            {
                fileData = binaryReader.ReadBytes(file.ContentLength);
                binaryReader.Close();
            }

            return fileData;
        }

        private void SaveFile(Guid uniqueID, string fileName, byte[] fileBytes)
        {
            string fileLocation = GetPath(uniqueID, fileName);

            System.IO.File.WriteAllBytes(fileLocation, fileBytes);
        }

        private string GetPath(Guid uniqueID, string fileName)
        {
            return GetFileLocation() + uniqueID + "." + fileName;
        }

        private ActionResult ReturnFile(string fileName, byte[] fileBytes)
        {
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        private static string GenerateDownloadLink(Guid uniqueID)
        {
            return "~/Home/FileDownload/" + uniqueID;
        }
        #endregion
    }
}