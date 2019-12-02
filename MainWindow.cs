using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Video.DirectShow;
using AForge.Video;
using Accord.Video.VFW;
using Accord.Video.FFMPEG;
using AForge.Imaging.Filters;


namespace USBCam
{
    public partial class MainWindow : Form
    {
        FilterInfoCollection videoDevicesList;
        VideoCaptureDevice cameraOne;
        VideoCaptureDevice cameraTwo;
        int brightess1 = 0;
        int contrast1 = 0;
        int saturation1 = 0;
        int brightess2 = 0;
        int contrast2 = 0;
        int saturation2 = 0;
        bool isRecording1 = false;
        bool isRecording2 = false;
       // bool startButtonClicked1 = false;
        //bool startButtonClicked2 = false;
        //VideoCaptureDevice videoSource;
        VideoFileWriter writer;
        private DateTime? firstFrameTime;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (videoSource.IsRunning)
           // {
            //    videoSource.Stop();
            //}
        }
        private void button_searchDev_Click(object sender, EventArgs e)
        {
            videoDevicesList = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevicesList)
            {
                cmbDevList1.Items.Add(videoDevice.Name);
                cmbDevList2.Items.Add(videoDevice.Name);
            }
        }

        private void CameraOne_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (isRecording1)
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    if (firstFrameTime != null)
                    {
                        writer.WriteVideoFrame(bitmap, DateTime.Now - firstFrameTime.Value);
                    }
                    else
                    {
                        writer.WriteVideoFrame(bitmap);
                        firstFrameTime = DateTime.Now;
                    }
                }
            }

            Bitmap bitmap1 = (Bitmap)eventArgs.Frame.Clone();
            BrightnessCorrection br = new BrightnessCorrection(brightess1);
            ContrastCorrection cr = new ContrastCorrection(contrast1);
            SaturationCorrection sr = new SaturationCorrection(saturation1);
            bitmap1 = br.Apply((Bitmap)bitmap1.Clone());
            bitmap1 = cr.Apply((Bitmap)bitmap1.Clone());
            bitmap1 = sr.Apply((Bitmap)bitmap1.Clone());
            try
            { 
                pbCam1.Image = bitmap1;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("A problem with image loading occured! You may want to close other apps using your camera!");
            }
        }

        private void CameraTwo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap2 = (Bitmap)eventArgs.Frame.Clone();
            BrightnessCorrection br = new BrightnessCorrection(brightess2);
            ContrastCorrection cr = new ContrastCorrection(contrast2);
            SaturationCorrection sr = new SaturationCorrection(saturation2);
            bitmap2 = br.Apply((Bitmap)bitmap2.Clone());
            bitmap2 = cr.Apply((Bitmap)bitmap2.Clone());
            bitmap2 = sr.Apply((Bitmap)bitmap2.Clone());

            try
            {
                pbCam2.Image = bitmap2;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("A problem with image loading occured! You may want to close other apps using your camera!");
            }

            if (isRecording2)
            {
                writer.WriteVideoFrame(bitmap2);
            }
        }

        private void buttonSsCam1_Click(object sender, EventArgs e)
        {
            try
            {
                cameraOne = new VideoCaptureDevice(videoDevicesList[cmbDevList1.SelectedIndex].MonikerString);
                cameraOne.NewFrame += new NewFrameEventHandler(CameraOne_NewFrame);
                cameraOne.Start();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void buttonCamOneStop_Click(object sender, EventArgs e)
        {
            try
            {
                cameraOne.Stop();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void buttonSsCam2_Click(object sender, EventArgs e)
        {
            try
            {
                cameraTwo = new VideoCaptureDevice(videoDevicesList[cmbDevList2.SelectedIndex].MonikerString);
                cameraTwo.NewFrame += new NewFrameEventHandler(CameraTwo_NewFrame);
                cameraTwo.Start();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void buttonCamTwoStop_Click(object sender, EventArgs e)
        {
            try
            {
                cameraTwo.Stop();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void buttonPictureCamOne_Click(object sender, EventArgs e)
        {
            buttonCamOneStop_Click(sender, e);
            try
            {
                Bitmap picture = (Bitmap)pbCam1.Image;
                saveFileDialog.Filter = "Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save an Image File";
                saveFileDialog.ShowDialog();
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
                picture.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                fs.Close();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("No writable image found! Make sure the camera view is visible in the application window!");
            }
            
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }

        private void buttonPictureCamTwo_Click(object sender, EventArgs e)
        {
            try
            {
                buttonCamTwoStop_Click(sender, e);
                Bitmap picture = (Bitmap)pbCam2.Image;
                saveFileDialog.Filter = "Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save an Image File";
                saveFileDialog.ShowDialog();
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog.OpenFile();
                picture.Save(fs, System.Drawing.Imaging.ImageFormat.Bmp);
                fs.Close();
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("No writable image found! Make sure the camera view is visible in the application window!");
            }
        }

        private void buttonRecordingCamOne_Click(object sender, EventArgs e)
        {
            if (cameraOne.IsRunning)
            {
                try
                {
                    var dialog = new SaveFileDialog();
                    dialog.FileName = "Video1";
                    dialog.DefaultExt = ".avi";
                    dialog.AddExtension = true;
                    var dialogresult = dialog.ShowDialog();
                    if (dialogresult != DialogResult.OK)
                    {
                        return;
                    }
                    firstFrameTime = null;
                    isRecording1 = true;
                    writer = new VideoFileWriter();
                    writer.Open(dialog.FileName, pbCam1.Image.Width, pbCam1.Image.Height, 30, VideoCodec.MPEG4);
                    buttonRecordingCamOne.Enabled = false;
                    buttonEndRecordingCamOne.Enabled = true;
                }
                catch
                {

                }
            }
        }

        private void buttonEndRecordingCamOne_Click(object sender, EventArgs e)
        {
            if (cameraOne.IsRunning)
            {
                isRecording1 = false;
                writer.Close();
                writer.Dispose();
                buttonRecordingCamOne.Enabled = true;
                buttonEndRecordingCamOne.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void jasnosc1TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraOne.IsRunning)
                    brightess1 = jasnosc1TrackBar.Value;
            }
            catch(NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }
        private void jasnosc2TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraTwo.IsRunning)
                    brightess2 = jasnosc2TrackBar.Value;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }
        private void kontrast1TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraOne.IsRunning)
                    contrast1 = kontrast1TrackBar.Value;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void nasycenie1TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraOne.IsRunning)
                    saturation1 = nasycenie1TrackBar.Value;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }
        private void kontrast2TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraTwo.IsRunning)
                    contrast2 = kontrast2TrackBar.Value;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void nasycenie2TrackBar_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (cameraTwo.IsRunning)
                    saturation2 = nasycenie2TrackBar.Value;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Camera not yet chosen!");
            }
        }

        private void cmbDevList1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pbCam1_Click(object sender, EventArgs e)
        {

        }

        /*private void trackBar1_Scroll(object sender, EventArgs e)
{

} */
    }
}

// https://www.mesta-automation.com/getting-started-with-computer-vision-in-c-sharp/