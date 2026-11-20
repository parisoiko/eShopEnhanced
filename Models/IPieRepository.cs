namespace eShopEnhanced.Models
{
    public interface IPieRepository
    {
        IEnumerable<Pie> AllPies { get; }
        IEnumerable<Pie> PiesOfTheWeek { get; }
        IEnumerable<Pie> PiesByCategory(string categoryName);
        Pie? GetPieById(int pieId);
    }
}
