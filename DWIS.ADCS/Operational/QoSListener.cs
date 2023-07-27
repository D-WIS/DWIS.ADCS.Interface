namespace DWIS.ADCS.Operational;

public class LivelinessLostStatus
{
	/// parameter list TBD
};

public class DataAvailableStatus
{
	/// parameter list TBD
};

public class DeadlineMissedStatus
{
	/// parameter list TBD
};

public class PublicationMatchedStatus
{
	/// parameter list TBD
};

public class SubscriptionMatchedStatus
{
	/// parameter list TBD
};

public interface IListener { };

// Quality of Service callbacks for data
public interface IQoSListener : IListener
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