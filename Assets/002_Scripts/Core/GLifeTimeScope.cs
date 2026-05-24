using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _002_Scripts.Core
{
    public class GLifeTimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            
            
        }
    }
}