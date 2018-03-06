using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougKlassen.Revit.Migration.Models
{
    /// <summary>
    /// A Revit family
    /// </summary>
    public class RevitFamily
    {
        /// <summary>
        /// The path of the directory of the .rfa file containing the family. The trailing backslash is included"
        /// </summary>
        public String SourceFilePath;

        /// <summary>
        /// The file name of the .rfa file containing the family
        /// </summary>
        public String SourceFileName;

        /// <summary>
        /// The complete path of the .rfa file containing the family
        /// </summary>
        public String SourceFileCompletePath
        {
            get
            {
                return SourceFilePath + SourceFileName;
            }
        }

        /// <summary>
        /// Construct a RevitFamily representing a specified .rfa file
        /// </summary>
        /// <param name="path">The path to the .rfa file containing the family</param>
        public RevitFamily(String path)
        {
            SourceFilePath = Path.GetDirectoryName(path);
            SourceFileName = Path.GetFileName(path);
        }
    }
}
