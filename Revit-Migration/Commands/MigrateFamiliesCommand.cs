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
        String workingDirectory;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            FolderBrowserDialog startingDirectoryDialog = new FolderBrowserDialog();
            startingDirectoryDialog.Description = "Select a directory containing families to be migrated";
            startingDirectoryDialog.ShowNewFolderButton = false;
            DialogResult result = startingDirectoryDialog.ShowDialog();

            switch (result)
            {
                //Directory selected by user
                case DialogResult.OK:
                    workingDirectory = startingDirectoryDialog.SelectedPath;
                    break;
                //Dialog cancelled
                case DialogResult.Cancel:
                    return Result.Cancelled;
                //Unexpected result
                default:
                    return Result.Failed;
                    break;
            }

            log.AppendLine("Family Migration: {0}", DateTime.Now.ToLongDateString());
            log.AppendLine("Directory selected: {0}", workingDirectory);

            String logFilePath = FileLocations.AddInDirectory + String.Format("{0}.log.txt", DateTime.Now.ToString("yyyyddMM.Hmm"));
            File.WriteAllText(logFilePath, log.Text);
            return Result.Succeeded;
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
