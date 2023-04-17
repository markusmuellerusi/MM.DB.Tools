using System.Collections.Generic;

namespace MM.DB.Tools
{
    public class SelectStatement : BaseStatement
    {
        public const string UnionKeyword = " UNION ";
        public const string DistinctKeyword = " DISTINCT ";
        public const string TopKeyword = " TOP ";
        public const string PercentKeyword = " PERCENT ";
        public const string AsKeyword = " AS ";
        public const string GroupByKeyword = " GROUP BY ";
        public const string HavingKeyword = " HAVING ";
        public const string OrderByKeyword = " ORDER BY ";
        public const int PosSelect = 0;

        public SelectStatement(string sql)
        {
            Sql = sql;
            SqlStatementType = SqlStatementTypeEnum.Select;
        }

        #region public members

        public string FieldsExpression { get; set; }
        public List<FieldExpression> Fields { get; set; }
        public string FromExpression { get; set; }
        public string WhereExpression { get; set; }
        public string GroupByExpression { get; set; }
        public List<string> Groups { get; set; }
        public string HavingExpression { get; set; }
        public string OrderByExpression { get; set; }
        public List<OrderExpression> Orders { get; set; }
        public bool HasUnion { get; set; }
        public bool HasTop { get; set; }
        public int TopValue { get; set; }
        public bool HasTopPercent { get; set; }
        public bool HasDistinct { get; set; }
        public bool HasWhere { get; set; }
        public bool HasGroupBy { get; set; }
        public bool HasHaving { get; set; }
        public bool HasOrderBy { get; set; }
        
        #endregion
    }
}
