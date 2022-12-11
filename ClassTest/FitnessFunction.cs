using System;
using System.Collections.Generic;
using System.Linq;

namespace Final_Project
{
    public class FitnessFunction
    {
        int peopleAmount;
        double peopleInterval; /* 排隊人潮進入時間間距 */
        Person.Strategy strategy;
        double weight_alpha = 0.5;
        double weight_beta = 0.7;

        double roomLength = 10; /* 廁所總長，單位：公尺 */
        double roomWidth = 5; /* 廁所總寬，單位：公尺 */
        double toiletLength = 0.3; /* 小便斗長，單位：公尺 */
        double toiletWidth = 0.33; /* 小便斗寬，單位：公尺 */


        public FitnessFunction(int peopleAmount, double peopleInterval, Person.Strategy strategy)
        {
            this.peopleAmount = peopleAmount;
            this.peopleInterval = peopleInterval;
            this.strategy = strategy;
        }

        public void SetWeight(double alpha, double beta)
        {
            weight_alpha = alpha;
            weight_beta = beta;
        }

        public void SetSize(double roomLength, double roomWidth, double toiletLength, double toiletWidth)
        {
            this.roomLength = roomLength;
            this.roomWidth = roomWidth;
            this.toiletLength = toiletLength;
            this.toiletWidth = toiletWidth;
        }

        public double Evaluate(int toiletWallAmount, int toiletAmountPerRow)
        {
            Toilet toilet;
            try
            {
                toilet = new Toilet(toiletWallAmount, toiletAmountPerRow, roomLength, roomWidth, toiletLength, toiletWidth);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return double.MaxValue;
            }
            //Console.WriteLine(toilet.ReservedStatus());

            Person[] sequence = new Person[peopleAmount];
            for (int i = 0; i < peopleAmount; i++)
            {
                sequence[i] = new Person(strategy, weight_alpha, weight_beta);
                sequence[i].Id = i + 1;
            }
            Queue<Person> waitingList = new Queue<Person>();
            LinkedList<Person> walkingList = new LinkedList<Person>();
            LinkedList<Person> peeingList = new LinkedList<Person>();
            for (int i = 0; i < peopleAmount; i++)
            {
                sequence[i].TimeLine.Arrived = 0; /* 假設所有人同時抵達排隊隊伍 */
                waitingList.Enqueue(sequence[i]);
            }

            double time = 0;
            waitingList.First().TimeLine.StartWalking = 0;
            bool begining = true;

            while (begining || peeingList.Count > 0)
            {
                /* 檢查有沒有人可以開始走了 */
                if (waitingList.Count > 0)
                {
                    if (waitingList.First().TimeLine.StartWalking <= time)
                    {
                        if (waitingList.First().ChooseToilet(toilet))
                        {
                            Person person = waitingList.Dequeue();
                            person.TimeLine.StartWalking = time;
                            person.TimeLine.SetStartPeeingTime(person.WalkingTime());
                            int[] toiletIdx = person.ChosenToiletIdx();
                            toilet.ReserveToilet(person.Id, toiletIdx[0], toiletIdx[1]);

                            //Console.WriteLine(String.Format("P{0} Reserved, Time: {1:0.00}", person.Id, time));
                            //Console.Write(toilet.ReservedStatus());

                            walkingList.AddLast(person);
                            if (waitingList.Count > 0)
                                waitingList.First().TimeLine.StartWalking = time + peopleInterval;
                        }
                    }
                }
                /* 檢查有沒有人走到小便斗了 */
                for (int i = 0; i < walkingList.Count; i++)
                {
                    Person person = walkingList.ElementAt(i);
                    if (person.TimeLine.GetStartPeeingTime() <= time)
                    {
                        int[] toiletIdx = person.ChosenToiletIdx();
                        toilet.OccupiedToilet(person.Id, toiletIdx[0], toiletIdx[1]);

                        //Console.WriteLine(String.Format("P{0} Arrived, Time: {1:0.00}", person.Id, time));
                        //Console.WriteLine(toilet.UsingStatus());

                        person.TimeLine.SetFinishPeeingTime(person.PeeingTime());
                        peeingList.AddLast(person);
                        walkingList.Remove(person);
                        i--;
                        begining = false;

                        /* 檢查相鄰有沒有人 */
                        //左
                        int leftToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0], toiletIdx[1] - 1);
                        if (leftToiletStatus > 0)
                        {
                            person.TimeLine.LeftOccupied = time;
                            sequence[leftToiletStatus - 1].TimeLine.RightOccupied = time;
                        }
                        //右
                        int rightToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0], toiletIdx[1] + 1);
                        if (rightToiletStatus > 0)
                        {
                            person.TimeLine.RightOccupied = time;
                            sequence[rightToiletStatus - 1].TimeLine.LeftOccupied = time;
                        }
                        //後
                        int backIdx = toiletIdx[0] % 2 == 0 ? 1 : -1;
                        int backToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0] + backIdx, toiletIdx[1]);
                        if (backToiletStatus > 0)
                        {
                            person.TimeLine.BackOccupied = time;
                            sequence[backToiletStatus - 1].TimeLine.BackOccupied = time;
                        }
                    }
                }
                /* 檢查有沒有人尿完了 */
                for (int i = 0; i < peeingList.Count; i++)
                {
                    Person person = peeingList.ElementAt(i);
                    if (person.TimeLine.GetFinishPeeingTime() <= time)
                    {
                        int[] toiletIdx = person.ChosenToiletIdx();
                        toilet.ReleaseToilet(toiletIdx[0], toiletIdx[1]);
                        peeingList.Remove(person);

                        //Console.Write(String.Format("P{0} Released, Time: {1:0.00}", person.Id, time));
                        //Console.Write(toilet.UsingStatus());

                        ///* 檢查相鄰有沒有人 */
                        //左
                        int leftToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0], toiletIdx[1] - 1);
                        if (leftToiletStatus > 0)
                        {
                            person.TimeLine.NeighborLeave(time, "left");
                            sequence[leftToiletStatus - 1].TimeLine.NeighborLeave(time, "right");
                        }
                        //右
                        int rightToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0], toiletIdx[1] + 1);
                        if (rightToiletStatus > 0)
                        {
                            person.TimeLine.NeighborLeave(time, "right");
                            sequence[rightToiletStatus - 1].TimeLine.NeighborLeave(time, "left");
                        }
                        //後
                        int backIdx = toiletIdx[0] % 2 == 0 ? 1 : -1;
                        int backToiletStatus = toilet.WhoIsUsingTheToilet(toiletIdx[0] + backIdx, toiletIdx[1]);
                        if (backToiletStatus > 0)
                        {
                            person.TimeLine.NeighborLeave(time, "back");
                            sequence[backToiletStatus - 1].TimeLine.NeighborLeave(time, "back");
                        }
                    }
                }
                time += 0.001;
            }

            /* 計算目標函數 */
            double allPeopleCost = 0;
            foreach (var person in sequence)
            {
                allPeopleCost += person.TotalCost(toilet);
            }
            double costAverage = allPeopleCost / peopleAmount;

            Console.WriteLine("平均成本 " + (costAverage / 40).ToString());
            return -costAverage / 40;
        }
    }
}
