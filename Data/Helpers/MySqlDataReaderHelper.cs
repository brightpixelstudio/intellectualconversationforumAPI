using MySqlConnector;

namespace intellectualconversationAPI.Data.Helpers
{
    public static class MySqlDataReaderHelper
    {
        public static int GetInt(MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? 0
                : Convert.ToInt32(reader.GetValue(ordinal));
        }

        public static string GetString(MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? string.Empty
                : reader.GetString(ordinal);
        }

        public static string? GetNullableString(
            MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        public static DateTime GetDateTime(
            MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return Convert.ToDateTime(reader.GetValue(ordinal));
        }

        public static DateTime? GetNullableDateTime(
            MySqlDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);

            return reader.IsDBNull(ordinal)
                ? null
                : Convert.ToDateTime(reader.GetValue(ordinal));
        }
    }
}