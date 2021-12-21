using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.OpenXR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Text;
using System.IO;

public class WorldSpaceXfer : NetworkBehaviour
{
    TrackableId myTrackableId;
    XRAnchorTransferBatch myAnchorTransferBatch = new XRAnchorTransferBatch();
    //System.Threading.Tasks.Task<System.IO.Stream> myStream;
    System.IO.Stream myStream;
    MemoryStream stream;
    bool localAnchorAdded = false;
    bool localBatchReady = false;
    bool batchSentToClients = false;

    public NetworkVariableString hostAnchor = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    // Start is called before the first frame update
    void Start()
    {
        myTrackableId = this.GetComponent<ARAnchor>().trackableId;
        localAnchorAdded = tryAddLocalAnchor();

    }

    // Update is called once per frame
    async void Update()
    {
        if (!localAnchorAdded || myTrackableId.Equals(TrackableId.invalidId))
        {
            Debug.Log("Stuck in Add Anchor Loop");
            localAnchorAdded = tryAddLocalAnchor();
        }

        //Debug.Log(hostAnchor.Value);
        if (IsHost && localAnchorAdded &&!localBatchReady)
        {
            Debug.Log(myAnchorTransferBatch.AnchorNames[0]);
            Debug.Log("Stuck in BatchXFerLoop");
            Debug.Log(myStream);
            myStream = await XRAnchorTransferBatch.ExportAsync(myAnchorTransferBatch);
            if(myStream != null)
            {
                localBatchReady = true;
            }


        }
        if(IsHost && localBatchReady && !batchSentToClients)
        {
            Debug.Log("In send Batch Loop");
            
            StreamReader reader = new StreamReader(myStream);
            string text = reader.ReadToEnd();
            Debug.Log(text);
            hostAnchor.Value = text;
            Debug.Log(hostAnchor.Value);
            batchSentToClients = true;
        }

        /*if (IsClient)
        {
            byteArray = Encoding.ASCII.GetBytes(hostAnchor.Value);
            stream = new MemoryStream(byteArray);
            myAnchorTransferBatch = XRAnchorTransferBatch.ImportAsync(stream).Result;
            myTrackableId = myAnchorTransferBatch.LoadAnchor("HostPosition");
        }*/
    }
    
    bool tryAddLocalAnchor()
    {
        myTrackableId = this.GetComponent<ARAnchor>().trackableId;
        return myAnchorTransferBatch.AddAnchor(myTrackableId, "HostPosition");
    }

    IEnumerator ExportBatch(XRAnchorTransferBatch myAnchorTransferBatch)
    {

        myStream = XRAnchorTransferBatch.ExportAsync(myAnchorTransferBatch).Result;
        //localBatchReady = true;
        yield return null;

    }

}
