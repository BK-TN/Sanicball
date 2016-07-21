using System.Collections;
using UnityEngine;

namespace Sanicball.Match
{
    public delegate void MatchMessageHandler<T>(T message) where T : MatchMessage;

    public abstract class MatchMessage
    {
    }
}