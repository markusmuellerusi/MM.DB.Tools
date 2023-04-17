namespace MM.DB.Tools
{
    public interface IStatement
    {
        string Sql { get; }
        SqlStatementTypeEnum SqlStatementType { get; }
    }

    public enum SqlStatementTypeEnum
    {
        Unknown,
        Select,
        Union,
        SelectInto,
        Update,
        Insert,
        Delete,
        Batch,
        Create,
        Drop,
        Truncate
    }
}
