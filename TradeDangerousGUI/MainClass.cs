﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Media;
using System.Data.SQLite;
using System.Collections;
using System.Web.Security;

namespace TDHelper
{
    public partial class Form1 : Form
    {
        #region Props
        // grab a static reference to the global settings
        public static TDSettings settingsRef = TDSettings.Instance;

        public string remoteManifestPath = "https://bitbucket.org/WombatFromHell/trade-dangerous-helper/downloads/TDHelper.manifest";
        public static string localDir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        public static string assemblyPath = System.Reflection.Assembly.GetEntryAssembly().Location;
        public static string localManifestPath = localDir + "\\TDHelper.manifest.tmp";
        public static string remoteArchiveLocalPath; // save the archive path
        public static string configFileDefault = localDir + "\\Default.xml";
        public static string updateLogPath = localDir + "\\update.log";

        public static bool hasParsed = false, isActive = false, callForReset = false;
        public static double t_CrTonTally, t_meanDist;
        public static string configFile = configFileDefault;
        public string r_fromBox, t_avoidBox, t_outputVerbosity, t_confirmCode, t_lastSystem, t_lastSysCheck, t_childTitle;
        public decimal t_belowPrice, t_Routes, t_EndJumps, t_StartJumps, r_unladenLY, r_ladenLY, l0_ladenLY, l1_ladenLY, t_ladenLY, t1_ladenLY, t2_ladenLY, t_lsFromStar, t_Supply, t_Demand;
        public bool hasRun, dropdownOpened, rebuildCache, t_localNavEnabled, t_csvExportCheckBox, stationsFilterChecked, oldDataRouteChecked;

        public int stn_marketBoxChecked, stn_blackmarketBoxChecked, stn_shipyardBoxChecked, stn_repairBoxChecked,
            stn_rearmBoxChecked, stn_refuelBoxChecked, stn_outfitBoxChecked;
        public int marketBoxChecked, blackmarketBoxChecked, shipyardBoxChecked, repairBoxChecked, rearmBoxChecked, refuelBoxChecked, outfitBoxChecked, fromPane = -1, runOutputState = -1;

        // for EDSC support
        private double t_refDist1 = 0.00f, t_refDist2 = 0.00f, t_refDist3 = 0.00f, t_refDist4 = 0.00f, t_refDist5 = 0.00f;
        public static List<string> latestLogPaths;
        public List<string> outputItems, currentMarkedStations;
        public static string t_itemListPath, t_shipListPath, t_AppConfigPath, recentLogPath, authCode;
        public String temp_src, temp_dest, temp_commod, temp_shipsSold, t_path, t_maxPadSize;
        public Stopwatch stopwatch = new Stopwatch();
        public Process td_proc = new Process(); // put this outside so we can kill the process

        // for Pilot's Log support
        //public DataSet pilotsLogSet = new DataSet("PilotsLog");
        private DataTable pilotsSystemLogTable = new DataTable("SystemLog"), retrieverCacheTable = new DataTable();
        private string pilotsLogDBPath = localDir + "\\TDHelper.db";
        private List<string> output_unclean = new List<string>();
        private Dictionary<string, string> netLogOutput = new Dictionary<string, string>();
        private List<KeyValuePair<string,string>> localSystemList = new List<KeyValuePair<string,string>>();
        private List<int> dgRowIDIndexer = new List<int>(), dgRowIndexer = new List<int>();
        private int pRowIndex = 0, dRowIndex = 0, batchedRowCount = -1, listLimit = 50;
        private bool hasRefreshedRecents, hasLogLoaded, loadedFromDB;
        private Object readNetLock = new Object();

        // for circular buffering in the output log
        const int circularBufferSize = 32768; // default to ~8 pages
        private StringBuilder circularBuffer = new StringBuilder(circularBufferSize);
        #endregion

        #region SettingsRelated
        private void buildSettings()
        {
            //
            // Deserialize variables from our settings class
            //

            // force InvariantCulture to prevent issues
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            // snag the newest data from the file if it exists
            if (checkIfFileOpens(configFile))
            {
                Deserialize(configFile);
                currentMarkedStations = parseMarkedStations();
            }

            // reset culture
            Thread.CurrentThread.CurrentCulture = userCulture;
        }

        private void writeSettings()
        {
            /*
             * This method writes all the known variables to an xml file.
             * 
             * This can also be used to generate a fresh file if necessary.
             */

            // save the path for reload on startup
            if (!String.IsNullOrEmpty(settingsRef.LastUsedConfig) 
                && settingsRef.LastUsedConfig.Contains("Default.xml"))
                settingsRef.LastUsedConfig = localDir + "\\Default.xml";
            else
                settingsRef.LastUsedConfig = configFile;

            serializeMarkedStations(currentMarkedStations); // convert object to built string
            copySettingsFromForm();
            validateSettings();

            settingsRef.LocationParent = saveWinLoc(this);
            settingsRef.SizeParent = saveWinSize(this);

            if (tabControl1.SelectedTab == tabControl1.TabPages["notesPage"])
                notesTextBox.SaveFile(notesFile, RichTextBoxStreamType.PlainText);

            Serialize(configFile);

            // call the parser to add new configs to the list
            validConfigs = parseValidConfigs();
            // refresh our datasource
            altConfigBox.DataSource = null;
            altConfigBox.DataSource = validConfigs[1];
            altConfigBox.SelectedIndex = altConfigBox.Items.IndexOf(Path.GetFileNameWithoutExtension(configFile));
        }

        private void loadSettings(string path)
        {
            // make sure to load our data as invariant
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (checkIfFileOpens(path) && validateConfigFile(path))
                configFile = path; // we've got a valid TDHelper config file

            // save our current used config file
            if (!String.IsNullOrEmpty(settingsRef.LastUsedConfig)
                && settingsRef.LastUsedConfig.Contains("Default.xml"))
                settingsRef.LastUsedConfig = localDir + "\\Default.xml";
            else
                settingsRef.LastUsedConfig = configFile;

            Serialize(configFileDefault, settingsRef.LastUsedConfig, "LastUsedConfig");

            buildSettings();
            validateSettings();
            copySettingsFromConfig();

            // don't populate if switching configs
            if (buttonCaller != 21)
            {
                buildOutput(true);
                buildPilotsLog();
            }

            // populate the notes page
            if (File.Exists(notesFile))
                notesTextBox.LoadFile(notesFile, RichTextBoxStreamType.PlainText);

            // call the parser to add new configs to the list
            validConfigs = parseValidConfigs();
            // refresh our datasource
            altConfigBox.DataSource = null;
            altConfigBox.DataSource = validConfigs[1];
            altConfigBox.SelectedIndex = altConfigBox.Items.IndexOf(Path.GetFileNameWithoutExtension(configFile));

            // reset our selected command for safety
            methodDropDown.SelectedIndex = 0;

            // reset to our previous culture type
            Thread.CurrentThread.CurrentCulture = userCulture;
        }
        #endregion

        #region Validation
        public void validateSettings()
        {
            // sanity check our inputs
            if (settingsRef.Credits < creditsBox.Minimum)
                settingsRef.Credits = creditsBox.Minimum; // this is a requirement
            else if (settingsRef.Credits > creditsBox.Maximum)
                settingsRef.Credits = creditsBox.Maximum;

            if (settingsRef.Capacity < capacityBox.Minimum)
                settingsRef.Capacity = capacityBox.Minimum;
            else if (settingsRef.Capacity > capacityBox.Maximum)
                settingsRef.Capacity = capacityBox.Maximum;

            if (settingsRef.AbovePrice < abovePriceBox.Minimum)
                settingsRef.AbovePrice = abovePriceBox.Minimum;
            else if (settingsRef.AbovePrice > abovePriceBox.Maximum)
                settingsRef.AbovePrice = abovePriceBox.Maximum;

            if (settingsRef.BelowPrice < belowPriceBox.Minimum)
                settingsRef.BelowPrice = belowPriceBox.Minimum;
            else if (settingsRef.BelowPrice > belowPriceBox.Maximum)
                settingsRef.BelowPrice = belowPriceBox.Maximum;

            if (settingsRef.PruneHops < pruneHopsBox.Minimum)
                settingsRef.PruneHops = pruneHopsBox.Minimum;
            else if (settingsRef.PruneHops > pruneHopsBox.Maximum)
                settingsRef.PruneHops = pruneHopsBox.Maximum;

            if (settingsRef.PruneScore < pruneScoreBox.Minimum)
                settingsRef.PruneScore = pruneScoreBox.Minimum;
            else if (settingsRef.PruneScore > pruneScoreBox.Maximum)
                settingsRef.PruneScore = pruneScoreBox.Maximum;

            if (settingsRef.Limit < limitBox.Minimum)
                settingsRef.Limit = limitBox.Minimum;
            else if (settingsRef.Limit > limitBox.Maximum)
                settingsRef.Limit = limitBox.Maximum;

            if (settingsRef.MaxLSDistance < maxLSDistanceBox.Minimum)
                settingsRef.MaxLSDistance = maxLSDistanceBox.Minimum;
            else if (settingsRef.MaxLSDistance > maxLSDistanceBox.Maximum)
                settingsRef.MaxLSDistance = maxLSDistanceBox.Maximum;

            if (settingsRef.LSPenalty < lsPenaltyBox.Minimum)
                settingsRef.LSPenalty = lsPenaltyBox.Minimum;
            else if (settingsRef.LSPenalty > lsPenaltyBox.Maximum)
                settingsRef.LSPenalty = lsPenaltyBox.Maximum;

            if (settingsRef.Stock < stockBox.Minimum)
                settingsRef.Stock = stockBox.Minimum;
            else if (settingsRef.Stock > stockBox.Maximum)
                settingsRef.Stock = stockBox.Maximum;

            if (settingsRef.GPT < gptBox.Minimum)
                settingsRef.GPT = gptBox.Minimum;
            else if (settingsRef.GPT > gptBox.Maximum)
                settingsRef.GPT = gptBox.Maximum;

            if (settingsRef.MaxGPT < maxGPTBox.Minimum)
                settingsRef.MaxGPT = maxGPTBox.Minimum;
            else if (settingsRef.MaxGPT > maxGPTBox.Maximum)
                settingsRef.MaxGPT = maxGPTBox.Maximum;

            if (settingsRef.Insurance < insuranceBox.Minimum)
                settingsRef.Insurance = insuranceBox.Minimum;
            else if (settingsRef.Insurance > insuranceBox.Maximum)
                settingsRef.Insurance = insuranceBox.Maximum;

            if (settingsRef.Margin < marginBox.Minimum)
                settingsRef.Margin = marginBox.Minimum;
            else if (settingsRef.Margin > marginBox.Maximum)
                settingsRef.Margin = marginBox.Maximum;

            if (settingsRef.Age < ageBox.Minimum)
                settingsRef.Age = ageBox.Minimum;
            else if (settingsRef.Age > ageBox.Maximum)
                settingsRef.Age = ageBox.Maximum;

            if (settingsRef.LadenLY < ladenLYBox.Minimum)
                settingsRef.LadenLY = ladenLYBox.Minimum; // this is a requirement
            else if (settingsRef.LadenLY > ladenLYBox.Maximum)
                settingsRef.LadenLY = ladenLYBox.Maximum;

            if (settingsRef.UnladenLY < unladenLYBox.Minimum)
                settingsRef.UnladenLY = unladenLYBox.Minimum;
            else if (settingsRef.UnladenLY > unladenLYBox.Maximum)
                settingsRef.UnladenLY = unladenLYBox.Maximum;

            // convert verbosity to a string
            if (settingsRef.Verbosity == 0)
                t_outputVerbosity = "";
            else if (settingsRef.Verbosity == 3)
                t_outputVerbosity = "-vvv";
            else if (settingsRef.Verbosity == 2)
                t_outputVerbosity = "-vv";
            else if (settingsRef.Verbosity == 1)
                t_outputVerbosity = "-v";

            if (settingsRef.Hops < hopsBox.Minimum && !settingsRef.Loop)
                settingsRef.Hops = hopsBox.Minimum;
            else if (settingsRef.Loop && settingsRef.Hops < 2)
            {
                settingsRef.Hops = 2;
                hopsBox.Text = "2";
            }
            else if (settingsRef.Hops > hopsBox.Maximum)
                settingsRef.Hops = hopsBox.Maximum;

            if (settingsRef.Jumps < jumpsBox.Minimum)
                settingsRef.Jumps = jumpsBox.Minimum;
            else if (settingsRef.Jumps > jumpsBox.Maximum)
                settingsRef.Jumps = jumpsBox.Maximum;

            // these only apply if we haven't copied them already
            if (t_StartJumps < startJumpsBox.Minimum)
                t_StartJumps = startJumpsBox.Minimum;
            else if (t_StartJumps > startJumpsBox.Maximum)
                t_StartJumps = startJumpsBox.Maximum;

            if (t_EndJumps < endJumpsBox.Minimum)
                t_EndJumps = endJumpsBox.Minimum;
            else if (t_EndJumps > endJumpsBox.Maximum)
                t_EndJumps = endJumpsBox.Maximum;

            if (settingsRef.CSVSelect < 0 && settingsRef.CSVSelect > 5)
                settingsRef.CSVSelect = 0;

            if (!containsPadSizes(settingsRef.Padsizes))
                settingsRef.Padsizes = "";

            if (String.IsNullOrEmpty(confirmBox.Text))
                t_confirmCode = "";

            // an exception is made for checkboxes, we shouldn't ever get here
            if (settingsRef.Towards && settingsRef.Loop)
                settingsRef.Loop = false;
            else if (settingsRef.Towards && String.IsNullOrEmpty(temp_dest))
                settingsRef.Towards = false;

            // sanity check in case of invalid input paths
            if (buttonCaller == 14)
                validateImportPath();
            else if (buttonCaller == 13)
                validateUploadPath();

            validatePython(null);
            validateTDPath(null);
            validateNetLogPath(null);

            // default to Run command if unset
            methodDropDown.SelectedIndex = methodIndex;

            // make sure we pull CSV paths after we validate our inputs
            if (!String.IsNullOrEmpty(settingsRef.TDPath)) { t_itemListPath = settingsRef.TDPath + @"\data\Item.csv"; }
            if (!String.IsNullOrEmpty(settingsRef.TDPath)) { t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv"; }
        }

        public static void validateTDPath(string altPath)
        {
            if (!String.IsNullOrEmpty(settingsRef.PythonPath) && !settingsRef.PythonPath.EndsWith("trade.exe", StringComparison.OrdinalIgnoreCase))
            {// bypass this routine if the python path validator sets our path for us (due to Trade Dangerous Installer)
                if (String.IsNullOrEmpty(settingsRef.TDPath) || !checkIfFileOpens(settingsRef.TDPath + "\\trade.py"))
                {
                    OpenFileDialog x = new OpenFileDialog();
                    x.Title = "Select Trade.py from the Trade Dangerous directory";

                    if (Directory.Exists(settingsRef.TDPath))
                        x.InitialDirectory = settingsRef.TDPath;

                    x.Filter = "Py files (*.py)|*.py";
                    if (x.ShowDialog() == DialogResult.OK)
                    {
                        settingsRef.TDPath = Path.GetDirectoryName(x.FileName);
                        // we have to create the item/ship paths again after the validation
                        t_itemListPath = settingsRef.TDPath + @"\data\Item.csv";
                        t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv";
                        Serialize(configFile, settingsRef.TDPath, "TDPath");
                    }
                    else
                    {
                        string localPath = altPath ?? ""; // prevent null
                        if (!String.IsNullOrEmpty(localPath) && checkIfFileOpens(localPath + "\\trade.py") || localPath.EndsWith(".py"))
                        {// if we have an alternate path, we can reset the variable here
                            settingsRef.TDPath = localPath;
                            t_itemListPath = settingsRef.TDPath + @"\data\Item.csv";
                            t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv";
                            Serialize(configFile, settingsRef.TDPath, "TDPath");
                        }
                        else
                            throw new Exception("TradeDangerous path is empty or invalid, cannot continue");
                    }
                }
            }
        }

        private void validateImportPath()
        {
            if (String.IsNullOrEmpty(settingsRef.ImportPath) || !checkIfFileOpens(settingsRef.ImportPath) && buttonCaller == 14)
            {// only execute if called from Import button
                OpenFileDialog x = new OpenFileDialog();
                x.Title = "Select a .prices file";

                if (Directory.Exists(settingsRef.ImportPath))
                    x.InitialDirectory = settingsRef.ImportPath;

                x.Filter = "Prices files|*.prices;*.updated;*.last|All files|*.*";
                if (x.ShowDialog() == DialogResult.OK)
                {
                    settingsRef.ImportPath = x.FileName;
                    Serialize(configFile, settingsRef.ImportPath, "ImportPath");
                }
            }
        }

        private void validateUploadPath()
        {
            if (String.IsNullOrEmpty(settingsRef.UploadPath) || !checkIfFileOpens(settingsRef.UploadPath) && buttonCaller == 13)
            {// only execute if called from Upload button
                OpenFileDialog x = new OpenFileDialog();
                x.Title = "Select a file to upload";

                if (Directory.Exists(settingsRef.UploadPath))
                    x.InitialDirectory = settingsRef.UploadPath;

                x.Filter = "Prices/CSV files|*.prices;*.csv|All files|*.*";
                if (x.ShowDialog() == DialogResult.OK)
                {
                    settingsRef.UploadPath = x.FileName;
                    Serialize(configFile, settingsRef.ImportPath, "UploadPath");
                }
            }
        }

        public static void validateNetLogPath(string altPath)
        {// override to avoid net log logic
            if (!settingsRef.DisableNetLogs)
            {
                if (String.IsNullOrEmpty(settingsRef.NetLogPath) || !checkIfFileOpens(Directory.GetParent(settingsRef.NetLogPath).ToString() + "\\AppConfigLocal.xml"))
                {// let's just ask the user where to look
                    OpenFileDialog x = new OpenFileDialog();
                    x.Title = "Select a valid Elite: Dangerous AppConfigLocal.xml";
                    x.Filter = "AppConfigLocal.xml|*.xml";
                    if (x.ShowDialog() == DialogResult.OK)
                    {
                        t_AppConfigPath = x.FileName;
                        settingsRef.NetLogPath = Directory.GetParent(t_AppConfigPath) + "\\Logs"; // set the appropriate Logs folder
                        Serialize(configFile); // serialize the whole class just in case
                        validateVerboseLogging(); // always validate if verboselogging is enabled
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(altPath) && Directory.Exists(settingsRef.NetLogPath) && settingsRef.NetLogPath.EndsWith("Logs"))
                        {
                            t_AppConfigPath = Directory.GetParent(altPath) + "\\AppConfigLocal.xml";
                            settingsRef.NetLogPath = altPath;
                            Serialize(configFile); // serialize the whole class just in case
                            validateVerboseLogging(); // always validate if verboselogging is enabled
                        }
                        else
                        {
                            DialogResult dialog2 = TopMostMessageBox.Show(true, true, "Cannot set NetLogPath to a valid directory.\r\nWe will disable scanning for recent systems, if you want to re-enable it, set a working path.", "Error", MessageBoxButtons.OK);
                            settingsRef.DisableNetLogs = true;
                            Serialize(configFile);
                        }
                    }
                }
                else
                {
                    // derive our AppConfigLocal.xml path from NetLogPath
                    t_AppConfigPath = Directory.GetParent(settingsRef.NetLogPath).ToString() + "\\AppConfigLocal.xml";
                    // double check the verbose logging state
                    validateVerboseLogging();
                }
            }
        }

        public static void validatePython(string altPath)
        {
            /*
             * This method attempts to find python.exe by using 'where', and 
             * if that should fail we then ask the user
             */

            // before we do anything else, check if the current path works
            if (String.IsNullOrEmpty(settingsRef.PythonPath) || !checkIfFileOpens(settingsRef.PythonPath))
            {
                OpenFileDialog x = new OpenFileDialog();
                x.Title = "Select your python.exe or trade.exe";
                x.Filter = "Python Interpreter (*.exe)|*.exe";
                if (x.ShowDialog() == DialogResult.OK)
                {
                    if (checkIfFileOpens(x.FileName))
                    {
                        settingsRef.PythonPath = Path.GetFullPath(x.FileName);
                        Serialize(configFile, settingsRef.PythonPath, "PythonPath");

                        if (settingsRef.PythonPath.EndsWith("trade.exe", StringComparison.OrdinalIgnoreCase))
                        {// we're running Trade Dangerous Installer, adjust the relative paths
                            settingsRef.TDPath = Directory.GetParent(settingsRef.PythonPath).ToString();
                            t_itemListPath = settingsRef.TDPath + @"\data\Item.csv";
                            t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv";
                            Serialize(configFile, settingsRef.TDPath, "TDPath");
                        }
                    }
                    else
                        throw new Exception("Unable to access the python interpreter, this is fatal");
                }
                else
                {
                    if (checkIfFileOpens(altPath))
                    {
                        settingsRef.PythonPath = altPath;
                        Serialize(configFile, settingsRef.PythonPath, "PythonPath");

                        if (settingsRef.PythonPath.EndsWith("trade.exe", StringComparison.OrdinalIgnoreCase))
                        {// we're running Trade Dangerous Installer, adjust the relative paths
                            settingsRef.TDPath = Directory.GetParent(settingsRef.PythonPath).ToString();
                            t_itemListPath = settingsRef.TDPath + @"\data\Item.csv";
                            t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv";
                            Serialize(configFile, settingsRef.TDPath, "TDPath");
                        }
                    }
                    else
                        throw new Exception("Unable to access the python interpreter, this is fatal");
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(settingsRef.PythonPath) && settingsRef.PythonPath.EndsWith("trade.exe", StringComparison.OrdinalIgnoreCase))
                {// make sure we adjust relative paths to CSVs if we need to
                    settingsRef.TDPath = Directory.GetParent(settingsRef.PythonPath).ToString();
                    t_itemListPath = settingsRef.TDPath + @"\data\Item.csv";
                    t_shipListPath = settingsRef.TDPath + @"\data\Ship.csv";
                }
            }
        }

        private bool validateEDSCInput()
        {
            if (!String.IsNullOrEmpty(refSysTextBox1.Text) && !String.IsNullOrEmpty(refSysTextBox2.Text) && !String.IsNullOrEmpty(refSysTextBox3.Text)
                && !String.IsNullOrEmpty(refSysTextBox4.Text) && !String.IsNullOrEmpty(refSysTextBox5.Text))
            {
                string[] refSystemInputs = new string[] { refSysTextBox1.Text, refSysTextBox2.Text, refSysTextBox3.Text, refSysTextBox4.Text, refSysTextBox5.Text};

                if (stringInList(temp_src, outputSysStnNames))
                    return false;

                // make sure none of the reference systems match each other in any way
                if (arrayContainsDuplicate(refSystemInputs))
                    return false; // if we have any duplicates, bail

                // check our distances to make sure they're sane
                if (!double.TryParse(edscLYBox1.Text, out t_refDist1) && t_refDist1 > 0.00f && t_refDist1 <= 999.99f)
                    return false; // if we hit insanity, bail
                if (!double.TryParse(edscLYBox2.Text, out t_refDist2) && t_refDist2 > 0.00f && t_refDist2 <= 999.99f)
                    return false;
                if (!double.TryParse(edscLYBox3.Text, out t_refDist3) && t_refDist3 > 0.00f && t_refDist3 <= 999.99f)
                    return false;
                if (!double.TryParse(edscLYBox4.Text, out t_refDist4) && t_refDist4 > 0.00f && t_refDist4 <= 999.99f)
                    return false;
                if (!double.TryParse(edscLYBox5.Text, out t_refDist5) && t_refDist5 > 0.00f && t_refDist5 <= 999.99f)
                    return false;

                return true; // success!
            }

            return false;
        }
        #endregion

        #region HelpFuncs
        public static bool checkIfFileOpens(String path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // throw if file can't be opened, hopefully
                    FileStream p = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    p.Close();
                    p.Dispose();
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private bool stringInList(string input, List<string> listToSearch)
        {// check if a partial string exists inside a list of strings, stop at first match
            for (int i = 0; i < listToSearch.Count; i++)
            {
                if (listToSearch[i].IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private bool stringInListExact(string input, List<string> listToSearch)
        {
            for (int i = 0; i < listToSearch.Count; i++)
            {// go in reverse to hit a match faster with our particular dataset
                if (listToSearch[i].Equals(input, StringComparison.InvariantCulture))
                    return true; // return on the first match
            }

            return false;
        }

        private bool stringInArray(string input, string[] stringsToSearch)
        {// true if partial string (insensitive) exists in a string array
            foreach (String s in stringsToSearch)
            {
                if (s.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private int indexInList(string input, List<string> listToSearch)
        {// return only an index of the first partial match (insensitive)
            int index = listToSearch.IndexOf(input);
            if (index >= 0)
                return index;
            else
                return -1;
        }

        private int indexInListExact(string input, List<string> listToSearch)
        {// return an index of the first exact match
            for (int i = listToSearch.Count - 1; i >= 0; i--)
            {// we should hit a match faster in reverse for our particular dataset
                if (listToSearch[i].Equals(input, StringComparison.InvariantCulture))
                {// only return the first match found from the bottom of the list
                    return i;
                }
            }

            return -1;
        }

        private bool listEqualsExact(List<string> list1, List<string> list2)
        {// quick loop to check equality of two string lists
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                    return false;
            }

            return true;
        }

        private bool listInListDesc(List<string> list1, List<string> list2)
        {// check if list1 exists in list2, descending order
            for (int i = 0; i < list1.Count; i++)
            {// compare exact indexes
                if (!list1[i].Equals(list2[i]))
                    return false; // break on the first negative
            }

            return true;
        }

        private bool arrayContainsDuplicate(string[] inputArray)
        {// compare the strings in the array to see if duplicates exist
            int count = 0;
            for (int i = 0; i < inputArray.Length; i++)
            {
                count = 1; // we always have at least 1 occurance
                for (int j = 0; j < inputArray.Length; j++)
                {
                    if (i != j)
                    {// only count uniques that are exactly equal
                        if (inputArray[i].Equals(inputArray[j], StringComparison.OrdinalIgnoreCase))
                            count++;
                    }
                }

                if (count > 1)
                    return true; // break as soon as any duplicates are found
            }

            return false;
        }

        private void populateStationPanel(string input)
        {
            // we need to split our system and station names to match with the DB
            if (!String.IsNullOrEmpty(input))
            {
                string[] tokens = input.Split(new string[] { "/" }, StringSplitOptions.None);
                if (tokens != null && tokens.Length == 2)
                {// has both system and station
                    string t_system = tokens[0];
                    string t_station = tokens[1];
                    if (!String.IsNullOrEmpty(t_system) && !String.IsNullOrEmpty(t_station))
                        grabStationData(t_system, t_station);

                    if (outputStationDetails.Count > 0)
                    {
                        /*
                         * The contents of each station field can be: 'Y', 'N', '?', 'S', 'M', 'L', or an int64/long
                         */
                        int t_stnls = int.Parse(outputStationDetails[0]);
                        if (t_stnls > lsFromStarBox.Minimum && t_stnls < lsFromStarBox.Maximum)
                            lsFromStarBox.Text = outputStationDetails[0];

                        if (containsPadSizes(outputStationDetails[1]))
                        {
                            stn_padSizeBox.Text = outputStationDetails[1];
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[2]))
                        {
                            rearmCheckBox.CheckState = parseCheckState(outputStationDetails[2]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[3]))
                        {
                            refuelCheckBox.CheckState = parseCheckState(outputStationDetails[3]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[4]))
                        {
                            repairCheckBox.CheckState = parseCheckState(outputStationDetails[4]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[5]))
                        {
                            outfitCheckBox.CheckState = parseCheckState(outputStationDetails[5]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[6]))
                        {
                            shipyardCheckBox.CheckState = parseCheckState(outputStationDetails[6]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[7]))
                        {
                            marketCheckBox.CheckState = parseCheckState(outputStationDetails[7]);
                        }

                        if (!String.IsNullOrEmpty(outputStationDetails[8]))
                        {
                            blackMarketCheckBox.CheckState = parseCheckState(outputStationDetails[8]);
                        }
                    }

                    // shipvendor textbox
                    if (outputStationShips.Count > 0)
                    {
                        shipsSoldBox.DataSource = null;
                        shipsSoldBox.DataSource = outputStationShips;
                    }
                    else
                        shipsSoldBox.DataSource = null;
                }
            }
        }

        public static void playAlert()
        {// a simple method for playing a custom beep.wav or the default system Beep
            SoundPlayer player = new SoundPlayer();
            Assembly thisExecutable = System.Reflection.Assembly.GetExecutingAssembly();
            string localSound = localDir + "\\notify.wav";

            if (checkIfFileOpens(localSound))
            {
                player.SoundLocation = localSound;
                player.LoadAsync();
                player.Play();
            }
            else
            {
                player.Stream = Properties.Resources.notify;
                player.Play();
            }
        }

        public static void playUnknown()
        {// a simple method for playing a custom beep.wav or the default system Beep
            SoundPlayer player = new SoundPlayer();
            Assembly thisExecutable = System.Reflection.Assembly.GetExecutingAssembly();
            string localSound = localDir + "\\unknown.wav";

            if (checkIfFileOpens(localSound))
            {
                player.SoundLocation = localSound;
                player.LoadAsync();
                player.Play();
            }
            else
            {
                player.Stream = Properties.Resources.unknown;
                player.Play();
            }
        }

        private static string removeExtraWhitespace(string input)
        {
            // should work with most patterns, and favorite systems/stations
            String pattern = @"^\s*!|^\s*(?=\D)|(?!\S)[ ]+(?=\/)|(?<=\/)[ ]+(?=\S)|(?<=\S)[ ]+(?!\S)";
            String sanitized = Regex.Replace(input, pattern, "", RegexOptions.Compiled);
            return sanitized;
        }

        private string cleanShipVendorInput(string input)
        {// just a simple method to clean invalid data from the shipvendor input
            return Regex.Replace(input, @"\s\[.*\]|(?<=\w)\,\s*(?!\w|\s+\w)|(?<=\s)\s+", "");
        }

        private static string filterOutput(string input)
        {// make sure we remove junk from the run output
            string[] exceptions = new string[] { "Command line:", "NOTE:", "SORRY:", "####", "Entering" };
            string[] filteredLines = input
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .SkipWhile(x => exceptions.Any(x.Contains)).ToArray();
            string result = string.Join(Environment.NewLine, filteredLines);
            return result;
        }

        private void writeSavedPage(string text, string file)
        {// this takes text as an input, filters it, and outputs to a file
            string filteredOutput = filterOutput(text);
            if (!String.IsNullOrEmpty(filteredOutput))
            {
                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter stream = new StreamWriter(fs))
                {
                    stream.Write(filteredOutput);
                }
            }
        }

        private void stackCircularBuffer(string input)
        {// here we generate our buffer to be consumed
            if (input.Length > circularBuffer.Capacity)
            {// if the input is bigger than our buffer size, toss what doesn't fit
                circularBuffer = new StringBuilder(circularBufferSize);
                circularBuffer.Append(input, input.Length - circularBuffer.Capacity, circularBuffer.Capacity);
            }
            else if (input.Length + circularBuffer.Length > circularBuffer.Capacity)
            {// in case we can append, but that would put us OOB
                circularBuffer.Remove(0, circularBuffer.Length + input.Length - circularBufferSize);
                circularBuffer.Append(input);
            }
            else
                circularBuffer.Append(input);

            readCircularBuffer();
        }

        private void readCircularBuffer()
        {// here we consume our buffer
            if (circularBuffer.Length > 0 && circularBuffer.Length <= circularBuffer.Capacity)
            {// if our buffer is full, display it
                this.td_outputBox.Invoke(new Action(() =>
                {
                    this.td_outputBox.Text = circularBuffer.ToString();
                }));
            }
            else
            {// if the buffer overflows, wipe and return empty
                circularBuffer = new StringBuilder(circularBufferSize);
            }
        }

        private string currentTimestamp()
        {
            return DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
        }

        private bool timestampIsNewer(string inputStamp1, string inputStamp2)
        {// true if timeStamp1 newer than timeStamp2, false if equal or older
            DateTime parsedStamp1, parsedStamp2;
            if (DateTime.TryParseExact(inputStamp1, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStamp1) && DateTime.TryParseExact(inputStamp2, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStamp2))
            {
                if (parsedStamp1.CompareTo(parsedStamp2) > 0)
                    return true;
                else
                    return false;
            }
            else
                throw new ArgumentException("Unable to parse column timestamps for comparison");
        }

        private string generateRecentTimestamp(string inputStamp)
        {// we take an input timestamp string, and try to generate a new timestamp from it by removing a second
            DateTime parsedStamp = new DateTime(), outputStamp = new DateTime();
            string format = "yy-MM-dd HH:mm:ss";

            if (DateTime.TryParseExact(inputStamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedStamp))
            {
                outputStamp = parsedStamp.AddSeconds(-1);
            }
            else
                outputStamp = DateTime.Now; // fail, but don't explode

            return outputStamp.ToString(format);
        }

        public static string Encrypt(string input)
        {// return a base64 string version of our encoded input
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(MachineKey.Protect(inputBytes));
        }

        public static string Decrypt(string input)
        {// return a normalized string version of our decoded input
            var inputBytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(MachineKey.Unprotect(inputBytes));
        }
        #endregion
    }
}