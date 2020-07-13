using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using DotNetZipFile = System.IO.Compression.ZipFile;

namespace XMLrepair
{
    public partial class XMLrepair : Form
    {
        private string FileName; // Public names to reduce # of varaibles and reduce code
        private string directoryName;
        private string Named;
        private string ext;
        private string errLineNum;
        private string repaired;
        private bool broken = false;
        private string xapPath; // Location of XAP file
        private string xapName;
        private string xapDirectory;
        private bool isXAP = false;
        private bool didFail = false;
        private bool didFail2 = false;
        private string xmlWriteErrors;
        private Cursor cur = Cursors.Default; //Set wait cursor to indicate application is working
        private List<char> charsToSubstitute = new List<char>
            {
               (char)0x0, //Null : Ctrl-@
			   (char)0x1, //Start of heading : Ctrl-A
			   (char)0x2, //Start of text : Ctrl-B
			   (char)0x3, //Break/end of text : Ctrl-C
			   (char)0x4, //End of transmission : Ctrl-D
			   (char)0x5, //Enquiry : Ctrl-E
			   (char)0x6, //Positive acknowledgment : Ctrl-F 
			   (char)0x7, //Bell : Ctrl-G
			   (char)0x8, //Backspace : Ctrl-H
	    	//  (char)0x9, //Horizontal tab : Ctrl-I
		   //  (char)0x0A, //Line feed : Ctrl-J
			   (char)0x0B, //Vertical tab : Ctrl-K
			   (char)0x0C, //Form feed : Ctrl-L
			// (char)0x0D, //Carriage return (Equivalent to the Enter or Return key) : Ctrl-M
			   (char)0x0E, //Shift out : Ctrl-N 
			   (char)0x0F, //Shift in/XON (resume output) : Ctrl-O 
			   (char)0x10, //Data link escape : Ctrl-P
			   (char)0x11, //Device control character 1 : Ctrl-Q
			   (char)0x12, //Device control character 2 : Ctrl-R
			   (char)0x13, //Device control character 3 : Ctrl-S
			   (char)0x14, //Device control character 4 : Ctrl-T
			   (char)0x15, //Negative acknowledgment : Ctrl-U
			   (char)0x16, //Synchronous idle : Ctrl-V
			   (char)0x17, //End of transmission block : Ctrl-W
			   (char)0x18, //Cancel : Ctrl-X 
			   (char)0x19, //End of medium : Ctrl-Y
			   (char)0x1A, //Substitute/end of file : Ctrl-Z
			   (char)0x1B, //Escape : Ctrl-[
			   (char)0x1C, //File separator : Ctrl-\
			   (char)0x1D, //Group separator : Ctrl-]
			   (char)0x1E, //Record separator : Ctrl-^
			   (char)0x1F, //Unit separator : Ctrl-_ 
               (char)0x7F //Delete 
            };

        public XMLrepair()
        {
            InitializeComponent();
        }

        private void XMLrepair_Load(object sender, EventArgs e)
        {

        }

        private void OpenXML_FileOk(object sender, CancelEventArgs e)
        {

        }

        public void GetXML_Click(object sender, EventArgs e) //when clicking browse
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog(); //start a new OpenfileDialog
            openFileDialog1.InitialDirectory = @"C:\"; //Set initial path to look for an XML
            openFileDialog1.Title = "Select a File to Repair";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.DefaultExt = "XML";
            openFileDialog1.Filter = "XML files (*.xml)|*.xml|idsk files (*.idsk)|*.idsk|XAP Files (*.xap)|*.xap|TOTAL Data Container (*.tdcx)|*.tdcx|Zip Files (*.zip)|*.zip|All Supported File Types (*.xml; *.idsk; *.xap; *.tdcx; *.zip)|*.xml;*.idsk;*.xap;*.tdcx;*.zip";
            openFileDialog1.FilterIndex = 6; //By default select the sisth option "All Support File Types"
            openFileDialog1.RestoreDirectory = true; //Use previoulsy selected path if different from default
            openFileDialog1.ReadOnlyChecked = true;
            openFileDialog1.ShowReadOnly = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                GetInformation(openFileDialog1.FileName); //verify selected file is an XML and get data
            }
            else
                return;
        }

        private void GetInformation(string file)
        {
            Cursor cur = Cursors.WaitCursor; //Set wait cursor to indicate application is working
            Cursor = cur;
            FileName = file; //loads file to global varaible
            //MessageBox.Show("Cleaned FileName length: " + FileName.Length + "\nOriginal file Length: "+ file.Length);

            if (Path.GetExtension(FileName).ToLower() == ".xml" || Path.GetExtension(FileName).ToLower() == ".idsk" || Path.GetExtension(FileName).ToLower() == ".tdcx")//verify we are checking an XML
            {
                isXAP = false;
                XMLPath.Text = FileName; //if file is XML, load path into textbox
                directoryName = Path.GetDirectoryName(FileName); // get path to file
                Named = Path.GetFileNameWithoutExtension(FileName); //get file name of file
                ext = Path.GetExtension(FileName).ToLower(); //Get the file extension XML or idsk
            }
            else if (Path.GetExtension(FileName).ToLower() == ".xap" || Path.GetExtension(FileName).ToLower() == ".zip") //If XAP or zip
            {
                XMLPath.Text = FileName;
                isXAP = true;
                xapPath = FileName;
                xapDirectory = Path.GetDirectoryName(FileName);
                xapName = Path.GetFileNameWithoutExtension(FileName);
                directoryName = Path.GetDirectoryName(FileName); // get path to file
                Named = Path.GetFileNameWithoutExtension(FileName); //get file name of file
                Application.DoEvents(); // Refresh gui

                try //Clear the temp directory we are using to avoid conflicts
                {
                    foreach (string files in Directory.GetFiles(@"C:\Temp\XAP"))
                    {
                        File.Delete(files);
                    }

                    foreach (string subDirectory in Directory.GetDirectories(@"C:\Temp\XAP"))
                    {
                        Directory.Delete(subDirectory, true);
                    }
                }
                catch (Exception)
                {
                    // Do nothing, Folder was already deleted or does no exist yet
                }

                try
                {

                    //https://stackoverflow.com/questions/52431224/how-to-detect-crc-errs-in-an-file-when-extracting-from-a-zip
                    Extract45Framework(FileName, @"C:\Temp\XAP\" + xapName);
                }
                catch (Exception ex) //if no file is selected, display message 
                {
                    string ERROR = ex.ToString(); //get error as string
                    int Length;
                    if (ERROR.Length < 600)
                        Length = ERROR.Length;
                    else
                        Length = 600;
                    ERROR = ERROR.Substring(0, Length); //only display up to the first 600 characters
                    MessageBox.Show("Errors were encountered unzipping the " + ext + " file, not all of the data could be extracted. \n\nPlease check the file for CRC or zip errors. \n\nThis utility attempt to repair the " + ext + " file but will be missing any data that could not be extracted from the original " + ext + "\n\nERROR:\n" + ERROR,
                     "Warning",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
                    Cursor = Cursors.Default;
                }

            }
            else //otherwise throw an error that only XML files are supported
            {
                MessageBox.Show("The file '" + Path.GetFileNameWithoutExtension(FileName) + "' cannot be checked.\n" + Path.GetExtension(FileName).ToLower() + " is not a valid File format.\nPlease select an xml, idsk, xap, tdcx or zip to repair and try again.",
                "Error: Invalid File type",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                return;
            }
            Cursor = Cursors.Default;
        }

        private void FixMXL_Click(object sender, EventArgs e) //when clicking Fix
        {
            if (XMLPath.Text == "" | XMLPath.Text == null) //if no file selected
            {
                MessageBox.Show("No file was selected, please select a file to check and try again.",
               "Error: sorry",
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
                return;
            }
            else if (isXAP) // If an XAP (or zip) is selected
            {
                try
                {
                    var files = Directory.GetFiles(@"C:\Temp\XAP\" + xapName, "*.xml")
                        .Concat(Directory.GetFiles(@"C:\Temp\XAP\" + xapName, "*.tdcx"))
                        .Concat(Directory.GetFiles(@"C:\Temp\XAP\" + xapName, "*.idsk")).ToArray(); //Files were can check

                if (files.Length == 0) //If we find no xml files, not a valid XAP
                {
                    MessageBox.Show("The file selected does not contain any file formats this tool can repair, please verify the selected file does not contain the data inside of a subfolder and contains at least one xml and try again.",
               "Error: sorry",
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
                    return;
                }

                foreach (string file in files)
                {
                    Cursor = Cursors.WaitCursor;
                    Named = Path.GetFileNameWithoutExtension(file); //get file name of file
                    ext = Path.GetExtension(file);
                    didFail = false; //Reset didFail for next iteration
                    xmlWriteErrors = "";
                    RepairFile(file, @"C:\Temp\XAP\" + xapName, xapName + "_"); //RepairFile(File, Write directory)
                    if (didFail == false)
                    {
                        File.Delete(file);
                        Application.DoEvents();
                        File.Move(@"C:\Temp\XAP\" + xapName + "\\" + "Repaired " + Named + ext, file);
                    }
                    else
                        File.Delete(@"C:\Temp\XAP\" + xapName + "\\" + "Repaired " + Named + "_bad" + ext);
                }   // end loop
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("The file '" + FileName + "' cannot be checked.\n\n" + "The zip structure is damaged or is password protected.",
                        "Error: Invalid Zip Structure",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Cursor = Cursors.Default;
                    return;
                }

                Cursor = Cursors.WaitCursor;

                if (File.Exists(xapDirectory + "\\Repaired " + xapName + Path.GetExtension(FileName).ToLower()))
                {
                    File.Delete(xapDirectory + "\\Repaired " + xapName + Path.GetExtension(FileName).ToLower());
                }
                Cursor = Cursors.WaitCursor;
                ZipFile.CreateFromDirectory(@"C:\Temp\XAP\" + xapName, xapDirectory + "\\Repaired " + xapName + Path.GetExtension(FileName).ToLower()); //recreate XAP
                XMLPath.Text = null;

                if (didFail2 == false)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("The file '" + Path.GetFileNameWithoutExtension(FileName) + ".xap' has been repaired and has been saved as:\n\n'" + xapDirectory + "\\Repaired " + xapName + ".XAP'\n\nLogs for each of the files check are located in:\n" + xapDirectory,
                "XAP Repaired",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
                }
                else
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("The file '" + Path.GetFileNameWithoutExtension(FileName) + ".xap' still contains error and has been saved as:\n\n'" + xapDirectory + "\\Repaired " + xapName + ".XAP'\n\nLogs for each of the files check are located in:\n" + xapDirectory,
                "XAP Repaired",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
                }


                return;
            }
            else //if xml/idsk/tdxc selected
            {
                RepairFile(FileName, directoryName); //If plain'ol XML, tdxc or idsk
            }
            Cursor = Cursors.Default;
            return;
        }

        private void RepairFile(string file, string location, string XAPName = "")
        {
            // Cursor info from https://www.c-sharpcorner.com/UploadFile/mahesh/cursors-in-C-Sharp/
            Cursor cur = Cursors.WaitCursor; //Set wait cursor to indicate application is working
            Cursor = cur;
            errLineNum = null;
            broken = false;
            repaired = null;
            StringBuilder results = new StringBuilder();
            try
            {
                string content = File.ReadAllText(file); //try to load selected file as XML
                string result;
                /* Parse string content one line at a time
                 * run through CleanInvalidXmlChars(content) and mark line numbers
                 * then recombine as a single file to log errors and line numbers
                 */
                using (StringReader reader = new StringReader(content))
                {
                    // Loop over the lines in the string.
                    int count = 0;
                    int PDF = 0;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        count++;

                        if (line.Trim().StartsWith("<DOCUMENT>"))
                        {
                            PDF = 1;
                        }

                        if (line.Trim().EndsWith("</DOCUMENT>"))
                        {
                            PDF = 0;
                        }

                        if (PDF == 0)
                        {
                            result = CleanInvalidXmlChars(line); //Get the cleaned file one line at a time
                            results.Append(result);

                            if (line.Length != result.Length)
                            {
                                broken = true;
                                errLineNum += "LINE: " + count.ToString() + Environment.NewLine + "BEFORE: " + line.Trim() + Environment.NewLine + Environment.NewLine + Environment.NewLine + "AFTER:  " + result.Trim() + Environment.NewLine + Environment.NewLine;
                            }
                        }
                        else
                        {
                            results.AppendLine(line);
                        }
                    }
                }

                repaired = results.ToString();
                if (repaired.Length > 26) //if data contains no valid XML header, there is not XML data
                {
                    if (broken)
                    {
                        //try catch for XML errors
                        WriteToFile(repaired, location, "Repaired " + Named, ext); //write the cleaned file back out
                        File.WriteAllText(directoryName + "\\Repaired " + XAPName + Named + ext + ".log", "Errors were found on the following line(s): " + Environment.NewLine + Environment.NewLine + errLineNum.Replace("\t", ""));
                        MessageBox.Show("The XML File: '" + Named + ext + "' has been repaired.",
                "XML Repaired");
                        Cursor = Cursors.WaitCursor;
                    }
                    else
                    {
                        //Try catch for xml error
                        WriteToFile(repaired, location, "Repaired " + Named, ext); //write the cleaned file back out
                        if (didFail == false)
                        {
                            File.WriteAllText(directoryName + "\\Repaired " + XAPName + Named + ext + ".log", "No errors were detected in '" + Named + "' by this utility; However, the file was re-encoded in UTF-8 which can resolve issues this utility cannot currently detect." + Environment.NewLine + " Please check the repaired file to see if the issue persists.");
                            MessageBox.Show("No bad information was detected in '" + Named + ext + "' " + Environment.NewLine + Environment.NewLine + "The file has been resaved in the UTF-8 format to resolve issues not currently detected by this utility.", "Information",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Asterisk);
                            Cursor = Cursors.WaitCursor;
                        }
                        else
                        {
                            File.WriteAllText(directoryName + "\\Repaired " + XAPName + Named + ext + ".log", "Errors were detected in '" + Named + "' by this utility which could not be resolved." + Environment.NewLine + " Please check the file to see if the issue can be corrected manually." + Environment.NewLine + Environment.NewLine + xmlWriteErrors);
                            MessageBox.Show("Bad information was detected in '" + Named + ext + "' " + Environment.NewLine + Environment.NewLine + "The file was not fixed.", "Error",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Error);
                            Cursor = Cursors.WaitCursor;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("The selcted file cannot be repaired, no recoverable information was detected.\n\nThe recoverd file length would be " + repaired.Length + " characters in length.\nA valid XML file would be at least 26 Characters in length.", "Error",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                    didFail = true;
                    Cursor = Cursors.Default;
                    return;
                }

                repaired = null; //free up memory
                content = null; //free up memory
            }

            catch (Exception ex) //if no file is selected, display message 
            {
                string ERROR = ex.ToString(); //get error as string
                int Length;
                if (ERROR.Length < 600)
                    Length = ERROR.Length;
                else
                    Length = 600;
                ERROR = ERROR.Substring(0, Length); //only display up to the first 600 characters
                MessageBox.Show("Please select an XML file to repair and try again.\n\nERROR: " + ERROR,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                return;
            }

            Cursor = Cursors.Default; //Return Cursor to normal
            return;
        }

        private string CleanInvalidXmlChars(string fileText)
        {
            foreach (char c in charsToSubstitute)
            {
                fileText = fileText.Replace(Convert.ToString(c), string.Empty);
            }

            StringBuilder b = new StringBuilder(fileText);
            b.Replace("<(null)", "<BAD");
            b.Replace("(null)>", "BAD>");

            Regex nullMatch = new Regex("<(.+?)_\\(NULL\\)(.+?)>");
            String result = nullMatch.Replace(b.ToString(), "<$1_BAD$2>"); //Remove (null) from Nodes
            result = result.Replace("(NULL)", "BAD");

            //force to convert UTF-8 standard will address this issue Invalid byte 1 of 1-byte UTF-8 sequence 
            return result;
        }

        public void WriteToFile(string contents, string path, string name, string ext)
        {
            try
            {
                XmlDocument xdoc = new XmlDocument(); //Create XML doc
                xdoc.LoadXml(contents); //Load cleaned data string into XML doc
                Directory.CreateDirectory(path); //Create path as needed
                xdoc.Save(@path + "\\" + name + ext); //Write out cleaned up XML as new file
            }
            catch (Exception ex) // IF it still fails
            {
                string ERROR = ex.ToString(); //get error as string
                int Length;  //Create variable to store laength
                if (ERROR.Length < 600) //if the error is less than 600 characters, 
                    Length = ERROR.Length; //display entire error
                else
                    Length = 600;
                ERROR = ERROR.Substring(0, Length); //only display up to the first 600 characters
                MessageBox.Show("The selected XML:\n '" + name.Substring(9) + ".xml' could not be repaired." + "\n\nERROR:\n " + ERROR,
              "Error repairing XML",
              MessageBoxButtons.OK,
              MessageBoxIcon.Error,
              MessageBoxDefaultButton.Button1,
              (MessageBoxOptions)0x40000);
                xmlWriteErrors = "The following error(s) were encountered while trying to process the file '" + name + "':" + Environment.NewLine + ex.ToString();
                if (isXAP == false)
                {
                    File.WriteAllText(@path + "\\" + name + "_bad.xml", contents); //write the bad XML data out as a string so we can see why it failed
                }

                didFail = true;
                didFail2 = true;

                contents = null; //free up memory
                return;
            }

            contents = null; //free up memory
        }

        private void XMLrepair_DragDrop(object sender, DragEventArgs e) //on drag and drop
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) //if a drag and drop
            {
                e.Effect = DragDropEffects.Move; //show the move symbol
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); //On drop, get a list of dropped files
                GetInformation(files[0]); //check the information for the first file to see if it is an xml
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void Extract45Framework(string zipFile, string extractPath)
        {
            ZipArchive zipArchive = DotNetZipFile.OpenRead(zipFile);
            string CRC = "The following file(s) could not be extracted and will be missing from the repaired archive:" + Environment.NewLine + Environment.NewLine;
            bool CRCerror = false;

            if (zipArchive.Entries != null && zipArchive.Entries.Count > 0)
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            // skip directory
                            continue;
                        }

                        string file = Path.Combine(extractPath, entry.FullName);
                        string path = Path.GetDirectoryName(file);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        if (File.Exists(file))
                        {
                            //Console.WriteLine("   - delete previous version...");
                            File.Delete(file);
                        }

                        entry.ExtractToFile(file);
                        long length = (new FileInfo(file)).Length;
                        if (entry.Length != length)
                        {
                            MessageBox.Show($"CRC error detected in '{entry.FullName}', the file could not be extracted!\nOnly {length} out of {entry.Length} bytes could be read.\n\nThe file will not be included in the repaired archive.");
                            File.Delete(file);
                            CRCerror = true;
                            CRC += entry.FullName + " - Only " + length +" out of "+entry.Length +" bytes could be read\n";
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }

            if (CRCerror)
            {
                File.WriteAllText(xapDirectory + "\\" + xapName + " (CRC errors).log", CRC);
            }
       
        }

        private void XMLrepair_DragEnter(object sender, DragEventArgs e) //on a drag enter
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move; //check for file and set move indicator
            else
                e.Effect = DragDropEffects.None;
        }
    }
}