using Kingmaker.TextTools;
using Kingmaker;

namespace WrathKoreanMod.TextTemplates;

internal class PlayerIsCommanderTemplate : TextTemplate
{
    public override int MinParamenters => 2;

    public override int MaxParamenters => 2;

    public override string Generate(bool capitalized, List<string> parameters)
    {
        return (Game.Instance.Player.Chapter >= 2) ? parameters[0] : parameters[1];
    }
}
