using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Project
{
    public class Toilet
    {
        double roomLength; /* 廁所總長，單位：公尺 */
        double roomWidth; /* 廁所總寬，單位：公尺 */
        double toiletLength; /* 小便斗長，單位：公尺 */
        double toiletWidth; /* 小便斗寬，單位：公尺 */

        int wallAmount; /* 牆壁排數 */
        int rowAmount; /* 小便斗排數 */
        int toiletAmountPerRow; /* 每排小便斗個數 */
        
        int totalAmount; /* 小便斗總數 */
        double toiletInterval; /* 小便斗間距 */
        double rowInterval; /* 排間距 */

        int[,] toiletReserveStatus;
        int[,] toiletUsingStatus;

        /// <summary>
        /// </summary>
        /// <param name="wallAmount">牆壁排數，需大於2</param>
        /// <param name="toiletAmountPerRow">每排小便斗個數，需大於0</param>
        /// <exception cref="Exception"></exception>
        public Toilet(int wallAmount, int toiletAmountPerRow, double roomLength, double roomWidth, double toiletLength, double toiletWidth)
        {
            if (wallAmount < 0)
                throw new Exception("Violate Constraint: 牆面數量不得小於0");
            if (toiletAmountPerRow < 0)
                throw new Exception("Violate Constraint: 每排小便斗數量不得小於0");
            this.wallAmount = wallAmount + 2;
            this.toiletAmountPerRow = toiletAmountPerRow;
            rowAmount = 2 * (this.wallAmount - 1);
            totalAmount = rowAmount * this.toiletAmountPerRow;
            toiletInterval = (roomLength - toiletLength * this.toiletAmountPerRow) / (this.toiletAmountPerRow + 1);
            if (toiletInterval < (0.8 - toiletLength))
                throw new Exception("Violate Constraint: 根據法規小便斗間距不得小於0.8公尺 (內政部營建署_公共建築物衛生設備設計手冊3-2.4)");
            rowInterval = roomWidth / (this.wallAmount - 1);
            if (rowInterval < (1.4 + 2 * toiletWidth))
                throw new Exception("Violate Constraint: 走道寬度需大於1.4公尺(考慮雙向通行)");
            toiletReserveStatus = new int[rowAmount, this.toiletAmountPerRow];
            toiletUsingStatus = new int[rowAmount, this.toiletAmountPerRow];
            this.roomLength = roomLength;
            this.roomWidth = roomWidth;
            this.toiletLength = toiletLength;
            this.toiletWidth = toiletWidth;
        }

        /// <summary>
        /// 小便斗排數
        /// </summary>
        /// <returns></returns>
        public int GetRowAmount() { return rowAmount; }

        /// <summary>
        /// 每排小便斗個數
        /// </summary>
        /// <returns></returns>
        public int GetToiletAmountPerRow() { return toiletAmountPerRow; }

        /// <summary>
        /// 小便斗總數
        /// </summary>
        /// <returns></returns>
        public int GetToiletAmount() { return totalAmount; }

        /// <summary>
        /// 小便斗間距(不含小便斗長度)
        /// </summary>
        /// <returns></returns>
        public double GetToiletInterval() { return toiletInterval; }

        /// <summary>
        /// 每一列牆壁間距(包含小便斗寬度)
        /// </summary>
        /// <returns></returns>
        public double GetRowInterval() { return rowInterval; }

        /// <summary>
        /// 預約特定編號的小便斗 Occupy the specific toilet.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>是否成功更改狀態</returns>
        public bool ReserveToilet(int id, int row, int col)
        {
            if (toiletReserveStatus[row, col] == 0)
            {
                toiletReserveStatus[row, col] = id;
                return true;
            }
            return false;
        }


        /// <summary>
        /// 占用特定編號的小便斗 Occupy the specific toilet.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>是否成功更改狀態</returns>
        public bool OccupiedToilet(int id, int row, int col)
        {
            if (toiletUsingStatus[row, col] == 0)
            {
                toiletUsingStatus[row, col] = id;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 釋放特定編號的小便斗 Release the specific toilet.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns>是否成功更改狀態</returns>
        public bool ReleaseToilet(int row, int col)
        {
            if (toiletReserveStatus[row, col] != 0)
            {
                toiletReserveStatus[row, col] = 0;
                toiletUsingStatus[row, col] = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 回傳該小便斗是否被占用
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool IsToiletReserved(int row, int col)
        {
            return toiletReserveStatus[row, col] > 0;
        }

        /// <summary>
        /// 回傳該小便斗被誰占用，ID
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public int WhoIsUsingTheToilet(int row, int col)
        {
            if (row < 0 || row >= rowAmount)
                return 0;
            if (col < 0 || col >= toiletAmountPerRow)
                return 0;
            return toiletUsingStatus[row, col];
        }

        /// <summary>
        /// 回傳到該小便斗的距離，單位：公尺。
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public double DistanceToToilet(int row, int col)
        {
            int aisleNumber = row / 2 + 1;
            int numberInRow = col + 1;

            double disMoveToRow;
            double disMoveToToilet;
            double disMoveForward;
            disMoveToRow = Math.Abs(roomWidth / 2/*Start Pos*/ - (aisleNumber * rowInterval - 0.5 * rowInterval/*Target Pos*/));
            disMoveToToilet = (toiletInterval + toiletLength) * numberInRow - 0.5 * toiletLength;
            disMoveForward = 0.5 * rowInterval - toiletWidth;
            return disMoveToRow + disMoveToToilet + disMoveForward;
        }

        /// <summary>
        /// 以文字繪製小便斗排列示意圖，o代表可用之小便斗、*代表已被預約之小便斗
        /// </summary>
        /// <returns></returns>
        public string ReservedStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n@ Toilet Reserved Status\n______________\n\n");
            for (int i =0;i<rowAmount;i++)
            {
                for (int j=0;j<toiletAmountPerRow;j++)
                {
                    sb.Append("-");
                    if (IsToiletReserved(i,j))
                        sb.Append("*");
                    else 
                        sb.Append("o");
                }
                sb.Append("-\n");
                if (i % 2 == 0)
                {
                    sb.Append("|");
                    for (int k = 0; k < toiletAmountPerRow * 2 - 1; k++)
                        sb.Append(" ");
                    sb.Append("|\n");
                }
            }
            sb.Append("______________\n\n");
            sb.Append(String.Format(" - Toilet Interval {0:.000} m\n", toiletInterval));
            sb.Append(String.Format(" | Row    Interval {0:.000} m\n\n", rowInterval));
            return sb.ToString();
        }

        /// <summary>
        /// 以文字繪製小便斗使用狀態示意圖，0代表可用之小便斗、數字代表使用該小便斗的人員編號
        /// </summary>
        /// <returns></returns>
        public string UsingStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n@ Toilet Using Status with Id\n______________\n\n");
            for (int i = 0; i < rowAmount; i++)
            {
                for (int j = 0; j < toiletAmountPerRow; j++)
                {
                    sb.Append("-");
                    int id = WhoIsUsingTheToilet(i, j);
                    sb.Append(id);
                }
                sb.Append("-\n");
                if (i % 2 == 0)
                {
                    sb.Append("|");
                    for (int k = 0; k < toiletAmountPerRow * 2 - 1; k++)
                        sb.Append(" ");
                    sb.Append("|\n");
                }
            }
            sb.Append("______________\n\n");
            sb.Append(String.Format(" - Toilet Interval {0:.000} m\n", toiletInterval));
            sb.Append(String.Format(" | Row    Interval {0:.000} m\n\n", rowInterval));
            return sb.ToString();
        }
    }
}
