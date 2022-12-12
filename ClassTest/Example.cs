using Final_Project;

int totalPeopleAmount = 100;
double peopleInterval = 4.5;
Person.Strategy strategy = Person.Strategy.CooperativeBehavior;

double restroomLength = 20;
double restroomWidth = 15;

double alpha = 0.5;
double beta = 0.7;



FitnessFunction fitness = new FitnessFunction(totalPeopleAmount, peopleInterval, strategy); //輸入：隊伍人數、每個人進廁所間隔時間、小便斗選擇行為
fitness.SetSize(restroomLength, restroomWidth, 0.385, 0.375); // 改變廁所尺寸及小便斗尺寸(單位：公尺)，輸入：廁所長、廁所寬、小便斗長、小便斗寬
fitness.SetWeight(alpha, beta);  // 改變權重(公式請參考word)，輸入：權重a,b


/* GA */
GeneticAlgorithm ga = new GeneticAlgorithm();
int bitLength = 10;
string ans = ga.Run(fitness, bitLength, 0.3, 0.3, 3);
int bitNum = bitLength / 2;
int toiletWallAmouunt = Convert.ToInt32(ans.Substring(0, bitNum), 2);
int toiletAmouuntPerRow = Convert.ToInt32(ans.Substring(bitNum, bitNum), 2);
Console.WriteLine("\nANS: " + toiletWallAmouunt.ToString() + ", " + toiletAmouuntPerRow.ToString());



/* 窮舉 */
//double maxCost = double.MinValue;
//string output = "";
//for (int i = 0; i < 8; i++)
//{
//    for (int j = 1; j < 30; j++)
//    {
//        double score = fitness.Evaluate(i, j);
//        Console.WriteLine(String.Format("\nVariable: {0:00}, {1:00} 每個人平均成本為 {2}", i, j, score*-1));

//        if (score > maxCost)
//        {
//            maxCost = score;
//            output = "\n\nOPTIMA: " + i + ", " + j + "每個人平均成本為 " + score*-1;
//        }
//        else if (score == maxCost)
//        {
//            output += "\nOPTIMA: " + i + ", " + j + "每個人平均成本為 " + score*-1;
//        }
//    }
//}
//Console.WriteLine(output);



/* 測試 */
//Console.WriteLine(fitness.Evaluate(4, 24));
//Toilet Toilet = new Toilet(4, 24, 20, 15, 0.385, 0.375);
//Console.WriteLine(Toilet.GetToiletAmount());