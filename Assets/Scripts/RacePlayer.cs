using System;
using System.Linq;
using Sanicball.Data;
using UnityEngine;

namespace Sanicball
{
    public class RaceFinishReport
    {
        private TimeSpan time;
        private int position;

        public RaceFinishReport(int position, TimeSpan time)
        {
            this.position = position;
            this.time = time;
        }

        public int Position { get { return position; } }
        public TimeSpan Time { get { return time; } }
    }

    public class NextCheckpointPassArgs : EventArgs
    {
        public Checkpoint CheckpointPassed { get; private set; }
        public int IndexOfPreviousCheckpoint { get; private set; }
        public TimeSpan CurrentLapTime { get; private set; }

        public NextCheckpointPassArgs(Checkpoint checkpointPassed, int indexOfPreviousCheckpoint, TimeSpan currentLapTime)
        {
            CheckpointPassed = checkpointPassed;
            IndexOfPreviousCheckpoint = indexOfPreviousCheckpoint;
            CurrentLapTime = currentLapTime;
        }
    }

    [Serializable]
    public class RacePlayer
    {
        private int lap;
        private int position;

        private Ball ball;
        private OmniCamera ballCamera;
        private int currentCheckpointIndex;
        private Vector3 currentCheckpointPos;
        private Checkpoint nextCheckpoint;
        private RaceFinishReport finishReport;

        private float lapTime;
        private float[] checkpointTimes;

        private StageReferences sr;

        public RacePlayer(Ball ball)
        {
            sr = StageReferences.Active;

            lap = 1;

            ball.CanMove = false;
            ball.CheckpointPassed += Ball_CheckpointPassed;
            ball.RespawnRequested += Ball_RespawnRequested;
            currentCheckpointPos = sr.checkpoints[0].transform.position;
            this.ball = ball;

            ball.CameraCreated += (sender, e) =>
            {
                ballCamera = e.CameraCreated.GetComponent<OmniCamera>();
                ballCamera.SetDirection(sr.checkpoints[0].transform.rotation);
            };

            checkpointTimes = new float[StageReferences.Active.checkpoints.Length];

            SetNextCheckpoint();
        }

        public event EventHandler<NextCheckpointPassArgs> NextCheckpointPassed;
        public event EventHandler FinishLinePassed;

        public bool IsLocalPlayer { get { return ball.Type == BallType.LobbyPlayer || ball.Type == BallType.Player; } }
        public string Name { get { return ball.name; } }
        public int Character { get { return ball.CharacterId; } }
        public int Lap { get { return lap; } }

        public int Position
        {
            get { return position; }
            set
            {
                position = value;
            }
        }

        public float Speed
        {
            get
            {
                return ball.GetComponent<Rigidbody>().velocity.magnitude;
            }
        }

        public ControlType CtrlType
        {
            get { return ball.CtrlType; }
        }

        public RaceFinishReport FinishReport
        {
            get { return finishReport; }
        }

        public bool RaceFinished
        {
            get { return finishReport != null; }
        }

        public bool LapRecordsEnabled { get; set; }

        public void StartRace()
        {
            ball.CanMove = true;
        }

        public void FinishRace(RaceFinishReport report)
        {
            if (finishReport == null)
            {
                finishReport = report;
                ball.CanMove = false;
            }
            else
            {
                Debug.LogError("RacePlayer tried to finish a race twice for some reason");
            }
        }

        public Checkpoint NextCheckpoint { get { return nextCheckpoint; } }

        public float CalculateRaceProgress()
        {
            //This function returns race progress as laps done (1..*) + progress to next lap (0..1)

            float progPerCheckpoint = 1f / sr.checkpoints.Length;

            float ballToNext = Vector3.Distance(ball.transform.position, nextCheckpoint.transform.position);
            float currToNext = Vector3.Distance(currentCheckpointPos, nextCheckpoint.transform.position);

            float distToNextPercentile = 1f - Mathf.Clamp(ballToNext / currToNext, 0f, 1f);

            float distToNextProg = distToNextPercentile * progPerCheckpoint;
            float lapProg = currentCheckpointIndex * progPerCheckpoint + distToNextProg;

            return Lap + lapProg;
        }

        private void Ball_CheckpointPassed(object sender, CheckpointPassArgs e)
        {
            if (e.CheckpointPassed == nextCheckpoint)
            {
                checkpointTimes[currentCheckpointIndex] = lapTime;

                //Call NextCheckpointPassed BEFORE doing anything else. This ensures things like lap records work correctly.
                if (NextCheckpointPassed != null)
                    NextCheckpointPassed(this, new NextCheckpointPassArgs(e.CheckpointPassed, currentCheckpointIndex, TimeSpan.FromSeconds(lapTime)));

                currentCheckpointIndex = (currentCheckpointIndex + 1) % sr.checkpoints.Length;
                currentCheckpointPos = e.CheckpointPassed.transform.position;

                if (currentCheckpointIndex == 0)
                {
                    lap++;

                    if (FinishLinePassed != null)
                        FinishLinePassed(this, EventArgs.Empty);

                    if (LapRecordsEnabled)
                    {
                        bool hyperspeed = ActiveData.Characters[Character].hyperspeed;
                        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                        int stage = ActiveData.Stages.Where(a => a.sceneName == sceneName).First().id;

                        ActiveData.RaceRecords.Add(new RaceRecord(
                            hyperspeed ? RecordType.HyperspeedLap : RecordType.Lap,
                            lapTime,
                            DateTime.Now,
                            stage,
                            Character,
                            checkpointTimes
                            ));

                        Debug.Log("Saved lap record (" + TimeSpan.FromSeconds(lapTime) + ")");
                    }

                    lapTime = 0;
                    checkpointTimes = new float[StageReferences.Active.checkpoints.Length];
                }

                SetNextCheckpoint();
            }
        }

        private void Ball_RespawnRequested(object sender, EventArgs e)
        {
            ball.transform.position = sr.checkpoints[currentCheckpointIndex].GetRespawnPoint() + Vector3.up * ball.transform.localScale.x * 0.5f;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            if (ballCamera)
            {
                ballCamera.SetDirection(sr.checkpoints[currentCheckpointIndex].transform.rotation);
            }
        }

        private void SetNextCheckpoint()
        {
            if (currentCheckpointIndex == sr.checkpoints.Length - 1)
            {
                nextCheckpoint = sr.checkpoints[0];
            }
            else
            {
                nextCheckpoint = sr.checkpoints[currentCheckpointIndex + 1];
            }
        }

        public void UpdateTimer(float dt)
        {
            lapTime += dt;
        }
    }
}