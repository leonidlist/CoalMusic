using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using CSCore.CoreAudioAPI;
using Microsoft.Win32;

namespace MusicPlayer
{
    class MainViewModel:PropertyChangedBase
    {
        private int _volume = 50;
        private int _timeProgress;
        private int _timeMin;
        private int _timeMax;
        private string _playButtonIcon;
        private string _trackName;
        private Player _player;
        private List<MMDevice> _devices;
        private Timer _timer;

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                _player.Volume = _volume;
                NotifyOfPropertyChange(() => Volume);
            }
        }

        public string PlayButtonIcon
        {
            get => _playButtonIcon;
            set
            {
                _playButtonIcon = value;
                NotifyOfPropertyChange(() => PlayButtonIcon);
            }
        }

        public int TimeProgress
        {
            get => _timeProgress;
            set
            {
                _timeProgress = value;
                NotifyOfPropertyChange(() => TimeProgress);
            }
        }

        public int TimeMin
        {
            get => _timeMin;
            set
            {
                _timeMin = value;
                NotifyOfPropertyChange(() => TimeMin);
            }
        }

        public int TimeMax
        {
            get => _timeMax;
            set
            {
                _timeMax = value;
                NotifyOfPropertyChange(() => TimeMax);
            }
        }

        public string TrackName
        {
            get => _trackName;
            set
            {
                _trackName = value;
                NotifyOfPropertyChange(() => TrackName);
            }
        }

        public MainViewModel()
        {
            _player = new Player();
            _devices = new List<MMDevice>();
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    foreach (var device in mmdeviceCollection)
                    {
                        _devices.Add(device);
                    }
                }
            }

            _player.PlaybackChanged += (e, a) => {
                switch (_player.PlaybackState)
                {
                    case CSCore.SoundOut.PlaybackState.Paused:
                        PlayButtonIcon = "Play";
                        break;
                    case CSCore.SoundOut.PlaybackState.Playing:
                        PlayButtonIcon = "Pause";
                        break;
                    default:
                        PlayButtonIcon = "WarningCircle";
                        break;
                }
            };

            _timer = new Timer(new TimerCallback((x) => { TimeProgress = _player.Position.Seconds; }), null, 0, 1000);
        }

        public void OpenTrack()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.ShowDialog();
            _player.Open(dlg.FileName, _devices[0]);
            PlayButtonIcon = "Play";
            TrackName = System.IO.Path.GetFileName(dlg.FileName);
            TimeMin = 0;
            TimeMax = (_player.Length.Minutes*60) + _player.Length.Seconds;
            _player.Volume = Volume;
        }

        public void PlayStopMusic()
        {
            if (_player.PlaybackState == CSCore.SoundOut.PlaybackState.Playing)
                _player.Pause();
            else if (_player.PlaybackState == CSCore.SoundOut.PlaybackState.Paused)
                _player.Play();
            else if(_player.PlaybackState == CSCore.SoundOut.PlaybackState.Stopped)
                _player.Play();
        }

        ~MainViewModel()
        {
            _player.Dispose();
        }
    }
}
