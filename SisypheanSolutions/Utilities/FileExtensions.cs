using System;
using System.IO;
using System.IO.Compression;
using System.Web;

namespace SisypheanSolutions.Utilities
{
    public class FileExtensions
    {
        /// <summary>
        /// Accepts an array of files and adds them to a zip archive, in memory.
        /// 
        /// Returns the byte[] of the zipped stream.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        internal static byte[] ZipFiles(HttpPostedFileBase[] files)
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

        /// <summary>
        /// Gets just the name of the file, minus the path and extension, from a string.
        /// </summary>
        /// <param name="file">The file name.</param>
        /// <returns>Returns the file name without the path and extension.</returns>
        internal static string GetFileName(string file)
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
        /// <param name="extension"></param>
        /// <returns>Returns the file name without the extension.</returns>
        internal static string RemoveExtension(string fileName, string extension)
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
        protected internal static string GetFileLocation()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "Files\\";
        }

        /// <summary>
        /// Provides the encrypted extension.
        /// </summary>
        /// <returns>Returns the encrypted extension. Useful if the type ever needs to be changed or added to.</returns>
        internal static string EncryptedExtension()
        {
            return ".encrypted";
        }

        /// <summary>
        /// Provides the zip extension.
        /// </summary>
        /// <returns>Returns the zip extension. Useful if the type ever needs to be changed or added to.</returns>
        internal static string ZipExtension()
        {
            return ".zip";
        }

        /// <summary>
        /// Accepts a file and converts it into a byte array.
        /// </summary>
        /// <param name="file">The file to be converted.</param>
        /// <returns>Returns the data in a byte array.</returns>
        internal static byte[] FileToByteArray(HttpPostedFileBase file)
        {
            byte[] fileData;
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
        internal static void SaveFile(Guid uniqueID, string fileName, byte[] fileBytes)
        {
            string fileLocation = GetPath(uniqueID, fileName);

            File.WriteAllBytes(fileLocation, fileBytes);
        }

        /// <summary>
        /// Gets the full path of the file: location + file name.
        /// </summary>
        /// <param name="uniqueID">The ID of the file being looked for.</param>
        /// <param name="fileName">The name of the file being looked for.</param>
        /// <returns>Returns the path of the file.</returns>
        private static string GetPath(Guid uniqueID, string fileName)
        {
            return GetFileLocation() + uniqueID + "." + fileName;
        }

        /// <summary>
        /// Generates the file name for encrypted files.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string SetEncryptedFileName(string password, string fileName)
        {
            //The extension is added twice. Once for password checking, once for file.
            fileName = fileName + EncryptedExtension();
            fileName = EncryptionExtensions.Base64Encode(EncryptionExtensions.EncryptString(fileName, password));
            fileName = fileName + EncryptedExtension();

            return fileName;
        }

        /// <summary>
        /// Generates the URL for the file download from the uniqueID parameter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="uniqueID">The unique ID associated with the file download.</param>
        /// <returns>Returns a URL as a string.</returns>
        internal static string GenerateDownloadLink(HttpContextBase context, Guid uniqueID)
        {
            return context?.Request.Url?.Authority + "/#/file/filedownload?uniqueID=" + uniqueID;
        }
    }
}