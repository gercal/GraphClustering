﻿
using System;
using System.Collections.Generic;
using System.Text;
using NetMining.ClusteringAlgo;
using NetMining.Data;

namespace NetMining.Evaluation
{
    public class ExternalEval
    {
        public double TotalAccuracy;
        public int TotalMatched;
        public int TotalSize;
        public List<GroundTruthMatch> Matches; 
        public readonly Partition P;
        public readonly LabelList L;
        public String TextResults;
        public String ShorterTextResults;
        public String NoNoiseTextResults;
        public class GroundTruthMatch
        {
            public readonly int PartitionClusterId;
            public readonly int GroundTruthClusterId;
            public readonly String GroundTruthLabel;
            public readonly int NumMatched;
            public readonly int TotalSize;
            public readonly double Accuracy;

            public GroundTruthMatch(int partId, int gtId, int nMatched, int size, double accuracy, String label)
            {
                PartitionClusterId = partId;
                GroundTruthClusterId = gtId;
                GroundTruthLabel = label;
                NumMatched = nMatched;
                TotalSize = size;
                Accuracy = accuracy;
            }
        }

        public ExternalEval(Partition clusterFile, LabelList labels)
        {
            P = clusterFile;
            L = labels;
            Matches = new List<GroundTruthMatch>();
            int[,] clusterMatching = labels.GetMatching(clusterFile);
            GreedyErrorEval(clusterMatching);
        }

        private void GreedyErrorEval(int[,] clusterMatching)
        {
            int truthCount = L.UniqueLabels.Count;
            int partitionCount = P.Clusters.Count;
            int[] assigned = new int[truthCount];
            bool[] ourCIsAssigned = new bool[partitionCount];
            for (int i = 0; i < partitionCount; i++)
                ourCIsAssigned[i] = false;

            KeyValuePair<int, int>[] gtBySize = new KeyValuePair<int, int>[truthCount];
            for (int gt = 0; gt < truthCount; gt++)
            {
                int labelSize = 0;
                for (int i = 0; i < partitionCount; i++)
                    labelSize += clusterMatching[gt, i];
                gtBySize[gt] = new KeyValuePair<int, int>(gt, labelSize);
            }
            //Sort descending by size
            // THIS LINE MAKES IT NOT WORK FOR K8 GRAPHS WITH NOISE!!
            // trying the if statement which will only sort for k=2,4 graphs
            if (L.LabelIndices.GetLength(0) < 1000)
            {
                Array.Sort(gtBySize, (x, y) => y.Value.CompareTo(x.Value));
            }
            int sumCorrect = 0;
            int noNoiseSumCorrect = 0;
            StringBuilder sb = new StringBuilder();
            StringBuilder ssb = new StringBuilder();
            StringBuilder nonoisesb = new StringBuilder();
int totalC = 0;
int noNoiseC = 0;
            //for each real cluster, assign the best of our clusters that hasn't
            //already been assigned
            for (int gtIndex = 0; gtIndex < truthCount; gtIndex++)
            {

                
                int realC = gtBySize[gtIndex].Key;
                int assignedClust = 0;
                for (int ourC = 0; ourC < partitionCount; ourC++)
                {
                    int num = clusterMatching[realC, ourC];
                    if (assigned[realC] < num && !ourCIsAssigned[ourC])
                    {
                        assigned[realC] = num;
                        assignedClust = ourC;
                    }
                }

                int sumRealC = 0;
                for (int i = 0; i < partitionCount; i++)
                    sumRealC += clusterMatching[realC, i];

                if (assigned[realC] == 0)
                {
                    Matches.Add(new GroundTruthMatch(-1, realC, 0, sumRealC, 0, L.UniqueLabels[realC]));
                    sb.AppendFormat("Label {0} was not assigned", L.UniqueLabels[realC]);
                }
                else
                {
                    ourCIsAssigned[assignedClust] = true;
                    sb.AppendFormat("Cluster {0} Assigned to Label {1} Accuracy: ({2}/{3}) {4}%",
                        assignedClust, L.UniqueLabels[realC], assigned[realC], sumRealC, 100.0 * (double)assigned[realC] / (double)sumRealC);
                    Matches.Add(new GroundTruthMatch(assignedClust, realC, assigned[realC], sumRealC, (double)assigned[realC] / sumRealC, L.UniqueLabels[realC]));
                    if (!L.UniqueLabels[realC].Equals("NA"))
                    {
                        //noNoiseC += sumRealC;
                    }
                    totalC += sumRealC;
                }

                sb.AppendLine();
                sumCorrect += assigned[realC];
                if (!L.UniqueLabels[realC].Equals("NA")) 
                {
                    noNoiseSumCorrect += assigned[realC];
                }
                
            }
            TotalMatched = sumCorrect;
            TotalSize = P.DataCount;
            TotalAccuracy = (double) sumCorrect/ P.DataCount;

            for (int q = 0; q < gtBySize.Length-1; q++ )
            {
                noNoiseC += gtBySize[q].Value;
            }

                sb.AppendFormat("Total Accuracy: ({0}/{1}) {2}%", sumCorrect, P.DataCount, 100.0 * (double)sumCorrect / (double)P.DataCount);
            ssb.AppendFormat("({0}/{1}), {2}%", sumCorrect, P.DataCount, 100.0 * (double)sumCorrect / (double)P.DataCount);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("Rev Accuracy:   ({0}/{1}) {2}%", sumCorrect, totalC, 100.0 * (double)sumCorrect / (double)totalC);
            sb.AppendLine();
            sb.AppendFormat("NoNoise Accuracy:   ({0}/{1}) {2}%", noNoiseSumCorrect, noNoiseC, 100.0 * (double)noNoiseSumCorrect / (double)noNoiseC);
            nonoisesb.AppendFormat("({0}/{1}), {2}%", noNoiseSumCorrect, noNoiseC, 100.0 * (double)noNoiseSumCorrect / (double)noNoiseC);
            sb.AppendLine();
            TextResults = sb.ToString();
            ShorterTextResults = ssb.ToString();
            NoNoiseTextResults = nonoisesb.ToString();
        }

        private static String OptimalErrorEval(Partition clusterFile, LabelList labels, int[,] clusterMatching)
        {
            return "To Be Implemented";
        }
    }
}
