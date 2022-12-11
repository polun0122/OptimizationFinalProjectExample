
namespace Final_Project
{
    public class TimeLine
    {
        public double Arrived;
        public double StartWalking;
        public double RightOccupied;
        public double LeftOccupied;
        public double BackOccupied;

        /// <summary>
        /// 右邊有人的時間
        /// </summary>
        public double TimeRightOccupied = 0;

        /// <summary>
        /// 左邊有人的時間
        /// </summary>
        public double TimeLeftOccupied = 0;

        /// <summary>
        /// 後面有人的時間
        /// </summary>
        public double TimeBackOccupied = 0;

        double startPeeing;
        double finishPeeing;

        public void Reset()
        {
            TimeRightOccupied = 0;
            TimeLeftOccupied = 0;
            TimeBackOccupied = 0;
        }

        public double GetStartPeeingTime() { return startPeeing; }

        public double GetFinishPeeingTime() { return finishPeeing; }

        public void SetStartPeeingTime(double walkingTime)
        {
            startPeeing = StartWalking + walkingTime;
        }

        public void SetFinishPeeingTime(double peeingTime)
        {
            finishPeeing = startPeeing + peeingTime;
        }

        public void NeighborLeave(double nowTime, string direction)
        {
            if (direction == "left")
            {
                TimeLeftOccupied += (nowTime - LeftOccupied);
            }
            else if (direction == "right")
            {
                TimeRightOccupied += (nowTime - RightOccupied);
            }
            else if (direction == "back")
            {
                TimeBackOccupied += (nowTime - BackOccupied);
            }
        }
    }
}
