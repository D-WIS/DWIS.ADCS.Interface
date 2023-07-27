namespace DWIS.ADCS.IQoSListener;

public interface IQoSListener : IListener
{
	// Notification that connectivity has been dropped.
	void LivelinessLost(LivelinessLostStatus status);
	// When New Data is available
	void DataAvailable(DataAvailableStatus status);
	// Notification that publication deadline is missed
	void DeadlineMissed(DeadlineMissedStatus status);
	// Lifetime of data has expired
	void DataDisposed();
	// New publisher available in the system
	void OnPublicationMatched(PublicationMatchedStatus status);
	// New subscriber available in the system
	void OnSubscriptionMatched(SubscriptionMatchedStatus status);
}