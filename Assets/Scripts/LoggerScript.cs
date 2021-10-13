using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif

public class LoggerScript : MonoBehaviour
{
    /// <summary>
    ///     Logging Script designed to produce detail Gaze Logs from Host and all CLients for research  
    /// </summary>
    /// 
    //define filePath
    #region Constants to modify
    private const string DataSuffix = "data";
    private const string CSVHeader = "Timestamp,TimeInMs,LocalGazeOrigin_x,LocalGazeOrigin_y,LocalGazeOrigin_z,LocalGazeDirection_x,LocalGazeDirection_y,LocalGazeDirection_z," +
        "HostGazeDirectionChange_x,HostGazeDirectionChange_y,HostGazeDirectionChange_z,ClientGazeDirectionChange_x,ClientGazeDirectionChange_y,ClientGazeDirectionChange_z, GazeDirectionDifference_x,GazeDirectionDifference_y,GazeDirectionDifference_z," +
        "OneSecondGazeDirectionChangeDifference.x,OneSecondGazeDirectionChangeDifference.y,OneSecondGazeDirectionChangeDifference.z,CubeColor";
    private const string SessionFolderRoot = "CSVLogger";
    #endregion

    #region private members
    private string m_sessionPath;
    private string m_filePath;
    private string m_recordingId;
    private string m_sessionId;
    private int flushCounter;

    private StringBuilder m_csvData;
    #endregion
    #region public members
    public string RecordingInstance => m_recordingId;
    Dictionary<ulong, Vector3> gazeComparison;
    Vector3 oneSecondGazeChangeDifference;
    private float gazeErrorCorrectionThreshold = .40f;
    private double stationaryGazeThreshold = .00009;
    bool eyesStationary = false;

    #endregion

    async void Start()
    {
        var prevGazeDirectionVector = CoreServices.InputSystem?.EyeGazeProvider.GazeDirection;
        await MakeNewSession();
        StartNewCSV();

    }
    async Task MakeNewSession()
    {
        m_sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string rootPath = "";
#if WINDOWS_UWP
            StorageFolder sessionParentFolder = await KnownFolders.PicturesLibrary
                .CreateFolderAsync(SessionFolderRoot,
                CreationCollisionOption.OpenIfExists);
            rootPath = sessionParentFolder.Path;
#else
        rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SessionFolderRoot);
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
#endif
        m_sessionPath = Path.Combine(rootPath, m_sessionId);
        Directory.CreateDirectory(m_sessionPath);
        Debug.Log("CSVLogger logging data to " + m_sessionPath);
    }

    public void StartNewCSV()
    {
        m_recordingId = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        var filename = m_recordingId + "-" + DataSuffix + ".csv";
        m_filePath = Path.Combine(m_sessionPath, filename);
        m_csvData = new StringBuilder();
        m_csvData.AppendLine(CSVHeader);
    }

    void Update()
    {

        
        gazeComparison = new Dictionary<ulong, Vector3>();

        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        if (eyeGazeProvider != null)
        {
                List<String> newRow = RowWithStartData();
                newRow.Add(eyeGazeProvider.GazeOrigin.x.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.y.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.z.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.x.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.y.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.z.ToString());
            if (NetworkManager.Singleton.IsHost)
            {

                var gazePairManagementComponent = GameObject.Find("GazePairConnectionManagement(Clone)");
                foreach(ulong client in gazePairManagementComponent.GetComponent<GazePairConnectionManagement>().getClientsInLobby().Keys)
                {
                    if (NetworkManager.Singleton.ConnectedClients.TryGetValue(client,
                    out var networkedClient))
                    {
                        var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
                            //Will always begin with Host Gaze data, as Host was added to Client list first in Connection Manager
                            if (player)
                            {
                            //If player' gaze is below "stationary" threshold stationaryGazeThreshold
                            if ((Math.Abs(player.GazeDirectionChange.Value.x) < stationaryGazeThreshold) || (Math.Abs(player.GazeDirectionChange.Value.y) < stationaryGazeThreshold) || (Math.Abs(player.GazeDirectionChange.Value.z) < stationaryGazeThreshold))
                                {
                                    eyesStationary = true;
                                }
 
                                gazeComparison.Add(client, player.GazeDirectionChange.Value);
                                newRow.Add(player.GazeDirectionChange.Value.x.ToString());
                                newRow.Add(player.GazeDirectionChange.Value.y.ToString());
                                newRow.Add(player.GazeDirectionChange.Value.z.ToString());
                            }

                    }
                }

                //If Host, compute the differnce between the two gazes based on the Host's gaze, host is gazeComparison.ElementAt(0)
                Vector3 hostDifferenceVector = gazeComparison.ElementAt(0).Value; //Start with Hosts gaze change
                Vector3 differenceVector = new Vector3(0, 0, 0);
                foreach (KeyValuePair<ulong, Vector3> client in gazeComparison.Skip(1))
                {
                    differenceVector.x = differenceVector.x + calculateDifference(hostDifferenceVector.x, client.Value.x);
                    differenceVector.y = differenceVector.y + calculateDifference(hostDifferenceVector.y, client.Value.y);
                    differenceVector.z = differenceVector.z + calculateDifference(hostDifferenceVector.z, client.Value.z);
                }
                if (!eyesStationary)
                {
                    oneSecondGazeChangeDifference.x = oneSecondGazeChangeDifference.x + differenceVector.x;
                    oneSecondGazeChangeDifference.y = oneSecondGazeChangeDifference.y + differenceVector.y;
                    oneSecondGazeChangeDifference.z = oneSecondGazeChangeDifference.z + differenceVector.z;
                }
                else
                {
                    //If eyes staionary
                    oneSecondGazeChangeDifference = oneSecondGazeChangeDifference + new Vector3(gazeErrorCorrectionThreshold / 30f, gazeErrorCorrectionThreshold / 30f, gazeErrorCorrectionThreshold / 30f);
                    eyesStationary = false;
                }
                newRow.Add(differenceVector.x.ToString());
                newRow.Add(differenceVector.y.ToString());
                newRow.Add(differenceVector.z.ToString());



            }


            //buffer flush kills the HL2 frame rate, so this limits the flush to about ~1/sec (~60 frames/sec, flushCounter updated every frame)
            flushCounter += 1;

                
            if (flushCounter == 60)
            {
                GameObject cube = GameObject.Find("GazeThresholdCube");
                var cubeRenderer = cube.GetComponent<Renderer>();

                newRow.Add(oneSecondGazeChangeDifference.x.ToString());
                newRow.Add(oneSecondGazeChangeDifference.y.ToString());
                newRow.Add(oneSecondGazeChangeDifference.z.ToString());
                if(oneSecondGazeChangeDifference.x > gazeErrorCorrectionThreshold || oneSecondGazeChangeDifference.y > gazeErrorCorrectionThreshold || oneSecondGazeChangeDifference.z > gazeErrorCorrectionThreshold)
                {
                    cubeRenderer.material.SetColor("_Color", Color.red);
                }
                else if (oneSecondGazeChangeDifference.x < gazeErrorCorrectionThreshold && oneSecondGazeChangeDifference.y < gazeErrorCorrectionThreshold && oneSecondGazeChangeDifference.z < gazeErrorCorrectionThreshold)
                {
                    cubeRenderer.material.SetColor("_Color", Color.green);
                }
                newRow.Add(cubeRenderer.material.color.ToString());
                AddRow(newRow);
                FlushData();
                oneSecondGazeChangeDifference = new Vector3();
                flushCounter = 0;


            }
            else
            {
                AddRow(newRow);
            }
            
        }
    }

    public void OnDestroy()
    {
        
    }
    private float calculateDifference(float number1, float number2)
    {
        float result = number1 > number2 ? number1 - number2 : number2 - number1;
        return result;
    }

    public void AddRow(List<String> rowData)
    {
        AddRow(string.Join(",", rowData.ToArray()));
    }

    public void AddRow(string row)
    {
        m_csvData.AppendLine(row);
    }


    public async void FlushData()
    {
        using (var csvWriter = new StreamWriter(m_filePath, true))
        {
             await csvWriter.WriteAsync(m_csvData.ToString());
        }
        m_csvData.Clear();
    }

    public List<String> RowWithStartData()
    {
        List<String> rowData = new List<String>();
        rowData.Add(DateTime.Now.ToString());
        long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        rowData.Add(milliseconds.ToString());
        return rowData;
    }
}