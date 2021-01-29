namespace RepairsApi
{
    public interface IProperty
    {
        string LevelCode { get; }
        string AreaOffice { get; }
        string SubtypCode { get; }
        string UPropZone { get; }
        string HouseRef { get; }
        string CatType { get; }
        string PropRef { get; }
    }
}