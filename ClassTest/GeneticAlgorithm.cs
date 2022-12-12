using System;
using System.Collections.Generic;
using System.Linq;
using Final_Project;

namespace Final_Project
{
    public class GeneticAlgorithm
    {
        private Random random = new Random();

        public string Generate(int length)
        {
            string ret = "";
            for (int i = 0; i < length; ++i)
            {
                if (random.Next(0, 2) == 1)
                {
                    ret += "1";
                }
                else
                {
                    ret += "0";
                }
            }
            return ret;
        }

        public string Select(IEnumerable<string> population, IEnumerable<double> fitnesses, double sum = 0.0)
        {
            // fitness proportionate selection.

            var fitArr = fitnesses.ToArray();
            if (sum == 0.0)
            {
                foreach (var fit in fitnesses)
                {
                    sum += fit;
                }
            }

            // normalize.
            for (int i = 0; i < fitArr.Length; ++i)
            {
                fitArr[i] /= sum;
            }

            var popArr = population.ToArray();

            Array.Sort(fitArr, popArr);

            sum = 0.0;

            var accumFitness = new double[fitArr.Length];

            // calculate accumulated normalized fitness values.
            for (int i = 0; i < accumFitness.Length; ++i)
            {
                sum += fitArr[i];
                accumFitness[i] = sum;
            }

            var val = random.NextDouble();

            for (int i = 0; i < accumFitness.Length; ++i)
            {
                if (accumFitness[i] > val)
                {
                    return popArr[i];
                }
            }
            return "";
        }

        public string Mutate(string chromosome, double probability)
        {
            string ret = "";
            double randomVariable = 0.0;
            foreach (char c in chromosome)
            {
                randomVariable = random.NextDouble();
                if (randomVariable < probability)
                {
                    if (c == '1')
                    {
                        ret += "0";
                    }
                    else
                    {
                        ret += "1";
                    }
                }
                else
                {
                    ret += c;
                }
            }
            return ret;
        }

        public IEnumerable<string> Crossover(string chromosome1, string chromosome2)
        {
            int randomPosition = random.Next(0, chromosome1.Length);
            string newChromosome1 = chromosome1.Substring(randomPosition) + chromosome2.Substring(0, randomPosition);
            string newChromosome2 = chromosome2.Substring(randomPosition) + chromosome1.Substring(0, randomPosition);
            return new string[] { newChromosome1, newChromosome2 };
        }

        public string Run(FitnessFunction fitness, int length, double crossoverProbability, double mutationProbability, int iterations = 100)
        {
            int populationSize = 500;
            // run population is population being generated.
            // test population is the population from which samples are taken.
            List<string> testPopulation = new List<string>(), runPopulation = new List<string>();
            string one = "", two = "";
            var randDouble = 0.0;

            // construct initial population.
            while (testPopulation.Count < populationSize)
            {
                testPopulation.Add(Generate(length));
            }

            var fitnesses = new double[testPopulation.Count];

            double sum = 0.0;

            // continuously generate populations until number of iterations is met.
            for (int iter = 0; iter < iterations; ++iter)
            {
                runPopulation = new List<string>();

                // calculate fitness for test population.
                sum = 0.0;
                fitnesses = new double[testPopulation.Count];
                for (int i = 0; i < fitnesses.Length; ++i)
                {
                    //Console.WriteLine(">> ***" + testPopulation[i].ToString());
                    //Console.WriteLine(testPopulation[i].Substring(0, 8));
                    //Console.WriteLine(testPopulation[i].Substring(testPopulation[i].Length - 8, 8));
                    int bitNum = length / 2;
                    int toiletWallAmount = Convert.ToInt32(testPopulation[i].Substring(0, bitNum), 2);
                    int toiletAmouuntPerRow = Convert.ToInt32(testPopulation[i].Substring(bitNum, bitNum), 2);

                    //Console.WriteLine(">> * *" + toiletWallAmouunt.ToString() + ", " + toiletAmouuntPerRow.ToString());

                    fitnesses[i] = fitness.Evaluate(toiletWallAmount, toiletAmouuntPerRow);

                    Console.WriteLine(">> VARIABLE: " + toiletWallAmount.ToString() + ", " + toiletAmouuntPerRow.ToString());
                    Console.WriteLine(">> FITNESS VAL: " + (fitnesses[i]*-1).ToString() + "\r\n");

                    sum += fitnesses[i];
                }

                // a population doesn't need to be generated for last iteration.
                // (using test population)
                if (iter == iterations - 1) break;

                while (runPopulation.Count < testPopulation.Count)
                {

                    one = Select(testPopulation, fitnesses, sum);
                    two = Select(testPopulation, fitnesses, sum);

                    // determine if crossover occurs.
                    randDouble = random.NextDouble();
                    if (randDouble <= crossoverProbability)
                    {
                        var stringArr = Crossover(one, two).ToList();
                        one = stringArr[0];
                        two = stringArr[1];
                    }

                    one = Mutate(one, mutationProbability);
                    two = Mutate(two, mutationProbability);

                    runPopulation.Add(one);
                    runPopulation.Add(two);
                }

                if (runPopulation[0] != "")
                {
                    testPopulation = runPopulation;
                }
            }

            // find best-fitting string.
            var testSort = testPopulation.ToArray();
            var fitSort = fitnesses.ToArray();

            Array.Sort(fitSort, testSort);

            return testSort[testSort.Length - 1];
        }
    }
}