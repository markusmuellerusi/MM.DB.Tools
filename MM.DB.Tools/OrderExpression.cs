namespace MM.DB.Tools
{
    public class OrderExpression
    {
        public string Expression { get; set; }
        public OrderDirection Direction { get; set; }

    }

    public enum OrderDirection
    {
        Asc,
        Desc
    }
}
