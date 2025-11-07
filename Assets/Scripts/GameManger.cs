
using UnityEngine;

public class GameManger : MonoBehaviour
{
    public enum State {Aim, Launch, Resolve}
    public State Current = State.Aim;

    public void SetState(State s) => Current = s;
}
