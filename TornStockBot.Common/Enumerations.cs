namespace TornStockBot.Common
{
    public enum MovingAverageType
    {
        Unknown = 0,
        Simple = 1,
        Exponential = 2
    }

    public enum MovingAverageLength
    {
        Unknown = 0,
        Short = 1,
        Medium = 2,
        Long = 3
    }

    public enum CrossingType
    {
        None = 0,
        Gold = 1,
        Death = 2
    }
}