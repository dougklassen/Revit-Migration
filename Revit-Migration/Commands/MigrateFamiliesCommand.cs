using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DougKlassen.Revit.Migration.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class MigrateFamiliesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            FolderBrowserDialog startingDirectoryDialog = new FolderBrowserDialog();
            startingDirectoryDialog.Description = "Select a directory containing families to be migrated";
            startingDirectoryDialog.ShowNewFolderButton = false;
            DialogResult result = startingDirectoryDialog.ShowDialog();

            switch (result)
            {
                case DialogResult.OK:
                    TaskDialog.Show("result", "OK");
                    break;
                case DialogResult.Cancel:
                    TaskDialog.Show("result", "Cancel");
                    break;
                default:
                    break;
            }

            //Command code goes here

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
