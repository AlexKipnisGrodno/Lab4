using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GeneticsProject
{
    public struct GeneticData
    {
        public string name; 
        public string organism;
        public string formula;
    };

    class Program
    {
        static List<GeneticData> data = new List<GeneticData>();

        static string GetFormula(string proteinName)
        {
            foreach (GeneticData item in data)
            {
                if (item.name.Equals(proteinName)) return item.formula;
            }
            return null;
        }
        static void ReadGeneticData(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            while (!reader.EndOfStream)
            { 
                string[] fragments = reader.ReadLine().Split('\t');
                GeneticData protein;
                protein.name = fragments[0];
                protein.organism = fragments[1];
                protein.formula = fragments[2];
                if (!IsValid(Decode(protein.formula)))
                {
                    Console.WriteLine("Ну чет не то");
                }
                data.Add(protein);
            }
            reader.Close();
        }
        static void ReadHandleCommands(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            StreamWriter writer = new StreamWriter("test.txt");
            int counter = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine(); counter++;
                string[] command = line.Split('\t');
                if (command[0].Equals("search"))
                {
                   writer.WriteLine($"{counter.ToString("D3")}   {"search"}   {Decode(command[1])}");
                    writer.WriteLine("organism                protein");
                    int index = SearchByFormula(command[1]);
                    if (index != -1)
                        writer.WriteLine($"{data[index].organism}    {data[index].name}");
                    else
                        writer.WriteLine("NOT FOUND");
                    writer.WriteLine("================================================");
                }
                if (command[0].Equals("diff"))
                {
                    writer.WriteLine($"{counter.ToString("D3")}   {"diff"}    {command[1]}    {command[2]}");
                    if (SearchByName(command[1]) != -1 && SearchByName(command[2]) != -1)
                    {
                        int diff = Diff(command[1], command[2]);
                        writer.WriteLine("amino-acids difference: ");
                        writer.WriteLine(diff);
                    }
                    else if (SearchByName(command[1]) == -1) writer.WriteLine($"Missing    {command[1]}");
                    else if (SearchByName(command[2]) == -1) writer.WriteLine($"Missing    {command[2]}");
                    else writer.WriteLine($"Missing    {command[1]}    {command[2]}");
                    writer.WriteLine("--------------------------------------------------------------------------");
                }
                if (command[0].Equals("mode"))
                {
                    writer.WriteLine($"{counter.ToString("D3")}   {"mode"}   {command[1]}");
                    writer.WriteLine("amino-acid occurs:");
                    var tuple = Mode(command[1]);
                    if (tuple != null)
                    {
                        writer.WriteLine($"{tuple.Item1}          {tuple.Item2}");
                    }
                    else writer.WriteLine($"Missing             {command[1]}");
                    writer.WriteLine("--------------------------------------------------------------------------");
                  
                }
            }
            reader.Close();
            writer.Close();
        }
        static bool IsValid(string formula)
        {
            List<char> letters = new List<char>() { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'Y' };
            foreach (char ch in formula)
            {
                if (!letters.Contains(ch)) return false;
            }
            return true;
        }

        static string Decode(string formula)
        {
            string decoded = String.Empty;
            for (int i = 0; i < formula.Length; i++)
            {
                if (char.IsDigit(formula[i]))
                {
                    char letter = formula[i + 1];
                    int conversion = formula[i] - '0';
                    for (int j = 0; j < conversion - 1; j++) decoded = decoded + letter;
                }
                else decoded = decoded + formula[i];
            }
            return decoded;
        }

        static int SearchByFormula(string item)
        {
            string decoded = Decode(item);
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].formula.Contains(decoded))
                {
                    return i;
                }
            }
            return - 1;
        } 

        static int SearchByName(string item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].name.Contains(item))
                {
                        return i;
                }
                
            }
            return -1;
        }

        static int Diff(string proteinOne, string proteinTwo)
        {
            int diff = 0;

            string formulaOne = GetFormula(proteinOne);
            string formulaTwo = GetFormula(proteinTwo);

            if(formulaOne != null && formulaTwo != null)
            {
                int minLength = Math.Min(formulaOne.Length, formulaTwo.Length);
                diff = Math.Max(formulaOne.Length, formulaTwo.Length) - minLength;
                for (int i = 0; i < minLength; i++)
                {
                    if (formulaOne[i] != formulaTwo[i])
                        diff++;
                }
                return diff;
            }
            return -1;
            
        }
        static Tuple<char, int> Mode(string protein)
        {
            Dictionary<char, int> dict = new Dictionary<char, int>();
            string formula = GetFormula(protein);
            if (formula != null)
            {
                foreach (char aminoAcid in formula)
                {
                    if (dict.ContainsKey(aminoAcid)) dict[aminoAcid] += 1;
                    else dict[aminoAcid] = 1;
                }
                int maxValue = 0;

                foreach (int value in dict.Values)
                {
                    if (value > maxValue) maxValue = value;
                }
                var sortedDict = new Dictionary<char, int>(dict);  
                char frequentAminoAcid;
                List<char> list = new List<char>();
                foreach (char ch in formula)
                {
                    if (dict[ch] == maxValue)
                        list.Add(ch);
                }
                list.Sort();
                frequentAminoAcid = list[0];
                return Tuple.Create(frequentAminoAcid, maxValue);
            } 
            else
            {
                return null;
            }
        }
        static void Main(string[] args)
        {
            ReadGeneticData("sequences.2.txt");
            ReadHandleCommands("commands.2.txt");  
        }
    }
}