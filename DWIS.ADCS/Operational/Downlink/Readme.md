Quesion:
1. should we add a method in DownlinkRequest:
	bool SetDownlinkSymbolTable(DownlinkSymbols[][] symbols)
	
1. for the third method: Surface_Equipment, where is its implementation
1. why the 'DownlinkRequestData' has a method field
1. what's the requestedDownlinkID used for?
1. why seperate DownlinkSpecificationsData and DownlinkRequestData
1. what's the ui may look like?
1. issues in repo.
1. the workflow from api calling sequence viewpoint
1. is DownlinkStateData return value needed as the return value of SendDownlinkRequest function, since the state constantly updating sereral times via QoSListenser
1. is Types::RequestStatus  needed in DownlinkRequestData, since we have Types::DownlinkStatus               downlinkStatus in state update, what's the purpose for it inrequest, should we add api to abort a pending request?
1. what 'others, and none' means in Types::DownlinkTypes  
1. for 'float                               durationSeconds;//Duration of the downlink', what about the symbols add together less than the time here?
1. is the delay start from aprove or receipt (delaySeconds in DownlinkRequestData)
1. for the unit inside 'DownlinkSymbols', how about define it as:
    ```
	struct DownlinkSymbols
        {
            float rampTimeMs;
            float holdTimeMs;
            Types<EngineeringUnits> amplitude;
            // amplitude;
            //string unit;
        }
    ```
    for different Types::DownlinkTypes: Flow:Types<EngineeringUnits::VolumetricFlow::cubic_meters_per_second> ;Rotation:Types<EngineeringUnits::AngularVelocity::radians_per_second> ;
1. rename 'DownlinkSymbols' into 'DownlinkSymbol'?
1. '   @optional DownlinkIndex[2]          downlinkIndex;   ', should the type be int? instead of DownlinkIndex[2], so that null stand or not used and omited (optional)
