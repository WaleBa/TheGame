global using Godot;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;

namespace GameE;//groups ! body id's

enum MobType//check all scaling going in mobs
{
    Zombie,//recoil power?
    SnakeHead,
    MainCristal
} 

public partial class Global : Node
{
    public const bool CONTROLLER = false;//use of #ifdef
	public const int MAX_DISTANCE_FROM_CENTRE = 1250;

    static Global _instance { get; set;}

    public override void _Ready()
	{
        _instance = this;
    }
}
