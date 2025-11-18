namespace EventFlow.DeclarativeSaga.StateMachine
{
    /// <summary>
    /// Interface that saga states must implement to support compensation job cancellation.
    /// The CompensationJobId is persisted through saga events with metadata, ensuring resilience.
    /// When a saga state implements this interface, the DeclarativeSaga can automatically
    /// cancel scheduled compensation jobs when expected events arrive before timeout.
    /// </summary>
    public interface IHasCompensationJobId
    {
        /// <summary>
        /// Gets or sets the Hangfire job ID for the scheduled compensation command.
        /// This property is used to cancel the compensation job if the expected event arrives before timeout.
        /// The value is persisted through saga events, ensuring it survives saga reloads.
        /// </summary>
        string? CompensationJobId { get; set; }
    }
}

