using System.Collections.Generic;
using System.Linq;
using BOA.EntityGeneration.DbModel.Types;
using DotNetDatabaseAccessUtilities;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.DbModel.SqlServerDataAccess
{
    public class IndexInfoAccess
    {
        #region Public Properties
        public IDatabase Database { get; set; }
        #endregion

        #region Public Methods
        public IReadOnlyList<IndexInfo> GetIndexInformation(string schema, string tableName)
        {
            var items = new List<IndexInfo>();

            var sql = $@"

SELECT '[' + s.NAME + '].[' + o.NAME + ']' AS 'table_name'
    ,+ i.NAME AS 'index_name'
    ,LOWER(i.type_desc) + CASE 
        WHEN i.is_unique = 1
            THEN ', unique'
        ELSE ''
        END + CASE 
        WHEN i.is_primary_key = 1
            THEN ', primary key'
        ELSE ''
        END AS 'index_description'
    ,STUFF((
            SELECT ', [' + sc.NAME + ']' AS ""text()""
            FROM  syscolumns AS          sc
            INNER JOIN sys.index_columns AS ic ON ic.object_id = sc.id
            AND ic.column_id                                   = sc.colid
            WHERE sc.id                                        = so.object_id
            AND ic.index_id                                    = i1.indid
            AND ic.is_included_column                          = 0
            ORDER BY key_ordinal
                FOR XML PATH('')
                ), 1, 2, '') AS 'indexed_columns'
               ,STUFF((
                          SELECT ', [' + sc.NAME + ']' AS ""text()""
            FROM  syscolumns AS          sc
            INNER JOIN sys.index_columns AS ic ON ic.object_id = sc.id
            AND ic.column_id                                   = sc.colid
            WHERE sc.id                                        = so.object_id
            AND ic.index_id                                    = i1.indid
            AND ic.is_included_column                          = 1
            FOR XML PATH('')
                ), 1, 2, '') AS 'included_columns'
            FROM  sysindexes AS    i1
            INNER JOIN sys.indexes AS i ON i.object_id   = i1.id
            AND i.index_id                               = i1.indid
            INNER JOIN sysobjects  AS o  ON o.id         = i1.id
            INNER JOIN sys.objects AS so ON so.object_id = o.id
            AND   is_ms_shipped = 0
            INNER JOIN sys.schemas AS s ON s.schema_id = so.schema_id
            WHERE so.type                              = 'U'
            AND i1.indid < 255
            AND i1.STATUS & 64           = 0 --index with duplicates
                AND i1.STATUS & 8388608  = 0 --auto  created index
                AND i1.STATUS & 16777216 = 0 --stats no recompute
                AND i.type_desc <> 'heap'
            AND so.NAME <> 'sysdiagrams'
            AND o.NAME = '{tableName}'
            AND s.NAME = '{schema}'
            ORDER BY  table_name
                    , index_name;
";
            Database.CommandText = sql;

            var reader = Database.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new IndexInfo
                {
                    Name           = reader["index_name"].ToString(),
                    IsNonClustered = reader["index_description"].ToString().Contains("nonclustered"),
                    IsClustered    = reader["index_description"].ToString().Contains("clustered") && !reader["index_description"].ToString().Contains("nonclustered"),
                    IsUnique       = reader["index_description"].ToString().Contains("unique"),
                    IsPrimaryKey   = reader["index_description"].ToString().Contains("primary key"),
                    ColumnNames    = reader["indexed_columns"].ToString().SplitAndClear(",").Select(x => x.Remove("[").Remove("]").Trim()).ToList()
                });
            }

            reader.Close();

            return items;
        }
        #endregion
    }
}