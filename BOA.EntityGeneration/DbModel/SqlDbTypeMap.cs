using System;
using System.Collections.Generic;
using System.Data;

// using static BOA.EntityGeneration.SqlDataType;

namespace BOA.EntityGeneration.DbModel
{
    public static class SqlDbTypeMap
    {
        #region Static Fields
        static readonly Dictionary<Type, SqlReaderMethods> SqlReaderMethodCache = new Dictionary<Type, SqlReaderMethods>
        {
            {typeof(string), SqlReaderMethods.GetStringValue},

            {typeof(decimal), SqlReaderMethods.GetDecimalValue},
            {typeof(decimal?), SqlReaderMethods.GetDecimalNullableValue},

            {typeof(bool), SqlReaderMethods.GetBooleanValue},
            {typeof(bool?), SqlReaderMethods.GetBooleanNullableValue},

            {typeof(DateTime), SqlReaderMethods.GetDateTimeValue},
            {typeof(DateTime?), SqlReaderMethods.GetDateTimeNullableValue},

            {typeof(long), SqlReaderMethods.GetInt64Value},
            {typeof(long?), SqlReaderMethods.GetInt64NullableValue},

            {typeof(Guid), SqlReaderMethods.GetGUIDValue},

            {typeof(int), SqlReaderMethods.GetInt32Value},
            {typeof(int?), SqlReaderMethods.GetInt32NullableValue},

            {typeof(short), SqlReaderMethods.GetInt16Value},
            {typeof(short?), SqlReaderMethods.GetInt16NullableValue},

            {typeof(byte), SqlReaderMethods.GetByteValue},
            {typeof(byte?), SqlReaderMethods.GetByteNullableValue},

            {typeof(double), SqlReaderMethods.GetDoubleValue},
            {typeof(double?), SqlReaderMethods.GetDoubleNullableValue},

            {typeof(float), SqlReaderMethods.GetSingleValue},
            {typeof(float?), SqlReaderMethods.GetSingleNullableValue},

            {typeof(byte[]), SqlReaderMethods.GetBinaryValue}
        };
        #endregion

        #region Public Methods
        public static string GetDotNetType(string dataType, bool isNullable)
        {
            if (SqlDbType.TinyInt.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetByte);
                }

                return DotNetTypeName.DotNetByte;
            }

            if (SqlDbType.SmallInt.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetInt16);
                }

                return DotNetTypeName.DotNetInt16;
            }

            if (SqlDbType.Int.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetInt32);
                }

                return DotNetTypeName.DotNetInt32;
            }

            if (SqlDbType.BigInt.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetInt64);
                }

                return DotNetTypeName.DotNetInt64;
            }

            if (SqlDbType.DateTime.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDateTime);
                }

                return DotNetTypeName.DotNetDateTime;
            }

            if (SqlDbType.Date.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDateTime);
                }

                return DotNetTypeName.DotNetDateTime;
            }

            if (SqlDbType.SmallDateTime.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDateTime);
                }

                return DotNetTypeName.DotNetDateTime;
            }

            if (SqlDbType.Bit.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetBool);
                }

                return DotNetTypeName.DotNetBool;
            }

            if (SqlDbType.UniqueIdentifier.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetGuid);
                }

                return DotNetTypeName.DotNetGuid;
            }

            if (SqlDbType.Float.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetSingle);
                }

                return DotNetTypeName.DotNetSingle;
            }

            if (SqlDbType.SmallMoney.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDecimal);
                }

                return DotNetTypeName.DotNetDecimal;
            }

            if (SqlDbType.Real.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDouble);
                }

                return DotNetTypeName.DotNetDouble;
            }

            if (SqlDbType.NText.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetStringName);
                }

                return DotNetTypeName.DotNetStringName;
            }

            if (SqlDbType.VarBinary.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetByteArray);
                }

                return DotNetTypeName.DotNetByteArray;
            }

            if (SqlDbType.Timestamp.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetByteArray);
                }

                return DotNetTypeName.DotNetByteArray;
            }

            if (SqlDbType.Binary.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetByteArray);
                }

                return DotNetTypeName.DotNetByteArray;
            }

            if (SqlDbType.Image.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetObject);
                }

                return DotNetTypeName.DotNetObject;
            }

            if (SqlDbType.Text.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetStringName);
                }

                return DotNetTypeName.DotNetStringName;
            }

            if (SqlDbType.Time.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetTimeSpan);
                }

                return DotNetTypeName.DotNetTimeSpan;
            }

            if (SqlDbType.Xml.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetStringName);
                }

                return DotNetTypeName.DotNetStringName;
            }

            if (SqlDbType.Decimal.ToString().Equals(dataType, StringComparison.OrdinalIgnoreCase))
            {
                if (isNullable)
                {
                    return DotNetTypeName.GetDotNetNullableType(DotNetTypeName.DotNetDecimal);
                }

                return DotNetTypeName.DotNetDecimal;
            }

            if (dataType.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("CHAR(", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("CHAR", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("NCHAR(", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("NVARCHAR(", StringComparison.OrdinalIgnoreCase) |
                dataType.StartsWith("NVARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                return DotNetTypeName.DotNetStringName;
            }

            if (dataType.StartsWith("NUMERIC", StringComparison.OrdinalIgnoreCase) || dataType.StartsWith("MONEY", StringComparison.OrdinalIgnoreCase))
            {
                return isNullable ? DotNetTypeName.DotNetNullableDecimal : DotNetTypeName.DotNetDecimal;
            }

            throw new ArgumentException(dataType);
        }

        public static SqlDbType GetSqlDbType(string dataType)
        {
            try
            {
                return (SqlDbType) Enum.Parse(typeof(SqlDbType), dataType, true);
            }
            catch (Exception)
            {
                // ignored
            }

            if (dataType.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.VarChar;
            }

            if (dataType.StartsWith("CHAR(", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Char;
            }

            if (dataType.StartsWith("NVARCHAR", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.NVarChar;
            }

            if (dataType.StartsWith("NCHAR", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.NChar;
            }

            if (dataType.StartsWith("BIT", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Bit;
            }

            if (dataType.StartsWith("NUMERIC", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Decimal;
            }

            if (dataType.StartsWith("MONEY", StringComparison.OrdinalIgnoreCase))
            {
                return SqlDbType.Money;
            }

            throw new ArgumentException(dataType);
        }

        public static SqlReaderMethods GetSqlReaderMethod(string dataType, bool isNullable)
        {
            if (dataType.IsEqual(SqlDbType.Timestamp))
            {
                return SqlReaderMethods.GetTimeStampValue;
            }

            if (dataType.IsEqual(SqlDbType.VarBinary) ||
                dataType.IsEqual(SqlDbType.Binary) ||
                dataType.IsEqual(SqlDbType.Image))
            {
                return SqlReaderMethods.GetBinaryValue;
            }

            if (dataType.IsEqual(SqlDbType.Float))
            {
                return isNullable ? SqlReaderMethods.GetSingleNullableValue : SqlReaderMethods.GetSingleValue;
            }

            if (dataType.IsEqual(SqlDbType.Real))
            {
                return isNullable ? SqlReaderMethods.GetDoubleNullableValue : SqlReaderMethods.GetDoubleValue;
            }

            if (dataType.IsEqual(SqlDbType.TinyInt))
            {
                return isNullable ? SqlReaderMethods.GetByteNullableValue : SqlReaderMethods.GetByteValue;
            }

            if (dataType.IsEqual(SqlDbType.SmallInt) ||
                dataType.Equals(typeof(short).Name, StringComparison.OrdinalIgnoreCase))
            {
                return isNullable ? SqlReaderMethods.GetInt16NullableValue : SqlReaderMethods.GetInt16Value;
            }

            if (dataType.IsEqual(SqlDbType.Int))
            {
                return isNullable ? SqlReaderMethods.GetInt32NullableValue : SqlReaderMethods.GetInt32Value;
            }

            if (dataType.IsEqual(SqlDbType.Time))
            {
                return SqlReaderMethods.GetTimeSpanValue;
            }

            if (dataType.IsEqual(SqlDbType.UniqueIdentifier))
            {
                return SqlReaderMethods.GetGUIDValue;
            }

            if (dataType.IsEqual(SqlDbType.BigInt) ||
                dataType.Equals("LONG", StringComparison.OrdinalIgnoreCase))
            {
                return isNullable ? SqlReaderMethods.GetInt64NullableValue : SqlReaderMethods.GetInt64Value;
            }

            if (dataType.IsEqual(SqlDbType.DateTime) ||
                dataType.IsEqual(SqlDbType.SmallDateTime) ||
                dataType.IsEqual(SqlDbType.Date))
            {
                return isNullable ? SqlReaderMethods.GetDateTimeNullableValue : SqlReaderMethods.GetDateTimeValue;
            }

            if (dataType.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("CHAR(", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("NCHAR", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("NVARCHAR", StringComparison.OrdinalIgnoreCase) ||
                dataType.IsEqual(SqlDbType.Text) ||
                dataType.IsEqual(SqlDbType.NText) ||
                dataType.IsEqual(SqlDbType.Xml))
            {
                return SqlReaderMethods.GetStringValue;
            }

            if (dataType.Equals("char", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("STRING", StringComparison.OrdinalIgnoreCase))
            {
                return SqlReaderMethods.GetStringValue;
            }

            if (dataType.StartsWith(SqlDbType.Bit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return isNullable ? SqlReaderMethods.GetBooleanNullableValue : SqlReaderMethods.GetBooleanValue;
            }

            if (dataType.StartsWith("NUMERIC", StringComparison.OrdinalIgnoreCase) ||
                dataType.StartsWith("MONEY", StringComparison.OrdinalIgnoreCase) ||
                dataType.IsEqual(SqlDbType.SmallMoney) ||
                dataType.IsEqual(SqlDbType.Decimal))
            {
                return isNullable ? SqlReaderMethods.GetDecimalNullableValue : SqlReaderMethods.GetDecimalValue;
            }

            throw new ArgumentException(dataType);
        }
        #endregion
    }
}