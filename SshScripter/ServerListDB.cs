using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace SshScripter
{
    public class ServerDB
    {
        public static DataTable ExecuteDB(string StrQuery)
        {



            SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=zzz.sqlite;Version=3;");
            m_dbConnection.Open();

            SQLiteCommand command = new SQLiteCommand(StrQuery, m_dbConnection);

            command.CommandText = StrQuery;

            using (var reader = command.ExecuteReader())
            {
                var dtSchema = reader.GetSchemaTable();
                var listCols = new List<DataColumn>();
                var rawData = new DataTable();
                var rowDto = new RowDto();
                var ArrayData = new ArrayList();
                var pageData = rawData.AsEnumerable();

                if (dtSchema != null)
                {
                    foreach (DataRow drow in dtSchema.Rows)
                    {
                        var columnName = Convert.ToString(drow["ColumnName"]);
                        var column = new DataColumn(columnName, (Type)drow["DataType"]);
                        column.Unique = false;
                        column.AllowDBNull = true;
                        column.AutoIncrement = false;

                        listCols.Add(column);
                        rawData.Columns.Add(column);
                    }
                }

                
                
                while (reader.Read())
                {
                    var dataRow = rawData.NewRow();
                    for (var i = 0; i < listCols.Count; i++) dataRow[listCols[i]] = reader[i];
                    rawData.Rows.Add(dataRow);
                }
                return rawData;
                foreach (var row in pageData)
                {
                    var dictionary = new Dictionary<string, string>();
                    for (var index = 0; index < row.ItemArray.Length; index++)
                    {
                        dictionary.Add(row.GetColumn(index), row.ItemArray[index].ToString());
                    }
                    ArrayData.Add(dictionary);
                }
                if (ArrayData.Count > 0)
                {
                    //rowDto.data = ArrayData;
                    
                    //return ArrayData;
                 }
            }
            m_dbConnection.Close();
            return null;
        }



    }
    internal static class DataRowExtensions
    {
        public static string GetColumn(this DataRow Row, int Ordinal)
        {
            return Row.Table.Columns[Ordinal].ColumnName;
        }
    }
    public class RowDto
    {
        public ArrayList data { get; set; }
    }
}



       