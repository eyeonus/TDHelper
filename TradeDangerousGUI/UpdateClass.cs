﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TDHelper
{
    public class Downloader : WebClient
    {
        public int Timeout { get; set; }

        public Downloader() : this(10000) { }

        public Downloader(int timeout)
        {
            this.Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = this.Timeout;
            }

            return request;
        }
    }

    public class UpdateClass
    {
        public static void writeToLog(string logPath, string message)
        {
            DateTime currentTime = DateTime.Now.ToLocalTime();
            FileInfo fileRef = new FileInfo(logPath);
            string outputString = string.Format("[{0}] {1}\r\n", currentTime, message);

            // make sure the log doesn't get too big (<1mb)
            if (fileRef.Exists && fileRef.Length > 1048576)
            {
                File.Delete(logPath);
            }

            // if it exists--we append, if it doesn't--we create
            using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter stream = new StreamWriter(fs))
            {
                stream.Write(outputString);
            }
        }

        public static bool downloadFile(string url, string outputPath)
        {// generic file downloader with a timeout

            bool result = false;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                using (Downloader client = new Downloader())
                {
                    client.DownloadFile(url, outputPath);
                }

                result = true;
            }
            catch (TimeoutException)
            {
                DialogResult d = TopMostMessageBox.Show(true, true, "HTTP download request timed out, retry?", "Error", MessageBoxButtons.YesNo);

                if (d == DialogResult.Yes)
                {
                    result = downloadFile(url, outputPath); // retry
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, e.Message + " [URL: " + url + "] " + (!string.IsNullOrEmpty(e.InnerException.Message) ? e.InnerException.Message : string.Empty));
            }

            return result;
        }

        public static bool compareAssemblyToManifest(string manifest, string path)
        {
            /* 
             * This method compares the md5sum of an assembly in a manifest to
             * a local assembly residing inside the path.
             */

            if (File.Exists(manifest))
            {
                XDocument doc = XDocument.Load(manifest);
                XElement root = doc.Element("Manifest").Element("Assembly");
                string manifestAssemblyName = root.Attribute("Name").Value;
                string localAssemblyPath = Path.Combine(path, manifestAssemblyName);

                if (!string.IsNullOrEmpty(manifestAssemblyName) && File.Exists(localAssemblyPath))
                {// resolve the local path to the assembly mentioned in the manifest
                    string manifestAssemblyHash = root.Element("MD5").Value;
                    string localAssemblyHash = calculateMD5(localAssemblyPath);

                    return (!string.IsNullOrEmpty(manifestAssemblyHash) && localAssemblyHash.Equals(manifestAssemblyHash));
                }
                else
                {
                    writeToLog(MainForm.updateLogPath, "The assembly mentioned in the manifest cannot be found: " + manifestAssemblyName);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool compareFileHashes(string path1, string path2)
        {// take two file paths, spit out true/false if their hashes match
            if (File.Exists(path1) && File.Exists(path2))
            {
                string firstHash = calculateMD5(path1);
                string secondHash = calculateMD5(path2);

                return firstHash.Equals(secondHash);
            }
            else
            {
                return false;
            }
        }
        
        public static bool validateManifest(string manifest)
        {// check if the manifest is valid
            if (File.Exists(manifest))
            {
                XDocument doc = XDocument.Load(manifest);
                XElement root = doc.Descendants("Assembly").FirstOrDefault();

                // assembly exists, file is probably okay
                return (root != null);
            }
            else
            {
                return false;
            }
        }

        public static void generateManifest(string workingPath, string manifest, string URL)
        {
            /*
             * Take a set of files, calculate data for each (assembly first), 
             * print the list to a manifest in XML in the working directory.
             */

            XDocument doc = new XDocument(new XElement("Manifest"));
            XElement root = doc.Element("Manifest");

            try
            {
                // let's make a proper manifest xml from the list of files
                if (Directory.Exists(workingPath) && Directory.GetFiles(workingPath).Length > 0)
                {
                    // only the non-debugging assembly
                    string[] fileList = {
                        "TDHelper.exe",
                        "System.Data.SQLite.dll",
                        "Newtonsoft.Json.dll",
                        "SharpConfig.dll" };

                    // put the assembly info first
                    if (!string.IsNullOrEmpty(fileList[0]))
                    {
                        string assemblyVersion = getFileVersion(fileList[0]);
                        string assemblyMD5 = calculateMD5(fileList[0]);
                        string assemblyExeName = Path.GetFileName(fileList[0]);

                        root.Add(new XElement("Assembly", new XAttribute("Name", assemblyExeName), new XElement("Version", assemblyVersion), new XElement("MD5", assemblyMD5)));

                        if (!string.IsNullOrEmpty(URL))
                        {
                            XElement el = root.Element("Assembly");
                            el.Add(new XElement("URL", URL));
                        }
                        else
                        {
                            writeToLog(MainForm.updateLogPath, "Possibly invalid manifest file, can't find URL tag in Assembly");
                        }

                        // Now the remaining files.
                        for (int i = 1; i < fileList.Length; ++i)
                        {

                            string fileMD5 = calculateMD5(fileList[i]);
                            string fileName = Path.GetFileName(fileList[i]);

                            root.Add(new XElement("Name", new XAttribute("Value", fileName), new XElement("MD5", fileMD5)));
                        }

                        doc.Save(manifest);
                    }
                    else
                    {
                        writeToLog(MainForm.updateLogPath, "Cannot find an assembly in the working path: " + workingPath);
                    }
                }
                else
                {
                    DialogResult d = TopMostMessageBox.Show(
                        true, 
                        true, 
                        "The manifest input directory does not contain any files, or cannot be created.\r\nPlease create the following directory and then confirm: " + workingPath, 
                        "Error", 
                        MessageBoxButtons.OKCancel);

                    if (d == DialogResult.OK)
                    {
                        generateManifest(workingPath, manifest, URL);
                    }
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, e.Message);
            }
        }

        public static void decompressZip(string inputFile, string outputPath)
        {// decompress all files in a zip to a directory
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(inputFile))
                {// overwrite all destination files to avoid errors
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        entry.ExtractToFile(Path.Combine(outputPath, entry.FullName), true);
                    }
                }
            }
            catch (IOException e)
            {
                writeToLog(MainForm.updateLogPath, "IOException: " + e.Message);
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, "Exception: " + e.Message);
            }
        }

        public static void decompressFile(string zipFile, string fileInZip, string outputFile)
        {// decompress a file in a zip to a directory, overwriting it if it exists
            try
            {
                if (File.Exists(zipFile))
                {
                    byte[] zipEntry = readFileInZip(zipFile, fileInZip);

                    File.WriteAllBytes(outputFile, zipEntry);
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, "Exception: " + e.Message);
            }
        }

        public static byte[] readFileInZip(string zipFile, string fileToRead)
        {// takes a zip file, reads a specific file inside it, outputs a byte array
            try
            {
                using (ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry entry = archive.GetEntry(fileToRead);

                    using (Stream stream = entry.Open())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, "Exception: " + e.Message);
                return null;
            }
        }

        public static string calculateMD5(string filePath)
        {// calculate an md5 string
            using (var md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {// output a valid lowercase md5sum
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static string getFileVersion(string filePath)
        {// return the file version of a given assembly
            if (File.Exists(filePath))
            {
                return AssemblyName.GetAssemblyName(filePath).Version.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public static string manifestAssemblyVersion(string manifest)
        {
            try
            {
                if (File.Exists(manifest))
                {
                    XDocument doc = XDocument.Load(manifest);
                    XElement root = doc.Element("Assembly");

                    if (root != null)
                    {
                        string rootAttr = root.Attribute("Name").Value;
                        return (!string.IsNullOrEmpty(rootAttr)) ? rootAttr : "";
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    throw new FileNotFoundException("Cannot find or open the manifest at path: " + manifest);
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, "Exception: " + e.Message);

                return string.Empty;
            }
        }

        public static string manifestAssemblyInfo(string manifest, string key)
        {
            try
            {
                if (File.Exists(manifest))
                {
                    XDocument doc = XDocument.Load(manifest);
                    XElement root = doc.Element("Manifest").Element("Assembly").Element(key);

                    if (root != null)
                    {// it's an element
                        string value = root.Value;
                        return (!string.IsNullOrEmpty(value)) ? value : string.Empty;
                    }
                    else
                    {// it's an attribute
                        // element doesn't exist, try an attribute?
                        XAttribute rootAttr = doc.Element("Manifest").Element("Assembly").Attribute(key);
                        if (rootAttr != null)
                        {
                            string value = rootAttr.Value;
                            return (!string.IsNullOrEmpty(value)) ? value : string.Empty;
                        }
                        else
                        {
                            return string.Empty; // couldn't find it
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("Cannot find or open the manifest at path: " + manifest);
                }
            }
            catch (Exception e)
            {
                writeToLog(MainForm.updateLogPath, "Exception: " + e.Message);
                return string.Empty;
            }
        }

        public static List<string> manifestFileList(string manifest)
        {
            List<string> output = new List<string>();

            output.Add(manifestAssemblyInfo(manifest, "Name"));

            XDocument doc = XDocument.Load(manifest);
            var roots = doc.Element("Manifest").Elements("Name").Attributes("Value");

            foreach (XAttribute n in roots)
            {
                output.Add(n.Value);
            }

            return output;
        }

        public static bool isValidURLArchive(string URI)
        {
            Uri validURI = null;

            if (Uri.TryCreate(URI, UriKind.Absolute, out validURI)
                && (validURI.Scheme == Uri.UriSchemeHttp || validURI.Scheme == Uri.UriSchemeHttps))
            {
                return URI.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }
    }
}
