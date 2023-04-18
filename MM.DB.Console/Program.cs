using System;
using MM.DB.Tools;

namespace MM.DB.Console
{
    internal class Program
    {
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            var sqlStatement = $"Select Top 10 Percent [table].[Field1], [db].[schema].[table].[Field2],{Environment.NewLine}" +
                               $"( select * from mySpace where ich = 1 ) As Me,{Environment.NewLine}" +
                               $"*, ( 1 - 4 ) ,{Environment.NewLine}" +
                               $"Test.*, a +     b, '(','(',1 + 2, [a], [b] by, c, ')'{Environment.NewLine}" +
                               $"from x{Environment.NewLine}"+
                               $"where a=1 and b=2{Environment.NewLine}" +
                               $"group by c, b + 1{Environment.NewLine}" +
                               $"having x=1 and y=2{Environment.NewLine}" +
                               "order by a, b desc";
            TraceLog.Logger.Information(sqlStatement);

            try
            {
                var sqlParser = new SqlParser();

                if (sqlParser.Parse(sqlStatement) is SelectStatement selectStatement)
                {
                    Print(selectStatement);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            
            System.Console.In.Read();
        }

        private static void Print(SelectStatement selectStatement)
        {
            TraceLog.Logger.Verbose($"Distinct: {selectStatement.HasDistinct}");
            TraceLog.Logger.Verbose($"Top: {selectStatement.HasTop} {selectStatement.TopValue}");
            TraceLog.Logger.Verbose($"Top is percent: {selectStatement.HasTopPercent}");
            TraceLog.Logger.Verbose($"FieldsExpression: {selectStatement.FieldsExpression}");
            if (selectStatement.Fields != null)
            {
                foreach (var field in selectStatement.Fields)
                {
                    TraceLog.Logger.Verbose($"\t Field: {field.Expression} AS {field.MaskedAlias} ");
                    if (field.SubSelect != null)
                    {
                        Print(field.SubSelect);
                        TraceLog.Logger.Verbose("_____________");
                    }
                }
            }

            TraceLog.Logger.Verbose($"FromExpression: {selectStatement.FromExpression}");
            TraceLog.Logger.Verbose($"WhereExpression: {selectStatement.WhereExpression}");
            TraceLog.Logger.Verbose($"GroupByExpression: {selectStatement.GroupByExpression}");
            if (selectStatement.Groups != null)
            {
                foreach (var group in selectStatement.Groups)
                {
                    TraceLog.Logger.Verbose($"\t Group: {@group}");
                }
            }

            TraceLog.Logger.Verbose($"HavingExpression: {selectStatement.HavingExpression}");
            TraceLog.Logger.Verbose($"OrderByExpression: {selectStatement.OrderByExpression}");
            if (selectStatement.Orders != null)
            {
                foreach (var order in selectStatement.Orders)
                {
                    TraceLog.Logger.Verbose($"\t Order: {order.Expression} {order.Direction}");
                }
            }
        }
    }
}
