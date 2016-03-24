using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

using System.Windows.Media;

namespace WpfApplication2
{
    public enum MediaType
    {
        MEDIA_BEGAIN = 0,
        MEDIA_DEAD,
        MEDIA_EAT,
        MEDIA_OVER,
        MEDIA_WIN,
        MEDIA_GO,
        MEDIA_SELECT
    }

    class MediaBackgroundThread
    {
        static private BackgroundWorker backgroundCalculator = null;
        static private MediaBackgroundThread inner_var = null;

        private MediaBackgroundThread()
        {
            backgroundCalculator = AllocBackgroundWorker();
        }

        private BackgroundWorker AllocBackgroundWorker()
        {
            BackgroundWorker background = new BackgroundWorker();
            background.WorkerSupportsCancellation = true;
            background.DoWork += new DoWorkEventHandler(BackgroundPlay_DoWork);
            background.RunWorkerCompleted +=
                                            new RunWorkerCompletedEventHandler(BackgroundPlay_Completed);

            return background;
        }

        public static void PlayMedia(MediaType type)
        {
            if (inner_var == null)
            {
                inner_var = new MediaBackgroundThread();
            }

            if (backgroundCalculator.IsBusy)
            {
                backgroundCalculator.CancelAsync();
                BackgroundWorker tmp_background = inner_var.AllocBackgroundWorker();
                tmp_background.RunWorkerAsync(type);
            }
            else
            {
                backgroundCalculator.RunWorkerAsync(type);
            }
        }

        private void BackgroundPlay_DoWork(object sender, DoWorkEventArgs e)
        {
            MediaType type = (MediaType)e.Argument;
            e.Result = DoPlaySpecifiedMedia(type, (BackgroundWorker)sender, e);
        }

        private int DoPlaySpecifiedMedia(MediaType type, BackgroundWorker worker, DoWorkEventArgs e)
        {
            MediaPlayer player = null;

            if (worker.CancellationPending)
            {
                Console.WriteLine("CancellationPending !");
                e.Cancel = true;
                return 1;
            }

            switch (type)
            {
                case MediaType.MEDIA_BEGAIN:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\Begin.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_DEAD:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\dead.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_EAT:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\eat.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_OVER:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\gameover.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_WIN:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\gamewin.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_GO:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\go.wav", UriKind.Absolute));
                    player.Play();
                    break;

                case MediaType.MEDIA_SELECT:
                    player = new MediaPlayer();
                    player.Open(new Uri(GameState.gWorkPath + @"\res\voice\select.wav", UriKind.Absolute));
                    player.Play();
                    break;

                default:
                    break;
            }

            return 0;
        }

        private void BackgroundPlay_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("BackgroundPlay_Completed for canceled !");
            }
            else
            {
                Console.WriteLine("BackgroundPlay_Completed for normal play ended !");
            }
        }

    }
}
