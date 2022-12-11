using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Final_Project
{
    public class Person
    {
        public int Id;
        public enum Strategy { LazyBehavior, CooperativeBehavior, MaximizeDistanceBehavior };

        double peeingTime = 21; /* 如廁時間 */
        double walkingSpeed = 1.48; /* 成人步行速度 */

        Strategy strategy; /* 小便斗選擇策略 */
        double weight_alpha; /* 權重a */
        double weight_beta; /* 權重b(隱私成本內) */


        public Person(Strategy strategy, double weight_alpha = 0.5, double weight_beta = 0.7)
        {
            this.strategy = strategy; /* 小便斗選擇策略 */
            this.weight_alpha = weight_alpha; /* 時間成本權重 */
            this.weight_beta = weight_beta; /* 左右兩側被占用的成本權重(隱私成本內) */
        }

        public void Reset()
        {
            TimeLine.Reset();
        }

        public TimeLine TimeLine = new TimeLine();

        public double waitingTime;
        /// <summary>
        /// 排隊等待時間
        /// </summary>
        public double WaitingTime()
        {
            waitingTime = TimeLine.StartWalking - TimeLine.Arrived;
            return waitingTime;
        }

        /// <summary>
        /// 移動至小便斗時間
        /// </summary>
        public double WalkingTime() { return walkingTime; }
        
        double walkingTime;

        /// <summary>
        /// 小便耗費時間
        /// </summary>
        /// <returns></returns>
        public double PeeingTime() { return peeingTime; }

        /// <summary>
        /// 計算個人總成本
        /// </summary>
        /// <returns></returns>
        public double TotalCost(Toilet toilet)
        {
            double toiletInterval = toilet.GetToiletInterval();
            double rowInterval = toilet.GetRowInterval();
            double totalCost = weight_alpha * GetTimeCost() + (1- weight_alpha) * GetPrivateCost(toiletInterval, rowInterval);
            return totalCost;
        }

        /// <summary>
        /// 計算個人隱私成本
        /// </summary>
        /// <returns></returns>
        public double GetPrivateCost(double toiletInterval, double rowInterval)
        {
            double privateCost;
            privateCost = weight_beta * (TimeLine.TimeRightOccupied + TimeLine.TimeLeftOccupied) / toiletInterval +
                          (1 - weight_beta) * TimeLine.TimeBackOccupied / rowInterval;
            return privateCost;
        }

        /// <summary>
        /// 計算個人時間成本
        /// </summary>
        /// <returns></returns>
        public double GetTimeCost()
        {
            double timeCost;
            timeCost = WaitingTime() + walkingTime + peeingTime;
            return timeCost;
        }

        int[] chosenToiletIdx;
        public int[] ChosenToiletIdx() { return chosenToiletIdx; }

        public bool ChooseToilet(Toilet toilet)
        {
            if (chosenToiletIdx != null) return false;

            Queue availableToilets = new Queue();
            Queue occupiedToilets = new Queue();
            Queue eligibleToilets;

            /* 若廁所已經全滿直接回傳 false */
            for (int i = 0; i < toilet.GetRowAmount(); i++)
            {
                for (int j = 0; j < toilet.GetToiletAmountPerRow(); j++)
                {
                    if (!toilet.IsToiletReserved(i, j))
                        availableToilets.Enqueue(new int[] { i, j });
                    else
                        occupiedToilets.Enqueue(new int[] { i, j });
                }
            }
            if (availableToilets.Count == 0)
                return false;

            if (strategy == Strategy.MaximizeDistanceBehavior)
            {
                /* MaximizeDistanceBehavior */
                eligibleToilets = MaximizeDistanceBehavior(availableToilets, occupiedToilets, toilet);
            }
            else if (strategy == Strategy.CooperativeBehavior)
            {
                /* CooperativeBehavior */
                eligibleToilets = CooperativeBehavior(availableToilets, occupiedToilets, toilet);
                if (eligibleToilets.Count > 0)
                    eligibleToilets = LazyBehavior(eligibleToilets, occupiedToilets, toilet);
                else
                    eligibleToilets = LazyBehavior(availableToilets, occupiedToilets, toilet);
            }
            else if (strategy == Strategy.LazyBehavior)
            {
                /* LazyBehavior */
                eligibleToilets = LazyBehavior(availableToilets, occupiedToilets, toilet);
            }
            else
            {
                eligibleToilets = new Queue();
                eligibleToilets.Enqueue(new int[] { -2, -2 });
            }
            chosenToiletIdx = (int[])eligibleToilets.Dequeue();
            walkingTime = toilet.DistanceToToilet(chosenToiletIdx[0], chosenToiletIdx[1]);
            return true;
        }

        private Queue LazyBehavior(Queue availableToilets, Queue occupiedToilets, Toilet toilet)
        {
            Queue eligibleList = new Queue();

            double minWalkingTime = double.MaxValue;
            foreach (int[] toiletIdx in availableToilets)
            {
                double timeToToilet = toilet.DistanceToToilet(toiletIdx[0], toiletIdx[1]) / walkingSpeed;
                if (timeToToilet < minWalkingTime)
                {
                    minWalkingTime = timeToToilet;
                    eligibleList.Clear();
                    eligibleList.Enqueue(toiletIdx);
                }
                else if (timeToToilet == minWalkingTime)
                {
                    eligibleList.Enqueue(toiletIdx);
                }
            }
            return eligibleList;
        }

        private Queue MaximizeDistanceBehavior(Queue availableToilets, Queue occupiedToilets, Toilet toilet)
        {
            Queue eligibleList = new Queue();

            double maxDistance = double.MinValue;
            double rowIntreval = toilet.GetRowInterval();
            double toiletInterval = toilet.GetToiletInterval();

            foreach (int[] availableToiletIdx in availableToilets)
            {
                double minNeighborDistance = double.MaxValue;
                foreach (int[] occupiedToiletIdx in occupiedToilets)
                {
                    double neighborDistance = (Math.Abs(availableToiletIdx[0] - occupiedToiletIdx[0])) * rowIntreval + (Math.Abs(availableToiletIdx[1] - occupiedToiletIdx[1])) * toiletInterval;
                    if (neighborDistance < minNeighborDistance)
                    {
                        minNeighborDistance = neighborDistance;
                    }
                }

                if (minNeighborDistance > maxDistance)
                {
                    maxDistance = minNeighborDistance;
                    eligibleList.Clear();
                    eligibleList.Enqueue(availableToiletIdx);
                }
                else if (minNeighborDistance == maxDistance)
                {
                    eligibleList.Enqueue(availableToiletIdx);
                }
            }
            return eligibleList;
        }

        private Queue CooperativeBehavior(Queue availableToilets, Queue occupiedToilets, Toilet toilet)
        {
            Queue eligibleList = new Queue();

            LinkedList<int> consecutiveToilet = new LinkedList<int>();
            int rowAmount = toilet.GetRowAmount();
            int toiletAmountPerRow = toilet.GetToiletAmountPerRow();
            for (int i =0; i< rowAmount; i++)
            {
                for (int j =0; j< toiletAmountPerRow; j++)
                {
                    if (!toilet.IsToiletReserved(i, j))
                    {
                        consecutiveToilet.AddLast(j);
                    }
                    else
                    {
                        consecutiveToiletHandle(i, eligibleList, consecutiveToilet, false);
                    }
                }
                consecutiveToiletHandle(i, eligibleList, consecutiveToilet, true);
            }
            return eligibleList;
        }

        private void consecutiveToiletHandle(int row, Queue eligibleList, LinkedList<int> consecutiveToilet, bool isBackEmpty)
        {
            if (consecutiveToilet.First == null)
                return;
            bool isFrontEmpty = (consecutiveToilet.First.Value == 0);
            int startIdx = isFrontEmpty ? 0 : 1;
            int stopIdx = isBackEmpty ? consecutiveToilet.Count : consecutiveToilet.Count - 1;
            int idxInterval;
            if (isFrontEmpty == isBackEmpty)
                idxInterval = (consecutiveToilet.Count % 2 == 0) ? 1 : 2;
            else
                idxInterval = (consecutiveToilet.Count % 2 == 0) ? 2 : 1;

            int minToiletCount = (isFrontEmpty == false && isBackEmpty == false) ? 2 : 1;

            if (consecutiveToilet.Count <= minToiletCount)
            {
                consecutiveToilet.Clear();
                return;
            }
            for (int idx = startIdx; idx < stopIdx; idx += idxInterval)
            {
                eligibleList.Enqueue(new int[] { row, consecutiveToilet.ElementAt(idx) });
            }
            consecutiveToilet.Clear();
        }

        private void listFrontEmptyBackEmpty(int row, Queue eligibleList, LinkedList<int> list)
        {
            if (list.Count <= 1)
                return;
            if ((list.Count % 2) == 0)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            else
            {
                for (int k = 0; k < list.Count; k += 2)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            list.Clear();
        }

        private void listFrontUsedBackUsed(int row, Queue eligibleList, LinkedList<int> list)
        {
            if (list.Count <= 2)
                return;
            if ((list.Count % 2) == 0)
            {
                for (int k = 1; k < list.Count - 1; k++)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            else
            {
                for (int k = 1; k < list.Count - 1; k += 2)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            list.Clear();
        }

        private void listFrontUsedBackEmpty(int row, Queue eligibleList, LinkedList<int> list)
        {
            if (list.Count <= 1)
                return;
            if ((list.Count % 2) == 0)
            {
                for (int k = 1; k < list.Count; k += 2)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            else
            {
                for (int k = 1; k < list.Count; k++)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            list.Clear();
        }

        private void listFrontEmptyBackUsed(int row, Queue eligibleList, LinkedList<int> list)
        {
            if (list.Count <= 1)
                return;
            if ((list.Count % 2) == 0)
            {
                for (int k = 0; k < list.Count - 1; k += 2)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            else
            {
                for (int k = 0; k < list.Count - 1; k++)
                {
                    eligibleList.Enqueue(new int[] { row, list.ElementAt(k) });
                }
            }
            list.Clear();
        }
    }
}
