using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

using DougKlassen.Revit.Migration.Models;

namespace DougKlassen.Revit.Migration.Commands
{
    class MigrateFamiliesErrorHandler
    {
        public static void MigrateFamiliesFailureHandler(Object sender, FailuresProcessingEventArgs args)
        {
            MigrationLog log = MigrationLog.Instance;

            var fa = args.GetFailuresAccessor();
            var fhOpts = fa.GetFailureHandlingOptions();

            foreach (var fma in fa.GetFailureMessages())
            {
                if (fma.GetSeverity() == FailureSeverity.Warning)
                {
                    log.AppendLine("!!warning");
                    log.AppendLine(fma.GetDescriptionText());
                    args.SetProcessingResult(FailureProcessingResult.Continue);
                    fa.DeleteWarning(fma);
                }
                else if (fma.GetSeverity() == FailureSeverity.Error)
                {
                    log.AppendLine("!!error");
                    log.AppendLine(fma.GetDescriptionText());

                    fhOpts.SetClearAfterRollback(true);
                    fa.SetFailureHandlingOptions(fhOpts);
                    args.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                    fa.RollBackPendingTransaction();
                }
            }
        }
    }
}
