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
        private IBallCamera ballCamera;
        private int currentCheckpointIndex;
        private Vector3 currentCheckpointPos;
        private Checkpoint nextCheckpoint;
        private RaceFinishReport finishReport;

        private float lapTime;
        private float[] checkpointTimes;

        private StageReferences sr;

        private Match.MatchMessenger matchMessenger;
        private Match.MatchPlayer associatedMatchPlayer;
        private bool waitingForCheckpointMessage;

        public RacePlayer(Ball ball, Match.MatchMessenger matchMessenger, Match.MatchPlayer associatedMatchPlayer)
        {
            sr = StageReferences.Active;

            this.matchMessenger = matchMessenger;
            this.associatedMatchPlayer = associatedMatchPlayer;
            matchMessenger.CreateListener<Match.CheckpointPassedMessage>(CheckpointPassedHandler);

            lap = 1;

            ball.CanMove = false;
            ball.CheckpointPassed += Ball_CheckpointPassed;
            ball.RespawnRequested += Ball_RespawnRequested;
            currentCheckpointPos = sr.checkpoints[0].transform.position;
            this.ball = ball;

            ball.CameraCreated += (sender, e) =>
            {
                ballCamera = e.CameraCreated;
                ballCamera.SetDirection(sr.checkpoints[0].transform.rotation);
            };

            checkpointTimes = new float[StageReferences.Active.checkpoints.Length];

            SetNextCheckpoint();
        }

        public event EventHandler<NextCheckpointPassArgs> NextCheckpointPassed;
        public event EventHandler FinishLinePassed;

        public bool IsPlayer { get { return ball.Type == BallType.Player; } }
        public string Name { get { return ball.Nickname; } }
        public int Character { get { return ball.CharacterId; } }
        public Transform Transform { get { return ball.transform; } }
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
                if (ball.Type == BallType.AI)
                    ball.CanMove = false;
                //Set layer to Racer Ghost to block collision with racing players
                ball.gameObject.layer = LayerMask.NameToLayer("Racer Ghost");
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
                if (ball.Type == BallType.Player && ball.CtrlType != ControlType.Remote)
                {
                    if (!waitingForCheckpointMessage)
                    {
                        //Send a match message for local players
                        matchMessenger.SendMessage(new Match.CheckpointPassedMessage(associatedMatchPlayer.ClientGuid, ball.CtrlType, lapTime));

                        waitingForCheckpointMessage = true;
                    }
                }
                else if (ball.Type == BallType.AI)
                {
                    //Invoke PassNextCheckpoint directly for AI balls
                    PassNextCheckpoint(lapTime);
                }
            }
        }

        private void CheckpointPassedHandler(Match.CheckpointPassedMessage msg)
        {
            if (msg.ClientGuid == associatedMatchPlayer.ClientGuid && msg.CtrlType == associatedMatchPlayer.CtrlType)
            {
                PassNextCheckpoint(msg.LapTime);

                waitingForCheckpointMessage = false;
            }
        }

        private void PassNextCheckpoint(float lapTime)
        {
            checkpointTimes[currentCheckpointIndex] = lapTime;

            //Call NextCheckpointPassed BEFORE doing anything else. This ensures things like lap records work correctly.
            if (NextCheckpointPassed != null)
                NextCheckpointPassed(this, new NextCheckpointPassArgs(nextCheckpoint, currentCheckpointIndex, TimeSpan.FromSeconds(lapTime)));

            currentCheckpointIndex = (currentCheckpointIndex + 1) % sr.checkpoints.Length;
            currentCheckpointPos = nextCheckpoint.transform.position;

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

                //Reset lap time
                this.lapTime = 0 + (this.lapTime - lapTime);
                checkpointTimes = new float[StageReferences.Active.checkpoints.Length];
            }

            SetNextCheckpoint();
        }

        private void Ball_RespawnRequested(object sender, EventArgs e)
        {
            ball.transform.position = sr.checkpoints[currentCheckpointIndex].GetRespawnPoint() + Vector3.up * ball.transform.localScale.x * 0.5f;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            if (ballCamera != null)
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

        public void RemoveMessageListeners()
        {
            matchMessenger.RemoveListener<Match.CheckpointPassedMessage>(CheckpointPassedHandler);
        }
    }
}