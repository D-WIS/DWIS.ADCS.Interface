namespace DWIS.ADCS.Operational;

struct LivelinessLostStatus
{
	/// parameter list TBD
};

struct DataAvailableStatus
{
	/// parameter list TBD
};

struct DeadlineMissedStatus
{
	/// parameter list TBD
};

struct PublicationMatchedStatus
{
	/// parameter list TBD
};

struct SubscriptionMatchedStatus
{
	/// parameter list TBD
};

interface Listener { };

// Quality of Service callbacks for data
interface QoSListener : Listener
{
	// Notification that connectivity has been dropped.
	void LivelinessLost( LivelinessLostStatus status);  
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