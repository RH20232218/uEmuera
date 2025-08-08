namespace uEmuera.Media.SystemSounds
{
    public static class Hand
    {
        public static void Play()
        {
            uEmuera.Logger.Info("uEmuera.Media.SystemSounds.Hand.Play");
        }
    }
    public static class Asterisk
    {
        public static void Play()
        {
            uEmuera.Logger.Info("uEmuera.Media.SystemSounds.Asterisk.Play");
        }
    }
}

namespace uEmuera.Media
{
    public static class Audio
    {
        static int bgmVolume = 100;
        static int seVolume = 100;
        public static void PlayBGM(string path)
        {
            uEmuera.Logger.Info($"PlayBGM: {path} vol={bgmVolume}");
        }
        public static void StopBGM()
        {
            uEmuera.Logger.Info("StopBGM");
        }
        public static void PlaySE(string path)
        {
            uEmuera.Logger.Info($"PlaySE: {path} vol={seVolume}");
        }
        public static void StopSE()
        {
            uEmuera.Logger.Info("StopSE");
        }
        public static void SetBGMVolume(int volume)
        {
            if (volume < 0) volume = 0; if (volume > 100) volume = 100;
            bgmVolume = volume;
            uEmuera.Logger.Info($"BGM Volume: {bgmVolume}");
        }
        public static void SetSEVolume(int volume)
        {
            if (volume < 0) volume = 0; if (volume > 100) volume = 100;
            seVolume = volume;
            uEmuera.Logger.Info($"SE Volume: {seVolume}");
        }
    }
}
