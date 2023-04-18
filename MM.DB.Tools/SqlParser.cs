using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MM.DB.Tools.Exceptions;

namespace MM.DB.Tools
{
    public class SqlParser
    {
        #region consts
        private const StringComparison IcIcCompare = StringComparison.InvariantCultureIgnoreCase;
        private static readonly char[] Delimiters = new[] { '\'', '"', '[', ']' };
        private static readonly char[] Operators = { '+', '-', '*', '%', '^', '/', '|', '&' };
        #endregion

        #region public methods

        public IStatement Parse(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL statement missing");
            }

            if (sql.StartsWith(BaseStatement.SelectKeyword, IcIcCompare))
            {
                return ParseSelectStatement(sql);
            }

            var type = SqlStatementTypeEnum.Unknown;

            if (sql.StartsWith(BaseStatement.InsertKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Insert;
            }
            if (sql.StartsWith(BaseStatement.UpdateKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Update;
            }
            if (sql.StartsWith(BaseStatement.DeleteKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Delete;
            }
            if (sql.StartsWith(BaseStatement.CreateKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Create;
            }
            if (sql.StartsWith(BaseStatement.DropKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Drop;
            }
            if (sql.StartsWith(BaseStatement.TruncateKeyword, IcIcCompare))
            {
                type = SqlStatementTypeEnum.Truncate;
            }

            throw new StatementNotSupportedException(type);
        }

        #endregion

        #region private Methods

        private static IStatement ParseSelectStatement(string sql)
        {
            sql = Linarize(sql);

            var length = sql.Length;
            var posDistinct = GetKeywordPos(sql, SelectStatement.DistinctKeyword);
            var posTop = GetKeywordPos(sql, SelectStatement.TopKeyword);
            var posTopPercent = GetKeywordPos(sql, SelectStatement.PercentKeyword);
            var posFrom = GetKeywordPos(sql, BaseStatement.FromKeyword);
            var posWhere = GetKeywordPos(sql, BaseStatement.WhereKeyword);
            var posGroupBy = GetKeywordPos(sql, SelectStatement.GroupByKeyword);
            var posHaving = GetKeywordPos(sql, SelectStatement.HavingKeyword);
            var posOrderBy = GetKeywordPos(sql, SelectStatement.OrderByKeyword);

            var selectStatement = new SelectStatement(sql)
            {
                HasWhere = posWhere != -1,
                HasGroupBy = posGroupBy != -1,
                HasHaving = posHaving != -1,
                HasOrderBy = posOrderBy != -1,
                HasDistinct = posDistinct != -1,
                HasTop = posTop != -1,
                HasTopPercent = posTopPercent != -1
            };

            var posFields = SelectStatement.PosSelect + BaseStatement.SelectKeyword.Length;
            if (selectStatement.HasDistinct)
            {
                posFields = posDistinct + SelectStatement.DistinctKeyword.Length;
            }
            if (selectStatement.HasTop)
            {
                posFields = sql.IndexOf(' ', posTop + SelectStatement.TopKeyword.Length) + 1;
                selectStatement.TopValue = !int.TryParse(SubStr(sql, posTop + SelectStatement.TopKeyword.Length, posFields).Trim(), out var topValue)
                    ? throw new ParserException("TOP value expected") : topValue;

                if (selectStatement.HasTopPercent)
                {
                    posFields = sql.IndexOf(' ', posTopPercent + SelectStatement.PercentKeyword.Length - 1);
                }
            }

            if (posWhere == -1) posWhere = length;
            if (posGroupBy == -1) posGroupBy = length;
            if (posHaving == -1) posHaving = length;
            if (posOrderBy == -1) posOrderBy = length;

            selectStatement.HasUnion = sql.IndexOf(SelectStatement.UnionKeyword, IcIcCompare) >= 0;
            if (sql.IndexOf(BaseStatement.IntoKeyword, IcIcCompare) >= 0)
                throw new StatementNotSupportedException(SqlStatementTypeEnum.SelectInto);

            selectStatement.FieldsExpression = SubStr(sql, posFields, posFrom);
            selectStatement.Fields = GetFields(selectStatement.FieldsExpression);

            selectStatement.FromExpression = SubStr(sql, posFrom + BaseStatement.FromKeyword.Length, 
                Math.Min(posWhere, Math.Min(posGroupBy, posOrderBy)));
            if (selectStatement.HasUnion && !selectStatement.FromExpression.StartsWith("("))
                throw new StatementNotSupportedException(SqlStatementTypeEnum.Union);


            if (selectStatement.HasWhere)
            {
                selectStatement.WhereExpression = SubStr(sql, posWhere + BaseStatement.WhereKeyword.Length, 
                    Math.Min(Math.Min(posGroupBy, posHaving), Math.Min(posOrderBy, length)));
            }

            if (selectStatement.HasGroupBy)
            {
                selectStatement.GroupByExpression = SubStr(sql, posGroupBy + SelectStatement.GroupByKeyword.Length, 
                    Math.Min(posHaving, Math.Min(posOrderBy, length)));
                selectStatement.Groups = GetFragments(selectStatement.GroupByExpression);
            }

            if (selectStatement.HasHaving)
            {
                selectStatement.HavingExpression = SubStr(sql, posHaving + SelectStatement.HavingKeyword.Length, 
                    Math.Min(posOrderBy, length));
            }

            if (selectStatement.HasOrderBy)
            {
                selectStatement.OrderByExpression = SubStr(sql, posOrderBy + SelectStatement.OrderByKeyword.Length, length);
                selectStatement.Orders = GetOrderItems(selectStatement.OrderByExpression);
            }

            return selectStatement;
        }


        private static string Linarize(string sql)
        {
            var result = string.Empty;
            if (string.IsNullOrWhiteSpace(sql)) return sql;

            var delimiter = char.MinValue;
            var isInDelimiters = false;
            const char space = ' ';
            char[] ignoreSpaceBefore = { ')' };
            char[] ignoreSpaceAfter = { ' ', '(' };
            var ignoreNextSpace = false;
            sql = sql.Replace(Environment.NewLine, space.ToString())
                .Replace("\n", space.ToString()).Trim();

            foreach (var c in sql)
            {
                if (Delimiters.Contains(c))
                {
                    if (isInDelimiters)
                    {
                        switch (delimiter)
                        {
                            case '"' when c == '"':
                            case '\'' when c == '\'':
                            case '[' when c == ']':
                                isInDelimiters = false;
                                break;
                        }
                    }
                    else
                    {
                        isInDelimiters = true;
                        delimiter = c;
                    }
                }

                if (isInDelimiters)
                {
                    result += c;
                    continue;
                }
                if (c == space && ignoreNextSpace)
                {
                    continue;
                }
                if (ignoreSpaceBefore.Contains(c))
                {
                    result = result.Trim();
                }
                ignoreNextSpace = ignoreSpaceAfter.Contains(c);

                result += c;
            }

            return result;
        }

        private static List<FieldExpression> GetFields(string fieldsFragment)
        {
            var fields = new List<FieldExpression>();
            var uniqueFields = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(fieldsFragment)) return fields;
            var fieldsList = GetFragments(fieldsFragment);

            foreach (var item in fieldsList.Where(f => !string.IsNullOrEmpty(f)))
            {
                var field = item;
                field = field.Replace(Environment.NewLine, " ").Trim();

                string fieldAlias;
                string fieldExpr;

                var pos = field.LastIndexOf(SelectStatement.AsKeyword, IcIcCompare);
                if (pos >= 0 && GetLevelForPos(field, pos) == 0)
                {
                    fieldAlias = SubStr(field, pos + SelectStatement.AsKeyword.Length, field.Length).Trim();
                    if (fieldAlias.StartsWith("[") && fieldAlias.EndsWith("]"))
                    {
                        fieldAlias = fieldAlias.Substring(1, fieldAlias.Length - 2);
                    }
                    fieldExpr = SubStr(field, 0, pos).Trim();
                    AddField(uniqueFields, fieldAlias.Trim(), fieldExpr.Trim());
                    continue;
                }

                pos = field.LastIndexOf("[", IcIcCompare);
                if (field.EndsWith("]") && pos >= 0 && GetLevelForPos(field, pos) == 0)
                {
                    fieldAlias = SubStr(field, pos + 1, field.Length - 1);
                    fieldExpr = field;
                    AddField(uniqueFields, fieldAlias.Trim(), fieldExpr.Trim());
                    continue;
                }

                pos = field.LastIndexOf(" ", IcIcCompare);
                if (pos >= 0 && GetLevelForPos(field, pos) == 0)
                {
                    fieldAlias = SubStr(field, pos + 1, field.Length);
                    fieldExpr = SubStr(field, 0, pos).Trim();

                    if (HasDigitsOnly(fieldAlias) || EndsWithOperator(fieldExpr))
                    {
                        fieldAlias = string.Empty;
                        fieldExpr = field;
                    }

                    AddField(uniqueFields, fieldAlias.Trim(), fieldExpr.Trim());
                    continue;
                }

                pos = field.LastIndexOf(".", IcIcCompare);
                if (pos >= 0 && GetLevelForPos(field, pos) == 0)
                {
                    fieldAlias = SubStr(field, pos + 1, field.Length);
                    fieldExpr = field;
                    AddField(uniqueFields, fieldAlias.Trim(), fieldExpr.Trim());
                    continue;
                }

                AddField(uniqueFields, field.Trim(), field.Trim());
            }

            foreach (var (alias, expression) in uniqueFields)
            {
                var field = new FieldExpression
                {
                    Alias = alias,
                    Expression = expression
                };

                if (!string.IsNullOrWhiteSpace(field.Expression) &&
                    field.Expression.StartsWith($"({BaseStatement.SelectKeyword}", IcIcCompare) &&
                    field.Expression.EndsWith(")"))
                {
                    field.SubSelect = (SelectStatement) ParseSelectStatement(field.Expression.Trim('(').Trim(')'));
                }

                fields.Add(field);
            }

            return fields;
        }

        private static List<OrderExpression> GetOrderItems(string orderByExpression)
        {
            var orderByExpressions = GetFragments(orderByExpression);
            var orderItems = new List<OrderExpression>();
            foreach (var orderExpression in orderByExpressions)
            {
                var exp = orderExpression.Trim();
                var orderItem = new OrderExpression();
                if (exp.EndsWith(" desc", IcIcCompare))
                {
                    orderItem.Expression = exp[..^5];
                    orderItem.Direction = OrderDirection.Desc;
                }
                else if (exp.EndsWith(" asc", IcIcCompare))
                {
                    orderItem.Expression = exp[..^4];
                    orderItem.Direction = OrderDirection.Asc;
                }
                else
                {
                    orderItem.Expression = exp;
                    orderItem.Direction = OrderDirection.Asc;
                }

                orderItems.Add(orderItem);
            }

            return orderItems;
        }

        public static List<string> GetFragments(string fragment, char separator = ',')
        {
            var fieldFragments = new List<string>();
            var sb = new StringBuilder();

            if (string.IsNullOrWhiteSpace(fragment))
                return fieldFragments;

            try
            {
                var delimiter = char.MinValue;
                var isInDelimiters = false;
                fragment = fragment.Trim();

                foreach (var c in fragment)
                {
                    if (Delimiters.Contains(c))
                    {
                        if (isInDelimiters)
                        {
                            switch (delimiter)
                            {
                                case '"' when c == '"':
                                case '\'' when c == '\'':
                                case '[' when c == ']':
                                    isInDelimiters = false;
                                    break;
                            }
                        }
                        else
                        {
                            isInDelimiters = true;
                            delimiter = c;
                        }
                    }

                    switch (isInDelimiters)
                    {
                        case false when c == separator:
                            fieldFragments.Add(sb.ToString().Trim());
                            sb.Clear();
                            continue;
                        case true:
                            sb.Append(c);
                            continue;
                        default:
                            sb.Append(c);
                            delimiter = char.MinValue;
                            break;
                    }
                }

                fieldFragments.Add(sb.ToString().Trim());
            }
            catch (Exception e)
            {
                TraceLog.Logger.LogError(e);
            }

            return fieldFragments;
        }

        private static void AddField(IDictionary<string, string> fields, string fieldName, string fieldExpression)
        {
            const char separator = '_';
            var index = 0;
            var isUnknownFieldName = false;
            var isExplicitIndex = false;
            var isIncremented = false;

            var baseFieldName = fieldName;

            if (string.IsNullOrEmpty(fieldName) || HasDigitsOnly(fieldName))
            {
                baseFieldName = FieldExpression.UnknownFieldName;
                index++;
                fieldName = baseFieldName + separator + index;
                isUnknownFieldName = true;
            }
            if (fieldName.EndsWith(FieldExpression.StarExpressionTag))
            {
                baseFieldName = FieldExpression.StarFieldName;
                index++;
                fieldName = baseFieldName + separator + index;
                isUnknownFieldName = true;
            }

            var segments = fieldName.Split('_');
            if (int.TryParse(segments[^1], out index))
            {
                baseFieldName = string.Empty;
                for (var s = 0; s < segments.Length - 1; s++)
                {
                    baseFieldName += segments[s] + separator;
                }
                baseFieldName = baseFieldName.Trim(separator);
                isExplicitIndex = !isUnknownFieldName;
            }

            fieldName = fieldName.Trim();
            var existingFieldName = string.Empty;
            while (fields.ContainsKey(fieldName))
            {
                if (!isIncremented) existingFieldName = fieldName;
                index++;
                isIncremented = true;
                fieldName = baseFieldName + separator + index;
            }

            if (isExplicitIndex && !string.IsNullOrEmpty(existingFieldName))
            {
                var name = fieldName;
                var list = fields.Keys.Select(field => existingFieldName.Equals(field) ?
                    new Tuple<string, string>(name, fields[field]) :
                    new Tuple<string, string>(field, fields[field])).ToList();

                fields.Clear();
                foreach (var (item1, item2) in list)
                {
                    fields.Add(item1, item2);
                }
                fieldName = existingFieldName;
            }

            fields.Add(fieldName, fieldExpression);
        }

        private static int GetKeywordPos(string sql, string keyWord)
        {
            var rIndex = -1;
            foreach (Match item in Regex.Matches(sql, keyWord, RegexOptions.IgnoreCase))
            {
                if (GetLevelForPos(sql, item.Index) != 0) continue;
                rIndex = item.Index;
                break;
            }
            return rIndex;
        }

        private static int GetLevelForPos(string sql, int pos)
        {
            var delimiter = char.MinValue;
            var level = 0;
            var isInDelimiters = false;

            for (var i = 0; i < pos; i++)
            {
                var c = sql[i];

                if (Delimiters.Contains(c))
                {
                    if (isInDelimiters)
                    {
                        switch (delimiter)
                        {
                            case '"' when c == '"':
                            case '\'' when c == '\'':
                            case '[' when c == ']':
                                isInDelimiters = false;
                                break;
                        }
                    }
                    else
                    {
                        isInDelimiters = true;
                        delimiter = c;
                    }
                }

                if (isInDelimiters) continue;
                if (sql[i] == '(') level++;
                if (sql[i] == ')') level--;
            }
            return level;
        }

        private static string SubStr(string text, int start, int end)
        {
            var str = text.Substring(start, end - start);
            return str.Trim();
        }

        private static bool HasDigitsOnly(string s)
        {
            return !string.IsNullOrEmpty(s) && s.All(char.IsDigit);
        }

        private static bool EndsWithOperator(string s)
        {
            return !string.IsNullOrEmpty(s) && Operators.Contains(s[^1]);
        }

        #endregion
    }
}
