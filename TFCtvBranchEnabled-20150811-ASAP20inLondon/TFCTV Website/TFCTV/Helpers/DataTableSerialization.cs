using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TFCTV.Helpers
{
    /// <summary>
    /// Converts a <see cref="DataRow"/> object to and from JSON.
    /// </summary>
    public class DataRowConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        ///
        public override void WriteJson(JsonWriter writer, object dataRow, JsonSerializer serializer)
        {
            DataRow row = dataRow as DataRow;

            // *** HACK: need to use root serializer to write the column value
            //     should be fixed in next ver of JSON.NET with writer.Serialize(object)
            JsonSerializer ser = new JsonSerializer();

            writer.WriteStartObject();
            foreach (DataColumn column in row.Table.Columns)
            {
                writer.WritePropertyName(column.ColumnName);
                ser.Serialize(writer, row[column]);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataRow).IsAssignableFrom(valueType);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a DataTable to JSON. Note no support for deserialization
    /// </summary>
    public class DataTableConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        public override void WriteJson(JsonWriter writer, object dataTable, JsonSerializer serializer)
        {
            DataTable table = dataTable as DataTable;
            DataRowConverter converter = new DataRowConverter();

            writer.WriteStartObject();

            writer.WritePropertyName("Rows");
            writer.WriteStartArray();

            foreach (DataRow row in table.Rows)
            {
                converter.WriteJson(writer, row, serializer);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataTable).IsAssignableFrom(valueType);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object value.</returns>
        ///
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="DataSet"/> object to JSON. No support for reading.
    /// </summary>
    public class DataSetConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        ///

        public override void WriteJson(JsonWriter writer, object dataset, JsonSerializer serializer)
        {
            DataSet dataSet = dataset as DataSet;

            DataTableConverter converter = new DataTableConverter();

            writer.WriteStartObject();

            writer.WritePropertyName("Tables");
            writer.WriteStartArray();

            foreach (DataTable table in dataSet.Tables)
            {
                converter.WriteJson(writer, table, serializer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        /// <summary>
        /// Determines whether this instance can convert the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type valueType)
        {
            return typeof(DataSet).IsAssignableFrom(valueType);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object value.</returns>

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}