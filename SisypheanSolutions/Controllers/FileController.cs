using SisypheanSolutions.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace SisypheanSolutions.Controllers
{
    public class FileController : Controller
    {
        private readonly FileExtensions _fileExtensions = new FileExtensions();

        public ActionResult FileManagerPartial()
        {
            return PartialView("_FileUpload");
        }

        public ActionResult FileDownloadPartial(string uniqueID)
        {
            return PartialView("_FileDownload", uniqueID);
        }

        public ActionResult FileLinkPartial(string link)
        {
            return PartialView("_FileLink", link);
        }

        /// <summary>
        /// Attempts to download the file directly if not encrypted.
        /// 
        /// If encrypted, redirect to the appropriate URL for password entry.
        /// </summary>
        /// <param name="uniqueID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FileDownload(string uniqueID)
        {
            try
            {
                var files = Directory.GetFiles(FileExtensions.GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    return Redirect("/#/file-not-found");
                }

                string fileName = FileExtensions.GetFileName(path);

                if (path.EndsWith(FileExtensions.EncryptedExtension()))
                {
                    return Redirect("/#/file-download?uniqueID=" + uniqueID);
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                return ReturnFile(fileName, fileBytes);
            }

            catch (Exception exception)
            {
                if (exception is InvalidOperationException && exception.InnerException?.Message == "Sequence contains more than one matching element.")
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
                string[] files = Directory.GetFiles(FileExtensions.GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    return Redirect("/#/file-not-found");
                }

                string fileName = EncryptionExtensions.Base64Decode(FileExtensions.GetFileName(path));
                fileName = EncryptionExtensions.DecryptString(fileName, password);

                if (!fileName.Contains(FileExtensions.EncryptedExtension()))
                {
                    return PartialView("_DownloadError");
                }

                byte[] bytesToBeDecrypted = System.IO.File.ReadAllBytes(path);
                byte[] decryptedBytes = EncryptionExtensions.DecryptFile(bytesToBeDecrypted, password);

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
                string[] errors;
                string[] files = Directory.GetFiles(FileExtensions.GetFileLocation());
                string path = files.SingleOrDefault(item => item.Contains(uniqueID));

                if (String.IsNullOrEmpty(path))
                {
                    errors = new[] { "The file you were looking for was not found." };
                    return Json(new { success = false, errors });
                }

                string fileName = EncryptionExtensions.Base64Decode(FileExtensions.GetFileName(path));
                fileName = EncryptionExtensions.DecryptString(fileName, password);

                if (fileName.Contains(FileExtensions.EncryptedExtension()))
                {
                    return Json(new { success = true });
                }

                errors = new[] { "The password entered is incorrect. Please try again." };
                return Json(new { success = false, errors });
            }

            catch (Exception exception)
            {
                string[] errors;
                if (exception is CryptographicException)
                {
                    errors = new[] { "The password entered is incorrect. Please try again." };
                }

                else
                {
                    errors = new[] { "There was a problem processing your download." };
                }

                return Json(new { success = false, errors });
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
                byte[] fileBytes;
                string fileName;
                Guid uniqueID = Guid.NewGuid();
                string link = FileExtensions.GenerateDownloadLink(HttpContext, uniqueID);

                //More than one file, probably should be zipped.
                if (files.Length > 1)
                {
                    fileBytes = FileExtensions.ZipFiles(files);
                    fileName = "BundledFiles" + FileExtensions.ZipExtension();
                }

                //Doesn't need zipping.
                else
                {
                    fileBytes = FileExtensions.FileToByteArray(files[0]);
                    fileName = files[0].FileName;
                }

                //If file is encrypted.
                if (!String.IsNullOrEmpty(password))
                {
                    fileBytes = EncryptionExtensions.EncryptFile(fileBytes, password);
                    fileName = FileExtensions.SetEncryptedFileName(password, fileName);
                }

                FileExtensions.SaveFile(uniqueID, fileName, fileBytes);

                return Json(new { success = true, link });
            }

            catch (Exception)
            {
                string[] errors = { "There was an error when processing your files. Please try re-uploading." };
                return Json(new { success = false, errors });
            }
        }

        /// <summary>
        /// Gets the file object from the bytes, name, and extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileBytes">The file data.</param>
        /// <returns>Returns the file.</returns>
        private ActionResult ReturnFile(string fileName, byte[] fileBytes)
        {
            if (fileName.EndsWith(FileExtensions.EncryptedExtension()))
            {
                fileName = FileExtensions.RemoveExtension(fileName, FileExtensions.EncryptedExtension());
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}