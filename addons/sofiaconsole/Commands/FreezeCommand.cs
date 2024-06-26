// ReSharper disable once CheckNamespace
namespace media.Laura.SofiaConsole.Commands;

public class FreezeCommand
{
    [ConsoleCommand("freeze", Description = "stops processing of mobs and projectiles")]
    public void Feeze()
    {
        foreach(Node2D node in Console.Instance.GetTree().Root.GetNode<Node2D>("MainScene").GetChildren())
        {
            if(node.IsInGroup("Mobs") || node.IsInGroup("Projectiles"))
                node.ProcessMode = Node.ProcessModeEnum.Disabled;
        }
    }
}