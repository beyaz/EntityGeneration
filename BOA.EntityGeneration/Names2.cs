using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetStringUtilities;

namespace BOA.EntityGeneration
{
    public static class Names2
    {
        #region Constants
        public const string INSERT_DATE      = nameof(INSERT_DATE);
        public const string INSERT_TOKEN_ID  = nameof(INSERT_TOKEN_ID);
        public const string INSERT_USER_ID   = nameof(INSERT_USER_ID);
        public const string RECORD_ID        = nameof(RECORD_ID);
        public const string ROW_GUID         = nameof(ROW_GUID);
        public const string UPDATE_DATE      = nameof(UPDATE_DATE);
        public const string UPDATE_TOKEN_ID  = nameof(UPDATE_TOKEN_ID);
        public const string UPDATE_USER_ID   = nameof(UPDATE_USER_ID);
        public const string UpdateHostIP     = nameof(UpdateHostIP);
        public const string UpdateHostName   = nameof(UpdateHostName);
        public const string UpdateSystemDate = nameof(UpdateSystemDate);

        public const string UpdateUserName = nameof(UpdateUserName);
        public const string VALID_FLAG     = nameof(VALID_FLAG);
        #endregion

        #region Public Methods
        public static string AsMethodParameter(this string columnName)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            columnName = ToContractName(columnName);

            //return columnName[0].ToString().ToLowerTR() + columnName.Substring(1);
            var firstChar = columnName[0].ToString().ToLowerTR();
            if (firstChar == "ı")
            {
                firstChar = "i";
            }

            return firstChar + columnName.Substring(1);
        }

        public static string ToContractName(this string dbObjectName)
        {
            if (string.IsNullOrEmpty(dbObjectName))
            {
                return dbObjectName;
            }

            if (dbObjectName.Length == 1)
            {
                return dbObjectName.ToUpper();
            }

            var names = dbObjectName.SplitAndClear("_");

            if (names.Count == 1)
            {
                if (IsCamelCase(names[0]))
                {
                    if (names[0].Any(char.IsLower))
                    {
                        return names[0];
                    }
                }
            }

            return string.Join(string.Empty, names.Select(name => name.Substring(0, 1).ToUpper(new CultureInfo("EN-US")) + name.Substring(1).ToLowerInvariant()));
        }
        #endregion

        #region Methods
        static bool IsCamelCase(string value)
        {
            return Regex.IsMatch(value, "(^[a-z]|[A-Z0-9])[a-z]*");
        }
        #endregion
    }
}