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
using System.Diagnostics;

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
    private const string CSVHeader = "Timestamp,TimeInMs,Time to Conduct Pairing,Number of Clients,Pairing Success?";
    private const string SessionFolderRoot = "CSVLogger";
    #endregion

    #region private members
    private string m_sessionPath;
    private string m_filePath;
    private string m_recordingId;
    private string m_sessionId;
    private int flushCounter;
    Stopwatch clock;


    private StringBuilder m_csvData;
    #endregion
    #region public members
    public string RecordingInstance => m_recordingId;
    public static LoggerScript Instance = null;



    #endregion

    async void Start()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        await MakeNewSession();
        StartNewCSV();
        flushCounter = 0;

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
        UnityEngine.Debug.Log("CSVLogger logging data to " + m_sessionPath);
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


    }

    public void OnDestroy()
    {
        FlushData();
    }
    public void startGridPairAttempt()
    {

        clock = Stopwatch.StartNew();
        
    }


    public void stopTimer()
    {
        clock.Stop();

    }

    public void stopGridPairAttempt()
    {
            List<String> newRow = RowWithStartData();
            newRow.Add(clock.Elapsed.ToString());
            newRow.Add(NetworkManager.Singleton.ConnectedClients.Keys.Count.ToString());
            var cryptoInstance = GameObject.Find("GazePairCrypto");
            newRow.Add(cryptoInstance.GetComponent<GazePairCrypto>().pairSucessful.ToString());
            AddRow(newRow);
            FlushData();

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