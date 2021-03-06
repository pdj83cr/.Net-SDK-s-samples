﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Computer_Vision_Demo
{
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    using VisioForge.Controls.CV;
    using VisioForge.Types;
    using VisioForge.Types.Sources;

    public partial class Form1 : Form
    {
        private FaceDetector faceDetector;

        private CarCounter carCounter;

        private PedestrianDetector pedestrianDetector;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text += " (SDK v" + VideoCapture1.SDK_Version + ", " + VideoCapture1.SDK_State + ")";

            foreach (var device in VideoCapture1.Video_CaptureDevicesInfo)
            {
                cbVideoInputDevice.Items.Add(device.Name);
            }

            if (cbVideoInputDevice.Items.Count > 0)
            {
                cbVideoInputDevice.SelectedIndex = 0;
                cbVideoInputDevice_SelectedIndexChanged(null, null);
            }

            cbIPCameraType.SelectedIndex = 0;
        }

        #region Face detection
        private void FaceDetectionAdd()
        {
            faceDetector = new FaceDetector
            {
                DrawEnabled = cbFDDraw.Checked,
                DrawColor = Color.Green,
                FramesToSkip = tbFDSkipFrames.Value,
                MinNeighbors = tbFDMinNeighbors.Value,
                ScaleFactor = tbFDScaleFactor.Value / 100.0f,
                VideoScale = tbFDDownscale.Value / 10.0f
            };

            if (rbFDCircle.Checked)
            {
                this.faceDetector.DrawShapeType = CVShapeType.Circle;
            }
            else
            {
                this.faceDetector.DrawShapeType = CVShapeType.Rectangle;
            }

            var path = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
            faceDetector.Init(
                path + "haarcascade_frontalface_default.xml",
                path + "haarcascade_eye.xml",
                path + "haarcascade_mcs_nose.xml",
                path + "haarcascade_mcs_mouth.xml",
                true);

            faceDetector.OnFaceDetected += OnFaceDetected;
        }

        private void FaceDetectionRemove()
        {
            if (this.faceDetector != null)
            {
                this.faceDetector.OnFaceDetected -= OnFaceDetected;
                this.faceDetector.Dispose();
                this.faceDetector = null;
            }
        }

        private void OnFaceDetected(object sender, OnCVFaceDetectedArgs e)
        {
            if (e.Faces.Length == 0)
            {
                return;
            }

            BeginInvoke(
                (Action)(() =>
                {
                    edFDFaces.Text = string.Empty;
                    foreach (var face in e.Faces)
                    {
                        edFDFaces.Text += $"Face at {face.Position.ToNiceString()}" + Environment.NewLine;
                    }
                }));
        }

        private void tbFDSkipFrames_Scroll(object sender, EventArgs e)
        {
            lbFDSkipFrames.Text = tbFDSkipFrames.Value.ToString();
        }

        private void tbFDDownscale_Scroll(object sender, EventArgs e)
        {
            lbFDDownscale.Text = (tbFDDownscale.Value / 10.0).ToString("F2");
        }

        private void tbMinNeighbors_Scroll(object sender, EventArgs e)
        {
            lbFDMinNeighbors.Text = tbFDMinNeighbors.Value.ToString();
        }

        private void tbScaleFactor_Scroll(object sender, EventArgs e)
        {
            lbFDScaleFactor.Text = (tbFDScaleFactor.Value / 100.0).ToString("F2");
        }

        #endregion

        #region Pedestrian detection
        private void PedestrianDetectionAdd()
        {
            pedestrianDetector = new PedestrianDetector()
            {
                DrawEnabled = cbPDDraw.Checked,
                DrawColor = Color.Green,
                FramesToSkip = tbPDSkipFrames.Value,
                VideoScale = tbPDDownscale.Value / 10.0f
            };

            pedestrianDetector.Init();

            pedestrianDetector.OnPedestrianDetected += OnPedestrianDetected;
        }

        private void PedestrianDetectionRemove()
        {
            if (pedestrianDetector != null)
            {
                pedestrianDetector.OnPedestrianDetected -= OnPedestrianDetected;
                pedestrianDetector.Dispose();
                pedestrianDetector = null;
            }
        }

        private void OnPedestrianDetected(object sender, OnCVPedestrianDetectedArgs e)
        {
            if (e.Items.Length == 0)
            {
                return;
            }

            BeginInvoke(
                (Action)(() =>
                {
                    edPDDetected.Text = string.Empty;
                    foreach (var item in e.Items)
                    {
                        edPDDetected.Text += $"Object at {item.ToNiceString()}" + Environment.NewLine;
                    }
                }));
        }

        private void tbPDDownscale_Scroll(object sender, EventArgs e)
        {
            lbPDDownscale.Text = (tbPDDownscale.Value / 10.0).ToString("F2");
        }

        private void tbPDSkipFrames_Scroll(object sender, EventArgs e)
        {
            lbPDSkipFrames.Text = tbPDSkipFrames.Value.ToString();
        }

        #endregion

        #region Car counter
        private void CarCounterAdd()
        {
            carCounter = new CarCounter()
            {
                ContoursDraw = cbCCDraw.Checked,
                TrackingLineDraw = cbCCDraw.Checked,
                CounterDraw = cbCCDraw.Checked
            };

            carCounter.Init();
            carCounter.OnCarsDetected += this.OnCarsDetected;
        }

        private void OnCarsDetected(object sender, OnCVCarDetectedArgs e)
        {
            BeginInvoke(
                (Action)(() =>
                {
                    edCCDetectedCars.Text = e.CarsCount.ToString();
                }));
        }

        private void CarCounterRemove()
        {
            if (carCounter != null)
            {
                carCounter.OnCarsDetected -= this.OnCarsDetected;
                carCounter.Dispose();
                carCounter = null;
            }
        }

        #endregion

        //IntPtr pd = IntPtr.Zero;


        private void ProcessFrame(RAWImage frame)
        {
            //if (pd == IntPtr.Zero)
            //{
            //    pd = CV.PedestrianDetectorInit();
            //    CV.PedestrianDetectorSetEngineSettings(pd, 0.5, 5, true, Color.YellowGreen);
            //}

            //long time;
            //CVPedestrians items = new CVPedestrians();
            //int count = CV.PedestrianDetectorProcess(pd, frame, ref items, out time);
            //Trace.WriteLine($"Count: {count}, time: {time}");

            faceDetector?.Process(frame);
            carCounter?.Process(frame);
            pedestrianDetector?.Process(frame);
        }

        #region Video capture source

        private void VideoCapture1_OnVideoFrameBuffer(object sender, VideoFrameBufferEventArgs e)
        {
            ProcessFrame(e.Frame);
        }

        private void cbVideoInputDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbVideoInputDevice.SelectedIndex != -1)
            {
                cbVideoInputFormat.Items.Clear();

                var deviceItem = VideoCapture1.Video_CaptureDevicesInfo.First(device => device.Name == cbVideoInputDevice.Text);
                if (deviceItem == null)
                {
                    return;
                }

                foreach (string format in deviceItem.VideoFormats)
                {
                    cbVideoInputFormat.Items.Add(format);
                }

                if (cbVideoInputFormat.Items.Count > 0)
                {
                    cbVideoInputFormat.SelectedIndex = 0;
                }

                cbFramerate.Items.Clear();

                foreach (string frameRate in deviceItem.VideoFrameRates)
                {
                    cbFramerate.Items.Add(frameRate);
                }

                if (cbFramerate.Items.Count > 0)
                {
                    cbFramerate.SelectedIndex = 0;
                }
            }
        }

        private void SelectIPCameraSource(out IPCameraSourceSettings settings)
        {
            settings = new IPCameraSourceSettings
            {
                URL = edIPUrl.Text
            };

            switch (cbIPCameraType.SelectedIndex)
            {
                case 0:
                    settings.Type = VFIPSource.Auto_VLC;
                    break;
                case 1:
                    settings.Type = VFIPSource.Auto_FFMPEG;
                    break;
                case 2:
                    settings.Type = VFIPSource.Auto_LAV;
                    break;
                case 3:
                    settings.Type = VFIPSource.RTSP_Live555;
                    break;
                case 4:
                    settings.Type = VFIPSource.HTTP_FFMPEG;
                    break;
                case 5:
                    settings.Type = VFIPSource.MMS_WMV;
                    break;
                case 6:
                    settings.Type = VFIPSource.RTSP_UDP_FFMPEG;
                    break;
                case 7:
                    settings.Type = VFIPSource.RTSP_TCP_FFMPEG;
                    break;
                case 8:
                    settings.Type = VFIPSource.RTSP_HTTP_FFMPEG;
                    break;
            }

            settings.AudioCapture = false;
            settings.Login = edIPLogin.Text;
            settings.Password = edIPPassword.Text;
            settings.Debug_Enabled = cbDebugMode.Checked;
            settings.Debug_Filename = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                      + "\\VisioForge\\ip_cam_log.txt";
        }

        private void SelectVideoCaptureSource()
        {
            VideoCapture1.Video_CaptureDevice = cbVideoInputDevice.Text;
            VideoCapture1.Video_CaptureDevice_Format_UseBest = cbUseBestVideoInputFormat.Checked;
            VideoCapture1.Video_CaptureDevice_Format = cbVideoInputFormat.Text;

            if (cbFramerate.SelectedIndex != -1)
            {
                VideoCapture1.Video_CaptureDevice_FrameRate = Convert.ToDouble(cbFramerate.Text, CultureInfo.CurrentCulture);
            }
        }

        private void VideoCapture1_OnError(object sender, ErrorsEventArgs e)
        {
            mmLog.Text = mmLog.Text + e.Message + Environment.NewLine;
        }

        private void ConfigureVideoCapture()
        {
            // select source
            VideoCapture1.Debug_Mode = cbDebugMode.Checked;
            VideoCapture1.Debug_Dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VisioForge\\";
            VideoCapture1.VLC_Path = Environment.GetEnvironmentVariable("VFVLCPATH");

            if (rbVideoCaptureDevice.Checked)
            {
                VideoCapture1.Mode = VFVideoCaptureMode.VideoPreview;
            }
            else
            {
                VideoCapture1.Mode = VFVideoCaptureMode.IPPreview;
            }

            if ((VideoCapture1.Mode == VFVideoCaptureMode.IPCapture) || (VideoCapture1.Mode == VFVideoCaptureMode.IPPreview))
            {
                // from IP camera
                IPCameraSourceSettings settings;
                SelectIPCameraSource(out settings);
                VideoCapture1.IP_Camera_Source = settings;
            }
            else if ((VideoCapture1.Mode == VFVideoCaptureMode.VideoCapture) || (VideoCapture1.Mode == VFVideoCaptureMode.VideoPreview) ||
                (VideoCapture1.Mode == VFVideoCaptureMode.AudioCapture) || (VideoCapture1.Mode == VFVideoCaptureMode.AudioPreview))
            {
                // from video capture device
                SelectVideoCaptureSource();
            }

            VideoCapture1.Audio_RecordAudio = false;
            VideoCapture1.Audio_PlayAudio = false;

            VideoCapture1.Video_Sample_Grabber_Enabled = true;
        }

        #endregion

        #region Media Player

        private void ConfigureMediaPlayer()
        {
            MediaPlayer1.Debug_Mode = cbDebugMode.Checked;
            MediaPlayer1.Debug_Dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VisioForge\\";

            MediaPlayer1.Source_Mode = VFMediaPlayerSource.LAV;
            MediaPlayer1.FilenamesOrURL.Clear();
            MediaPlayer1.FilenamesOrURL.Add(edFilename.Text);

            MediaPlayer1.Video_Renderer.Video_Renderer = VFVideoRenderer.EVR;
        }

        private void MediaPlayer1_OnVideoFrameBuffer(object sender, VideoFrameBufferEventArgs e)
        {
            ProcessFrame(e.Frame);
        }

        #endregion

        private void btStart_Click(object sender, EventArgs e)
        {
            mmLog.Clear();

            if (rbVideoFile.Checked)
            {
                ConfigureMediaPlayer();
            }
            else
            {
                ConfigureVideoCapture();
            }

            // add face detection
            if (cbFDEnabled.Checked)
            {
                FaceDetectionAdd();
            }

            // add car counter
            if (cbCCEnabled.Checked)
            {
                CarCounterAdd();
            }

            // add car counter
            if (cbPDEnabled.Checked)
            {
                PedestrianDetectionAdd();
            }

            if (rbVideoFile.Checked)
            {
                MediaPlayer1.Show();
                VideoCapture1.Hide();
                MediaPlayer1.Play();
            }
            else
            {
                MediaPlayer1.Hide();
                VideoCapture1.Show();
                VideoCapture1.Start();
            }
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            VideoCapture1.Stop();
            MediaPlayer1.Stop();

            FaceDetectionRemove();
            CarCounterRemove();
            PedestrianDetectionRemove();
        }

        private void btOpenFile_Click(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                edFilename.Text = dlgOpenFile.FileName;
            }
        }
    }
}
