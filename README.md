# require-child

This plugin adds `RequireChild` attribute for C# in godot

## Example

It can simplify this code:

```csharp
using Godot;

public class Player : Node
{
    private Sprite _Sprite;

    public override void _Ready()
    {
        _Sprite = GetNode<Sprite>("./Sprite");
    }

    public override void _Process(float delta)
    {
        _Sprite.RotationDegrees += delta * 100;
    }
}
```

To this code:

```csharp
using Godot;
using Picalines.Godot.RequireChild;

public class Player : Node
{
    [RequireChild("Sprite")] private readonly Sprite _Sprite; // finally I can use readonly fields for nodes!

    public override void _Process(float delta)
    {
        _Sprite.RotationDegrees += delta * 100;
    }
}
```

So you can add this attribute to field / auto property to remove `GetNode`s from `_Ready`. It's basicly an `onready` in C#!

Also it will throw an exception if you forgot to add child node. No more unhelpful null reference exceptions!

## Setup

Here's simple steps to install & use this plugin:

1. Download `addons/require-child` from this repo. Paste it into `res://`

2. In `Project Settings > Autoload` add `res://addons/require-child/RequireChildHandler.cs` as a singletone

## How it works

`RequireChildHandler` singletone assigns the fields with `RequireChild` attribute before `_Ready` (but after `_EnterTree`!)

To do that it firstly scans every custom non-abstract class that inherits `Node` and has members with *the* attribute. Sounds *heavy*, but it happens once, when a game launches. Assignment happens in `SceneTree.node_added` signal, where the plugin lookups it's dictionary

## What I *don't* recommend

Here's two edgecases, where I wouldn't use this plugin:

1. "Optional" nodes. At first I wanted to add something like `RequireChild("Sprite", Optional = true)`, but it increases complexity. Also it just sounds weird to *require optionally*

2. Mutable fields / properties or some "dynamic" logic. Plugin does the assignments every time a node enters the `SceneTree` (it can happen more than once!), so it will be hard to debug
