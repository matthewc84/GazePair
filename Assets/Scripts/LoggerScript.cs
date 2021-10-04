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

#if WINDOWS_UWP
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif

public class LoggerScript : MonoBehaviour
{

    //define filePath
    #region Constants to modify
    private const string DataSuffix = "data";
    private const string CSVHeader = "Timestamp,TimeInMs,SessionID,RecordingID,GazeOrigin_x,GazeOrigin_y,GazeOrigin_z,GazeDirection_x,GazeDirection_y,GazeDirection_z,GazeDirectionChange_x,GazeDirectionChange_y,GazeDirectionChange_z";
    //private const string CSVHeader = "Timestamp,TimeInMs,SessionID,RecordingID,GazeOrigin_x,GazeOrigin_y,GazeOrigin_z,GazeDirection_x,GazeDirection_y,GazeDirection_z,Hit Object_Name,Hit_Object_Distance,Hit_Object_x,Hit_Object_y,Hit_Object_z";
    private const string SessionFolderRoot = "CSVLogger";
    #endregion

    #region private members
    private string m_sessionPath;
    private string m_filePath;
    private string m_recordingId;
    private string m_sessionId;
    private int flushCounter;
    private Vector3 prevGazeDirectionVector;

    private StringBuilder m_csvData;
    #endregion
    #region public members
    public string RecordingInstance => m_recordingId;
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
        GameObject GazeMonitorObject = GameObject.Find("GazeMonitor(Clone)");

        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
        if (eyeGazeProvider != null)
        {

            EyeTrackingTarget lookedAtEyeTarget = EyeTrackingTarget.LookedAtEyeTarget;
            Vector3 GazeDirectionChange = eyeGazeProvider.GazeDirection - prevGazeDirectionVector;
            // If gaze hit GameObject

            if (lookedAtEyeTarget != null)
            {
                List<String> newRow = RowWithStartData();
                newRow.Add(eyeGazeProvider.GazeOrigin.x.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.y.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.z.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.x.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.y.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.z.ToString());
                if (GazeMonitorObject != null)
                {
                    var GazeMonitorScript = GazeMonitorObject.GetComponent<GazeMonitor>();

                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.x.ToString());
                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.y.ToString());
                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.z.ToString());
                }
                //newRow.Add(eyeGazeProvider.HitInfo.collider.ToString());
                //newRow.Add(eyeGazeProvider.HitInfo.distance.ToString());
                //newRow.Add(eyeGazeProvider.HitInfo.point.x.ToString());
                //newRow.Add(eyeGazeProvider.HitInfo.point.y.ToString());
                //newRow.Add(eyeGazeProvider.HitInfo.point.z.ToString());
                flushCounter += 1;

                AddRow(newRow);
                if (flushCounter == 60)
                {
                    FlushData();
                    flushCounter = 0;
                }
            }
            else
            {
                // If no target is hit, show the object at a default distance along the gaze ray.
                List<String> newRow = RowWithStartData();
                newRow.Add(eyeGazeProvider.GazeOrigin.x.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.y.ToString());
                newRow.Add(eyeGazeProvider.GazeOrigin.z.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.x.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.y.ToString());
                newRow.Add(eyeGazeProvider.GazeDirection.normalized.z.ToString());
                if (GazeMonitorObject != null)
                {
                    var GazeMonitorScript = GazeMonitorObject.GetComponent<GazeMonitor>();

                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.x.ToString());
                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.y.ToString());
                    newRow.Add(GazeMonitorScript.GazeDirectionChange.Value.normalized.z.ToString());
                }
                flushCounter += 1;

                AddRow(newRow);
                if (flushCounter == 60)
                {
                    FlushData();
                    flushCounter = 0;
                }

            }

            
        }
        prevGazeDirectionVector = eyeGazeProvider.GazeDirection;
    }

    public void OnDestroy()
    {
        
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
        rowData.Add(m_recordingId);
        rowData.Add(m_recordingId);
        return rowData;
    }
}