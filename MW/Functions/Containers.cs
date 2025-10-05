using Irony.Parsing;
using MW.Audio;
using MW.Helpers;
using MW.Parsing;
using MW.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Containers
    {
        [Function(name: "t", astType: AstType.CSObject, description: "Create track")]
        public static Container CreateTrack(MethodContext context)
        {
            var container = new Container();
            AddChildren(context, container, 0);

            return container;
        }

        [Function(name: "a", astType: AstType.CSObject, description: "Add element")]
        public static Container AddTrack(MethodContext context)
        {
            if (context.Args.Count < 2)
            {
                throw new RunException($"{nameof(AddTrack)} requires 2 or more arguments");
            }

            var container = context.Args[0].Evaluate<Container>(context.Thread);
            if (container == null)
            {
                throw new RunException($"{nameof(AddTrack)} first argument must be a {typeof(Song)} or {typeof(Container)}");
            }

            AddChildren(context, container, 1);

            return container;
        } 

        private static void AddChildren(MethodContext context, Container container, int startAt = 0)
        { 
            for (int i = startAt; i < context.Args.Count; ++i)
            {
                var child = context.Args[i].Evaluate(context.Thread);

                // Extract offset?
                if (context.Args[i].Type == AstType.Object)
                {
                    var elements = (child as ObjectNode)!.Elements;

                    if (elements.ContainsKey())
                }

                switch (child)
                {
                    case AudioSource:
                    case Container:
                        {
                            if (child == container)
                            {
                                throw new RunException($"Cannot add a container to itself");
                            }
                            container.Instances.Add(Instance.CreateFrom(child));
                            break;
                        }
                    default:
                        {
                            throw new RunException($"Arguments must be of type {nameof(AudioSource)} or {nameof(Container)}");
                        }
                }
            }
        }
    }
}
