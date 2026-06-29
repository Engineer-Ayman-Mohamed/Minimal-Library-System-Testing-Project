namespace LibrarySystem.LoadTests.Configs;

public static class LoadTestConfig
{
    public static class Load
    {
        public const int Users = 30;
        public const int DurationSeconds = 60;
        public const int ThinkTimeMs = 2000;
        public const int ExpectedMinThroughput = 50;
    }

    public static class Stress
    {
        public static readonly int[] UserStages = { 10, 25, 50, 75, 100 };
        public const int StageDurationSeconds = 45;
        public const int ThinkTimeMs = 500;
    }

    public static class Spike
    {
        public const int NormalUsers = 10;
        public const int SpikeUsers = 100;
        public const int NormalDurationSeconds = 30;
        public const int SpikeDurationSeconds = 30;
        public const int RecoveryDurationSeconds = 45;
        public const int ThinkTimeMs = 1500;
    }

    public static class Thresholds
    {
        public const double MaxAverageMs = 500;
        public const double MaxP95Ms = 1000;
        public const double MaxErrorRate = 0.01;
        public const double MaxSpikeErrorRate = 0.10;
        public const double MaxP95InRecoveryMultiplier = 1.5;
    }

    public static class Seed
    {
        public const int Books = 60;
        public const int Members = 30;
    }
}