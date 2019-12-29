using System;
using System.IO;

namespace BOA.EntityGeneration.UI.Container
{
    class CheckInCommentAccess
    {
        #region Properties
        static string TempFile => Path.GetTempPath() + AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".txt");
        #endregion

        #region Public Methods
        public static string GetCheckInComment()
        {
            var comment = "";
            if (File.Exists(TempFile))
            {
                comment = File.ReadAllText(TempFile).Trim();
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                comment = "2235# - AutoCheckInByEntityGenerator";
            }

            return comment;
        }

        public static void SaveCheckInComment(string comment)
        {
            File.WriteAllText(TempFile, comment);
        }
        #endregion
    }
}