using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace OC2TAS
{
    public class VideoRecorder
    {
        Process process;
        int width;
        int height;

        public VideoRecorder()
        {
            width = Screen.width;
            height = Screen.height;

            process = new Process();
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            process.StartInfo.FileName = dir + "\\ffmpeg.exe";
            string arg = "-f image2pipe -pix_fmt yuv420p -s {0}x{1} -an -framerate 50 -i - -c:v libx264 -preset ultrafast -crf 18 -movflags +faststart -y {2}";
            string output = "\"D:/TAS output/output_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4\"";
            process.StartInfo.Arguments = string.Format(arg, width, height, output);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
        }

        public void AddFrame()
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            byte[] bytes = ImageConversion.EncodeToPNG(tex);
            UnityEngine.Object.Destroy(tex);
            BinaryWriter writer = new BinaryWriter(process.StandardInput.BaseStream);
            writer.Write(bytes);
        }
        public void Close()
        {
            // process.WaitForExit();
            process.Close();
        }
    }

    public class AudioRecorder: MonoBehaviour
    {
        const int HEADER_SIZE = 44;
        private bool recording;
        private FileStream fileStream;
        public float totalTime;

        public void OnDestroy()
        {
            EndRecord();
        }

        public void OnAudioFilterRead(float[] data, int channels)
        {
            if (recording) ConvertAndWrite(data);
        }

        public void StartRecord()
        {
            recording = true;
            string path = "D:/TAS output/output_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav";
            fileStream = CreateEmpty(path);
            totalTime = 0f;
        }

        public void EndRecord()
        {
            recording = false;
            WriteHeader();
        }

        static FileStream CreateEmpty(string path)
        {
            var fileStream = new FileStream(path, FileMode.Create);
            byte emptyByte = new byte();
            for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
                fileStream.WriteByte(emptyByte);
            return fileStream;
        }

        private void ConvertAndWrite(float[] samples)
        {
            float min = 0.001f;
            int i1;
            for (i1 = 0; i1 < samples.Length; i1++)
                if (Mathf.Abs(samples[i1]) > min)
                    break;

            int i2;
            for (i2 = samples.Length - 1; i2 > 0; i2--)
                if (Mathf.Abs(samples[i2]) > min)
                    break;

            int len = i2 - i1 + 1;
            if (len <= 0) return;
            totalTime += (float)len / AudioSettings.outputSampleRate / 2;

            Int16[] intData = new Int16[len];
            //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

            Byte[] bytesData = new Byte[len * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            int rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < len; i++)
            {
                intData[i] = (short)(samples[i1 + i] * rescaleFactor);
                Byte[] byteArr;
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            fileStream.Write(bytesData, 0, bytesData.Length);
        }

        private void WriteHeader()
        {
            int hz = AudioSettings.outputSampleRate;

            fileStream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            fileStream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
            fileStream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            fileStream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            fileStream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            fileStream.Write(subChunk1, 0, 4);

            UInt16 four = 4;
            UInt16 two = 2;
            UInt16 one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            fileStream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(two);
            fileStream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(hz);
            fileStream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(hz * 4);
            fileStream.Write(byteRate, 0, 4);

            Byte[] blockAlign = BitConverter.GetBytes(four);
            fileStream.Write(blockAlign, 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            fileStream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            fileStream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - HEADER_SIZE);
            fileStream.Write(subChunk2, 0, 4);

            fileStream.Close();
        }
    }
}