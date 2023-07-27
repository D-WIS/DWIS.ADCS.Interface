namespace DWIS.ADCS;

public interface IQoSListener : IListener
{
	// Notification that connectivity has been dropped.
	// Mechanism to notify a subscriber that a producer of data is no longer valid. Does not tell us why the DataWriter is no longer available.
	void LivelinessLost(LivelinessLostStatus status);
	//  Mechanism to notify a subscriber when new data is available for reading.
	void DataAvailable(DataAvailableStatus status);
	// Mechanism to notify a subscriber that the expected deadline for a new data sample has expired.
	void DeadlineMissed(DeadlineMissedStatus status);
	// Mechanism to notify a subscriber that the DataWriter has disposed of the topic instance. This instance will never be seen again
	void DataDisposed();
	// Mechanism to notify a subscriber that a new publisher is available.
	void OnPublicationMatched(PublicationMatchedStatus status);
	// New subscriber available in the system
	void OnSubscriptionMatched(SubscriptionMatchedStatus status);
}