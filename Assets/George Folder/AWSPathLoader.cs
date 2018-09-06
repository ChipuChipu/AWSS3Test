using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AWSPathLoader {

	static AWSLoader.ResponseReceivedEvent ReceivedEventReference;

	public static void SubscribeAWSPathStructureToAWSLoader() {
		ReceivedEventReference = new AWSLoader.ResponseReceivedEvent (AWSPathStructure.RenderAWSResponse);
		AWSLoader.SubscribeOnResponseReceived (ReceivedEventReference);
	}

	public static void UnsubscribeAWSPathStructureToAWSLoader() {
		AWSLoader.UnsubscribeOnResponseReceived (ReceivedEventReference);
	}


}
