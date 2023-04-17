using System;

namespace MM.DB.Tools.Exceptions
{
    public class StatementNotSupportedException : NotSupportedException
    {
        private readonly SqlStatementTypeEnum _sqlStatementType;

        public StatementNotSupportedException()
        {
            _sqlStatementType = SqlStatementTypeEnum.Unknown;
        }

        public StatementNotSupportedException(SqlStatementTypeEnum sqlStatementType)
        {
            _sqlStatementType = sqlStatementType;
        }

        public override string Message
        {
            get
            {
                switch (_sqlStatementType)
                {
                    case SqlStatementTypeEnum.Insert:
                        return "BATCH is not supported";
                    case SqlStatementTypeEnum.Update:
                        return "UPDATE is not supported";
                    case SqlStatementTypeEnum.Delete:
                        return "DELETE is not supported";
                    case SqlStatementTypeEnum.Batch:
                        return "BATCH is not supported";
                    case SqlStatementTypeEnum.SelectInto:
                        return "SELECT INTO is not supported";
                    case SqlStatementTypeEnum.Union:
                        return "UNION ONLY SUPPORTED WITH FORMAT: " +
                               "SELECT * FROM " +
                               "(" +
                               "SELECT [fields] FROM [tables] [GROUPBY] [WHERE] [ORDERBY] " +
                               "UNION " +
                               "SELECT [fields] FROM [tables] [GROUPBY] [WHERE] [ORDERBY]" +
                               ") [GROUPBY] [WHERE] [ORDERBY]";
                    default:
                        return "Statement is not supported";
                }
            }
        }
    }
}
