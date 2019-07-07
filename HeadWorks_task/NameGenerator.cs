using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task
{
    public class NameGenerator
    {
        private List<string> pre = new List<string>();
        private List<string> mid = new List<string>();
        private List<string> sur = new List<string>();
        private readonly List<string> romanNames = new List<string>() { @"-a","-al","-au +c","-an","-ba","-be","-bi","-br +v","-da","-di","-do","-du","-e","-eu +c","-fa",
                                                                        "bi","be","bo","bu","nul +v","gu","da","au +c -c","fri","gus",
                                                                        "+tus","+lus","+lius","+nus","+es","+ius -c","+cus","+tor","+cio","+tin" };
        private static char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
        private static char[] Consonants = { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y' };
        private static Random rnd = new Random();
        public NameGenerator()
        {
            foreach (var line in romanNames)
            {
             if (line.Length > 0)
                {
                    if (line[0] == '-')
                        pre.Add(line.Substring(1).ToLower());
                    else if (line[0] == '+')
                        sur.Add(line.Substring(1).ToLower());
                    else
                        mid.Add(line.ToLower());
                }
            }
        }
        private string Upper(string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        private bool ContainsConsFirst(List<string> array)
        {
            foreach (string s in array)
            {
                if (ConsonantFirst(s)) return true;
            }
            return false;
        }

        private bool ContainsVocFirst(List<string> array)
        {
            foreach (string s in array)
            {
                if (VowelFirst(s)) return true;
            }
            return false;
        }

        private bool AllowCons(List<string> array)
        {
            foreach (string s in array)
            {
                if (HatesPreviousVowels(s) || !HatesPreviousConsonants(s)) return true;
            }
            return false;
        }

        private bool AllowVocs(List<string> array)
        {
            foreach (string s in array)
            {
                if (HatesPreviousConsonants(s) || HatesPreviousVowels(s) == false) return true;
            }
            return false;
        }

        private bool ExpectsVowel(string s)
        {
            if (s.Substring(1).Contains("+v")) return true;
            else return false;
        }
        private bool EexpectsConsonant(string s)
        {
            if (s.Substring(1).Contains("+c")) return true;
            else return false;
        }
        private bool HatesPreviousVowels(string s)
        {
            if (s.Substring(1).Contains("-c")) return true;
            else return false;
        }
        private bool HatesPreviousConsonants(string s)
        {
            if (s.Substring(1).Contains("-v")) return true;
            else return false;
        }

        private string PureSyl(string s)
        {
            s = s.Trim();
            if (s[0] == '+' || s[0] == '-') s = s.Substring(1);
            return s.Split(' ')[0];
        }

        private bool VowelFirst(string s)
        {
            return Vowels.Contains(char.ToLower(s[0]));
        }

        private bool ConsonantFirst(string s)
        {
            return Consonants.Contains(char.ToLower(s[0]));
        }

        private bool VowelLast(string s)
        {
            return Vowels.Contains(char.ToLower(s[s.Length - 1]));
        }

        private bool ConsonantLast(string s)
        {
            return Consonants.Contains(char.ToLower(s[s.Length - 1]));
        }
        public string Compose(int syls)
        {
            if (syls < 1) throw new ApplicationException("compose(int syls) can't have less than 1 syllable");
            int expecting = 0; // 1 for Vowel, 2 for consonant
            int last = 0; // 1 for Vowel, 2 for consonant
            string name;
            int a = (int)(rnd.NextDouble() * pre.Count);

            if (VowelLast(PureSyl(pre[a]))) last = 1;
            else last = 2;

            if (syls > 2)
            {
                if (ExpectsVowel(pre[a]))
                {
                    expecting = 1;
                    if (ContainsVocFirst(mid) == false) throw new ApplicationException("Expecting middle part starting with Vowel, " +
                             "but there is none. You should add one, or remove requirement for one.. ");
                }
                if (EexpectsConsonant(pre[a]))
                {
                    expecting = 2;
                    if (ContainsConsFirst(mid) == false) throw new ApplicationException("Expecting middle part starting with consonant, " +
                     "but there is none. You should add one, or remove requirement for one.. ");
                }
            }
            else
            {
                if (ExpectsVowel(pre[a]))
                {
                    expecting = 1;
                }
                if (EexpectsConsonant(pre[a]))
                {
                    expecting = 2;
                }
            }
            int[] b = new int[syls];
            for (int i = 0; i < b.Length - 2; i++)
            {

                do
                {
                    b[i] = (int)(rnd.NextDouble() * mid.Count);
                }
                while (expecting == 1 && VowelFirst(PureSyl(mid[b[i]])) == false || expecting == 2 && ConsonantFirst(PureSyl(mid[b[i]])) == false
                        || last == 1 && HatesPreviousVowels(mid[b[i]]) || last == 2 && HatesPreviousConsonants(mid[b[i]]));

                expecting = 0;
                if (ExpectsVowel(mid[b[i]]))
                {
                    expecting = 1;
                }
                if (EexpectsConsonant(mid[b[i]]))
                {
                    expecting = 2;
                }
               
                if (VowelLast(PureSyl(mid[b[i]]))) last = 1;
                else last = 2;
            }

            int c;
            do
            {
                c = (int)(rnd.NextDouble() * sur.Count);
            }
            while (expecting == 1 && VowelFirst(PureSyl(sur[c])) == false || expecting == 2 && ConsonantFirst(PureSyl(sur[c])) == false
                    || last == 1 && HatesPreviousVowels(sur[c]) || last == 2 && HatesPreviousConsonants(sur[c]));

            name = Upper(PureSyl(pre[a].ToLower()));
            for (int i = 0; i < b.Length - 2; i++)
            {
                name += PureSyl(mid[b[i]].ToLower());
            }
            if (syls > 1)
                name += PureSyl(sur[c].ToLower());
            return name;
        }
    }
}
