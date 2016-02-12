using System;
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
        public ActionResult FileManagerPartial()
        {
            return PartialView("_FileUpload");
        }

        public ActionResult FileDownloadPartial(string uniqueID)
        {
            return PartialView("_FileDownload", uniqueID);
        }

        /// <summary>
        /// Attempts to download the file directly if not encrypted.
        /// 
        /// If encrypted, redirect to the appropriate URL for password entry.
        /// </summary>
        /// <param name="uniqueID"></param>
        /// <returns></returns>
        public ActionResult FileDownload(string uniqueID)
        {
            try
            {
                var files = Directory.GetFiles(GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    return Redirect("/#/file-not-found");
                }

                string fileName = GetFileName(path);

                if (path.EndsWith(EncryptedExtension()))
                {
                    return Redirect("/#/file-download?uniqueID=" + uniqueID);
                }

                else
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                    return ReturnFile(fileName, fileBytes);
                }
            }

            catch (Exception exception)
            {
                if (exception is InvalidOperationException && exception.InnerException.Message == "Sequence contains more than one matching element.")
                {
                    return Redirect("/#/duplicate-file");
                }

                //Need exception logging.
                return Redirect("/#/error");
            }
        }

        /// <summary>
        /// This method is the for the file download post, which is only encrypted files, as they need a password.
        /// 
        /// Should be validated first via AJAX with ValidateDownload(). This allows quick feedback if the password is incorrect.
        /// </summary>
        /// <param name="uniqueID">The guid that causes uniqueness for the files.</param>
        /// <param name="password">The password used to decrypt the files.</param>
        /// <returns>Returns the file if found or an error.</returns>
        [HttpPost]
        public ActionResult FileDownload(string uniqueID, string password = "")
        {
            try
            {
                string[] files = Directory.GetFiles(GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    return Redirect("/#/file-not-found");
                }

                string fileName = Base64Decode(GetFileName(path));
                fileName = DecryptString(fileName, password);

                if (!fileName.Contains(EncryptedExtension()))
                {
                    return PartialView("_DownloadError");
                }

                byte[] bytesToBeDecrypted = System.IO.File.ReadAllBytes(path);
                byte[] decryptedBytes = DecryptFile(bytesToBeDecrypted, password);

                return ReturnFile(fileName, decryptedBytes);
            }

            catch
            {
                return PartialView("_DownloadError");
            }
        }

        /// <summary>
        /// Validates that a file can be successfully decrypted by checking if it can decrypt the file name.
        /// Returns Json response (for AJAX).
        /// </summary>
        /// <param name="uniqueID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ValidateDownload(string uniqueID, string password)
        {
            try
            {
                string[] files = Directory.GetFiles(GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    string[] errors = { "The file you were looking for was not found." };
                    return Json(new { success = false, errors });
                }

                string fileName = Base64Decode(GetFileName(path));
                fileName = DecryptString(fileName, password);

                if (fileName.Contains(EncryptedExtension()))
                {
                    return Json(new { success = true });
                }

                else
                {
                    string[] errors = { "The password entered is incorrect. Please try again." };
                    return Json(new { success = false, errors });
                }
            }

            catch (Exception exception)
            {
                if (exception is CryptographicException)
                {
                    string[] errors = { "The password entered is incorrect. Please try again." };
                    return Json(new { success = false, errors });
                }

                else
                {
                    string[] errors = { "There was a problem processing your download." };
                    return Json(new { success = false, errors });
                }
            }
        }

        /// <summary>
        /// Upload method for one or more files.
        ///     <para>
        ///     If a single file:
        ///     <para>
        ///             Checks to see if the file needs to be encrypted, if so, encrypts and uploads.
        ///     
        ///             Otherwise, the file is uploaded directly.
        ///         </para>
        ///     </para>
        ///     <para>
        ///     If multiple files:
        ///         <para>
        ///             Checks to see if the files need to be encrypted, if so, bundles files into a zip file, encrypts, and uploads.
        ///     
        ///             Otherwise, the files are bundled and uploaded directly.
        ///         </para>
        ///     </para>
        /// </summary>
        /// <param name="files"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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
                    byte[] zipBytes = ZipFiles(files);

                    string fileName = "BundledArchive.zip";

                    //The extension is added twice. Once for password checking, once for file.
                    if (password != "")
                    {
                        zipBytes = EncryptFile(zipBytes, password);
                        fileName = fileName + EncryptedExtension();
                        fileName = Base64Encode(EncryptString(fileName, password));
                        fileName = fileName + EncryptedExtension();
                    }

                    SaveFile(uniqueID, fileName, zipBytes);
                }

                //Doesn't need zipping.
                else
                {
                    byte[] fileBytes = FileToByteArray(files[0]);
                    string fileName = files[0].FileName;
                    if (password != "")
                    {
                        //The extension is added twice. Once for password checking, once for file.
                        fileName = fileName + EncryptedExtension();
                        fileBytes = EncryptFile(fileBytes, password);
                        fileName = Base64Encode(EncryptString(fileName, password));
                        fileName = fileName + EncryptedExtension();
                    }

                    SaveFile(uniqueID, fileName, fileBytes);
                }

                return Json(new { success = true });
            }

            catch (Exception)
            {
                string[] errors = { "There was an error when processing your files. Please try re-uploading." };
                return Json(new { success = false, errors });
            }
        }

        /// <summary>
        /// Accepts an array of files and adds them to a zip archive, in memory.
        /// 
        /// Returns the byte[] of the zipped stream.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private byte[] ZipFiles(HttpPostedFileBase[] files)
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

                return compressedFileStream.ToArray();
            }
        }

        #region Encryption
        /// <summary>
        /// Encrypts a file from a given byte[] with the provided password.
        /// </summary>
        /// <param name="bytesToBeEncrypted"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private byte[] EncryptFile(byte[] bytesToBeEncrypted, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return AES_Encrypt(bytesToBeEncrypted, passwordBytes);
        }

        /// <summary>
        /// Decrypts a file from a given byte[] with the provided password.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private byte[] DecryptFile(byte[] bytesToBeDecrypted, string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            return AES_Decrypt(bytesToBeDecrypted, passwordBytes);
        }

        /// <summary>
        /// Encrypts a string with the given password.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncryptString(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string encryptedString = Convert.ToBase64String(bytesEncrypted);

            return encryptedString;
        }

        /// <summary>
        /// Decrypts a string with the appropriate password.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string DecryptString(string input, string password)
        {
            // Get the bytes of the string
            byte[] bytesToBeDecrypted = Convert.FromBase64String(input);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string decryptedString = Encoding.UTF8.GetString(bytesDecrypted);

            return decryptedString;
        }

        /// <summary>
        /// Encrypts byte[] with 256 AES Encryption.
        /// </summary>
        /// <param name="bytesToBeEncrypted"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Decrypts AES encrypted byte[].
        /// </summary>
        /// <param name="bytesToBeDecrypted"></param>
        /// <param name="passwordBytes"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets just the name of the file, minus the path and extension, from a string.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>Returns the file name without the path and extension.</returns>
        private string GetFileName(string file)
        {
            int delimiterIndex = file.IndexOf('.', 0);

            string fileName = file.Substring(delimiterIndex + 1);

            fileName = RemoveExtension(fileName, EncryptedExtension());

            return fileName;
        }

        /// <summary>
        /// Removes the extension on the end of a file.
        /// </summary>
        /// <param name="fileName">The file name to have the extension removed.</param>
        /// <returns>Returns the file name without the extension.</returns>
        private string RemoveExtension(string fileName, string extension)
        {
            if (fileName.EndsWith(extension))
            {
                fileName = fileName.Substring(0, fileName.Length - extension.Length);
            }

            return fileName;
        }

        /// <summary>
        /// Provides the current location of file storage.
        /// </summary>
        /// <returns>Returns a string containing the current file save location data. Useful to change once.</returns>
        private string GetFileLocation()
        {
            return "D:\\EncryptedFiles\\";
        }

        /// <summary>
        /// Provides the encrypted extension.
        /// </summary>
        /// <returns>Returns the encrypted extension. Useful if the type ever needs to be changed or added to.</returns>
        private string EncryptedExtension()
        {
            return ".encrypted";
        }

        /// <summary>
        /// Provides the zip extension.
        /// </summary>
        /// <returns>Returns the zip extension. Useful if the type ever needs to be changed or added to.</returns>
        private string ZipExtension()
        {
            return ".zip";
        }

        /// <summary>
        /// Accepts a file and converts it into a byte array.
        /// </summary>
        /// <param name="file">The file to be converted.</param>
        /// <returns>Returns the data in a byte array.</returns>
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

        /// <summary>
        /// Writes a file to the location generated from the file name and data.
        /// </summary>
        /// <param name="uniqueID">The unique ID of the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileBytes">The file data.</param>
        private void SaveFile(Guid uniqueID, string fileName, byte[] fileBytes)
        {
            string fileLocation = GetPath(uniqueID, fileName);

            System.IO.File.WriteAllBytes(fileLocation, fileBytes);
        }

        /// <summary>
        /// Gets the full path of the file: location + file name.
        /// </summary>
        /// <param name="uniqueID">The ID of the file being looked for.</param>
        /// <param name="fileName">The name of the file being looked for.</param>
        /// <returns>Returns the path of the file.</returns>
        private string GetPath(Guid uniqueID, string fileName)
        {
            return GetFileLocation() + uniqueID + "." + fileName;
        }

        /// <summary>
        /// Gets the file object from the bytes, name, and extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileBytes">The file data.</param>
        /// <returns>Returns the file.</returns>
        private ActionResult ReturnFile(string fileName, byte[] fileBytes)
        {
            if(fileName.EndsWith(EncryptedExtension()))
            {
                fileName = RemoveExtension(fileName, EncryptedExtension());
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        /// <summary>
        /// UTF8 encodes plain text.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Converts base 64 encoded data to UTF8 string.
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        /// Generates the URL for the file download from the uniqueID parameter.
        /// </summary>
        /// <param name="uniqueID">The unique ID associated with the file download.</param>
        /// <returns>Returns a URL as a string.</returns>
        private static string GenerateDownloadLink(Guid uniqueID)
        {
            return "~/Home/FileDownload/" + uniqueID;
        }
        #endregion
    }
}