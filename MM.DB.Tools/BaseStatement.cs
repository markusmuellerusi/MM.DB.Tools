using System.Collections.Generic;

namespace MM.DB.Tools
{
    public abstract class BaseStatement : IStatement
    {
        public const string SelectKeyword = "SELECT ";
        public const string InsertKeyword = "INSERT ";
        public const string UpdateKeyword = "UPDATE ";
        public const string DeleteKeyword = "DELETE ";
        public const string CreateKeyword = "CREATE ";
        public const string DropKeyword = "DROP ";
        public const string TruncateKeyword = "TRUNCATE ";

        public const string FromKeyword = " FROM ";
        public const string WhereKeyword = " WHERE ";
        public const string IntoKeyword = " INTO ";
        public string Sql { get; protected set; }
        public SqlStatementTypeEnum SqlStatementType { get; protected set; }
    }
}
