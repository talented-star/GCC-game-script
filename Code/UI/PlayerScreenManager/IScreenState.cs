using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScreenState
{
    public ScreenState GetScreenState();
}

public enum ScreenState
{
    Closed, Opened
}