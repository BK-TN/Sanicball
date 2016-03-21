using System;

namespace Sanicball.Data
{
    public enum RecordType
    {
        Lap,
        HyperspeedLap
    }

    [Serializable]
    public class RaceRecord
    {
        private RecordType type;
        private float time;
        private DateTime date;
        private int stage;
        private int character;
        private float[] checkpointTimes;

        public RecordType Type { get { return type; } }
        public float Time { get { return time; } }
        public DateTime Date { get { return date; } }
        public int Stage { get { return stage; } }
        public int Character { get { return character; } }
        public float[] CheckpointTimes { get { return checkpointTimes; } }

        public RaceRecord(RecordType type, float time, DateTime date, int stage, int character, float[] checkpointTimes)
        {
            this.type = type;
            this.time = time;
            this.date = date;
            this.stage = stage;
            this.character = character;
            this.checkpointTimes = checkpointTimes;
        }
    }
}