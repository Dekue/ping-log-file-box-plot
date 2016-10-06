using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace PingMedian
{
    class Program
    {
        static double calcMedian(List<double> l)
        {
            if (l.Count % 2 == 0)
                return (l[l.Count / 2 - 1] + l[l.Count / 2]) / 2;
            else
                return l[l.Count / 2];
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] is string)
                {
                    try
                    {
                        string line;
                        string[] miliSeconds, transmittedP, receivedP;
                        int transmitted = 0, received = 0;
                        List<double> l = new List<double>();
                        StreamReader f = new StreamReader(args[0]);
                        Regex regexMs = new Regex(@"time\=\d+(\.\d{1,3})?\sms");
                        Regex regexTransmitted = new Regex(@"\d+\spackets");
                        Regex regexReceived = new Regex(@"\d+\sreceived");
                        Match match;

                        while ((line = f.ReadLine()) != null)
                        {
                            match = regexMs.Match(line);
                            if (match.Success)
                            {
                                miliSeconds = match.Value.ToString().Split(' ');
                                miliSeconds = miliSeconds[0].Split('=');

                                double miliSecondsDigit = double.Parse(miliSeconds[1], new CultureInfo("en-US"));
                                l.Add(miliSecondsDigit);
                            }
                            match = regexTransmitted.Match(line);
                            if (match.Success)
                            {
                                transmittedP = match.Value.ToString().Split(' ');
                                transmitted = Convert.ToInt32(transmittedP[0]);
                            }
                            match = regexReceived.Match(line);
                            if (match.Success)
                            {
                                receivedP = match.Value.ToString().Split(' ');
                                received = Convert.ToInt32(receivedP[0]);
                            }
                        }
                        f.Close();
                        l.Sort();

                        List<double> lQ25, lQ75;
                        double median, Q25, Q75;

                        if (l.Count % 2 == 0)
                        {
                            median = (l[l.Count / 2 - 1] + l[l.Count / 2]) / 2;

                            lQ75 = l.GetRange(l.Count / 2 + 1, l.Count / 2 - 1);
                            Q75 = calcMedian(lQ75);

                            lQ25 = l.GetRange(0, l.Count / 2 - 1);
                            Q25 = calcMedian(lQ25);
                        }
                        else
                        {
                            median = l[l.Count / 2];

                            lQ75 = l.GetRange(l.Count / 2 + 1, l.Count / 2);
                            Q75 = calcMedian(lQ75);

                            lQ25 = l.GetRange(0, l.Count / 2);
                            Q25 = calcMedian(lQ25);
                        }

                        double IQR = Q75 - Q25, lowerWhisker = l.Max(), upperWhisker = l.Min();
                        int lowerExtremeOutlier = 0, upperExtremeOutlier = 0, lowerOutlier = 0, upperOutlier = 0;
                        try
                        {
                            for (int i = l.Count-1; i > 0; i--)
                            {
                                if (IQR * 1.5 + Q75 > l[i])
                                {
                                    upperWhisker = l[i];
                                    break;
                                }
                                if (IQR * 3 + Q75 > l[i])
                                    upperExtremeOutlier++;
                                else
                                    upperOutlier++;
                            }

                            for (int i = 0; i < l.Count; i++)
                            {
                                if (Q25 - IQR * 1.5 < l[i])
                                {
                                    lowerWhisker = l[i];
                                    break;
                                }
                                if (Q25 - IQR * 3 < l[i])
                                    lowerExtremeOutlier++;
                                else
                                    lowerOutlier++;
                            }
                        }
                        catch
                        {
                        }

                        string newPath = args[0] + ".median";
                        StreamWriter newF = new StreamWriter(newPath, false);

                        newF.WriteLine("x " + median.ToString(CultureInfo.GetCultureInfo("en-US")) + " " +
                            Q25.ToString(CultureInfo.GetCultureInfo("en-US")) + " " +
                            Q75.ToString(CultureInfo.GetCultureInfo("en-US")) + " " +
                            upperWhisker.ToString(CultureInfo.GetCultureInfo("en-US")) + " " +
                            lowerWhisker.ToString(CultureInfo.GetCultureInfo("en-US")));
                        //newF.WriteLine("WhiskerMax: " + (IQR * 1.5 + Q75) + ", WhiskerMin: " + (Q25 - IQR * 1.5));
                        newF.WriteLine("lower outliers: " + lowerOutlier + ", extreme lower outliers: " + lowerExtremeOutlier + " (Min: " + l.Min() + ")");
                        newF.WriteLine("upper outliers: " + upperOutlier + ", extreme upper outliers: " + upperExtremeOutlier + " (Max: " + l.Max() + ")");
                        newF.WriteLine("packets transmitted: " + transmitted + ", received: " + received + ", packet loss: " + (transmitted - received));
                        newF.Close();
                    }
                    catch
                    {
                        System.Console.WriteLine("Ping log file is malformed.");
                    }
                }
                else
                    System.Console.WriteLine("Please specify the path to a sortable Textfile as an argument.");
            }
            else
                System.Console.WriteLine("Please specify the path to a sortable Textfile as an argument.");
        }
    }
}
