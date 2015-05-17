namespace AgilefantTimes.API.Agilefant
{
    /// <summary>
    /// Indicates an item can have time logged against it
    /// </summary>
    public interface IAgilefantLoggable
    {
        /// <summary>
        /// The id of the item in the backlog
        /// </summary>
        int Id { get; }
    }
}
