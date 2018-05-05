using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

using DougKlassen.Revit.Migration.Models;

namespace DougKlassen.Revit.Migration.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MigrateFamiliesCommand : IExternalCommand
    {
        private MigrationLog log = MigrationLog.Instance;
        private String logFilePath = FileLocations.AddInDirectory + String.Format("{0}.log.txt", DateTime.Now.ToString("yyyyMMdd.Hmm"));
        private String sourceDirectory, destinationDirectory;
        /// <summary>
        /// Full paths of each family to migrate
        /// </summary>
        private List<String> familiesToMigrate;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            log.AppendLine("Family Migration: {0}", DateTime.Now.ToLongDateString());

            //Prompt the user for a directory containing files to be migrated
            DialogResult result;
            FolderBrowserDialog sourceDirectoryDialog = new FolderBrowserDialog();
            sourceDirectoryDialog.Description = "Select a directory containing families to be migrated";
            sourceDirectoryDialog.ShowNewFolderButton = false;
            result = sourceDirectoryDialog.ShowDialog();
            switch (result)
            {
                //Directory selected by user
                case DialogResult.OK:
                    sourceDirectory = sourceDirectoryDialog.SelectedPath;
                    break;
                //Dialog cancelled
                case DialogResult.Cancel:
                    return Result.Cancelled;
                //Unexpected result
                default:
                    return Result.Failed;
            }
            log.AppendLine("Source directory selected: {0}", sourceDirectory);

            //Prompt the user for a directory to send migrated files to
            FolderBrowserDialog destinationDirectoryDialog = new FolderBrowserDialog();
            destinationDirectoryDialog.Description = "Select an output directory";
            destinationDirectoryDialog.ShowNewFolderButton = true;
            result = destinationDirectoryDialog.ShowDialog();
            switch (result)
            {
                //Directory selected by user
                case DialogResult.OK:
                    destinationDirectory = destinationDirectoryDialog.SelectedPath;
                    break;
                //Dialog cancelled
                case DialogResult.Cancel:
                    return Result.Cancelled;
                //Unexpected result
                default:
                    return Result.Failed;
            }
            log.AppendLine("Output directory selected: {0}", destinationDirectory);

            //Open each family and save in the current version of revit
            var app = commandData.Application.Application;
            var openOpts = new OpenOptions();
            openOpts.Audit = true;
            familiesToMigrate = GetEligibleFamiles(sourceDirectory);
            log.AppendLine("{0} elligible families found", familiesToMigrate.Count());
            foreach (var sourceFilePath in familiesToMigrate)
            {
                try
                {
                    Document dbDoc;
                    ModelPath sourceModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(sourceFilePath);
                    log.AppendLine("\n\n Processing {0}", sourceFilePath);
                    dbDoc = app.OpenDocumentFile(sourceModelPath, openOpts);
                    var destFilePath = GetDestinationFilePath(sourceFilePath, destinationDirectory);
                    log.AppendLine(" Destination File Path: {0}", destFilePath);
                    ModelPath destinationModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(destFilePath);
                    try
                    {
                        var saveOpts = new SaveAsOptions();
                        saveOpts.Compact = true;
                        saveOpts.MaximumBackups = 2;
                        saveOpts.OverwriteExistingFile = true;
                        //dbDoc.SaveAs(destinationModelPath, saveOpts);
                        dbDoc.Close(false);
                        log.AppendLine(" Saved migrated family: {0}", destFilePath);
                    }
                    catch (Exception e)
                    {
                        log.AppendLine(" ! Could not save {0} to {1}", sourceFilePath, destinationModelPath);
                        log.LogException(e);
                    }
                }
                catch (Exception e)
                {
                    log.AppendLine(" ! Could not open {0}", sourceFilePath);
                    log.LogException(e);
                }
            }

            File.WriteAllText(logFilePath, log.Text);
            return Result.Succeeded;
        }

        /// <summary>
        /// Return all familes elligible for upgrade in a specified directory. The directory is searched recursively
        /// </summary>
        /// <param name="sourceDirectory">The directory to search</param>
        /// <returns>A List of families found in the directory</returns>
        private List<String> GetEligibleFamiles(String sourceDirectory)
        {
            var selectedFamiles = new List<String>();
            Regex exclusionRegex = new Regex(@"\.[\d]{2,4}\.rfa", RegexOptions.IgnoreCase);            

            List<String> filePaths = Directory.EnumerateFiles(sourceDirectory, "*.rfa", SearchOption.AllDirectories).ToList();
            foreach (var path in filePaths)
            {
                String fileName = Path.GetFileName(path);
                if (!exclusionRegex.IsMatch(fileName))
                {
                    selectedFamiles.Add(path);
                }
            }

            return selectedFamiles;
        }

        /// <summary>
        /// Get the complete destination file path
        /// </summary>
        /// <param name="sourceFilePath">The path of the source file</param>
        /// <param name="destinationDirectoryPath">The path of the destination directory, with or without a trailing \</param>
        /// <returns>The complete path of the destination file</returns>
        private String GetDestinationFilePath(String sourceFilePath, String destinationDirectoryPath)
        {            
            if (!destinationDirectoryPath.EndsWith(@"\"))
            {
                destinationDirectoryPath += @"\";
            }

            Regex xlFileNameRegEx = new Regex(@"^\(XL\) ");
            Regex underscoreRegEx = new Regex(@"_");
            Regex dashSpaceRegex = new Regex(@" - ");

            String oldFileName = Path.GetFileName(sourceFilePath);
            String newFileName = oldFileName;
            newFileName = xlFileNameRegEx.Replace(newFileName, "b.");
            newFileName = underscoreRegEx.Replace(newFileName, "-");
            newFileName = dashSpaceRegex.Replace(newFileName, "-");

            String destinationFilePath = String.Empty;
            destinationFilePath = destinationDirectoryPath + newFileName;

            if (!newFileName.Equals(oldFileName))
            {
                log.AppendLine("Rename: {0} --> {1}",
                    oldFileName,
                    newFileName);
            }

            return destinationFilePath;
        }
    }

    /// <summary>
    /// Class used by Revit to determine when the command is active
    /// </summary>
    public class MigrateFamiliesCommandAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            //set the command to always be available, including in a zero doc state
            return true;
        }
    }
}
