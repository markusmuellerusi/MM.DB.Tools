using System.Linq;

namespace MM.DB.Tools
{
    public class FieldExpression
    {
        public const string StarExpressionTag = "*";
        public const string StarFieldName = "StarExpr";
        public const string UnknownFieldName = "Expr";
        
        public string Expression { get; set; }
        public string Alias { get; set; }
        public string MaskedAlias => AliasNeedsBrackets(Alias) ? $"[{Alias}]" : Alias;

        public string QualifiedName { get; set; }

        public bool IsStarExpression => !string.IsNullOrWhiteSpace(Alias) && Alias.StartsWith(StarFieldName);

        private static bool AliasNeedsBrackets(string alias)
        {
            return !string.IsNullOrWhiteSpace(alias) &&
                   (char.IsDigit(alias[0]) || alias.Any(c => !char.IsDigit(c) && !char.IsLetter(c)));
        }
    }
}
