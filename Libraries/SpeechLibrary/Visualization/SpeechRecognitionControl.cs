using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuxiliaryLibrary;
using AudioLibrary;
using AudioLibrary.Visualization;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Speech.Recognition;

namespace SpeechLibrary.Visualization
{
    public partial class SpeechRecognitionControl : SoundVisualizer
    {
        private const int DEFAULT_DISPLAY_MILLISECOND_SLEEP_TIME = 200;
   //     private const int DEFAULT_RECOGNIZER_MILLISECOND_SLEEP_TIME = 200;
        private const int DEFAULT_DEVICE_NUMBER = 0;
        private const int DEFAULT_SAMPLE_RATE = 44100;
        private const short DEFAULT_BITS_PER_SAMPLE = 16;
        private const short DEFAULT_NUMBER_OF_CHANNELS = 2;
        private const double DEFAULT_DISPLAY_DURATION = 2;
        private const double DEFAULT_STORAGE_DURATION = 4;
        private const int DEFAULT_MOVING_AVERAGE_LENGTH = 10;
        private const int DEFAULT_DETECTION_THRESHOLD = 500; // 300;
        private const double DEFAULT_SILENCE_TIME_MARGIN = 0.50;
        private const double DEFAULT_SNIPPET_DURATION = 0.100; // s (NAudio constant)

        private IWaveIn captureDevice;
        private SpeechRecognitionEngine speechRecognitionEngine = null;
        private List<WAVSound> snippetList = null;
        private Thread displayThread = null;
        private Boolean displayRunning = false;
        private Thread recognitionThread = null;
        private static object lockObject = new object();
        private int displayMillisecondSleepTime = DEFAULT_DISPLAY_MILLISECOND_SLEEP_TIME;
        private int deviceNumber = DEFAULT_DEVICE_NUMBER;
        private int sampleRate = DEFAULT_SAMPLE_RATE;
        private short bitsPerSample = DEFAULT_BITS_PER_SAMPLE;
        private short numberOfChannels = DEFAULT_NUMBER_OF_CHANNELS;
        private double storageDuration = DEFAULT_STORAGE_DURATION;
        private Boolean showSoundStream = false;
        private Boolean inUtterance = false;
        private int movingAverageLength = DEFAULT_MOVING_AVERAGE_LENGTH;
        private int detectionThreshold = DEFAULT_DETECTION_THRESHOLD;
        private double silenceTimeMargin = DEFAULT_SILENCE_TIME_MARGIN;
        private double displayDuration = DEFAULT_DISPLAY_DURATION;
        private int startSnippetIndex = 0;
        private int silenceIndexDuration = 0;
        private int firstSilenceIndex = 0;
        private WAVSound displaySound = null;
        private double snippetDuration = DEFAULT_SNIPPET_DURATION; // Default value in NAudio.
        private Boolean recognizeAutomatically = true;

        public event EventHandler<StringEventArgs> SoundRecognized = null;
        public event EventHandler<WAVSoundEventArgs> SoundDetected = null;

        public SpeechRecognitionControl()
        {
            InitializeComponent();
        }

        private IWaveIn CreateWaveInDevice()
        {
            IWaveIn newWaveIn;
            newWaveIn = new WaveInEvent() { DeviceNumber = deviceNumber };
            int channels = 2;
            newWaveIn.WaveFormat = new NAudio.Wave.WaveFormat(sampleRate, channels);
            newWaveIn.DataAvailable += OnDataAvailable;
            newWaveIn.RecordingStopped += OnRecordingStopped;
            return newWaveIn;
        }

        public static List<string> GetDeviceNames()
        {
            List<string> deviceNameList = new List<string>();
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                deviceNameList.Add(deviceInfo.ProductName);
            }
            return deviceNameList;
        }

        private void ShowSound(WAVSound sound)
        {
            ClearHistory();
            SetSound(sound);
        }

        private void DisplayLoop()
        {
            while (displayRunning)
            {
                Thread.Sleep(displayMillisecondSleepTime);
                if (displaySound != null)
                {
                    if (Monitor.TryEnter(lockObject, 1))
                    {
                        if (InvokeRequired) { this.BeginInvoke(new MethodInvoker(() => ShowSound(displaySound))); }
                        else { ShowSound(displaySound); }
                        Monitor.Exit(lockObject);
                    }
                }
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Monitor.Enter(lockObject);
                WAVSound sound = new WAVSound("", sampleRate, numberOfChannels, bitsPerSample);
                sound.AppendSamplesAsBytes(e.Buffer);
                snippetDuration = sound.GetDuration();
                if (displaySound == null) { displaySound = sound.Copy(); }
                else { displaySound.Append(sound); }
                if (displaySound.GetDuration() > displayDuration)
                {
                    displaySound = displaySound.Extract(displaySound.GetDuration() - displayDuration, displaySound.GetDuration());
                }
                if (snippetList == null) { snippetList = new List<WAVSound>(); }
                snippetList.Add(sound);
                double totalDuration = snippetList.Count*snippetList[0].Samples[0].Count / (double)snippetList[0].SampleRate;
                if (totalDuration > storageDuration)
                {
                    snippetList.RemoveAt(0);
                    startSnippetIndex--;
                    firstSilenceIndex--;
                }
                RunDetection(sound, snippetList.Count-1);
                sw.Stop();
                double elapsedTime = sw.ElapsedTicks / (double)Stopwatch.Frequency;
                Monitor.Exit(lockObject);
            }
        }

        private void RunDetection(WAVSound sound, int snippetIndex)
        {
            if (!inUtterance)
            {
                double detectionStart = sound.GetFirstTimeAboveThreshold(0, movingAverageLength, detectionThreshold);
                if (detectionStart > 0)
                {
                    inUtterance = true;
                    startSnippetIndex = snippetIndex;
                    silenceIndexDuration = 0;
                }
            }
            else
            {
                double detectionStart = sound.GetFirstTimeAboveThreshold(0, movingAverageLength, detectionThreshold);
                if (detectionStart < 0)  // Nothing detected.
                {
                    if (silenceIndexDuration == 0) { firstSilenceIndex = snippetIndex; }
                    silenceIndexDuration++;
                    double silenceDuration = silenceIndexDuration * snippetDuration;
                    if (silenceDuration > silenceTimeMargin)
                    {
                        inUtterance = false;
                        if (startSnippetIndex < 0) { startSnippetIndex = 0; } // In case the start of the sound has been shifted outside the storage duration
                        if (firstSilenceIndex < 0) { firstSilenceIndex = 0; } // Should not happen, if the storage duration is sufficient (= large than the required end silence)
                        List<WAVSound> detectionList = snippetList.GetRange(startSnippetIndex, firstSilenceIndex-startSnippetIndex + 1);
                        WAVSound soundToRecognize = WAVSound.Join(detectionList, null);
                        if (recognizeAutomatically)
                        {
                            RunRecognizer(soundToRecognize);
                        }
                    }
                }
            }
        }

        private void RecognitionLoop(WAVSound soundToRecognize)
        {
            try
            {
                soundToRecognize.GenerateMemoryStream();
                speechRecognitionEngine.SetInputToWaveStream(soundToRecognize.WAVMemoryStream);
                RecognitionResult r = speechRecognitionEngine.Recognize();
                if (r != null)
                {
                    OnSoundRecognized(r);
                }
                OnSoundDetected(soundToRecognize);
            }
            catch
            {
                // Nothing to do here - try-catch needed to avoid (rare) crashes when the WAVE stream cannot be found.
            }
            finally
            {
               // Nothing to do here ...
            }
        }

        private void OnSoundRecognized(RecognitionResult r)
        {
            if (SoundRecognized != null)
            {
                EventHandler<StringEventArgs> handler = SoundRecognized;
                StringEventArgs e = new StringEventArgs(r.Text);
                handler(this, e);
            }
        }

        private void OnSoundDetected(WAVSound detectedSound)
        {
            if (SoundRecognized != null)
            {
                EventHandler<WAVSoundEventArgs> handler = SoundDetected;
                WAVSoundEventArgs e = new WAVSoundEventArgs(detectedSound);
                handler(this, e);
            }
        }

        public void RunRecognizer(WAVSound soundToRecognize)
        {
            recognitionThread = new Thread(new ThreadStart(() => RecognitionLoop(soundToRecognize)));
            recognitionThread.Start();
        }

        private void StartDisplay()
        {
            if (!displayRunning)
            {
                displayRunning = true;
                displayThread = new Thread(new ThreadStart(DisplayLoop));
                displayThread.Start();
            }
        }

        private void StopDisplay()
        {
            if (displayRunning)
            {
                displayRunning = false;
            }
        }

        public void StartContinuousRecording()
        {
            if (captureDevice == null)
            {
                snippetList = null;
                displaySound = null;
                if (showSoundStream) { StartDisplay(); }
                TimerResolution.TimeBeginPeriod(1);
                captureDevice = CreateWaveInDevice();
                captureDevice.StartRecording();
            }
        }

        public void StopContinuousRecording()
        {
            if (captureDevice != null)
            {
                displayRunning = false;
                captureDevice.StopRecording();
                snippetList = null;
                inUtterance = false;
                displaySound = null;
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            captureDevice = null;
        }

        public void SetSpeechRecognitionEngine(SpeechRecognitionEngine speechRecognitionEngine)
        {
            this.speechRecognitionEngine = speechRecognitionEngine;
        }

        public Boolean ShowSoundStream
        {
            get { return showSoundStream; }
            set
            {
                showSoundStream = value;
                if (showSoundStream) { StartDisplay(); }
                else { StopDisplay(); }
            }
        }

        public int DeviceID
        {
            get { return deviceNumber; }
            set { deviceNumber = value; }
        }

        public int SampleRate
        {
            get { return sampleRate; }
            set
            {
                if (captureDevice == null)
                {
                    sampleRate = value;
                }
            }
        }

        public int MovingAverageLength
        {
            get { return movingAverageLength; }
            set
            {
                if (captureDevice == null) { movingAverageLength = value; }
            }
        }

        public int DetectionThreshold
        {
            get { return detectionThreshold; }
            set
            {
                if (captureDevice == null) { detectionThreshold = value; }
            }
        }

        public double StorageDuration
        {
            get { return storageDuration; }
            set
            {
                if (captureDevice == null) { storageDuration = value; }
            }
        }

        public double DisplayDuration
        {
            get { return displayDuration; }
            set
            {
                if (captureDevice == null) { displayDuration = value; }
            }
        }

        public int DisplayMillisecondSleepTime
        {
            get { return displayMillisecondSleepTime; }
            set
            {
                if (captureDevice == null) { displayMillisecondSleepTime = value; }
            }
        }

        public double SilenceTimeMargin
        {
            get { return silenceTimeMargin; }
            set { silenceTimeMargin = value; }
        }

        public Boolean RecognizeAutomatically
        {
            get { return recognizeAutomatically; }
            set
            {
                if (captureDevice == null) { recognizeAutomatically = value; }
            }
        }
    }
}
