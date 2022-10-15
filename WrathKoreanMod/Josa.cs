using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Kingmaker.ElementsSystem;
using Kingmaker.Utility;

namespace WrathKoreanMod;

public static class Josa
{
    private static readonly Dictionary<string, JosaPair> _josaPatternPaird = new();

    static Josa()
    {
        RegisterJosaPair("(이)가", "이", "가");
        RegisterJosaPair("(와)과", "과", "와");
        RegisterJosaPair("(을)를", "을", "를");
        RegisterJosaPair("(은)는", "은", "는");
        RegisterJosaPair("(아)야", "아", "야");
        RegisterJosaPair("(이)여", "이여", "여");
        RegisterJosaPair("(으)로", "으로", "로", exceptRieul: true);
        RegisterJosaPair("(이)라", "이라", "라");
    }

    private static void RegisterJosaPair(string key, string josa1, string josa2, bool exceptRieul = false)
    {
        if (key.Length != 4 || key[0] != '(')
        {
            throw new ArgumentException($"올바르지 않은 조사쌍 정의: {key}");
        }

        _josaPatternPaird.Add(key, new JosaPair(josa1, josa2, exceptRieul));
    }

    public static string Process(string src)
    {
        using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
        StringBuilder builder = pooledStringBuilder.Builder;

        int lastHeadIndex = 0;

        for (int i = 1; i < src.Length; i++)
        {
            if (src[i] != '(')
            {
                continue;
            }

            string key = src.Substring(i, 4);

            if (_josaPatternPaird.TryGetValue(key, out var pair))
            {
                char prevChar = src[i - 1];
                if (prevChar < '가' || prevChar > '힣') // 한글 문자가 아닐 경우
                {
                    continue;
                }

                builder.Append(src, lastHeadIndex, i - lastHeadIndex);
                i += 4;
                lastHeadIndex = i;

                if ((!pair.exceptRieul && HasJong(prevChar)) ||
                    (pair.exceptRieul && HasJongExceptRieul(prevChar)))
                {
                    builder.Append(pair.josa1);
                }
                else
                {
                    builder.Append(pair.josa2);
                }
            }
        }

        if (lastHeadIndex == 0)
        {
            return src;
        }

        builder.Append(src, lastHeadIndex, src.Length - lastHeadIndex);

        return builder.ToString();
    }

    private static bool HasJong(char inChar)
    {
        if (inChar >= 0xAC00 && inChar <= 0xD7A3) // 가 ~ 힣
        {
            int localCode = inChar - 0xAC00; // 가~ 이후 로컬 코드 
            int jongCode = localCode % 28;
            if (jongCode > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private static bool HasJongExceptRieul(char inChar)
    {
        if (inChar >= 0xAC00 && inChar <= 0xD7A3)
        {
            int localCode = inChar - 0xAC00;
            int jongCode = localCode % 28;
            if (jongCode == 8 || jongCode == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private sealed class JosaPair
    {
        public JosaPair(string josa1, string josa2, bool exceptRieul = false)
        {
            this.josa1 = josa1;
            this.josa2 = josa2;
            this.exceptRieul = exceptRieul;
        }

        public readonly string josa1;
        public readonly string josa2;
        public readonly bool exceptRieul;
    }
}
