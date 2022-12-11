using Final_Project;

FitnessFunction fitness = new FitnessFunction(10, 10, Person.Strategy.MaximizeDistanceBehavior); //輸入：隊伍人數、每個人進廁所間隔時間、小便斗選擇行為

/* OPTIONAL
fitness.SetWeight(0.9, 0.6);  // 改變權重(公式請參考word)，輸入：權重a,b
fitness.SetSize(10, 5, 0.3, 0.33); // 改變廁所尺寸及小便斗尺寸(單位：公尺)，輸入：廁所長、廁所寬、小便斗長、小便斗寬
*/

//double score = fitness.Evaluate(1, 13);
//Console.WriteLine("每個人平均成本為 " + score);

GeneticAlgorithm ga = new GeneticAlgorithm();

int bitLength = 8;

string ans = ga.Run(fitness, bitLength, 0.1, 0.1);

int bitNum = bitLength / 2;
int toiletWallAmouunt = Convert.ToInt32(ans.Substring(0, bitNum), 2);
int toiletAmouuntPerRow = Convert.ToInt32(ans.Substring(bitNum, bitNum), 2);
Console.WriteLine("ANS: " + toiletWallAmouunt.ToString() + ", " + toiletAmouuntPerRow.ToString());

