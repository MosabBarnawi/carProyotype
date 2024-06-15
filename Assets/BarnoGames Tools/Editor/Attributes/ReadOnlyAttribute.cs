using System;
using UnityEngine;

namespace BarnoGames.Tools
{
    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
