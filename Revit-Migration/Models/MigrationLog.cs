using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougKlassen.Revit.Migration.Models
{
    /// <summary>
    /// A file based log for recording session info
    /// </summary>
    public sealed class MigrationLog
    {
        /// <summary>
        /// The private backing member for the singleton
        /// </summary>
        private static readonly MigrationLog instance = new MigrationLog();

        /// <summary>
        /// The content of the log
        /// </summary>
        private StringBuilder logText;

        /// <summary>
        /// The public instance of the singleton
        /// </summary>
        public static MigrationLog Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// The private constructor
        /// </summary>
        private MigrationLog()
        {
            logText = new StringBuilder(String.Empty);
        }

        /// <summary>
        /// Expose the contents of the log as a String
        /// </summary>
        public String Text
        {
            get
            {
                return logText.ToString();
            }
            set
            {
                logText = new StringBuilder(value);
            }
        }

        /// <summary>
        /// Add a line of text to the log
        /// </summary>
        /// <param name="text">The text to be added</param>
        public void AppendLine(String text)
        {
            logText.AppendLine(text);
        }

        /// <summary>
        /// Add a line of text to the log
        /// </summary>
        /// <param name="text">The text to be added</param>
        /// <param name="args">Variables to be written into the text string</param>
        public void AppendLine(String text, params object[] args)
        {
            logText.AppendLine(String.Format(text, args.Select(o => o.ToString()).ToArray()));
        }

        /// <summary>
        /// Record an exception to the log
        /// </summary>
        /// <param name="exception">The exception to be recorded</param>
        public void LogException(Exception exception)
        {
            AppendLine("!!exception");
            AppendLine("--{0} {1}", DateTime.Now, exception.GetType());
            AppendLine("--{0}", exception.Message);
            AppendLine("--{0}", exception.StackTrace);
        }
    }
}
