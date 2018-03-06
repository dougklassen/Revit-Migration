using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DougKlassen.Revit.Migration.Models;

namespace DougKlassen.Revit.Migration.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MigrateFamiliesCommand : IExternalCommand
    {
        private MigrationLog log = MigrationLog.Instance;
        private String sourceDirectory, destinationDirectory;
        private List<RevitFamily> familiesToMigrate;

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

            familiesToMigrate = GetEligibleFamiles(sourceDirectory);
            log.AppendLine("{0} elligible families found", familiesToMigrate.Count());
            foreach (var family in familiesToMigrate)
            {
                log.AppendLine("  {0}", family.SourceFileName);
            }

            String logFilePath = FileLocations.AddInDirectory + String.Format("{0}.log.txt", DateTime.Now.ToString("yyyyddMM.Hmm"));
            File.WriteAllText(logFilePath, log.Text);
            return Result.Succeeded;
        }

        /// <summary>
        /// Return all familes elligible for upgrade in a specified directory. The directory is searched recursively
        /// </summary>
        /// <param name="sourceDirectory">The directory to search</param>
        /// <returns>A List of families found in the directory</returns>
        private List<RevitFamily> GetEligibleFamiles(String sourceDirectory)
        {
            List<RevitFamily> selectedFamiles = new List<RevitFamily>();

            List<String> filePaths = Directory.EnumerateFiles(sourceDirectory, "*.rfa", SearchOption.AllDirectories).ToList();
            foreach (var path in filePaths)
            {
                selectedFamiles.Add(new RevitFamily(path));
            }

            return selectedFamiles;
        }
    }

    public class MigrateFamiliesCommandAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            //set the command to always be available, including in a zero doc state
            return true;
        }
    }
}
